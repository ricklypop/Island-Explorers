using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DeviceConsole
{
	public class ScreenConsole : MonoBehaviour
	{
		#region inspector
		// menu vars
		public int menuButtonWidth = 80;		// menu button width
		public int menuButtonHeight = 30;		// menu button height
		public int menuFontSize = 16;			// menu font initial size

		// console vars
		public int consoleButtonWidth = 80;	// console button width
		public int consoleButtonHeight = 30;	// console button height
		public int consoleFontSize = 16;		// console font initial size
		public int consoleFontSizeStep = 1;
		public float consoleSizeScaleStep = 0.1f;

		public Color consoleBgColor = new Color(0, 0, 0, 0.8f);

		// other vars
		public int maxLogCount = 100;
		public bool showConsole = true;

		// gesture
		public GameObject gestureDetectorPrefab;
		#endregion

		#region consts
		private const string STR_DRAG = "Drag";
		private const string STR_SHOW_CONSOLE = "Show Console";
		private const string STR_HIDE_CONSOLE = "Hide Console";
		private const string STR_EXPAND = ">>>";
		private const string STR_COLLAPSE = "<<<";
		private const string STR_WIDTH_LONGER = ">";
		private const string STR_WIDTH_SHORTEN = "<";
		private const string STR_HEIGHT_LONGER = "v";
		private const string STR_HEIGHT_SHORTEN = "^";
		private const string STR_ADD = "+";
		private const string STR_SUB = "-";
		private const string STR_AUTO_SCROLL_TO_BOTTOM = "Auto Scroll To Bottom";
		private const string STR_RESIZE_WHILE_ROTATE = "Resize While Rotate";
		private const string STR_MENU = "Menu";
		private const string STR_CONSOLE = "Console";
		private const string STR_CLEAR = "Clear";
		private const string STR_CLOSE = "X";

		private const string LOGTYPE_INFO = "I";
		private const string LOGTYPE_WARNING = "W";
		private const string LOGTYPE_ERROR = "E";
		private const string LOGTYPE_UNKNOWN = "?";
		private const int ADSORB_PADDING = 10;
		#endregion

		#region screen vars
		private int m_screenWidth;
		private int m_screenHeight;
		private bool m_shouldUpdateScreenSize;
		#endregion

		#region menu vars
		private Rect m_menuBounds = new Rect();
		private Rect m_menuDragArea = new Rect();

		private bool m_shouldResetMenuPosition;
		private bool m_shouldUpdateMenuWidth;
		private bool m_shouldUpdateMenuHeight;
		
		private int m_curMenuFontSize;
		private bool m_shouldUpdateMenuButtonStyle;
		private bool m_shouldUpdateMenuLabelStyle;

		private GUIStyle m_menuButtonStyle;
		private GUIStyle m_menuLabelStyle;

		private bool m_showMenuControlButtons = true;
		private bool m_resizeWhileRotate = true;
		#endregion

		#region console vars
		private Vector2 m_consoleSizeRatio = new Vector2(0.5f, 0.9f);	// console window initial ratio
		private Rect m_consoleBounds = new Rect();
		private Rect m_consoleDragArea = new Rect();
		
		private float m_consoleWidthScale = 1f;
		private float m_consoleHeightScale = 1f;
		
		private bool m_shouldResetConsolePosition;
		private bool m_shouldUpdateConsoleWidth;
		private bool m_shouldUpdateConsoleHeight;
		
		private int m_curConsoleFontSize;	// console font current size
		private bool m_shouldUpdateConsoleButtonStyle;
		private bool m_shouldUpdateConsoleLabelStyle;

		private bool m_autoScrollToConsoleBottom = true;
		private bool m_shouldScrollToConsoleBottom;

		private bool m_showInfo = true;
		private bool m_showWarning = true;
		private bool m_showError = true;
		private bool m_showTime = false;

		private int m_infoCount;
		private int m_warningCount;
		private int m_errorCount;

		private Texture2D m_iconInfo;
		private Texture2D m_iconWarning;
		private Texture2D m_iconError;
		
		private Vector2 m_consoleScrollPosition = new Vector2();

		private GUIStyle m_consoleBgStyle = new GUIStyle();
		private GUIStyle m_consoleLabelStyle;
		private GUIStyle m_consoleButtonStyle;

		private int m_hSpace;
		private int m_wSpace;
		#endregion

		#region gesture vars
		private GameObject m_gestureDetectorObj;
		private DragDetector m_dragDetector;
		private Rect m_consoleLogDragArea = new Rect();
		#endregion

		#region temp vars
		// menu vars
		private GUIContent m_menuShowHideConsoleContent = new GUIContent();
		private GUIContent m_menuShowHideControlContent = new GUIContent();

		private GUILayoutOption m_menuButtonWidthOption;
		private GUILayoutOption m_menuButtonHeightOption;

		// console vars
		private GUIContent m_consoleLogTypeContent = new GUIContent();
		private GUILayoutOption m_consoleLogTypeWidthOption = GUILayout.Width(18);
		private GUILayoutOption m_consoleTimeWidthOption = GUILayout.Width(90);
		
		private GUIContent m_consoleInfoContent = new GUIContent();
		private GUIContent m_consoleWarningContent = new GUIContent();
		private GUIContent m_consoleErrorContent = new GUIContent();
		private GUIContent m_consoleTimeContent = new GUIContent("Time");
		
		private GUILayoutOption m_consoleButtonWidthOption;
		private GUILayoutOption m_consoleButtonHeightOption;
		private GUILayoutOption m_consoleCloseButtonWidthOption;
		private GUILayoutOption m_consoleCloseButtonHeightOption;

		// others
		private bool m_tempBool;
		private Vector2 m_tempPoint = new Vector2();
		#endregion

		void Start()
		{
			CheckParams();
			InitScreen();
			InitMenu();
			InitConsole();
			InitLogHandler();
			CreateGestureDetector();

			LogHandler.RegisterLogCallback();
			LogHandler.onNewLogToScreen += HandleOnNewLogToScreen;
		}

		void OnDestroy()
		{
			DestroyGestureDetector();

			LogHandler.UnregisterLogCallback();
			LogHandler.onNewLogToScreen -= HandleOnNewLogToScreen;
		}

		void OnEnable()
		{
			LogHandler.shouldLogToScreen = true;
		}
		
		void OnDisable()
		{
			LogHandler.shouldLogToScreen = false;
		}

		void OnGUI()
		{
			if (IsScreenSizeChanged() && m_resizeWhileRotate)
			{
				DoIfScreenSizeChanged();
			}
			
			ProcessUpdateRequests();
			
			// draw menu window
			GUIHelper.AdsorbToFullScreen(ref m_menuBounds, ADSORB_PADDING);
			m_menuBounds = GUI.Window(0, m_menuBounds, DrawMenu, STR_MENU);
			
			// draw console window
			if (showConsole)
			{
				GUIHelper.AdsorbToFullScreen(ref m_consoleBounds, ADSORB_PADDING);
				m_consoleBounds = GUI.Window(1, m_consoleBounds, DrawConsole, STR_CONSOLE);

				if (m_dragDetector == null)
				{
					m_consoleDragArea.Set(0, 0, m_consoleBounds.width, m_consoleBounds.height-m_hSpace);
				}
				else
				{
					m_consoleDragArea.Set(0, 0, m_consoleBounds.width, m_hSpace);
					m_consoleLogDragArea.Set(m_consoleBounds.x+m_wSpace, m_consoleBounds.y+m_hSpace, m_consoleBounds.width-m_wSpace*2, m_consoleBounds.height-m_hSpace*2);
				}
			}
		}

		private void CheckParams()
		{
			// menu vars
			CheckIntValue("menuButtonWidth", menuButtonWidth, 0);
			CheckIntValue("menuButtonHeight", menuButtonHeight, 0);
			CheckIntValue("menuFontSize", menuFontSize, 0);

			// console vars
			CheckIntValue("consoleButtonWidth", consoleButtonWidth, 0);
			CheckIntValue("consoleButtonHeight", consoleButtonHeight, 0);
			CheckIntValue("consoleFontSize", consoleFontSize, 0);
			CheckIntValue("consoleFontSizeStep", consoleFontSizeStep, 0);
			CheckFloatValue("consoleSizeScaleStep", consoleSizeScaleStep, 0);
		}

		private void InitLogHandler()
		{
			LogHandler.LogToScreen.MaxCount = maxLogCount;
		}

		private void ProcessUpdateRequests()
		{
			// process update requests for screen
			if (m_shouldUpdateScreenSize)
			{
				UpdateScreenSize();
				m_shouldUpdateScreenSize = false;
			}
			
			// process update requests for menu
			if (m_shouldResetMenuPosition)
			{
				ResetMenuPosition();
				m_shouldResetMenuPosition = false;
			}
			if (m_shouldUpdateMenuWidth)
			{
				UpdateMenuWidth();
				m_shouldUpdateMenuWidth = false;
			}
			if (m_shouldUpdateMenuHeight)
			{
				UpdateMenuHeight();
				m_shouldUpdateMenuHeight = false;
			}
			
			if (m_shouldUpdateMenuButtonStyle)
			{
				UpdateMenuButtonStyle();
				m_shouldUpdateMenuButtonStyle = false;
			}
			
			if (m_shouldUpdateMenuLabelStyle)
			{
				UpdateMenuLabelStyle();
				m_shouldUpdateMenuLabelStyle = false;
			}
			
			// process update requests for console
			if (m_shouldResetConsolePosition)
			{
				ResetConsolePosition();
				m_shouldResetConsolePosition = false;
			}
			if (m_shouldUpdateConsoleWidth)
			{
				UpdateConsoleWidth();
				m_shouldUpdateConsoleWidth = false;
			}
			if (m_shouldUpdateConsoleHeight)
			{
				UpdateConsoleHeight();
				m_shouldUpdateConsoleHeight = false;
			}
			
			if (m_shouldUpdateConsoleButtonStyle)
			{
				UpdateConsoleButtonStyle();
				m_shouldUpdateConsoleButtonStyle = false;
			}
			
			if (m_shouldUpdateConsoleLabelStyle)
			{
				UpdateConsoleLabelStyle();
				m_shouldUpdateConsoleLabelStyle = false;
			}
		}

		#region screen func
		private void InitScreen()
		{
			m_screenWidth = Screen.width;
			m_screenHeight = Screen.height;
			m_shouldUpdateScreenSize = false;
		}

		private bool IsScreenSizeChanged()
		{
			if ((m_screenWidth != Screen.width) || (m_screenHeight != Screen.height))
			{
				return true;
			}
			return false;
		}
		
		private void DoIfScreenSizeChanged()
		{
			m_shouldUpdateScreenSize = true;
			
			m_shouldResetMenuPosition = true;
			m_shouldUpdateMenuWidth = true;
			m_shouldUpdateMenuHeight = true;
			
			m_shouldResetConsolePosition = true;
			m_shouldUpdateConsoleWidth = true;
			m_shouldUpdateConsoleHeight = true;
		}

		private void UpdateScreenSize()
		{
			m_screenWidth = Screen.width;
			m_screenHeight = Screen.height;
		}

		#endregion

		#region menu func
		private void InitMenu()
		{
			m_shouldUpdateMenuWidth = true;
			m_shouldUpdateMenuHeight = true;
			
			m_curMenuFontSize = menuFontSize;
			m_shouldUpdateMenuButtonStyle = true;
			m_shouldUpdateMenuLabelStyle = true;
			
			m_menuButtonWidthOption = GUILayout.Width(menuButtonWidth);
			m_menuButtonHeightOption = GUILayout.Height(menuButtonHeight);
		}

		private void ResetMenuPosition()
		{
			m_menuBounds.x = 0;
			m_menuBounds.y = 0;
		}
		
		private void UpdateMenuWidth()
		{
			int width = menuButtonWidth * 2 + 40;
			m_menuBounds.width = width;
		}
		
		private void UpdateMenuHeight()
		{
			int rows = 1;	// show/hide console button
			if (showConsole)
			{
				rows += 1;	// expand/collapse button

				if (m_showMenuControlButtons)
				{
					rows += 4;	// control console buttons
					rows += 1;	// resize while rotate button
				}
			}
			rows += 1;		// tips

			int titleHeight = 18;
			int totalHeight = rows * (menuButtonHeight + 5) + titleHeight;
			m_menuBounds.height = totalHeight;
		}
		
		private void UpdateMenuButtonStyle()
		{
			if (m_menuButtonStyle == null)
			{
				m_menuButtonStyle = new GUIStyle(GUI.skin.button);
			}
			m_menuButtonStyle.fontSize = m_curMenuFontSize;
		}
		
		private void UpdateMenuLabelStyle()
		{
			if (m_menuLabelStyle == null)
			{
				m_menuLabelStyle = new GUIStyle(GUI.skin.label);
				m_menuLabelStyle.alignment = TextAnchor.MiddleCenter;
			}
			m_menuLabelStyle.fontSize = m_curMenuFontSize;
		}

		private void DrawMenu(int windowID)
		{
			DrawMenuShowHideConsoleButton();

			if (showConsole)
			{
				DrawMenuShowHideControlButton();
				
				if (m_showMenuControlButtons)
				{
					DrawMenuConsoleButtons();
					DrawMenuResizeButton();
				}
			}

			DrawMenuTips();
			
			DragMenu();
		}

		private void DrawMenuTips()
		{
			GUILayout.Label(STR_DRAG, m_menuLabelStyle, m_menuButtonHeightOption);
		}
		
		private void DragMenu()
		{
			m_menuDragArea.Set(0, 0, m_menuBounds.width, m_menuBounds.height);
			GUI.DragWindow(m_menuDragArea);
		}

		private void DrawMenuShowHideConsoleButton()
		{
			m_menuShowHideConsoleContent.text = showConsole ? STR_HIDE_CONSOLE : STR_SHOW_CONSOLE;
			bool show = Toggle(showConsole, m_menuShowHideConsoleContent, m_menuButtonStyle, m_menuButtonHeightOption);
			if (show != showConsole)
			{
				m_shouldUpdateMenuHeight = true;
			}
			showConsole = show;
		}

		private void DrawMenuShowHideControlButton()
		{
			m_menuShowHideControlContent.text = m_showMenuControlButtons ? STR_COLLAPSE : STR_EXPAND;
			bool show = Toggle(m_showMenuControlButtons, m_menuShowHideControlContent, m_menuButtonStyle, m_menuButtonHeightOption);
			if (show != m_showMenuControlButtons)
			{
				m_shouldUpdateMenuHeight = true;
			}
			m_showMenuControlButtons = show;
		}

		private void DrawMenuConsoleButtons()
		{
			GUILayout.BeginHorizontal();
			bool isIncreaseConsoleWidth = GUILayout.Button(STR_WIDTH_LONGER, m_menuButtonStyle, m_menuButtonWidthOption, m_menuButtonHeightOption);
			GUILayout.FlexibleSpace();
			bool isDecreaseConsoleWidth = GUILayout.Button(STR_WIDTH_SHORTEN, m_menuButtonStyle, m_menuButtonWidthOption, m_menuButtonHeightOption);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			bool isIncreaseConsoleHeight = GUILayout.Button(STR_HEIGHT_LONGER, m_menuButtonStyle, m_menuButtonWidthOption, m_menuButtonHeightOption);
			GUILayout.FlexibleSpace();
			bool isDecreaseConsoleHeight = GUILayout.Button(STR_HEIGHT_SHORTEN, m_menuButtonStyle, m_menuButtonWidthOption, m_menuButtonHeightOption);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			bool isIncreaseFontSize = GUILayout.Button(STR_ADD, m_menuButtonStyle, m_menuButtonWidthOption, m_menuButtonHeightOption);
			GUILayout.FlexibleSpace();
			bool isDecreaseFontSize = GUILayout.Button(STR_SUB, m_menuButtonStyle, m_menuButtonWidthOption, m_menuButtonHeightOption);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			m_tempBool = m_autoScrollToConsoleBottom;
			m_autoScrollToConsoleBottom = Toggle(m_autoScrollToConsoleBottom, STR_AUTO_SCROLL_TO_BOTTOM, m_menuButtonStyle, m_menuButtonHeightOption);
			if (m_tempBool != m_autoScrollToConsoleBottom)
			{
				NotifyLogChanged();
			}
			GUILayout.EndHorizontal();
			
			if (isIncreaseConsoleWidth)
			{
				m_consoleWidthScale += consoleSizeScaleStep;
				m_shouldUpdateConsoleWidth = true;
			}
			else if (isDecreaseConsoleWidth)
			{
				m_consoleWidthScale -= consoleSizeScaleStep;
				if (m_consoleWidthScale < consoleSizeScaleStep)
				{
					m_consoleWidthScale = consoleSizeScaleStep;
				}
				m_shouldUpdateConsoleWidth = true;
			}
			
			if (isIncreaseConsoleHeight)
			{
				m_consoleHeightScale += consoleSizeScaleStep;
				m_shouldUpdateConsoleHeight = true;
			}
			else if (isDecreaseConsoleHeight)
			{
				m_consoleHeightScale -= consoleSizeScaleStep;
				if (m_consoleHeightScale < consoleSizeScaleStep)
				{
					m_consoleHeightScale = consoleSizeScaleStep;
				}
				m_shouldUpdateConsoleHeight = true;
			}
			
			if (isIncreaseFontSize)
			{
				m_curConsoleFontSize += consoleFontSizeStep;
				
//				m_shouldUpdateConsoleButtonStyle = true;
				m_shouldUpdateConsoleLabelStyle = true;
			}
			else if (isDecreaseFontSize)
			{
				m_curConsoleFontSize -= consoleFontSizeStep;
				if (m_curConsoleFontSize < consoleFontSizeStep)
				{
					m_curConsoleFontSize = consoleFontSizeStep;
				}
				
//				m_shouldUpdateConsoleButtonStyle = true;
				m_shouldUpdateConsoleLabelStyle = true;
			}
		}

		private void DrawMenuResizeButton()
		{
			m_resizeWhileRotate = Toggle(m_resizeWhileRotate, STR_RESIZE_WHILE_ROTATE, m_menuButtonStyle, m_menuButtonHeightOption);
		}
		#endregion

		#region console func
		private void InitConsole()
		{
			m_iconInfo = GUIHelper.CreateIcon(Color.white, 12, 12, 2, 2);
			m_iconWarning = GUIHelper.CreateIcon(Color.yellow, 12, 12, 2, 2);
			m_iconError = GUIHelper.CreateIcon(Color.red, 12, 12, 2, 2);
			
			Texture2D bgTex = null;
			if (!consoleBgColor.Equals(new Color(0, 0, 0, 0)))
			{
				bgTex = GUIHelper.CreateImage(consoleBgColor, 16, 16);
			}
			SetBgStyle(bgTex);
			
			m_shouldResetConsolePosition = true;
			m_shouldUpdateConsoleWidth = true;
			m_shouldUpdateConsoleHeight = true;
			
			m_curConsoleFontSize = consoleFontSize;
			m_shouldUpdateConsoleButtonStyle = true;
			m_shouldUpdateConsoleLabelStyle = true;
			
			m_consoleButtonWidthOption = GUILayout.Width(consoleButtonWidth);
			m_consoleButtonHeightOption = GUILayout.Height(consoleButtonHeight);
			
			m_consoleCloseButtonWidthOption = GUILayout.Width(consoleButtonWidth / 2);
			m_consoleCloseButtonHeightOption = GUILayout.Height(consoleButtonHeight);

			m_hSpace = (consoleButtonHeight > 30) ? consoleButtonHeight : 30;
			m_hSpace += 15;
			m_wSpace = 30;
		}

		private void ResetConsolePosition()
		{
			m_consoleBounds.x = m_menuBounds.width + 10;
			m_consoleBounds.y = 0;
		}
		
		private void UpdateConsoleWidth()
		{
			float width = m_screenWidth * m_consoleSizeRatio.x * m_consoleWidthScale;
			m_consoleBounds.width = width;
		}
		
		private void UpdateConsoleHeight()
		{
			float height = m_screenHeight * m_consoleSizeRatio.y * m_consoleHeightScale;
			m_consoleBounds.height = height;
		}
		
		private void UpdateConsoleButtonStyle()
		{
			if (m_consoleButtonStyle == null)
			{
				m_consoleButtonStyle = new GUIStyle(GUI.skin.button);
			}
			m_consoleButtonStyle.fontSize = m_curConsoleFontSize;
		}
		
		private void UpdateConsoleLabelStyle()
		{
			if (m_consoleLabelStyle == null)
			{
				m_consoleLabelStyle = new GUIStyle(GUI.skin.label);
			}
			m_consoleLabelStyle.fontSize = m_curConsoleFontSize;
		}

		private void ScrollToConsoleBottom()
		{
			m_consoleScrollPosition.y = Mathf.Infinity;
		}

		private void DrawConsole(int windowID)
		{
			GUIHelper.SaveColors();
			
			m_consoleScrollPosition = GUILayout.BeginScrollView(m_consoleScrollPosition, m_consoleBgStyle);
			
			ResetLogCount();
			LinkedList<LogEntry> entries = LogHandler.LogToScreen.Entries;
			foreach (LogEntry entry in entries)
			{
				StatistickLogCount(entry.logType);
				if (!NeedShowLogType(entry.logType))
				{
					continue;
				}
				DrawLogEntry(entry);
			}
			
			GUILayout.EndScrollView();

			if (m_shouldScrollToConsoleBottom)
			{
				ScrollToConsoleBottom();
				m_shouldScrollToConsoleBottom = false;
			}
			
			DrawToolbar();
			
			GUIHelper.RestoreColors();
			
			DragConsole();
		}

		private void DragConsole()
		{
			GUI.DragWindow(m_consoleDragArea);
		}
		
		private void SetBgStyle(Texture2D bgTex)
		{
			m_consoleBgStyle.border = new RectOffset(0, 0, 0, 0);
			m_consoleBgStyle.padding = new RectOffset(0, 0, 0, 0);
			m_consoleBgStyle.margin = new RectOffset(0, 0, 0, 0);
			m_consoleBgStyle.stretchWidth = true;
			if (bgTex != null)
			{
				m_consoleBgStyle.normal.background = bgTex;
			}
		}
		
		private void ResetLogCount()
		{
			m_infoCount = 0;
			m_warningCount = 0;
			m_errorCount = 0;
		}
		
		private void StatistickLogCount(LogType logType)
		{
			if (logType == LogType.Log)
			{
				m_infoCount++;
			}
			else if (logType == LogType.Warning)
			{
				m_warningCount++;
			}
			else if (logType == LogType.Error)
			{
				m_errorCount++;
			}
		}
		
		private bool NeedShowLogType(LogType logType)
		{
			if ((logType == LogType.Log && !m_showInfo)
			    || (logType == LogType.Warning && !m_showWarning)
			    || (logType == LogType.Error && !m_showError)
			    || (logType == LogType.Exception && !m_showError))
			{
				return false;
			}
			return true;
		}
		
		private void DrawLogEntry(LogEntry entry)
		{
			GUILayout.BeginHorizontal();
			
			// draw logType
			Texture2D icon = m_iconError;
			if (entry.logType == LogType.Log) 
			{
				icon = m_iconInfo;
			}
			else if (entry.logType == LogType.Warning)
			{
				icon = m_iconWarning;
			}
			else if (entry.logType == LogType.Error)
			{
				icon = m_iconError;
			}
			m_consoleLogTypeContent.image = icon;
			GUILayout.Label(m_consoleLogTypeContent, m_consoleLogTypeWidthOption);
			
			// draw timestamp
			if (m_showTime)
			{
				GUILayout.Label(entry.timeStamp, m_consoleTimeWidthOption);
			}
			
			// draw message
			GUILayout.Label(entry.message, m_consoleLabelStyle);
			
			GUILayout.EndHorizontal();
		}

		private void DrawToolbar()
		{
			GUILayout.BeginHorizontal();
			
			// clear
			if (GUILayout.Button(STR_CLEAR, m_consoleButtonStyle, m_consoleButtonWidthOption, m_consoleButtonHeightOption))
			{
				LogHandler.LogToScreen.Clear();
			}
			
			// space
			GUILayout.FlexibleSpace();

			bool isLogChanged = false;

			// info
			string info = CreateLogTypeText(LogType.Log, m_infoCount);
			m_consoleInfoContent.text = info;
			m_consoleInfoContent.image = m_iconInfo;
			m_tempBool = m_showInfo;
			m_showInfo = Toggle(m_showInfo, m_consoleInfoContent, m_consoleButtonStyle, m_consoleButtonWidthOption, m_consoleButtonHeightOption);
			if (m_tempBool != m_showInfo)
			{
				isLogChanged = true;
			}
			
			// warning
			string warning = CreateLogTypeText(LogType.Warning, m_warningCount);
			m_consoleWarningContent.text = warning;
			m_consoleWarningContent.image = m_iconWarning;
			m_tempBool = m_showWarning;
			m_showWarning = Toggle(m_showWarning, m_consoleWarningContent, m_consoleButtonStyle, m_consoleButtonWidthOption, m_consoleButtonHeightOption);
			if (m_tempBool != m_showWarning)
			{
				isLogChanged = true;
			}

			// error
			string error = CreateLogTypeText(LogType.Error, m_errorCount);
			m_consoleErrorContent.text = error;
			m_consoleErrorContent.image = m_iconError;
			m_tempBool = m_showError;
			m_showError = Toggle(m_showError, m_consoleErrorContent, m_consoleButtonStyle, m_consoleButtonWidthOption, m_consoleButtonHeightOption);
			if (m_tempBool != m_showError)
			{
				isLogChanged = true;
			}

			// time
			m_tempBool = m_showTime;
			m_showTime = Toggle(m_showTime, m_consoleTimeContent, m_consoleButtonStyle, m_consoleButtonWidthOption, m_consoleButtonHeightOption);
			if (m_tempBool != m_showTime)
			{
				isLogChanged = true;
			}

			if (isLogChanged)
			{
				NotifyLogChanged();
			}

			// space
			GUILayout.FlexibleSpace();
			
			// close
			if (GUILayout.Button(STR_CLOSE, m_consoleButtonStyle, m_consoleCloseButtonWidthOption, m_consoleCloseButtonHeightOption))
			{
				showConsole = false;
				m_shouldUpdateMenuHeight = true;
			}
			
			GUILayout.EndHorizontal();
		}
		
		private static string CreateLogTypeText(LogType logType, int logCount)
		{
			string strLogType = LOGTYPE_UNKNOWN;
			switch (logType)
			{
			case LogType.Log:
				strLogType = LOGTYPE_INFO;
				break;
				
			case LogType.Warning:
				strLogType = LOGTYPE_WARNING;
				break;
				
			case LogType.Error:
				strLogType = LOGTYPE_ERROR;
				break;
			}
			
			string strLogCount = (logCount > 99) ? "99+" : (logCount + "");
			string text = string.Format("{0}({1})", strLogType, strLogCount);
			return text;
		}

		private void HandleOnNewLogToScreen(LogEntry entry)
		{
			NotifyLogChanged();
		}

		private void NotifyLogChanged()
		{
			if (m_autoScrollToConsoleBottom)
			{
				m_shouldScrollToConsoleBottom = true;
			}
		}
		#endregion

		#region gesture detector
		void CreateGestureDetector()
		{
			if (gestureDetectorPrefab != null)
			{
				m_gestureDetectorObj = Instantiate(gestureDetectorPrefab) as GameObject;
				m_gestureDetectorObj.name = gestureDetectorPrefab.name;
				m_gestureDetectorObj.transform.parent = this.transform;
				
				#if UNITY_EDITOR
				m_dragDetector = m_gestureDetectorObj.GetComponent<MouseDragDetector>();
				#else
				m_dragDetector = m_gestureDetectorObj.GetComponent<TouchDragDetector>();
				#endif
			}
			
			if (m_dragDetector != null)
			{
				m_dragDetector.onDrag += HandleOnDrag;
			}
		}
		
		void DestroyGestureDetector()
		{
			if (m_dragDetector != null)
			{
				m_dragDetector.onDrag -= HandleOnDrag;
			}
			
			if (m_gestureDetectorObj != null)
			{
				GameObject.Destroy(m_gestureDetectorObj);
			}
		}
		
		void HandleOnDrag(Vector2 startPos, Vector2 endPos, Vector2 delta)
		{
			if (!showConsole)
			{
				return;
			}

			m_tempPoint.Set(startPos.x, Screen.height - startPos.y);
			if (m_consoleLogDragArea.Contains(m_tempPoint))
			{
				m_consoleScrollPosition.y += delta.y;
			}
		}
		#endregion

		#region utils
		private static bool Toggle(bool flag, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
		{
			Color bgColor = flag ? Color.black : GUIHelper.BackgroundColor;
			GUIHelper.SetBackgroundColor(bgColor);
			
			if (GUILayout.Button(content, style, options))
			{
				flag = !flag;
			}
			
			GUIHelper.RestorBackgroundColor();
			return flag;
		}

		private static bool Toggle(bool flag, string text, GUIStyle style, params GUILayoutOption[] options)
		{
			Color bgColor = flag ? Color.black : GUIHelper.BackgroundColor;
			GUIHelper.SetBackgroundColor(bgColor);
			
			if (GUILayout.Button(text, style, options))
			{
				flag = !flag;
			}
			
			GUIHelper.RestorBackgroundColor();
			return flag;
		}

		private static void CheckIntValue(string valueName, int curValue, int minValue)
		{
			if (curValue <= minValue)
			{
				string s = string.Format("Param \"{0}\" invalid. Current: {1}. It should > {2}", valueName, curValue, minValue);
				Debug.LogError(s);
			}
		}
		
		private static void CheckFloatValue(string valueName, float curValue, float minValue)
		{
			if (curValue <= minValue)
			{
				string s = string.Format("Param \"{0}\" invalid. Current: {1}. It should > {2}", valueName, curValue, minValue);
				Debug.LogError(s);
			}
		}
		#endregion
	}
}


