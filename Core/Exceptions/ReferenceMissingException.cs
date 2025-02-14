using System;
using System.Text;

namespace Castrimaris.Core.Exceptions {

    /// <summary>
    /// General exception for checking if some class isn't assigned in the Editor. Most useful to shorten null checks
    /// </summary>
    public class ReferenceMissingException : SystemException {

        private const string CLASS_TAG = "[CLASS_NAME]";
        private const string ERROR_MESSAGE = "No reference set for " + CLASS_TAG + "! Did you forget to assign it in the Editor?";
        public ReferenceMissingException(string className) : base(Format(className)) {}

        private static string Format(string className) {
            var builder = new StringBuilder(ERROR_MESSAGE);
            builder.Replace(CLASS_TAG, className);
            return builder.ToString();
        }
    }
}