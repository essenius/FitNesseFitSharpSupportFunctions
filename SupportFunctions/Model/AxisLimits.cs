// Copyright 2017-2019 Rik Essenius
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

namespace SupportFunctions.Model
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used by FitSharp")]
    internal class AxisLimits
    {
        public AxisLimits(DateTime startTimestamp, DateTime endTimestamp, Dimension yDimension)
        {
            const bool snapToGrid = true;
            StartTimestamp = startTimestamp;
            EndTimestamp = endTimestamp;
            var timeInterval = (EndTimestamp - StartTimestamp).TotalSeconds;
            TimeUnit = new TimeUnitForDisplay(timeInterval);

            X = new Dimension(0, TimeUnit.ConvertFromSeconds(timeInterval), !snapToGrid);
            Y = yDimension;
        }

        public DateTime EndTimestamp { get; }
        public DateTime StartTimestamp { get; }
        public TimeUnitForDisplay TimeUnit { get; }
        public Dimension X { get; }
        public Dimension Y { get; }

        public void EnsureNonZeroRanges()
        {
            X.EnsureNonZeroRange(0D, 1D);
            Y.EnsureNonZeroRange(0.1, 0.1);
        }
    }
}