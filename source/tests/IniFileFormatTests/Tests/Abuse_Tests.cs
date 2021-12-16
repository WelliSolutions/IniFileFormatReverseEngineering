using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests
{
    /// <summary>
    /// Here we have tests where we deliberately try to break stuff.
    /// </summary>
    [TestClass]
    public class Abuse_Tests : IniFileTestBase
    {
        // TODO: Review
        [TestMethod]
        public void Given_ASectionNameWithAnotherSectionName_When_TheContentIsAccessed_Then_TheSecondSectionIsIgnored()
        {
            EnsureDeleted();
            var twosections = "sec][sec2]";
            WritePrivateProfileString(twosections, keyname, defaultvalue, FileName);
            Assert.AreEqual("[" + twosections + "]\r\n" + keyname + "=" + defaultvalue + "\r\n", ReadIniFile());

            var sb = DefaultStringBuilder();

            // Insight: A section must begin on a new line. It can't begin behind an existing section
            var bytes = GetIniString_SB_Unicode("sec", keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);

            // Insight: Accessing by the originally use section name is not possible
            bytes = GetIniString_SB_Unicode(twosections, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertZero(bytes);
            AssertSbEqual("", sb);

            // Insight: Accessing by any hidden name is not possible
            bytes = GetIniString_SB_Unicode("sec2", keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertZero(bytes);
            AssertSbEqual("", sb);
            bytes = GetIniString_SB_Unicode("sec2]", keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertZero(bytes);
            AssertSbEqual("", sb);

        }

        // TODO: Review
        [TestMethod]
        public void Given_ASectionNameWithNewlineAndAnotherSectionName_When_TheContentIsAccessed_Then_TheSecondSectionIsIgnored()
        {
            EnsureDeleted();
            var twosections = "sec]\r\n[sec2]";
            WritePrivateProfileString(twosections, keyname, defaultvalue, FileName);
            // Insight: newlines will not be stripped when writing
            Assert.AreEqual("[" + twosections + "]\r\n" + keyname + "=" + defaultvalue + "\r\n", ReadIniFile());

            var sb = DefaultStringBuilder();

            // Insight (confirmed): A section begins on a new line.
            var bytes = GetIniString_SB_Unicode("sec2", keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);

            // The lonely section is empty
            bytes = GetIniString_SB_Unicode("sec", keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertZero(bytes);
            AssertSbEqual("", sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_ASectionNameMakingUpANewKeyValuePair_When_TheFakeKeyIsAccessed_Then_WeGetTheFakeValue()
        {
            EnsureDeleted();
            WritePrivateProfileString("sec]\r\nkey=value", keyname, defaultvalue, FileName);

            var sb = DefaultStringBuilder();

            // Insight: it's possible to write a key/value pair via the section, but it has a ] in the end
            var bytes = GetIniString_SB_Unicode("sec", "key", null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength("value]", bytes);
            AssertSbEqual("value]", sb);

            // Sure enough we're able to work around that ..., but then we have a dangling ] on the next line
            EnsureDeleted();
            WritePrivateProfileString("sec]\r\nkey=value\r\n", keyname, defaultvalue, FileName);
            bytes = GetIniString_SB_Unicode("sec", "key", null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength("value", bytes);
            AssertSbEqual("value", sb);
        }
    }
}