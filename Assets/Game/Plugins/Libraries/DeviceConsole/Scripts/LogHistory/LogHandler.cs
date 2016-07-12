using System;
using UnityEngine;
using System.Collections;

namespace DeviceConsole
{
	public static class LogHandler
	{
        public static bool shouldLogToScreen = false;

        public static System.Action<LogEntry> onNewLogToScreen;
		
		private static LogToBuffer m_logToScreen = new LogToBuffer();

        public static LogToBuffer LogToScreen
		{
			get {return m_logToScreen;}
		}

		public static void RegisterLogCallback()
		{
			Application.RegisterLogCallback(LogCallback);
		}

		public static void UnregisterLogCallback()
		{
			Application.RegisterLogCallback(null);
		}

		private static void LogCallback(string logString, string stackTrace, LogType type)
		{
			if (!shouldLogToScreen)
			{
				return;
			}

			LogEntry entry = new LogEntry();
			entry.logType = type;
			entry.message = (logString != null) ? logString : "Null";
			entry.timeStamp = string.Format("{0:HH:mm:ss.ffff}", DateTime.Now);

			LogToScreen.LogImpl(entry);
            if (onNewLogToScreen != null)
			{
                onNewLogToScreen(entry);
            }
		}
	}
}

