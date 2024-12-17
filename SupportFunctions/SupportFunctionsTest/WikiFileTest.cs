// Copyright 2017-2024 Rik Essenius
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
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SupportFunctions.Model;
using static System.FormattableString;

namespace SupportFunctionsTest
{
    [TestClass]
    public class WikiFileTest
    {
        private static string ImageCode(string leaf) =>
            Invariant($"<img src='http://files/testResults/wiki/files/testResults/wiki{leaf}'/>");

        [DataTestMethod]
        [TestCategory("Unit")]
        [DataRow("image/bmp", new byte[]
        {
            0x42, 0x4D, 0x1E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1A, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00, 0xFF, 0x00
        })]
        [DataRow("image/gif", new byte[]
        {
            0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x00, 0xff, 0x00, 0x2c, 0x00, 0x00, 0x00,
            0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x00, 0x3b
        })]
        [DataRow("image/ico", new byte[]
        {
            0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x30, 0x00, 0x00,
            0x00, 0x16, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00,
            0x00, 0x00
        })]
        [DataRow("image/jpeg", new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x48, 0x00,
            0x48, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC2, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00,
            0xFF, 0xC4, 0x00, 0x14, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x01, 0x3F, 0x10
        })]
        [DataRow("image/png", new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00,
            0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89, 0x00,
            0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01,
            0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
        })]
        [DataRow("image/unknown", new byte[] { })]
        public void WikiFileMimeTypeTest(string format, byte[] image) => Assert.AreEqual(format, WikiFile.MimeType(image));

        [TestMethod]
        [TestCategory("Integration")]
        public void WikiFileUniquePathTest()
        {
            var root = Path.GetTempPath();
            var wikiFile = new WikiFile(root, "TestPage");
            var wikiFolder = Path.Combine(root, @"files\testResults\TestPage");
            Directory.CreateDirectory(wikiFolder);
            var dir = new DirectoryInfo(wikiFolder);
            foreach (var file in dir.EnumerateFiles("*test*.rik"))
            {
                file.Delete();
            }

            var a = wikiFile.UniquePathFor("test.rik", 0);
            Assert.IsTrue(a.EndsWith("000101010000000000_test_1.rik", StringComparison.Ordinal), "test 1");
            File.Create(a).Close();
            var b = wikiFile.UniquePathFor("test.rik", 0);
            Assert.IsTrue(b.EndsWith("000101010000000000_test_2.rik", StringComparison.Ordinal), "test 2");
            Assert.IsTrue(wikiFile.UniquePathFor(string.Empty, 0).EndsWith("000101010000000000__1", StringComparison.Ordinal), "test 3");
            Assert.IsTrue(wikiFile.UniquePathFor(null, 0).EndsWith("000101010000000000__1", StringComparison.Ordinal), "test 4");
            Assert.IsTrue(wikiFile.UniquePathFor(".rik", 0).EndsWith("000101010000000000__1.rik", StringComparison.Ordinal), "test 5");
            File.Delete(a);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void WikiFileUniquePathTest2()
        {
            const string format = "yyyyMMddHHmmssffff";
            var culture = CultureInfo.InvariantCulture;
            var startTime = DateTime.Now.ToString(format, culture);
            var file = new WikiFile("c:\\", "Data").UniquePathFor("demofile");
            Assert.IsNotNull(file);
            var timestamp = Path.GetFileNameWithoutExtension(file).Substring(0, 18);
            var endTime = DateTime.Now.ToString(format, culture);
            Assert.IsTrue(string.Compare(startTime, timestamp, StringComparison.Ordinal) <= 0, $"{startTime} <= {timestamp}");
            Assert.IsTrue(string.Compare(timestamp, endTime, StringComparison.Ordinal) <= 0, $"{timestamp} <= {endTime}");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void WikiFileWikiLinkTest()
        {
            var wikiFile = new WikiFile("c:\\test", "wiki");
            Assert.AreEqual(ImageCode("/test1"), wikiFile.WikiLink(@"c:\test\files\testResults\wiki\test1"), "simple test");
            Assert.AreEqual(ImageCode("/sub/test1"), wikiFile.WikiLink(@"c:\test\files\testResults\wiki\sub\test1"),
                "test with subfolder");
            Assert.AreEqual(ImageCode("/test1.test2"), wikiFile.WikiLink(@"c:\test\files\testResults\wiki\test1.test2"),
                "test with dots");
            Assert.IsNull(wikiFile.WikiLink("d:\\test1"), "test with different root");
            Assert.AreEqual(ImageCode(string.Empty), wikiFile.WikiLink(@"c:\test\files\testResults\wiki"), "empty test");
        }
    }
}
