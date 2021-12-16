using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWrittenWithHashtagCommentInValueOnly_When_TheContentIsAccessed_Then_WeGetTheFullValue()
        {
            EnsureDeleted();
            WindowsAPI.WritePrivateProfileString(sectionname, keyname, "value#nocomment", FileName);

            // Insight: Hashtag in value is not a comment
            var sb = DefaultStringBuilder();
            var bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength("value#nocomment", bytes);
            AssertionHelper.AssertSbEqual("value#nocomment", sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWrittenWithHashtagCommentInKeyOnly_When_TheContentIsAccessed_Then_WeGetTheFullValue()
        {
            EnsureDeleted();
            WindowsAPI.WritePrivateProfileString(sectionname, "key#comment", defaultvalue, FileName);

            // Insight: Hashtag in key is not a comment
            var sb = DefaultStringBuilder();
            var bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, "key#comment", null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength(defaultvalue, bytes);
            AssertionHelper.AssertSbEqual(defaultvalue, sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWrittenWithHashtagCommentAtBeginOfKey_When_TheContentIsAccessed_Then_WeGetTheFullValue()
        {
            EnsureDeleted();
            WindowsAPI.WritePrivateProfileString(sectionname, "#key", defaultvalue, FileName);

            // Insight: Hashtag at the beginning of a key is not a comment
            var sb = DefaultStringBuilder();
            var bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, "#key", null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength(defaultvalue, bytes);
            AssertionHelper.AssertSbEqual(defaultvalue, sb);
        }
    }
}