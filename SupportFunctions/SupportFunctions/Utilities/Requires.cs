// Copyright 2017-2023 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Diagnostics;

namespace SupportFunctions.Utilities
{
    internal static class Requires
    {
        private static string CallingMethod()
        {
            var callingFrame = new StackTrace().GetFrame(2);
            Debug.Assert(callingFrame != null, "callingFrame != null");
            var method = callingFrame.GetMethod();
            Debug.Assert(method != null, "method != null");
            return method.Name;
        }

        public static void Condition(bool condition, string description)
        {
            if (!condition)
            {
                throw new ArgumentException($"'{description}' not met in '{CallingMethod()}'");
            }
        }

        public static void NotNull(object value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(null, $"'{name}' cannot be null in '{CallingMethod()}'");
            }
        }

        public static void NotNullOrEmpty(string value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(null, $"'{name}' cannot be null in '{CallingMethod()}'");
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"'{name}' cannot be empty in '{CallingMethod()}'");
            }
        }
    }
}
