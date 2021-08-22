// Copyright 2020-2021 Rik Essenius
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using SupportFunctions.Utilities;
using static System.FormattableString;

namespace SupportFunctions
{
    /// <summary>Fixtures to access methods on parameters or static classes via reflection</summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "FitSharp entry point")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "FitSharp entry point")]
    public sealed class ReflectionFunctions
    {
        private static Type FindStaticClass(string className)
        {
            var type = Type.GetType(className.TypeName()) ?? Type.GetType("System." + className.ToTitleCase());
            if (type == null) throw new TypeLoadException(Invariant($"Could not find static class '{className}'"));
            return type;
        }

        /// <summary>Get the value of a field, property or method</summary>
        /// <param name="member">
        ///     the field, property or method to get the value from. Parameters can be specified between parentheses ()
        ///     Specify a static class via the dot notation, e.g. Math.PI.
        ///     If a method has no parameters, don't specify parentheses (it would be interpreted as an empty string)
        /// </param>
        /// <returns>the value of a field, property or method of a static class</returns>
        public static object Get(string member) => GetOf(member, null);

        /// <param name="member">the value, field.property or method to call</param>
        /// <param name="input">the entity to call the method on</param>
        /// <returns>the value of a field, property or method of an entity</returns>
        public static object GetOf(string member, string input)
        {
            Requires.NotNullOrEmpty(member, nameof(member));
            const string pattern = @"\((.*)\)$";
            var regex = new Regex(pattern, RegexOptions.None);
            var match = regex.Match(member);
            if (!match.Success) return GetWithParamsOf(member, null, input);
            var param = match.Groups[1].Captures[0].Value.Split(',').Select(s => s.Trim()).ToArray<object>();
            member = member.Split('(')[0].TrimEnd();
            if (input == null)
            {
                input = param[0].ToString();
                param = param.Skip(1).ToArray();
            }
            return GetWithParamsOf(member, param, input);
        }

        /// <summary>Like Get Of, but with parameters as a separate entity</summary>
        /// <param name="member">the method name</param>
        /// <param name="parameters">the parameters for the method (in FitNesse array format)</param>
        /// <param name="input">the entity to call the method on</param>
        /// <returns>the result of the method call</returns>
        public static object GetWithParamsOf(string member, object[] parameters, string input)
        {
            Requires.NotNullOrEmpty(member, nameof(member));
            // if we are asking for a static class member, the input could be null
            var convertedInput = input?.CastToInferredType();
            var inputType = convertedInput?.GetType();
            parameters ??= Array.Empty<object>();

            // we use a list here to make it easier to insert the input value for static calls
            var convertedParams = parameters.Select(p => ((string)p).CastToInferredType()).ToList();

            // assume it is a static call (e.g. Math) if there is a dot in the name
            if (member.Contains(".")) return StaticFunctionCall(member, convertedInput, convertedParams);

            // No static call, so try if it is a property or method on the object
            var types = convertedParams.Select(t => t.GetType()).ToArray();
            if (TryGetMember(inputType, member, convertedInput, types, convertedParams.ToArray(), out var outValue))
            {
                return outValue;
            }

            // final fallback: try string
            if (TryGetMember(typeof(string), member, input, types, convertedParams.ToArray(), out outValue))
            {
                return outValue;
            }
            throw new MissingMethodException(MissingMemberMessage(member, inputType));
        }

        /// <summary>Find a member by the name, Look exact to start with, if that doesn't work look case insensitive.</summary>
        /// <param name="type">the type to look in</param>
        /// <param name="name">the member name to look for</param>
        /// <returns>the case sensitive name of the member</returns>
        private static string MemberName(Type type, string name)
        {
            var exactMember = type.GetMember(name);
            if (exactMember.Length > 0) return exactMember[0].Name;
            return (from member in type.GetMembers()
                where member.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                select member.Name).FirstOrDefault();
        }

        private static string MissingMemberMessage(string member, Type inputType) =>
            Invariant($"Could not find property, field or method '{member}' for type ") +
            (inputType != null && inputType.Name != "String" ? Invariant($"'{inputType.Name}' or ") : "") + "'String'";


        private static object StaticFunctionCall(string function, object convertedInput, List<object> convertedParams)
        {
            // Look for the last dot; the class could be fully specified (and also contain dots)
            var index = function.LastIndexOf(".", StringComparison.Ordinal);
            var className = function.Substring(0, index).Trim();
            var type = FindStaticClass(className);
            var rawMemberName = function.Substring(index + 1).Trim();

            // insert the input to the parameter arrays if not null - it is the first parameter for static calls
            if (convertedInput != null) convertedParams.Insert(0, convertedInput);
            var types = convertedParams.Select(t => t.GetType()).ToArray();
            if (TryGetMember(type, rawMemberName, null, types, convertedParams.ToArray(), out var outValue))
            {
                return outValue;
            }
            throw new MissingMemberException(Invariant($"Could not find static property, field or method '{function}'"));
        }

        private static bool TryGetMember(Type type, string function, object input,
            Type[] types, object[] parameters, out object output)
        {
            output = null;
            if (type == null) return false;
            var memberName = MemberName(type, function);
            if (memberName == null) return false;
            var property = type.GetProperty(memberName);
            if (property != null)
            {
                output = property.GetValue(input, null);
                return true;
            }
            var field = type.GetField(memberName);
            if (field != null)
            {
                output = field.GetValue(input);
                return true;
            }
            var method = type.GetMethod(memberName, types);
            if (method == null)
            {
                // if we had decimal parameters, retry with double (those are more common)
                for (var i = 0; i < types.Length; i++)
                {
                    if (types[i] == typeof(decimal)) types[i] = typeof(double);
                }
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] is decimal) parameters[i] = parameters[i].To<double>();
                }
                method = type.GetMethod(function, types);
            }
            if (method == null) return false;
            output = method.Invoke(input, parameters);
            return true;
        }
    }
}
