using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.IntendedUse
{
    /// <summary>
    /// Windows is not cases sensitive for its file names.
    /// But how about file contents?
    /// </summary>
    [TestClass]
    public class Casing_Tests : IniFileTestBase
    {
        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpAppName)]
        [TestMethod]
        public void Given_AnSectionWithUpperCaseLetters_When_TheContentIsAccessedWithLowerCase_Then_WeGetTheExpectedValue()
        {
            EnsureDefaultContent_UsingFile();
            var sb = DefaultStringBuilder();

            // Insight: section name reading is case insensitive
            var bytes = GetIniString_SB_Unicode(sectionname.ToLower(), keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);

            bytes = GetIniString_SB_Unicode(sectionname.ToUpper(), keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);

        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpKeyName)]
        [TestMethod]
        public void Given_AnEntryWithUpperCaseLetter_When_TheContentIsAccessedWithLowerCase_Then_WeGetTheExpectedValue()
        {
            EnsureDefaultContent_UsingFile();
            var sb = DefaultStringBuilder();

            // Insight: key reading is case insensitive
            var bytes = GetIniString_SB_Unicode(sectionname, keyname.ToLower(), null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);

            bytes = GetIniString_SB_Unicode(sectionname, keyname.ToUpper(), null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpKeyName)]
        [TestMethod]
        public void Given_AnExistingKey_When_WritingTheKeyWithUpperCase_Then_TheExistingCasingIsKept()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}={inivalue}");
            var sb = DefaultStringBuilder();

            WritePrivateProfileStringW(sectionname, keyname.ToUpper(), inivalue2, FileName);

            // Insight: the casing of the existing key is kept
            Assert.AreEqual($"[{sectionname}]\r\n{keyname}={inivalue2}\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpAppName)]
        [TestMethod]
        public void Given_AnExistingSection_When_WritingTheSectionWithUpperCase_Then_TheExistingCasingIsKept()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}={inivalue}");
            var sb = DefaultStringBuilder();

            // Insight: the casing of the existing section is kept
            WritePrivateProfileStringW(sectionname.ToUpper(), keyname, inivalue2, FileName);
            Assert.AreEqual($"[{sectionname}]\r\n{keyname}={inivalue2}\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpString)]
        [TestMethod]
        public void Given_AnExistingValue_When_WritingTheValueWithUpperCase_Then_ThenewValueIsUsed()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}={inivalue}");
            var sb = DefaultStringBuilder();

            WritePrivateProfileStringW(sectionname, keyname, inivalue.ToUpper(), FileName);

            // Insight: values are written as given
            Assert.AreEqual($"[{sectionname}]\r\n{keyname}={inivalue.ToUpper()}\r\n", File.ReadAllText(FileName));
        }
    }
}