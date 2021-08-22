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

// maintained for compatibility

using System;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member -- not needed as Obsolete.

namespace SupportFunctions
{
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "Required for FitNesse")]
    [Obsolete("Use CommonFunctions")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "FitSharp entry point")]
    public sealed class EchoSupport
    {
        [Obsolete("Use CommonFunctions.Echo")]
        public static object Echo(object input) => input;
    }
}
