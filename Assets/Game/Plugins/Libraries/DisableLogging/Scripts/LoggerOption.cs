using UnityEngine;
using System.Collections;

namespace DisableLogging
{
	[ExecuteInEditMode]
	public class LoggerOption : MonoBehaviour
	{
		public bool logInfo = false;
		public bool logWarning = true;
		public bool logError = true;
		public bool enableAllForEditorMode = true;
		public bool enableAllForDevelopmentBuild = true;

		public bool apply = true;
		
		void Start()
		{
			DoApply();
		}

		void Update()
		{
			if (apply)
			{
				DoApply();
				apply = false;
			}
		}

		void DoApply()
		{
			if (enableAllForEditorMode && IsEditorMode())
			{
				EnableAllLogs();
				return;
			}

			if (enableAllForDevelopmentBuild && IsDevelopmentBuild())
			{
				EnableAllLogs();
				return;
			}

			EnableLogsWithOptions();
		}

		private void EnableLogsWithOptions()
		{
			Logger.enableLogInfo = logInfo;
			Logger.enableLogWarning = logWarning;
			Logger.enableLogError = logError;
		}
		
		private void EnableAllLogs()
		{
			Logger.enableLogInfo = true;
			Logger.enableLogWarning = true;
			Logger.enableLogError = true;
		}

		private static bool IsDevelopmentBuild()
		{
			#if UNITY_EDITOR
				return false;
			#else
				return Debug.isDebugBuild;
			#endif
		}

		private static bool IsEditorMode()
		{
			#if UNITY_EDITOR
				return true;
			#else
				return false;
			#endif
		}
	}
}
