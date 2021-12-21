using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IniFileFormatTests
{
    static class AssertionHelper
    {
        public static void AssertZero(uint bytes)
        {
            Assert.AreEqual((uint)0, bytes);
        }

        public static void AssertASCIILength(string expected, uint bytes)
        {
            Assert.AreEqual((uint)Encoding.ASCII.GetBytes(expected).Length, bytes);
        }

        public static void AssertSbEqual(string expected, StringBuilder sb)
        {
            Assert.AreEqual(expected, sb.ToString());
        }

        public static void AssertFileEqualASCII(string expected, string fileName)
        {
            var asciiContent = File.ReadAllText(fileName, Encoding.ASCII);
            Assert.AreEqual(expected, asciiContent);
        }
    }
}
