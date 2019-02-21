// Copyright 2017-2019 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Diagnostics;
using Microsoft.Win32;

namespace SupportFunctions.Model
{
    internal class RegistryWrapper
    {
        private readonly RegistryKey _baseKey;

        public RegistryWrapper() => _baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);

        public string DateTimeFormat => ShortDateFormat + " " + TimeFormat;
        private static string InternationalLocation => "Control Panel\\International";

        public string ShortDateFormat => InternationalValue(ShortDateFormatLocation).ToString();

        private static string ShortDateFormatLocation => "sShortDate";

        public string TimeFormat => InternationalValue(TimeFormatLocation).ToString();

        private static string TimeFormatLocation => "sTimeFormat";

        private object InternationalValue(string key)
        {
            var internationalKey = _baseKey.OpenSubKey(InternationalLocation);
            Debug.Assert(internationalKey != null, "internationalKey != null");
            return internationalKey.GetValue(key);
        }

        // This cannot be done without elevated privileges, so disabling
        // private void SetInternationalValue(string key, object value)
        // {
        //    var internationalKey = _baseKey.OpenSubKey(InternationalLocation);
        //    Debug.Assert(internationalKey != null, "internationalKey != null");
        //    internationalKey.SetValue(key, value);
        // }
    }
}