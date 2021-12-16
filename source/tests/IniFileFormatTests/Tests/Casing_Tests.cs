using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests
{
    /// <summary>
    /// Windows is not cases sensitive for its file names.
    /// But how about file contents?
    /// </summary>
    [TestClass]
    public class Casing_Tests : IniFileTestBase
    {
        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWithKnownContent_When_TheContentIsAccessedWithDifferentSectionCasing_Then_WeGetTheExpectedValue()
        {
            EnsureDefaultContent_UsingFile();
            var sb = DefaultStringBuilder();

            // Insight: section name reading is case insensitive
            var bytes = GetIniString_SB_Unicode(sectionname.ToLower(), keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWithKnownContent_When_TheContentIsAccessedWithDifferentKeyCasing_Then_WeGetTheExpectedValue()
        {
            EnsureDefaultContent_UsingFile();
            var sb = DefaultStringBuilder();

            // Insight: key reading is case insensitive
            var bytes = GetIniString_SB_Unicode(sectionname, keyname.ToLower(), null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }
    }
}