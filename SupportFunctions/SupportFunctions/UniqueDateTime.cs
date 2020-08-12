// Copyright 2015-2020 Rik Essenius
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
using System.Threading;

namespace SupportFunctions
{
    /// <summary>UniqueDateTime always returns a unique data/time value</summary>
    public static class UniqueDateTime
    {
        private static long _lastTimeStamp = DateTime.Now.Ticks;

        /// <summary>the value of Now in ticks</summary>
        public static long NowTicks
        {
            get
            {
                long original, newValue;
                do
                {
                    original = _lastTimeStamp;
                    var now = DateTime.Now.Ticks;
                    newValue = Math.Max(now, original + 1);
                } while (Interlocked.CompareExchange(ref _lastTimeStamp, newValue, original) != original);

                return newValue;
            }
        }

        /// <summary>the value of Now in UTC in ticks</summary>
        public static long UtcNowTicks => new DateTime(NowTicks).ToUniversalTime().Ticks;
    }
}
