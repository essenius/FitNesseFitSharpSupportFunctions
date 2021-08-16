// Copyright 2021 Rik Essenius
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
using System.Collections.Generic;

namespace SupportFunctions.Utilities
{

#if NET5_0
    using System.Text.Json;

    internal static class DictionarySerializer
    {
        public static string Serialize(Dictionary<string, string> dictionary)
        {
            return JsonSerializer.Serialize(dictionary);
        }

        public static  Dictionary<string, string> Deserialize(string input)
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(input);
        }

        public static Type ParseExceptionType => typeof(JsonException);

    }
#else
    using System.Web.Script.Serialization;
 
    internal static class DictionarySerializer
    {
        public static string Serialize(Dictionary<string, string> dictionary)
        {
            return new JavaScriptSerializer().Serialize(dictionary);
        }

        public static  Dictionary<string, string> Deserialize(string input)
        {
            return new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(input);
        }

        public static Type ParseExceptionType => typeof(ArgumentException);

    }

#endif
}