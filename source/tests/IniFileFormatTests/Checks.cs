using System;
using System.Diagnostics.CodeAnalysis;

namespace IniFileFormatTests
{
    /// <summary>
    /// Maybe this attribute can provide a better overview of what's actually tested.
    /// My Given_When_Then names are potentially long and hard to understand.
    /// This could make it machine readable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Checks : Attribute
    {
        public Checks(Parameter parameter, string parameterValue = "<unknown>")
        {
            Parameter = parameter;
            ParameterValue = parameterValue;
        }

        public Checks(Paragraph paragraph)
        {
            Paragraph = paragraph;
        }

        public string ParameterValue { get; }

        public Parameter Parameter { get; }

        public Paragraph Paragraph { get; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Parameter
    {
        lpAppName,
        lpKeyName,
        lpDefault,
        lpReturnedString,
        nSize,
        lpFileName,
        lpString
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Paragraph
    {
        syntax,
        returnValue,
        remarks
    }
}