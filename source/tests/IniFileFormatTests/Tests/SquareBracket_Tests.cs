using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IniFileFormatTests.SpecialCharacters
{
    /// <summary>
    /// Checking the characters which have special meaning in INI files.
    /// This time: square brackets (from sections)
    /// </summary>
    [TestClass]
    public class SquareBracket_Tests : IniFileTestBase
    {
        // TODO: Review
        [TestMethod]
        public void Given_ASectionNameWithOpeningBracket_When_TheContentIsAccessed_Then_WeGetTheExpectedValue()
        {
            EnsureDeleted();
            var square = "[sec";
            WindowsAPI.WritePrivateProfileString(square, keyname, defaultvalue, FileName);
            // Insight: the section name is written as given, so special handling for opening square bracket
            Assert.AreEqual("[" + square + "]\r\n" + keyname + "=" + defaultvalue + "\r\n", ReadIniFile());

            var sb = DefaultStringBuilder();

            // Insight: the section name can be accessed again.
            // Parsing of the section name does not seem to restart at the [ opening square bracket.
            var bytes = WindowsAPI.GetIniString_SB_Unicode(square, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength(defaultvalue, bytes);
            AssertionHelper.AssertSbEqual(defaultvalue, sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_ASectionNameWithClosingBracket_When_TheContentIsAccessed_Then_WeGetTheExpectedValue()
        {
            EnsureDeleted();
            var whoops = "sec]whoops";
            WindowsAPI.WritePrivateProfileString(whoops, keyname, defaultvalue, FileName);
            // Insight: the section name is written as given, so special handling for the ] closing square bracket
            Assert.AreEqual("[" + whoops + "]\r\n" + keyname + "=" + defaultvalue + "\r\n", ReadIniFile());

            var sb = DefaultStringBuilder();

            // Insight: read parsing stops at first ]
            var bytes = WindowsAPI.GetIniString_SB_Unicode("sec", keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength(defaultvalue, bytes);
            AssertionHelper.AssertSbEqual(defaultvalue, sb);

            // Insight: it's not possible to read the section by passing the original string
            bytes = WindowsAPI.GetIniString_SB_Unicode(whoops, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertZero(bytes);
            AssertionHelper.AssertSbEqual("", sb);
        }

        // TODO: tests for dangling square brackets
    }
}