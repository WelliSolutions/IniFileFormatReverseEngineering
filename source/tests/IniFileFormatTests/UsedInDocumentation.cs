using System;

namespace IniFileFormatTests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class UsedInDocumentation : Attribute
    {
        public UsedInDocumentation(string file = null)
        {
            File = file;
        }

        public string File;
    }
}