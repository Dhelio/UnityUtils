using System;

namespace Castrimaris.Core.Collections {

    /// <summary>
    /// A version of the <see cref="DateTime"/> struct that is serializable in Unity's inspector.
    /// </summary>
    [Serializable]
    public class SerializableDateTime {
        // These fields store the date and time components
        public int year;
        public int month;
        public int day;
        public int hour;
        public int minute;
        public int second;

        // Default constructor
        public SerializableDateTime() { }

        // Constructor to create SerializableDateTime from a DateTime
        public SerializableDateTime(DateTime dateTime) {
            year = dateTime.Year;
            month = dateTime.Month;
            day = dateTime.Day;
            hour = dateTime.Hour;
            minute = dateTime.Minute;
            second = dateTime.Second;
        }

        // Converts SerializableDateTime to DateTime
        public DateTime ToDateTime() {
            return new DateTime(year, month, day, hour, minute, second);
        }

        // Static method to get the current time as SerializableDateTime
        public static SerializableDateTime Now() {
            return new SerializableDateTime(DateTime.Now);
        }

        // Override ToString method for easy debugging
        public override string ToString() {
            return ToDateTime().ToString();
        }
    }
}