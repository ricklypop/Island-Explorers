using UnityEngine;
using System.Collections;

namespace DeviceConsole
{
	public class Example : MonoBehaviour 
	{
		#region inspector
		public GameObject context;
		public int logButtonWidth = 80;
		public int logButtonHeight = 30;
		public int logButtonFontSize = 16;
		#endregion

		private Rect m_btnPos;
		private GUIStyle m_btnStyle;
		private bool m_isInited = false;

		void Start()
		{
		}

		void Init()
		{
			m_btnPos = new Rect(0, 0, logButtonWidth, logButtonHeight);
			
			m_btnStyle = new GUIStyle(GUI.skin.button);
			m_btnStyle.fontSize = logButtonFontSize;
		}
		
		void OnGUI()
		{
			if (!m_isInited)
			{
				Init();
				m_isInited = true;
			}

			m_btnPos.x = 10;
			m_btnPos.y = Screen.height - logButtonHeight - 10;

			bool isClick = GUI.Button(m_btnPos, "Log", m_btnStyle);
			if (isClick)
			{
				DoLogs(context);
			}
		}
		
		public static void DoLogs(Object context) 
		{
			Debug.Log("Log a message");
			Debug.Log("Log a message with a context", context);

			Debug.LogWarning("Log a warning message");
			Debug.LogWarning("Log a warning message with a context", context);

			Debug.LogError("Log an error message");
			Debug.LogError("Log an error message with a context", context);

			Debug.LogException(new System.Exception("Log an exception message"));
			Debug.LogException(new System.Exception("Log an exception message with a context"), context);
		}
	}
}


