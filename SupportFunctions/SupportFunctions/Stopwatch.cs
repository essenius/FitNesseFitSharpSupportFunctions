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

using System.Collections.Generic;

namespace SupportFunctions
{
    /// <summary>Timing functions for rudimentary performance tests</summary>
    public class Stopwatch
    {
        private readonly Dictionary<string, System.Diagnostics.Stopwatch> _stopwatchDictionary;

        /// <summary>Initialize a new Stopwatch</summary>
        public Stopwatch() => _stopwatchDictionary = new Dictionary<string, System.Diagnostics.Stopwatch>();

        private System.Diagnostics.Stopwatch GetStopwatch(string id)
        {
            System.Diagnostics.Stopwatch stopwatch;
            if (!_stopwatchDictionary.ContainsKey(id))
            {
                stopwatch = new System.Diagnostics.Stopwatch();
                _stopwatchDictionary.Add(id, stopwatch);
            }
            else
            {
                stopwatch = _stopwatchDictionary[id];
            }
            return stopwatch;
        }

        /// <returns>the elapsed time in seconds of a stopwatch</returns>
        public double ReadStopwatch(string id) => GetStopwatch(id).ElapsedMilliseconds / 1000.0;

        /// <summary>Reset a stopwatch to 0</summary>
        public void ResetStopwatch(string id) => GetStopwatch(id).Reset();

        /// <summary>Reset a stopwatch to 0 and start counting</summary>
        public void RestartStopwatch(string id) => GetStopwatch(id).Restart();

        /// <summary>Start (or continue) a stopwatch</summary>
        public void StartStopwatch(string id) => GetStopwatch(id).Start();

        /// <summary>Stop a stopwatch and return elapsed time in seconds</summary>
        public double StopStopwatch(string id)
        {
            GetStopwatch(id).Stop();
            return ReadStopwatch(id);
        }
    }
}
