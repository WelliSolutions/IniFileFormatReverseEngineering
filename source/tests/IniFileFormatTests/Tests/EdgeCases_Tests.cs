using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.Limits
{
    [TestClass]
    public class EdgeCases_Tests : IniFileTestBase
    {
        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Parameter.lpAppName)]
        [TestMethod]
        public void Given_ASectionWithNoName_When_UsingEmptyString_Then_WeGetTheValue()
        {
            EnsureASCII($"[]\r\n{keyname}={inivalue}\r\n");
            var sb = DefaultStringBuilder();

            // Insight: a section with no name can be read from
            var bytes = GetIniString_SB_Unicode("", keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Parameter.lpAppName)]
        [TestMethod]
        public void Given_AnEmptySectionName_When_WritingAValue_Then_ASectionWithoutNameIsCreated()
        {
            EnsureEmptyASCII();
            WritePrivateProfileStringW("", keyname, inivalue, FileName);

            // Insight: a section with no name can be written to
            Assert.AreEqual($"[]\r\n{keyname}={inivalue}\r\n", File.ReadAllText(FileName));
        }
    }
}