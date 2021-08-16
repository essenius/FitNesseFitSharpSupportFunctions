// Copyright 2015-2021 Rik Essenius
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
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices.AccountManagement;

namespace SupportFunctions
{
    /// <summary>User information</summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "FitSharp entry point")]
    public sealed class UserInfo
    {
        /// <summary>The current user’s display name</summary>
        public static string DisplayName
        {
            get
            {
#if NET5_0
                if (OperatingSystem.IsWindows())
#endif
                    return UserPrincipal.Current.DisplayName;
#if NET5_0
                return Environment.UserName;
#endif
            }
        }

        /// <summary>The current user’s user name</summary>
        public static string UserName
        {
            get
            {
                var domain = Environment.UserDomainName;
                if (string.IsNullOrEmpty(domain)) return Environment.UserName;
                return domain + "\\" + Environment.UserName;
            }
        }
    }
}
