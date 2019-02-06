// Copyright 2015-2019 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;

namespace SupportFunctions
{
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Used by FitSharp")]
    public class UserInfo
    {
        public static string DisplayName => UserPrincipal.Current.DisplayName;

        public static Dictionary<string, string> FixtureDocumentation { get; } = new Dictionary<string, string>
        {
            {string.Empty, "User Information"},
            {nameof(DisplayName), "The current user’s display name"},
            {nameof(UserName), "The current user’s user name"}
        };

        public static string UserName => WindowsIdentity.GetCurrent().Name;
    }
}