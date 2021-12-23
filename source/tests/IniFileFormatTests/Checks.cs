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
        public Checks(Parameter parameter, string contentValue = "<unknown>")
        {
            Parameter = parameter;
            Value = contentValue;
        }

        public Checks(Paragraph paragraph)
        {
            Paragraph = paragraph;
        }

        public Checks(Method method)
        {
            Method = method;
        }

        public Checks(FileContent fileContent, string contentValue)
        {
            FileContent = fileContent;
            Value = contentValue;
        }

        public string Value { get; }

        public Parameter Parameter { get; }

        public Paragraph Paragraph { get; }

        public Method Method { get; }
        public FileContent FileContent { get; }
    }

    public enum Method
    {
        GetPrivateProfileStringA,
        GetPrivateProfileStringW,
        WritePrivateProfileStringA,
        WritePrivateProfileStringW,
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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum FileContent
    {
        lpAppName,
        lpKeyName,
        lpString,
    }
}