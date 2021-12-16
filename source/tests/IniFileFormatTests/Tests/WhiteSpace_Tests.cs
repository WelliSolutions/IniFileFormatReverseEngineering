using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IniFileFormatTests.SpecialCharacters
{
    /// <summary>
    /// Checking the characters which have special meaning in INI files.
    /// This time: whitespace (because: humans editing files)
    /// </summary>
    [TestClass]
    public class WhiteSpace_Tests : IniFileTestBase
    {
        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWrittenWithSpaces_When_TheContentIsAccessed_Then_SpacesAreIgnored()
        {
            EnsureDeleted();
            var space_sec = "   sec   ";
            var space_key = "  key  ";
            var space_value = " value ";
            WindowsAPI.WritePrivateProfileString(space_sec, space_key, space_value, FileName);

            // Insight: section and key are written to the INI file without spaces
            // Insight: value is written to the INI file with spaces
            // Insight: the file has a trailing newline
            Assert.AreEqual("[sec]\r\nkey= value \r\n", ReadIniFile());

            var sb = DefaultStringBuilder();

            // Values can be read without spaces (well, they are written without spaces, so this was expected)
            var bytes = WindowsAPI.GetIniString_SB_Unicode(space_sec.Trim(), space_key.Trim(), null, sb, (uint)sb.Capacity, FileName);
            // Insight: the value is read without spaces
            var expected = space_value.Trim();
            AssertionHelper.AssertASCIILength(expected, bytes);
            AssertionHelper.AssertSbEqual(expected, sb);

            // Insight: Values can also be read with spaces
            bytes = WindowsAPI.GetIniString_SB_Unicode(space_sec, space_key, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength(expected, bytes);
            AssertionHelper.AssertSbEqual(expected, sb);

        }

        // TODO: Review
        [TestMethod]
        public void Given_ASectionNameWithSpacesBeforeTheBracket_When_TheContentIsAccessed_Then_TheSpacesAreIgnored()
        {
            EnsureASCII($"   [{sectionname}]\r\n{keyname}={defaultvalue}\r\n");

            var sb = DefaultStringBuilder();

            // Insight: Spaces before a section are ignored when reading the file
            var bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength(defaultvalue, bytes);
            AssertionHelper.AssertSbEqual(defaultvalue, sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_ASectionNameWithSpacesWithinTheBracket_When_TheContentIsAccessed_Then_TheSpacesAreIgnored()
        {
            EnsureASCII($"[   {sectionname}]\r\n{keyname}={defaultvalue}\r\n");

            var sb = DefaultStringBuilder();

            // Insight: Spaces within a section are ignored
            var bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength(defaultvalue, bytes);
            AssertionHelper.AssertSbEqual(defaultvalue, sb);

            bytes = WindowsAPI.GetIniString_SB_Unicode("   " + sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength(defaultvalue, bytes);
            AssertionHelper.AssertSbEqual(defaultvalue, sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_ASectionNameWithTabsWithinTheBracket_When_TheContentIsAccessed_Then_TheTabsAreIgnored()
        {
            EnsureASCII($"[\t{sectionname}]\r\n{keyname}={defaultvalue}\r\n");

            var sb = DefaultStringBuilder();

            // Insight: Tabs within a section are ignored when being read
            var bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength(defaultvalue, bytes);
            AssertionHelper.AssertSbEqual(defaultvalue, sb);

            // Insight: you can't access with a tab in the request
            bytes = WindowsAPI.GetIniString_SB_Unicode("\t" + sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertZero(bytes);
            AssertionHelper.AssertSbEqual("", sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_AKeyWithSpacesBeforeAndAfter_When_TheContentIsAccessed_Then_TheKeyCanBeFound()
        {
            EnsureASCII($"[{sectionname}]\r\n  \t {keyname}  ={defaultvalue}\r\n");

            var sb = DefaultStringBuilder();

            // Insight: Spaces and tabs before and after a key are ignored when a file is read
            var bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertASCIILength(defaultvalue, bytes);
            AssertionHelper.AssertSbEqual(defaultvalue, sb);

            // Insight: You can't query for a value with tabs
            bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, "  \t {keyname}  ", null, sb, (uint)sb.Capacity, FileName);
            AssertionHelper.AssertZero(bytes);
            AssertionHelper.AssertSbEqual("", sb);
        }
    }
}