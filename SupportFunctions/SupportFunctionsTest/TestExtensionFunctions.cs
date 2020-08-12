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

using System.Data;
using System.Linq;

namespace SupportFunctionsTest
{
    public static class TestExtensionFunctions
    {
        public static double? ToNullableDouble(this object input)
        {
            double? output = null;
            if (double.TryParse(input.ToString(), out var result)) output = result;
            return output;
        }

        public static int? ToNullableInt(this object input)
        {
            int? output = null;
            if (int.TryParse(input.ToString(), out var result)) output = result;
            return output;
        }

        public static string ValueIfExists(this DataRow row, string columnName)
        {
            return row.Table.Columns.Cast<DataColumn>().Any(column => column.ColumnName == columnName) ? row[columnName].ToString() : null;
        }
    }
}
