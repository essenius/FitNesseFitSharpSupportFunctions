﻿// Copyright 2017-2020 Rik Essenius
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

        [TestMethod, TestCategory("Unit")]
        public void WikiFileMimeTypeTest()
        {
            byte[] bmp =
            {
                0x42, 0x4D, 0x1E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1A, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00,
                0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00, 0xFF, 0x00
            };

            byte[] gif =
            {
                0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x00, 0xff, 0x00, 0x2c, 0x00, 0x00, 0x00,
                0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x00, 0x3b
            };

            byte[] icon =
            {
                0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x30, 0x00, 0x00,
                0x00, 0x16, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00,
                0x01, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00,
                0x00, 0x00
            };

            byte[] jpg =
            {
                0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x48, 0x00,
                0x48, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xC2, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00,
                0xFF, 0xC4, 0x00, 0x14, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x01, 0x3F, 0x10
            };

            byte[] png =
            {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00,
                0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4, 0x89, 0x00,
                0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00, 0x05, 0x00, 0x01,
                0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
            };

            byte[] tiff =
            {
                0x4D, 0x4D, 0x00, 0x2A, 0x00, 0x00, 0x00, 0x08, 0x00, 0x07, 0x01, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,
                0x01, 0x00, 0x01, 0x00, 0x00, 0x01, 0x01, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00,
                0x01, 0x06, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x11, 0x00, 0x03, 0x00,
                0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x17, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01,
                0x00, 0x00, 0x01, 0x1A, 0x00, 0x05, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x64, 0x01, 0x1B, 0x00,
                0x05, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x00, 0x20, 0x20, 0x20, 0x62,
                0x79, 0x20, 0x61, 0x6C, 0x6F, 0x6B
            };

            // just a "small" emf saved with Powerpoint. Can probably optimize this.
            byte[] emf =
            {
                0x01, 0x00, 0x00, 0x00, 0x6C, 0x00, 0x00, 0x00, 0x4E, 0x08, 0x00, 0x00, 0x90, 0x05, 0x00, 0x00, 0x7F,
                0x08, 0x00, 0x00, 0xC1, 0x05, 0x00, 0x00, 0x62, 0x23, 0x00, 0x00, 0xD3, 0x17, 0x00, 0x00, 0x90, 0x23,
                0x00, 0x00, 0x01, 0x18, 0x00, 0x00, 0x20, 0x45, 0x4D, 0x46, 0x00, 0x00, 0x01, 0x00, 0x28, 0x04, 0x00,
                0x00, 0x15, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x98, 0x12, 0x00, 0x00, 0x9E, 0x1A, 0x00, 0x00, 0xC9, 0x00, 0x00, 0x00, 0x20,
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x11,
                0x03, 0x00, 0x00, 0x65, 0x04, 0x00, 0x46, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00,
                0x00, 0x45, 0x4D, 0x46, 0x2B, 0x01, 0x40, 0x01, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00,
                0x02, 0x10, 0xC0, 0xDB, 0x00, 0x00, 0x00, 0x00, 0x58, 0x02, 0x00, 0x00, 0x58, 0x02, 0x00, 0x00, 0x46,
                0x00, 0x00, 0x00, 0x58, 0x01, 0x00, 0x00, 0x4C, 0x01, 0x00, 0x00, 0x45, 0x4D, 0x46, 0x2B, 0x30, 0x40,
                0x02, 0x00, 0x10, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x2A, 0x40, 0x00,
                0x00, 0x24, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x32,
                0x40, 0x00, 0x01, 0x1C, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x10, 0x06, 0x45, 0x00, 0x60,
                0xB4, 0x44, 0x00, 0x00, 0x40, 0x41, 0x00, 0x00, 0x40, 0x41, 0x2A, 0x40, 0x00, 0x00, 0x24, 0x00, 0x00,
                0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x25, 0x40, 0x00, 0x00, 0x10,
                0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x40, 0x03, 0x00, 0x0C, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x22, 0x40, 0x04, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x1E, 0x40, 0x09, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x21, 0x40, 0x07, 0x00,
                0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2A, 0x40, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x18,
                0x00, 0x00, 0x00, 0xB0, 0x02, 0x2C, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB0, 0x02,
                0x2C, 0x3A, 0x00, 0x70, 0x06, 0x45, 0x00, 0x20, 0xB5, 0x44, 0x08, 0x40, 0x00, 0x02, 0x34, 0x00, 0x00,
                0x00, 0x28, 0x00, 0x00, 0x00, 0x02, 0x10, 0xC0, 0xDB, 0x00, 0x00, 0x00, 0x00, 0x90, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0xBE, 0x45, 0x00, 0x00, 0x00, 0x41, 0x00, 0x00, 0x00, 0x00, 0x02,
                0x10, 0xC0, 0xDB, 0x00, 0x00, 0x00, 0x00, 0xD5, 0x9B, 0x5B, 0xFF, 0x08, 0x40, 0x01, 0x03, 0x2C, 0x00,
                0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x02, 0x10, 0xC0, 0xDB, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x7F, 0x3F, 0xFF, 0xFF, 0x7F, 0x3F,
                0x00, 0x01, 0x0B, 0x41, 0x15, 0x40, 0x01, 0x00, 0x10, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x21, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x62, 0x00, 0x00, 0x00, 0x0C, 0x00,
                0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x3A, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00,
                0x00, 0x24, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3E, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02,
                0x00, 0x00, 0x00, 0x5F, 0x00, 0x00, 0x00, 0x38, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x38, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x38, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x22, 0x01,
                0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5B, 0x9B, 0xD5, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x25, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x01,
                0x00, 0x00, 0x00, 0x25, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x80, 0x57, 0x00,
                0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x4E, 0x08, 0x00, 0x00, 0x90, 0x05, 0x00, 0x00, 0x7F, 0x08, 0x00,
                0x00, 0xC1, 0x05, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x34, 0x43, 0x44, 0x2D, 0x34, 0x43, 0x44, 0x2D,
                0x25, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x80, 0x25, 0x00, 0x00, 0x00, 0x0C,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x24, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x41, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00,
                0x01, 0x00, 0x00, 0x00, 0x3A, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x46,
                0x00, 0x00, 0x00, 0x8C, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x45, 0x4D, 0x46, 0x2B, 0x2A, 0x40,
                0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x2A, 0x40, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x26, 0x40, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x08, 0x40, 0x02, 0x04, 0x18, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x02, 0x10, 0xC0, 0xDB,
                0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x10, 0x34, 0x40, 0x02, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x4C, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x63, 0x08, 0x00, 0x00, 0xA5, 0x05,
                0x00, 0x00, 0x6A, 0x08, 0x00, 0x00, 0xAC, 0x05, 0x00, 0x00, 0x63, 0x08, 0x00, 0x00, 0xA5, 0x05, 0x00,
                0x00, 0x08, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x29, 0x00, 0xAA, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x22, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x46, 0x00, 0x00, 0x00,
                0x1C, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x45, 0x4D, 0x46, 0x2B, 0x02, 0x40, 0x00, 0x00, 0x0C,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00
            };

            Assert.AreEqual("image/bmp", WikiFile.MimeType(bmp));
            Assert.AreEqual("image/gif", WikiFile.MimeType(gif));
            Assert.AreEqual("image/ico", WikiFile.MimeType(icon));
            Assert.AreEqual("image/jpeg", WikiFile.MimeType(jpg));
            Assert.AreEqual("image/png", WikiFile.MimeType(png));
            Assert.AreEqual("image/tiff", WikiFile.MimeType(tiff));
            Assert.AreEqual("image/unknown", WikiFile.MimeType(Array.Empty<byte>()));
            Assert.AreEqual("image/x-emf", WikiFile.MimeType(emf));
        }

        [TestMethod, TestCategory("Integration")]
        public void WikiFileUniquePathTest()
        {
            var root = Path.GetTempPath();
            var wikiFile = new WikiFile(root, "TestPage");
            var wikiFolder = Path.Combine(root, "files\\testResults\\TestPage");
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

        [TestMethod, TestCategory("Unit")]
        public void WikiFileUniquePathTest2()
        {
            const string format = @"yyyyMMddHHmmssffff";
            var culture = CultureInfo.InvariantCulture;
            var startTime = DateTime.Now.ToString(format, culture);
            var file = new WikiFile("c:\\", "Data").UniquePathFor(@"demofile");
            Assert.IsNotNull(file);
            var timestamp = Path.GetFileNameWithoutExtension(file).Substring(0, 18);
            var endTime = DateTime.Now.ToString(format, culture);
            Assert.IsTrue(string.Compare(startTime, timestamp, StringComparison.Ordinal) <= 0, $"{startTime} <= {timestamp}");
            Assert.IsTrue(string.Compare(timestamp, endTime, StringComparison.Ordinal) <= 0, $"{timestamp} <= {endTime}");
        }

        [TestMethod, TestCategory("Integration")]
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
