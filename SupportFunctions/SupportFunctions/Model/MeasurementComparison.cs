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
using SupportFunctions.Utilities;

namespace SupportFunctions.Model
{
    internal class MeasurementComparison : IMeasurementComparison
    {
        public MeasurementComparison(Measurement expected, Measurement actual, Tolerance tolerance = null, Type compareType = null)
        {
            Timestamp = new ValueComparison(expected?.Timestamp.ToRoundTripFormat(), actual?.Timestamp.ToRoundTripFormat());
            Value = new ValueComparison(expected?.Value, actual?.Value, tolerance, compareType);
            IsGood = new ValueComparison(expected?.IsGood, actual?.IsGood);
        }

        public IValueComparison IsGood { get; }

        public bool IsOk() => Timestamp.IsOk() && Value.IsOk() && IsGood.IsOk();

        public string OutcomeMessage
        {
            get
            {
                if (Timestamp.Outcome != CompareOutcome.None) return Timestamp.Outcome.ToString();
                var result = new List<string>();
                if (Value.Outcome != CompareOutcome.None) result.Add(Value.Outcome.ToString());
                if (!IsGood.IsOk()) result.Add("IsGoodIssue");
                return result.Count == 0 ? CompareOutcome.None.ToString() : string.Join("; ", result);
            }
        }

        public object TableResult(string message) => IsOk() ? message.Pass() : message.Fail();

        public IValueComparison Timestamp { get; }

        public IValueComparison Value { get; }
    }
}
