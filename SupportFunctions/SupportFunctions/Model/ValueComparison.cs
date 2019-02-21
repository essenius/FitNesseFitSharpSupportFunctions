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
using System.Collections.Generic;
using System.Diagnostics;
using SupportFunctions.Utilities;
using static System.FormattableString;
using static System.Globalization.CultureInfo;

namespace SupportFunctions.Model
{
    internal class ValueComparison : IValueComparison
    {
        // {0} is expected, {1} is actual
        private const string MissingCaption = "[{0}] missing";

        private const string NoneCaption = "{1}";
        private const string OutsideToleranceCaption = "{1} != {0}";
        private const string SurplusCaption = "[{1}] surplus";
        private const string ValueIssueCaption = "[{1}] expected [{0}]";
        private const string WithinToleranceCaption = "{1} ~= {0}";

        private readonly Dictionary<CompareOutcome, string> _messageDictionary = new Dictionary<CompareOutcome, string>
        {
            {CompareOutcome.Missing, MissingCaption},
            {CompareOutcome.None, NoneCaption},
            {CompareOutcome.OutsideToleranceIssue, OutsideToleranceCaption},
            {CompareOutcome.Surplus, SurplusCaption},
            {CompareOutcome.ValueIssue, ValueIssueCaption},
            {CompareOutcome.WithinTolerance, WithinToleranceCaption}
        };

        public ValueComparison(object expected, object actual, Tolerance tolerance = null, Type compareType = null)
        {
            ExpectedValueIn = expected;
            ActualValueIn = actual;
            Tolerance = tolerance;
            ToleranceBase = Tolerance?.DataRange;
            CompareType = compareType;
            Outcome = RunComparison();
        }

        private object ActualValueIn { get; }

        private Type CompareType { get; set; }

        private object ExpectedValueIn { get; }

        private bool IsNumericalComparisonWithoutToleranceRange
            => Tolerance != null && Tolerance.DataRange == null && CompareType.IsNumeric() && ExpectedValueIn.IsNumeric();

        private bool IsToleranceUsed => Outcome == CompareOutcome.WithinTolerance || Outcome == CompareOutcome.OutsideToleranceIssue;

        private bool IsZeroToleranceComparison => !CompareType.IsNumeric() || Tolerance == null || Tolerance.Value.Equals(0.0);

        private Tolerance Tolerance { get; }

        private double? ToleranceBase { get; set; }

        public string ActualValueOut { get; private set; }

        public string DeltaMessage => DeltaOut;

        public string DeltaOut { get; private set; }

        public string DeltaPercentageMessage
        {
            get
            {
                if (!IsToleranceUsed || ToleranceBase == null) return string.Empty;
                var deltaPercentage = DeltaOut?.To<double>() / ToleranceBase;
                var result = deltaPercentage == null || double.IsInfinity(deltaPercentage.Value)
                    ? string.Empty
                    : Invariant($"{Math.Abs(deltaPercentage.Value):P1}");
                return result;
            }
        }

        public string ExpectedValueOut { get; private set; }

        public bool IsOk() => IsOk(Outcome);

        public CompareOutcome Outcome { get; set; }

        public string TableResult(string message)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty.Report();
            return IsOk(Outcome) ? message.Pass() : message.Fail();
        }

        public string ValueMessage => string.Format(InvariantCulture, _messageDictionary[Outcome], ExpectedValueOut, ActualValueOut);

        private CompareOutcome DoubleComparison()
        {
            Debug.Assert(Tolerance != null, "Tolerance != null");
            var precision = Tolerance.Precision;
            var delta = Math.Abs(ExpectedValueIn.To<double>() - ActualValueIn.To<double>());
            var roundedActual = ActualValueIn.To<double>().RoundedTo(precision);
            ActualValueOut = roundedActual.To<string>();

            // We align the precision of the delta to that of the expected/actual values 
            if (precision == null) precision = Math.Max(ExpectedValueIn.FractionalDigits(), ActualValueIn.FractionalDigits());
            DeltaOut = delta.RoundedTo(precision).To<string>();

            return delta <= Tolerance.Value ? CompareOutcome.WithinTolerance : CompareOutcome.OutsideToleranceIssue;
        }

        private void InferCompareTypeIfNotSpecified()
        {
            if (CompareType == null) CompareType = ActualValueIn.InferType(ExpectedValueIn.InferType());
        }

        public static bool IsOk(CompareOutcome outcome) => outcome == CompareOutcome.None || outcome == CompareOutcome.WithinTolerance;

        private CompareOutcome LongComparison()
        {
            var delta = Math.Abs(ExpectedValueIn.To<long>() - ActualValueIn.To<long>());
            var roundedTolerance = (Tolerance?.Value ?? 0).To<long>();
            DeltaOut = delta.To<string>();
            return delta <= roundedTolerance
                ? CompareOutcome.WithinTolerance
                : CompareOutcome.OutsideToleranceIssue;
        }

        private static CompareOutcome? OutcomeWithNullInput(object expected, object actual)
        {
            if (expected == null && actual == null) return CompareOutcome.None;
            if (expected == null) return CompareOutcome.Surplus;
            if (actual == null) return CompareOutcome.Missing;
            return null;
        }

        private string RoundViaTolerance(object target)
        {
            if (target == null) return null;
            var compareType = CompareType ?? target.To<string>().InferType();
            return (Tolerance != null && compareType.IsFloatingPoint() && target.IsNumeric()
                ? target.To<double>().RoundedTo(Tolerance.Precision)
                : target).To<string>(); // duh
        }

        private CompareOutcome RunComparison()
        {
            ActualValueOut = RoundViaTolerance(ActualValueIn);
            ExpectedValueOut = RoundViaTolerance(ExpectedValueIn);
            var outcome = OutcomeWithNullInput(ExpectedValueIn, ActualValueIn);
            if (outcome != null) return outcome.Value;

            // now we are sure neither ExpectedIn nor ActualIn is null
            // start doing a plain text comparison. If we have an exact match, we're done (no issues)
            // edge case: ∞ not equal to Infinity
            if (ExpectedValueIn.Equals(ActualValueIn)) return CompareOutcome.None;

            // if the compare type is not specified we have indivudual comparisons, and 
            // expected/actual values may have different types. So then get a compatible type.
            InferCompareTypeIfNotSpecified();

            object actualValue;
            try
            {
                actualValue = ActualValueIn.To(CompareType);
            }
            catch (FormatException)
            {
                return CompareOutcome.ValueIssue;
            }

            // If we have a tolerance with a numerical comparison and the data range was not set,
            // we need to set it via the current expected value, to calculate the absolute tolerance
            if (IsNumericalComparisonWithoutToleranceRange)
            {
                Tolerance.DataRange = ExpectedValueIn.To<double>();
                ToleranceBase = Tolerance.DataRange;
            }

            // We're done if the in-values are different but out-values are the same (can occur e.g. with e-notations,
            // or with different cultures such as ∞ vs Infinity).
            // Since this can also occur due to rounding, we return WithinTolerance rather than None if we don't
            // find something like Infinity in the actual out value
            if (ExpectedValueOut.Equals(actualValue.To<string>()))
            {
                return ActualValueOut.Contains("Infinity") ? CompareOutcome.None : CompareOutcome.WithinTolerance;
            }

            // Fail if the comparison is non-numerical or the tolerance is 0. An exact match would have caught a pass.
            if (IsZeroToleranceComparison) return CompareOutcome.ValueIssue;

            // the calculation for floats and ints is slightly different because of the precision
            return CompareType.IsFloatingPoint() ? DoubleComparison() : LongComparison();
        }
    }
}