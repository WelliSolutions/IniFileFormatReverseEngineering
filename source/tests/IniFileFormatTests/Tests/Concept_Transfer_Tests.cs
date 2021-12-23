using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests
{
    /// <summary>
    /// These tests check whether concepts that have been described somewhere
    /// will also apply in other situations
    /// </summary>
    [TestClass]
    public class Concept_Transfer_Tests : IniFileTestBase
    {
        /// <summary>
        /// Reasoning: quotes are stripped from values when reading.
        /// This test checks if they are also stripped from keys.
        /// </summary>
        [Checks(Parameter.lpKeyName)]
        [TestMethod]
        public void Given_AKeyWithQuotes_When_TheKeyIsUsed_Then_NoQuotesAreStripped()
        {
            foreach (var quote in new[] { '\'', '\"' })
            {
                EnsureASCII($"[{sectionname}]\r\n{quote}{keyname}{quote}={inivalue}\r\n");
                var sb = DefaultStringBuilder();
                var bytes = GetIniString_SB_Unicode(sectionname, quote + keyname + quote, null, sb, (uint)sb.Capacity, FileName);

                // Insight: the value can be accessed using quotes in the key name
                AssertASCIILength(inivalue, bytes);
                Assert.AreEqual(0, Marshal.GetLastWin32Error());

                // Insight: the value can't be accessed without quotes in the key name
                bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
                AssertZero(bytes);
            }
        }

        /// <summary>
        /// Reasoning: quotes are stripped from values when reading.
        /// This test checks if they are also stripped from sections.
        /// </summary>
        [Checks(Parameter.lpAppName)]
        [TestMethod]
        public void Given_ASectionWithQuotes_When_TheKeyIsUsed_Then_NoQuotesAreStripped()
        {
            foreach (var quote in new[] { '\'', '\"' })
            {
                EnsureASCII($"[{quote}{sectionname}{quote}]\r\n{keyname}={inivalue}\r\n");
                var sb = DefaultStringBuilder();
                var bytes = GetIniString_SB_Unicode(quote + sectionname + quote, keyname, null, sb, (uint)sb.Capacity, FileName);

                // Insight: the value can be accessed using quotes in the section name
                AssertASCIILength(inivalue, bytes);
                Assert.AreEqual(0, Marshal.GetLastWin32Error());

                // Insight: the value can't be accessed without quotes in the section name
                bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
                AssertZero(bytes);
            }
        }
    }
}