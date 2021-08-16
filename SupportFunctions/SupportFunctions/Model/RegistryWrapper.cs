// Copyright 2017-2021 Rik Essenius
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
using Microsoft.Win32;
using SupportFunctions.Utilities;
using System.Runtime.Versioning;


namespace SupportFunctions.Model
{
    internal static class RegistryWrapper
    {
        public static string DateTimeFormat => ShortDateFormat + " " + TimeFormat;
        private static string InternationalLocation => "Control Panel\\International";

        public static string ShortDateFormat
        {
            get
            {
#if NET5_0
                if (OperatingSystem.IsWindows())
#endif
                    return InternationalValue(ShortDateFormatLocation).ToString();
#if NET5_0
                return "dd-MMM-yyyy";
#endif                
            }
        }

        private static string ShortDateFormatLocation => "sShortDate";

        public static string TimeFormat
        {
            get
            {
#if NET5_0
                if (OperatingSystem.IsWindows())
#endif
                    return InternationalValue(TimeFormatLocation).ToString();
#if NET5_0
                return "HH:mm:ss";
#endif                
            }
        }

        private static string TimeFormatLocation => "sTimeFormat";

#if NET5_0
        [SupportedOSPlatform("windows")]
#endif
        private static object InternationalValue(string key)
        {
            var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            var internationalKey = baseKey.OpenSubKey(InternationalLocation);
            Requires.NotNull(internationalKey, nameof(internationalKey));
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
