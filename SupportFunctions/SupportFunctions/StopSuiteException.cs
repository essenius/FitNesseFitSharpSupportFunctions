﻿// Copyright 2016-2024 Rik Essenius
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

namespace SupportFunctions
{
    /// <inheritdoc />
    /// <summary>Stop Suite Exception - raise when you want FitNesse to stop executing the current test suite</summary>
    [Serializable]
    public class StopSuiteException : Exception
    {
        /// <inheritdoc />
        /// <summary>base constructor</summary>
        public StopSuiteException()
        {
        }

        /// <inheritdoc />
        /// <summary>constructor with message param</summary>
        /// <param name="message">the exception message</param>
        public StopSuiteException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        /// <summary>Constructor with message and inner exception</summary>
        /// <param name="message">the message</param>
        /// <param name="innerException">the inner exception</param>
        public StopSuiteException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
