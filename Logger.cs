using System.Diagnostics;

namespace CastrimarisStudios.Utilities
{
    /// <summary>
    /// Custom logger
    /// </summary>
    public static class Log
    {
        private static string GetCallingClassName()
        {
            return new StackFrame(2).GetMethod().DeclaringType.FullName;
        }

        /// <summary>
        /// Displays an error message in the log window. Prepends the classname in front of it too.
        /// </summary>
        /// <param name="Message">The message to display</param>
        public static void Error(string Message)
        {
            
#if UNITY_EDITOR
            UnityEngine.Debug.LogError($"{GetCallingClassName()} - {Message}");
#else
            Console.WriteLine($"[ERROR] - {GetCallingClassName()} - {Message}");
#endif
        }

        /// <summary>
        /// Displays an warning message in the log window. Prepends the classname in front of it too.
        /// </summary>
        /// <param name="Message">The message to display</param>
        public static void Warning(string Message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning($"{GetCallingClassName()} - {Message}");
#else
            Console.WriteLine($"[WARNING] - {GetCallingClassName()} - {Message}");
#endif
        }

        /// <summary>
        /// Displays an info message in the log window. Prepends the classname in front of it too.
        /// </summary>
        /// <param name="Message">The message to display</param>
        public static void Info(string Message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"{GetCallingClassName()} - {Message}");
#else
            Console.WriteLine($"[INFO] - {GetCallingClassName()} - {Message}");
#endif
        }

        /// <summary>
        /// Displays an debug message in the log window. The message is printed only if the application is built as "Development". Prepends the classname in front of it too.
        /// </summary>
        /// <param name="Message">The message to display</param>
        public static void Debug(string Message)
        {
            if (UnityEngine.Debug.isDebugBuild)
#if UNITY_EDITOR
                UnityEngine.Debug.Log($"{GetCallingClassName()} - {Message}");
#else
                Console.WriteLine($"[DEBUG] - {GetCallingClassName()} - {Message}");
#endif
        }
    }
}
