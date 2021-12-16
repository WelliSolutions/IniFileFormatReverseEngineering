using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IniFileFormatTests.SpecialCharacters
{
    /// <summary>
    /// Checking the characters which have special meaning in INI files.
    /// This time: semicolons (for comments)
    /// </summary>
    [TestClass]
    public class Semicolon_Tests : IniFileTestBase
    {
        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWrittenWithSemicolonCommentInValueOnly_When_TheContentIsAccessed_Then_WeGetTheFullValue()
        {
            EnsureDeleted();
            WindowsAPI.WritePrivateProfileString(sectionname, keyname, "value ;nocomment", FileName);
            // Insight: Semicolon in value is not a comment

            var sb = DefaultStringBuilder();
            var bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength("value ;nocomment", bytes);
            AssertionHelper.AssertSbEqual("value ;nocomment", sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWrittenWithSemicolonCommentInKeyOnly_When_TheContentIsAccessed_Then_WeGetTheFullValue()
        {
            EnsureDeleted();
            WindowsAPI.WritePrivateProfileString(sectionname, "key ;comment", defaultvalue, FileName);
            // Insight: Semicolon in key is not a comment

            var sb = DefaultStringBuilder();
            var bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, "key ;comment", null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength(defaultvalue, bytes);
            AssertionHelper.AssertSbEqual(defaultvalue, sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWrittenWithSemicolonCommentAtBeginOfKey_When_TheContentIsAccessed_Then_WeGetTheDefaultValue()
        {
            EnsureDeleted();
            WindowsAPI.WritePrivateProfileString(sectionname, ";key", defaultvalue, FileName);
            // Insight: SemiColon at the beginning of the key is a comment

            var sb = DefaultStringBuilder();
            var bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, ";key", null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertZero(bytes);
            AssertionHelper.AssertSbEqual(string.Empty, sb);
        }
    }
}