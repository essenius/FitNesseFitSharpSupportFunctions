using System;
using System.Diagnostics;

namespace SupportFunctions.Utilities
{
    internal class Requires
    {
        private static string CallingMethod()
        {
            var stacktrace = new StackTrace();
            return stacktrace.GetFrame(2).GetMethod().Name;
        }

        public static void NotEmpty(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"'{name}' cannot be null or empty in '{CallingMethod()}'");
            }
        }

        public static void NotNull(object value, string name)
        {
            if (value == null)
            {
                throw new ArgumentException($"'{name}' cannot be null in '{CallingMethod()}'");
            }
        }

        public static void Condition(bool condition, string description)
        {
            if (!condition)
            {
                throw new ArgumentException($"'{description}' not met in '{CallingMethod()}'");
            }
        }
    }
}
