using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.SpecialCharacters
{
    /// <summary>
    /// Checking the characters which have special meaning in INI files.
    /// This time: hashtags / number sign (for comments)
    /// Because: https://en.wikipedia.org/wiki/INI_file#Comments_2
    /// </summary>
    [TestClass]
    public class Hashtag_Tests : IniFileTestBase
    {
        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(FileContent.lpString, "#")]
        [TestMethod]
        public void Given_AnIniFileWrittenWithHashtagInValue_When_TheContentIsAccessed_Then_WeGetTheHashtag()
        {
            EnsureDeleted();
            WritePrivateProfileStringW(sectionname, keyname, "#nocomment", FileName);

            // Insight: Hashtag in value is not a comment
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength("#nocomment", bytes);
            AssertSbEqual("#nocomment", sb);
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(FileContent.lpString, "#")]
        [TestMethod]
        public void Given_AnIniFileWrittenWithHashtagInKey_When_TheContentIsAccessed_Then_WeGetTheHashtag()
        {
            EnsureDeleted();
            WritePrivateProfileStringW(sectionname, "#comment", defaultvalue, FileName);

            // Insight: Hashtag in key is not a comment
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, "#comment", null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }
    }
}