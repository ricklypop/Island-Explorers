using UnityEngine;
using System.Collections;

namespace DisableLogging
{
	[ExecuteInEditMode]
	public class LoggerExample : MonoBehaviour 
	{
		public GameObject context;
		public bool log = false;

		void Start()
		{
			// if you don't like using LoggerOption.cs, 
			// just use the following codes to set logger option.
//			Logger.enableLogInfo = false;	// This variable controls "Logger.Log()".
//			Logger.enableLogWarning = true;	// This variable controls "Logger.LogWarning()".
//			Logger.enableLogError = true;	// This variable controls "Logger.LogError()" and "Logger.LogException()".
		}
		
		void Update()
		{
			if (log)
			{
				DoLogs(context);
				log = false;
			}
		}
		
		public static void DoLogs(Object context) 
		{
			// Examples of Logger.Log()
			Logger.Log("Log a message");
			Logger.Log("Log a message with a specific color", Color.blue);
			Logger.Log("Log a message with a user-defined color", new Color(0f, 0.5f, 0f));

			// Examples of Logger.LogWarning()
			Logger.LogWarning("Log a warning message");
			Logger.LogWarning("Log a warning message with a specific color", Color.yellow);
			Logger.LogWarning("Log a warning message with a user-defined color", new Color(1f, 0.5f, 0.25f));

			// Examples of Logger.LogError()
			Logger.LogError("Log an error message");
			Logger.LogError("Log an error message with a specific color", Color.red);
			Logger.LogError("Log an error message with a user-defined color", new Color(1f, 0f, 1f));

			// Examples of Logger.LogException()
			Logger.LogException(new System.Exception("Log an exception message"));
			Logger.LogException(new System.Exception("Log an exception message with a specific color"), Color.red);
			Logger.LogException(new System.Exception("Log an exception message with a user-defined color"), new Color(1f, 0f, 1f));

			// Examples of Logging With Context
			Logger.Log("Log a message with a context", context);
			Logger.Log("Log a message with a specific color and context", Color.green, context);

			Logger.LogWarning("Log a warning message with a context", context);
			Logger.LogWarning("Log a warning message with a specific color and context", Color.cyan, context);

			Logger.LogError("Log an error message with a context", context);
			Logger.LogError("Log an error message with a specific color and context", Color.red, context);

			Logger.LogException(new System.Exception("Log an exception message with a context"), context);
			Logger.LogException(new System.Exception("Log an exception message with a specific color and context"), Color.red, context);
		}
	}
}


