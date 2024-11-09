using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace Castrimaris.Core {

    /// <summary>
    /// Compares two strings, with support for strings that have numbers in them (e.g. lightmap-12, lightmap-1, etc)
    /// </summary>
    public class AlphanumericComparer : IComparer<string> {
        public int Compare(string x, string y) {
            if (x == null || y == null) {
                return 0;
            }

            string[] xParts = Regex.Split(x, "([0-9]+)");
            string[] yParts = Regex.Split(y, "([0-9]+)");

            for (int i = 0; i < System.Math.Min(xParts.Length, yParts.Length); i++) {
                if (xParts[i] != yParts[i]) {
                    int xNum, yNum;
                    // If both parts are numeric, compare them as numbers
                    if (int.TryParse(xParts[i], out xNum) && int.TryParse(yParts[i], out yNum)) {
                        return xNum.CompareTo(yNum);
                    }
                    // Otherwise, compare them as strings
                    return string.Compare(xParts[i], yParts[i], StringComparison.Ordinal);
                }
            }

            // If one string has more parts than the other, the shorter one should come first
            return xParts.Length.CompareTo(yParts.Length);
        }
    }
}