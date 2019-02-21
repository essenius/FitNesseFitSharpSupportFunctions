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
using System.Collections.Generic;
using System.Linq;
using SupportFunctions.Model;

namespace SupportFunctionsTest
{
    internal class FitNessePageMock : FitNessePage
    {
        private readonly string _inputText;

        public FitNessePageMock(string inputText)
        {
            _inputText = inputText;
            UsedUri = string.Empty;
        }

        public string UsedUri { get; private set; }

        protected override List<string> RestCall(string uri)
        {
            UsedUri += Uri.UnescapeDataString(uri) + FitNessePageTest.UriSeparator;
            if (_inputText == null) return new List<string>();
            var result = _inputText.Split('\n').ToList();
            if (string.IsNullOrEmpty(result.Last())) result.RemoveAt(result.Count - 1);
            return result;
        }
    }
}