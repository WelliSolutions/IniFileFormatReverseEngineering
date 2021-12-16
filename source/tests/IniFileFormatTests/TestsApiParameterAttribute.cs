using System;

namespace IniFileFormatTests
{
    /// <summary>
    /// Maybe this attribute can provide a better overview of what's actually tested.
    /// My Given_When_Then names are potentially long
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestsApiParameterAttribute : Attribute
    {
        public TestsApiParameterAttribute(string parameterName, string parameterValue = null)
        {
            ParameterName = parameterName;
            ParameterValue = parameterName;
        }

        public string ParameterValue { get; }

        public string ParameterName { get; }
    }
}