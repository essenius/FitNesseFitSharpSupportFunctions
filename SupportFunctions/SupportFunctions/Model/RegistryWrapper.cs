// Copyright 2017-2024 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using Microsoft.Win32;
using SupportFunctions.Utilities;
#if NET5_0_OR_GREATER
using System;
using System.Runtime.Versioning;
#endif

namespace SupportFunctions.Model
{
    internal class RegistryWrapper
    {
        private static string InternationalLocation => "Control Panel\\International";

        public virtual string ShortDateFormat
        {
            get
            {
#if NET5_0_OR_GREATER
                if (OperatingSystem.IsWindows())
#endif
                    return InternationalValue(ShortDateFormatLocation).ToString();
#if NET5_0_OR_GREATER
                return "dd-MMM-yyyy";
#endif
            }
        }

        private static string ShortDateFormatLocation => "sShortDate";

        public virtual string TimeFormat
        {
            get
            {
#if NET5_0_OR_GREATER
                if (OperatingSystem.IsWindows())
#endif
                    return InternationalValue(TimeFormatLocation).ToString();
#if NET5_0_OR_GREATER
                return "HH:mm:ss";
#endif
            }
        }

        private static string TimeFormatLocation => "sTimeFormat";

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        private object InternationalValue(string key)
        {
            var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            var internationalKey = baseKey.OpenSubKey(InternationalLocation);
            Requires.NotNull(internationalKey, nameof(internationalKey));
            return internationalKey!.GetValue(key);
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
