// Copyright 2015-2019 Rik Essenius
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using static System.FormattableString;

namespace SupportFunctions.Model
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by FitSharp")]
    internal class WikiFile
    {
        private const string TestResultsFolder = "files\\testResults";

        private readonly string _wikiPage;
        private readonly string _wikiPagePath;
        private readonly string _wikiRoot;

        [Documentation("Experimental. Do not use")]
        public WikiFile(string wikiRoot, string wikiPage)
        {
            _wikiRoot = wikiRoot;
            _wikiPage = Path.Combine(TestResultsFolder, wikiPage);
            _wikiPagePath = Path.Combine(wikiRoot, _wikiPage);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static string MimeType(byte[] imageData)
        {
            try
            {
                Guid id;
                using (var ms = new MemoryStream(imageData))
                {
                    using (var img = Image.FromStream(ms))
                    {
                        id = img.RawFormat.Guid;
                    }
                }
                if (id == ImageFormat.Jpeg.Guid || id == ImageFormat.Exif.Guid) return "image/jpeg";
                if (id == ImageFormat.Png.Guid) return "image/png";
                if (id == ImageFormat.Gif.Guid) return "image/gif";
                if (id == ImageFormat.Bmp.Guid || id == ImageFormat.MemoryBmp.Guid) return "image/bmp";
                if (id == ImageFormat.Icon.Guid) return "image/ico";
                if (id == ImageFormat.Tiff.Guid) return "image/tiff";
                if (id == ImageFormat.Emf.Guid) return "image/x-emf";
            }
            catch (Exception)
            {
                // all exceptions imply we don't know this image type
            }

            return "image/unknown";
        }

        public string UniquePathFor(string baseName) => UniquePathFor(baseName, UniqueDateTime.NowTicks);

        public string UniquePathFor(string baseName, long ticks)
        {
            if (baseName == null) baseName = string.Empty;
            var name = Path.GetFileNameWithoutExtension(baseName);
            var extension = Path.GetExtension(baseName);
            var timestamp = new DateTime(ticks).ToString("yyyyMMddHHmmssffff", CultureInfo.InvariantCulture);
            name = Invariant($"{timestamp}_{name}");
            var path = Path.Combine(_wikiPagePath, name);
            var i = 1;
            string result;
            do
            {
                result = Invariant($"{path}_{i++}{extension}");
            } while (File.Exists(result));
            return result;
        }

        public string WikiLink(string path)
        {
            Debug.Assert(path != null, "path != null");
            return !path.StartsWith(_wikiRoot, StringComparison.OrdinalIgnoreCase)
                ? null
                : "<img src='http://" + (_wikiPage + path.Substring(_wikiRoot.Length)).Replace("\\", "/") + "'/>";
        }
    }
}