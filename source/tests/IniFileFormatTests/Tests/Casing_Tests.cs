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
        [DoNotRename("Used in documentation")]
        [TestsApiParameter("lpAppName")]
        [TestMethod]
        public void Given_AnSectionWithUpperCaseLetters_When_TheContentIsAccessedWithLowerCase_Then_WeGetTheExpectedValue()
        {
            EnsureDefaultContent_UsingFile();
            var sb = DefaultStringBuilder();

            // Insight: section name reading is case insensitive
            var bytes = GetIniString_SB_Unicode(sectionname.ToLower(), keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);

            bytes = GetIniString_SB_Unicode(sectionname.ToUpper(), keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);

        }

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("lpKeyName")]
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
    }
}