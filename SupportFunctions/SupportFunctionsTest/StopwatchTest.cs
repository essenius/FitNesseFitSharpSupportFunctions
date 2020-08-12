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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions;
using SupportFunctions.Utilities;

namespace SupportFunctionsTest
{
    [TestClass]
    public class StopwatchTest
    {
        [TestMethod, TestCategory("Integration")]
        public void StopwatchTest1()
        {
            const double tolerance = 0.05;
            var stopwatch = new Stopwatch();
            stopwatch.StartStopwatch("test1");
            CommonFunctions.WaitSeconds(0.2);
            var elapsed = stopwatch.StopStopwatch("test1");
            ("Elapsed1: " + elapsed).Log();
            Assert.IsTrue(Math.Abs(elapsed - 0.2) < tolerance, "elapsed1");
            stopwatch.StartStopwatch("test1");
            CommonFunctions.WaitSeconds(0.05);
            elapsed = stopwatch.StopStopwatch("test1");
            ("Elapsed2: " + elapsed).Log();
            Assert.IsTrue(Math.Abs(elapsed - 0.25) < tolerance, "elapsed2");
            stopwatch.RestartStopwatch("test1");
            CommonFunctions.WaitSeconds(0.025);
            elapsed = stopwatch.StopStopwatch("test1");
            ("Elapsed3: " + elapsed).Log();
            var elapsedRead = stopwatch.ReadStopwatch("test1");
            ("Elapsed read: " + elapsedRead).Log();
            Assert.IsTrue(Math.Abs(elapsed - elapsedRead) <= 1e-9);

            Assert.IsTrue(Math.Abs(elapsed - 0.025) < tolerance, "elapsed3");
            stopwatch.ResetStopwatch("test1");
            elapsed = stopwatch.ReadStopwatch("test1");
            ("Elapsed4: " + elapsed).Log();
            Assert.IsTrue(elapsed < tolerance, "elapsed4");
        }
    }
}
