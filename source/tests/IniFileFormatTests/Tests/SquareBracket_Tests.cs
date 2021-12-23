using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.SpecialCharacters
{
    /// <summary>
    /// Checking the characters which have special meaning in INI files.
    /// This time: square brackets (from sections)
    /// </summary>
    [TestClass]
    public class SquareBracket_Tests : IniFileTestBase
    {
        [UsedInDocumentation]
        [Checks(Parameter.lpAppName)]
        [TestMethod]
        public void Given_ASectionNameWithOpeningBracket_When_TheValueIsAccessed_Then_WeGetTheExpectedValue()
        {
            EnsureDeleted();
            var square = "[sec";
            WritePrivateProfileStringW(square, keyname, inivalue, FileName);

            // Insight: the section name is written as given, no special handling for opening square bracket
            Assert.AreEqual("[" + square + "]\r\n" + keyname + "=" + inivalue + "\r\n", ReadIniFile());

            var sb = DefaultStringBuilder();

            // Insight: the section name can be accessed again.
            // Parsing of the section name does not seem to restart at the [ opening square bracket.
            var bytes = GetIniString_SB_Unicode(square, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation]
        [Checks(Parameter.lpAppName)]
        [TestMethod]
        public void Given_ASectionNameWithClosingBracket_When_TheContentIsAccessed_Then_WeDontGetTheValue()
        {
            EnsureDeleted();
            var whoops = "sec]whoops";
            WritePrivateProfileStringW(whoops, keyname, inivalue, FileName);

            // Insight: the section name is written as given, no special handling for the ] closing square bracket
            Assert.AreEqual("[" + whoops + "]\r\n" + keyname + "=" + inivalue + "\r\n", ReadIniFile());

            var sb = DefaultStringBuilder();

            // Insight: it's not possible to read the section by passing the original string
            var bytes = GetIniString_SB_Unicode(whoops, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);

            // Insight: read parsing stops at first ]
            bytes = GetIniString_SB_Unicode("sec", keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation]
        [TestMethod]
        public void Given_KeyValueBehindClosingSquareBracket_When_WeTryToAccessTheValue_Then_WeDontGetTheValue()
        {
            EnsureASCII($"[{sectionname}]{keyname}={inivalue}\r\n"); // Important: no newline after ]

            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);

            // Insight: text after the closing ] of the section is ignored
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }

        [UsedInDocumentation]
        [TestMethod]
        public void Given_AValueWithoutAnySection_When_WeTryToAccessIt_Then_WeDontGetTheValue()
        {
            EnsureASCII($"{keyname}={inivalue}\r\n[{sectionname}]\r\n{keyname2}={inivalue2}\r\n");
            var sb = DefaultStringBuilder();

            // Insight: values outside a section cannot be accessed with an empty string
            var bytes = GetIniString_SB_Unicode("", keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);

            foreach (var empty in new[] { ' ', '\0', '\r', '\n', '\v' })
            {
                // Insight: values outside a section cannot be accessed by any whitespace character
                bytes = GetIniString_SB_Unicode(empty.ToString(), keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
                AssertASCIILength(defaultvalue, bytes);
                AssertSbEqual(defaultvalue, sb);
            }
        }

        [UsedInDocumentation]
        [TestMethod]
        public void Given_ASectionNameWithMissingClosingBracket_When_WeAccessAKey_Then_WeGetTheValue()
        {
            EnsureASCII($"[{sectionname}   \r\n{keyname}={inivalue}\r\n");
            var sb = DefaultStringBuilder();

            // Insight: the closing square bracket is not needed
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation]
        [TestMethod]
        public void Given_ASectionNameWithMissingOpeningBracket_When_WeAccessAKey_Then_WeDontGetTheValue()
        {
            EnsureASCII($"{sectionname}]\r\n{keyname}={inivalue}\r\n");
            var sb = DefaultStringBuilder();

            // Insight: the opening square bracket is needed
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }
    }
}