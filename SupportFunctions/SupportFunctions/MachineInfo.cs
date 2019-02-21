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

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace SupportFunctions
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp"),
     SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Used by FitSharp"),
     SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Used by FitSharp"),
     Documentation("Machine information")]
    public class MachineInfo
    {
        [Documentation("Return the FQDN of the current machine, i.e. machine name with full domain")]
        public static string FullyQualifiedDomainName() => FullyQualifiedDomainName(Environment.MachineName);

        [Documentation("Return the FQDN of computerName, i.e. machine name with full domain")]
        public static string FullyQualifiedDomainName(string computerName)
        {
            Debug.Assert(computerName != null, "computerName != null");
            return Dns.GetHostEntry(computerName).HostName;
        }
    }
}