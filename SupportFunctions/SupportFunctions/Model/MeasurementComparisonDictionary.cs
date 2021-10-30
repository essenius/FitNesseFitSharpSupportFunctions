// Copyright 2017-2020 Rik Essenius
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
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal class MeasurementComparisonDictionary : SortedDictionary<DateTime, IMeasurementComparison>
    {
        public MeasurementComparisonDictionary()
        {
        }

        private MeasurementComparisonDictionary(IDictionary<DateTime, IMeasurementComparison> inDictionary) :
            base(inDictionary)
        {
        }

        public MeasurementComparisonDictionary Subset(DateTime? startTimestamp, DateTime? endTimestamp)
        {
            return new MeasurementComparisonDictionary(
                this.Where(
                        kvp => kvp.Key.IsWithinTimeRange(startTimestamp, endTimestamp)
                    )
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }
    }
}
