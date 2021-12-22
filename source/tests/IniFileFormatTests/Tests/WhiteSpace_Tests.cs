using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.SpecialCharacters
{
    /// <summary>
    /// Checking the characters which have special meaning in INI files.
    /// This time: whitespace (because: humans editing files)
    /// </summary>
    [TestClass]
    public class WhiteSpace_Tests : IniFileTestBase
    {

        [UsedInDocumentation]
        [TestsApiParameter("lpDefault")]
        [TestMethod]
        public void Given_ADefaultValueWithTrailingWhitespace_When_TheDefaultValueIsReturned_Then_OnlySpacesAreStripped()
        {
            foreach (var blank in new[] { "\t", "\v", "\r", "\n" })
            {
                EnsureDefaultContent_UsingAPI();
                var sb = DefaultStringBuilder();
                var defaultValue = $" default{blank}";
                var bytes = GetIniString_SB_Unicode(sectionname, "NonExistingKey", defaultValue, sb, (uint)sb.Capacity,
                    FileName);
                AssertASCIILength(defaultValue, bytes);
                // Insight: According the documentation, trailing blanks are stripped
                // Insight: Leading blanks are not stripped
                AssertSbEqual(defaultValue, sb);
            }
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpReturnedString")]
        [TestMethod]
        public void Given_AnIniFileWithKnownContent_When_TheContentIsAccessed_Then_BlanksAreStripped()
        {
            foreach (var blank in new[] { " ", "\t", "\v" })
            {
                EnsureASCII($"[{sectionname}]\r\n{keyname}={blank}{inivalue}{blank}\r\n");
                var sb = DefaultStringBuilder();

                // Insight: Trailing spaces are stripped
                // Insight: Leading spaces are stripped
                var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
                AssertASCIILength(inivalue, bytes);
                AssertSbEqual(inivalue, sb);
            }
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpAppName")]
        [TestsApiParameter("lpKeyName")]
        [TestMethod]
        public void Given_AnIniFileWrittenWithSpaces_When_TheContentIsWritten_Then_SpacesAreStripped()
        {
            EnsureDeleted();
            var space_sec = "   sec   ";
            var space_key = "  key  ";
            var space_value = " value ";
            WritePrivateProfileString(space_sec, space_key, space_value, FileName);

            // Insight: section and key are written to the INI file without spaces
            // Insight: value is written to the INI file with spaces
            // Insight: the file has a trailing newline
            Assert.AreEqual("[sec]\r\nkey= value \r\n", ReadIniFile());

            var sb = DefaultStringBuilder();

            // Values can be read without spaces (well, they are written without spaces, so this was expected)
            var bytes = GetIniString_SB_Unicode(space_sec.Trim(), space_key.Trim(), null, sb, (uint)sb.Capacity, FileName);
            // Insight: the value is read without spaces
            var expected = space_value.Trim();
            AssertASCIILength(expected, bytes);
            AssertSbEqual(expected, sb);

            // Insight: Values can also be read with spaces
            bytes = GetIniString_SB_Unicode(space_sec, space_key, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(expected, bytes);
            AssertSbEqual(expected, sb);

        }

        // TODO: Review
        [TestMethod]
        public void Given_ASectionNameWithSpacesBeforeTheBracket_When_TheContentIsAccessed_Then_TheSpacesAreIgnored()
        {
            EnsureASCII($"   [{sectionname}]\r\n{keyname}={defaultvalue}\r\n");

            var sb = DefaultStringBuilder();

            // Insight: Spaces before a section are ignored when reading the file
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_ASectionNameWithSpacesWithinTheBracket_When_TheContentIsAccessed_Then_TheSpacesAreIgnored()
        {
            EnsureASCII($"[   {sectionname}]\r\n{keyname}={defaultvalue}\r\n");

            var sb = DefaultStringBuilder();

            // Insight: Spaces within a section are ignored
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);

            bytes = GetIniString_SB_Unicode("   " + sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_ASectionNameWithTabsWithinTheBracket_When_TheContentIsAccessed_Then_TheTabsAreIgnored()
        {
            EnsureASCII($"[\t{sectionname}]\r\n{keyname}={defaultvalue}\r\n");

            var sb = DefaultStringBuilder();

            // Insight: Tabs within a section are ignored when being read
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);

            // Insight: you can't access with a tab in the request
            bytes = GetIniString_SB_Unicode("\t" + sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertZero(bytes);
            AssertSbEqual("", sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_AKeyWithSpacesBeforeAndAfter_When_TheContentIsAccessed_Then_TheKeyCanBeFound()
        {
            EnsureASCII($"[{sectionname}]\r\n  \t {keyname}  ={defaultvalue}\r\n");

            var sb = DefaultStringBuilder();

            // Insight: Spaces and tabs before and after a key are ignored when a file is read
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);

            // Insight: You can't query for a value with tabs
            bytes = GetIniString_SB_Unicode(sectionname, "  \t {keyname}  ", null, sb, (uint)sb.Capacity, FileName);
            AssertZero(bytes);
            AssertSbEqual("", sb);
        }
    }
}