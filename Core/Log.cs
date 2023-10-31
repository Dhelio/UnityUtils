using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UDebug = UnityEngine.Debug;

namespace Castrimaris.Core {

    /// <summary>
    /// Custom Log class to make it easier to log different level messages, based on the build type,
    /// LogLevel type and more. Automatically logs name of the class and name of the method too.
    /// </summary>
    public class Log {

        #region Public Fields

        public static LogLevel LogLevel = LogLevel.DEBUG;

        #endregion Public Fields

        #region Public Methods

        /// <summary>
        /// Logs a Debug level message
        /// </summary>
        /// <param name="Content">The message to log</param>
        /// <param name="CallingClassFilePath">
        /// Optional field for the class name. If unspecified, it uses the name of the actual file
        /// of the class.
        /// </param>
        /// <param name="CallingMemberName">
        /// Optional field for the method name. If unspecified, it uses the actual name of the method.
        /// </param>
        public static void D(string Content, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.DEBUG || Application.isEditor || UDebug.isDebugBuild) {
                var formattedMessage = FormatMessage(Content, CallingClassFilePath, CallingMemberName);
                UDebug.Log(formattedMessage);
            }
        }

        /// <summary>
        /// Logs a Debug level message
        /// </summary>
        /// <param name="Exception">The message to log</param>
        /// <param name="CallingClassFilePath">
        /// Optional field for the class name. If unspecified, it uses the name of the actual file
        /// of the class.
        /// </param>
        /// <param name="CallingMemberName">
        /// Optional field for the method name. If unspecified, it uses the actual name of the method.
        /// </param>
        public static void D(System.Exception Exception, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.DEBUG || Application.isEditor || UDebug.isDebugBuild) {
                var formattedMessage = FormatMessage(Exception.ToString(), CallingClassFilePath, CallingMemberName);
                UDebug.Log(formattedMessage);
            }
        }

        /// <summary>
        /// Logs an Information level message
        /// </summary>
        /// <param name="Content">The message to log</param>
        /// <param name="CallingClassFilePath">
        /// Optional field for the class name. If unspecified, it uses the name of the actual file
        /// of the class.
        /// </param>
        /// <param name="CallingMemberName">
        /// Optional field for the method name. If unspecified, it uses the actual name of the method.
        /// </param>
        public static void I(string Content, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.INFO) {
                var formattedMessage = FormatMessage(Content, CallingClassFilePath, CallingMemberName);
                UDebug.Log(formattedMessage);
            }
        }

        /// <summary>
        /// Logs an Information level message
        /// </summary>
        /// <param name="Exception">The message to log</param>
        /// <param name="CallingClassFilePath">
        /// Optional field for the class name. If unspecified, it uses the name of the actual file
        /// of the class.
        /// </param>
        /// <param name="CallingMemberName">
        /// Optional field for the method name. If unspecified, it uses the actual name of the method.
        /// </param>
        public static void I(System.Exception Exception, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.INFO) {
                var formattedMessage = FormatMessage(Exception.ToString(), CallingClassFilePath, CallingMemberName);
                UDebug.Log(formattedMessage);
            }
        }

        /// <summary>
        /// Logs an Warning level message
        /// </summary>
        /// <param name="Content">The message to log</param>
        /// <param name="CallingClassFilePath">
        /// Optional field for the class name. If unspecified, it uses the name of the actual file
        /// of the class.
        /// </param>
        /// <param name="CallingMemberName">
        /// Optional field for the method name. If unspecified, it uses the actual name of the method.
        /// </param>
        public static void W(string Content, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.WARN) {
                var formattedMessage = FormatMessage(Content, CallingClassFilePath, CallingMemberName);
                UDebug.LogWarning(formattedMessage);
            }
        }

        /// <summary>
        /// Logs an Warning level message
        /// </summary>
        /// <param name="Exception">The message to log</param>
        /// <param name="CallingClassFilePath">
        /// Optional field for the class name. If unspecified, it uses the name of the actual file
        /// of the class.
        /// </param>
        /// <param name="CallingMemberName">
        /// Optional field for the method name. If unspecified, it uses the actual name of the method.
        /// </param>
        public static void W(System.Exception Exception, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.WARN) {
                var formattedMessage = FormatMessage(Exception.ToString(), CallingClassFilePath, CallingMemberName);
                UDebug.LogWarning(formattedMessage);
            }
        }

        /// <summary>
        /// Logs an Error level message
        /// </summary>
        /// <param name="Content">The message to log</param>
        /// <param name="CallingClassFilePath">
        /// Optional field for the class name. If unspecified, it uses the name of the actual file
        /// of the class.
        /// </param>
        /// <param name="CallingMemberName">
        /// Optional field for the method name. If unspecified, it uses the actual name of the method.
        /// </param>
        public static void E(string Content, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.ERROR) {
                var formattedMessage = FormatMessage(Content, CallingClassFilePath, CallingMemberName);
                UDebug.LogError(formattedMessage);
            }
        }

        /// <summary>
        /// Logs an Error level message
        /// </summary>
        /// <param name="Exception">The message to log</param>
        /// <param name="CallingClassFilePath">
        /// Optional field for the class name. If unspecified, it uses the name of the actual file
        /// of the class.
        /// </param>
        /// <param name="CallingMemberName">
        /// Optional field for the method name. If unspecified, it uses the actual name of the method.
        /// </param>
        public static void E(System.Exception Exception, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.ERROR) {
                var formattedMessage = FormatMessage(Exception.ToString(), CallingClassFilePath, CallingMemberName);
                UDebug.LogError(formattedMessage);
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Formats the message to a predefined structure.
        /// </summary>
        private static string FormatMessage(string Content, string ClassFilePath, string CallingMemberName) {
            var classNameWithExtension = ClassFilePath.Split("\\").Last();
            var className = classNameWithExtension.Remove(classNameWithExtension.Length - 3);
            return $"[{className}.{CallingMemberName}] - {Content}"; //Actual structure of the Log
        }

        #endregion Private Methods
    }

    public enum LogLevel {
        DEBUG = 0, INFO = 1, WARN = 2, ERROR = 3
    }
}
