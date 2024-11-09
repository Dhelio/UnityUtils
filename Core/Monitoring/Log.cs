using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UDebug = UnityEngine.Debug;

namespace Castrimaris.Core.Monitoring {

    /// <summary>
    /// Custom Log class to make it easier to log different level messages, based on the build type,
    /// LogLevel type and more. Automatically logs name of the class and name of the method too.
    /// </summary>
    public class Log {

        #region Public Fields

        public static LogLevel LogLevel = LogLevel.DEBUG;
        public static TextMeshPro debug3D = null;
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
        public static void D(string Content, bool Use3DDebug = false, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.DEBUG || Application.isEditor || UDebug.isDebugBuild) {
                var formattedMessage = FormatMessage(Content, CallingClassFilePath, CallingMemberName);
                UDebug.Log(formattedMessage);
                if (Use3DDebug && Log3D.IsInstanced) Log3D.Instance.Append(formattedMessage);
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
        public static void D(System.Exception Exception, bool Use3DDebug = false, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.DEBUG || Application.isEditor || UDebug.isDebugBuild) {
                var formattedMessage = FormatMessage(Exception.ToString(), CallingClassFilePath, CallingMemberName);
                UDebug.Log(formattedMessage);
                if (Use3DDebug && Log3D.IsInstanced) Log3D.Instance.Append(formattedMessage);
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
        public static void I(string Content, bool Use3DDebug = false, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.INFO) {
                var formattedMessage = FormatMessage(Content, CallingClassFilePath, CallingMemberName);
                UDebug.Log(formattedMessage);
                if (Use3DDebug && Log3D.IsInstanced) Log3D.Instance.Append(formattedMessage);
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
        public static void I(System.Exception Exception, bool Use3DDebug = false, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.INFO) {
                var formattedMessage = FormatMessage(Exception.ToString(), CallingClassFilePath, CallingMemberName);
                UDebug.Log(formattedMessage);
                if (Use3DDebug && Log3D.IsInstanced) Log3D.Instance.Append(formattedMessage);
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
        public static void W(string Content, bool Use3DDebug = false, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.WARN) {
                var formattedMessage = FormatMessage(Content, CallingClassFilePath, CallingMemberName);
                UDebug.LogWarning(formattedMessage);
                if (Use3DDebug && Log3D.IsInstanced) Log3D.Instance.Append(formattedMessage);
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
        public static void W(System.Exception Exception, bool Use3DDebug = false, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.WARN) {
                var formattedMessage = FormatMessage(Exception.ToString(), CallingClassFilePath, CallingMemberName);
                UDebug.LogWarning(formattedMessage);
                if (Use3DDebug && Log3D.IsInstanced) Log3D.Instance.Append(formattedMessage);
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
        public static void E(string Content, bool Use3DDebug = false, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.ERROR) {
                var formattedMessage = FormatMessage(Content, CallingClassFilePath, CallingMemberName);
                UDebug.LogError(formattedMessage);
                if (Use3DDebug && Log3D.IsInstanced) Log3D.Instance.Append(formattedMessage);
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
        public static void E(System.Exception Exception, bool Use3DDebug = false, [CallerFilePath] string CallingClassFilePath = "", [CallerMemberName] string CallingMemberName = "") {
            if (LogLevel <= LogLevel.ERROR) {
                var formattedMessage = FormatMessage(Exception.ToString(), CallingClassFilePath, CallingMemberName);
                UDebug.LogError(formattedMessage);
                if (Use3DDebug && Log3D.IsInstanced) Log3D.Instance.Append(formattedMessage);
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Formats the message to a predefined structure.
        /// </summary>
        private static string FormatMessage(string messageContent, string classFilePath, string callingMethodName) {
            var classNameWithExtension = classFilePath.Split("\\").Last();
            var className = classNameWithExtension.Remove(classNameWithExtension.Length - 3); //Remove the extension of the class
            return $"[{className}.{callingMethodName}] - {messageContent}"; //Actual structure of the Log
        }

        //TODO
        private static string WrapWithColorInEditor(LogLevel Level) {
            string hex;
            switch (Level) {
                case LogLevel.ERROR:
                    hex = "FF0000";
                    break;
                case LogLevel.WARN:
                    hex = "FFFF00";
                    break;
                default:
                    hex = "00FF00";
                    break;
            }
            //builder.Insert(startIndex, $"<color=#{hex}>");
            //builder.Append("</color>");
            return hex;
        }

        #endregion Private Methods
    }

    public enum LogLevel {
        DEBUG = 0, INFO = 1, WARN = 2, ERROR = 3
    }
}