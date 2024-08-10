// Copyright 2015-2024 Rik Essenius
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
using System.DirectoryServices.AccountManagement;

namespace SupportFunctions
{
    /// <summary>User information</summary>
    public sealed class UserInfo
    {
        // slow functions, so use caching
        private static string _userName;
        private static string _displayName;


        /// <summary>The current user’s display name</summary>
        public static string DisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(_displayName)) return _displayName;
#if NET5_0_OR_GREATER
                if (OperatingSystem.IsWindows())
#endif
                    _displayName = UserPrincipal.Current.DisplayName;

                if (string.IsNullOrEmpty(_displayName))
                {
                    _displayName = Environment.UserName;
                }
                return _displayName;
            }
        }

        /// <summary>The current user’s username</summary>
        public static string UserName
        {
            get
            {
                if (_userName != null) return _userName;
                _userName = Environment.UserName;
                var domain = Environment.UserDomainName;
                if (string.IsNullOrEmpty(domain)) 
                    _userName = domain + "\\" + _userName;
                return _userName;
            }
        }
    }
}
