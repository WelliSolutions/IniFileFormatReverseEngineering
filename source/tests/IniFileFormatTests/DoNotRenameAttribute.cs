using System;

namespace IniFileFormatTests
{
    public class DoNotRenameAttribute : Attribute
    {
        public DoNotRenameAttribute(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; }
    }
}