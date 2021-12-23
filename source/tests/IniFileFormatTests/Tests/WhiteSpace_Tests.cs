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
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpDefault, " ")]
        [TestMethod]
        public void Given_ADefaultValueWithSpaces_When_TheDefaultValueIsReturned_Then_TrailingSpacesAreStripped()
        {
            EnsureDefaultContent_UsingAPI();
            var sb = DefaultStringBuilder();
            var defaultValue = "   default   ";
            var bytes = GetIniString_SB_Unicode(sectionname, "NonExistingKey", defaultValue, sb, (uint)sb.Capacity,
                FileName);

            // Insight: Trailing spaces are stripped
            // Insight: Leading spaces are not stripped
            AssertASCIILength(defaultValue.TrimEnd(), bytes);
            AssertSbEqual(defaultValue.TrimEnd(), sb);
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpDefault, "\t\v\r\n")]
        [TestMethod]
        public void Given_ADefaultValueWithTrailingWhitespace_When_TheDefaultValueIsReturned_Then_OnlySpacesAreStripped()
        {
            foreach (var blank in new[] { "\t", "\v", "\r", "\n" })
            {
                EnsureDefaultContent_UsingAPI();
                var sb = DefaultStringBuilder();
                var defaultValue = $"{blank}default{blank}";
                var bytes = GetIniString_SB_Unicode(sectionname, "NonExistingKey", defaultValue, sb, (uint)sb.Capacity,
                    FileName);
                AssertASCIILength(defaultValue, bytes);
                // Insight: tab, vertical tab, carriage return and line feed are not stripped
                // Insight: Leading blanks are not stripped
                AssertSbEqual(defaultValue, sb);
            }
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(FileContent.lpString, " \t\v")]
        [TestMethod]
        public void Given_AValueWithWhitespace_When_TheValueIsRead_Then_WhitespaceIsStripped()
        {
            foreach (var blank in new[] { " ", "\t", "\v" })
            {
                EnsureASCII($"[{sectionname}]\r\n{keyname}={blank}{inivalue}{blank}\r\n");
                var sb = DefaultStringBuilder();

                // Insight: Trailing spaces, tabs and vertical tabs are stripped
                // Insight: Leading spaces, tabs and vertical tabs are stripped
                var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
                AssertASCIILength(inivalue, bytes);
                AssertSbEqual(inivalue, sb);
            }
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpString, " \t\v\r\n")]
        [TestMethod]
        public void Given_AValueParameterWithWhitespaces_When_TheValueIsWritten_Then_NothingIsStripped()
        {
            foreach (var blank in new[] { " ", "\t", "\v", "\r", "\n" })
            {
                EnsureDeleted();
                var space_value = $"{blank}value{blank}";
                WritePrivateProfileStringW(sectionname, keyname, space_value, FileName);

                // Insight: value is written to the INI file with whitespaces
                Assert.AreEqual($"[{sectionname}]\r\n{keyname}={space_value}\r\n", ReadIniFile());
            }
        }

        /// <summary>
        /// Whitespaces can be escaped using quotation marks. This does not happen when writing a value.
        /// <see cref="Quotes_Tests"/>
        /// </summary>
        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpString, " \t\v\r\n")]
        [TestMethod]
        public void Given_AValueWithWhitespaces_When_TheValueIsWritten_Then_NothingIsEscaped()
        {
            foreach (var blank in new[] { " ", "\t", "\v", "\r", "\n" })
            {
                EnsureDeleted();
                var space_value = $"{blank}value{blank}";
                WritePrivateProfileStringW(sectionname, keyname, space_value, FileName);

                // Insight: value is written to the INI file with whitespaces but they are not escaped
                Assert.AreEqual($"[{sectionname}]\r\n{keyname}={space_value}\r\n", ReadIniFile());
            }
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpAppName, " ")]
        [TestMethod]
        public void Given_ASectionParameterWithSpaces_When_TheSectionIsWritten_Then_SpacesAreStripped()
        {
            foreach (var blank in new[] { " " })
            {
                EnsureDeleted();
                var withWhitespace = $"{blank}sec{blank}";
                WritePrivateProfileStringW(withWhitespace, keyname, inivalue, FileName);

                // Insight: section and key are written to the INI file without spaces
                // Insight: value is written to the INI file with spaces
                // Insight: the file has a trailing newline
                Assert.AreEqual($"[sec]\r\n{keyname}={inivalue}\r\n", ReadIniFile());

                var sb = DefaultStringBuilder();

                // Values can be read without spaces (well, they are written without spaces, so this was expected)
                var bytes = GetIniString_SB_Unicode(withWhitespace.Trim(), keyname, defaultvalue, sb, (uint)sb.Capacity,
                    FileName);
                AssertASCIILength(inivalue, bytes);
                AssertSbEqual(inivalue, sb);
            }
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpAppName, "\t\r\n\v")]
        [TestMethod]
        public void Given_ASectionParameterWithWhitespaces_When_TheSectionIsWritten_Then_OnlySpacesAreStripped()
        {
            foreach (var blank in new[] { "\t", "\r", "\v", "\n" })
            {
                EnsureDeleted();
                var withWhitespace = $"{blank}sec{blank}";
                WritePrivateProfileStringW(withWhitespace, keyname, inivalue, FileName);

                // Insight: section and key are written to the INI file without spaces
                // Insight: value is written to the INI file with spaces
                // Insight: the file has a trailing newline
                Assert.AreEqual($"[{withWhitespace}]\r\n{keyname}={inivalue}\r\n", ReadIniFile());
            }
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, " ")]
        [TestMethod]
        public void Given_AKeyParameterWithSpaces_When_TheValueIsWritten_Then_SpacesAreStripped()
        {
            foreach (var blank in new[] { " " })
            {
                EnsureDeleted();
                var space_key = $"{blank}key{blank}";
                WritePrivateProfileStringW(sectionname, space_key, inivalue, FileName);

                // Insight: section and key are written to the INI file without spaces
                // Insight: value is written to the INI file with spaces
                // Insight: the file has a trailing newline
                Assert.AreEqual($"[{sectionname}]\r\nkey={inivalue}\r\n", ReadIniFile());

                var sb = DefaultStringBuilder();

                // Values can be read without spaces (well, they are written without spaces, so this was expected)
                var bytes = GetIniString_SB_Unicode(sectionname, space_key.Trim(), defaultvalue, sb, (uint)sb.Capacity,
                    FileName);
                // Insight: the value is read without spaces
                AssertASCIILength(inivalue, bytes);
                AssertSbEqual(inivalue, sb);
            }
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, "\t\v\r\n")]
        [TestMethod]
        public void Given_AKeyParameterWithWhitespaces_When_TheValueIsWritten_Then_OnlySpacesAreStripped()
        {
            foreach (var blank in new[] { "\t", "\v", "\r", "\n" })
            {
                EnsureDeleted();
                var space_key = $"{blank}key{blank}";
                WritePrivateProfileStringW(sectionname, space_key, inivalue, FileName);

                // Insight: section and key are written to the INI file without spaces
                // Insight: value is written to the INI file with spaces
                // Insight: the file has a trailing newline
                Assert.AreEqual($"[{sectionname}]\r\n{blank}key{blank}={inivalue}\r\n", ReadIniFile());
            }
        }


        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(FileContent.lpAppName, " \t\v\r\n")]
        [TestMethod]
        public void Given_ASectionNameWithWhitespacesBeforeTheBracket_When_TheValueIsRead_Then_TheWhitespaceIsIgnored()
        {
            foreach (var blank in new[] { " ", "\t", "\v", "\r", "\n" })
            {
                EnsureASCII($"{blank}[{sectionname}]\r\n{keyname}={inivalue}\r\n");

                var sb = DefaultStringBuilder();

                // Insight: Spaces before a section are ignored when reading the file
                var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
                AssertASCIILength(inivalue, bytes);
                AssertSbEqual(inivalue, sb);
            }
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(FileContent.lpAppName, " \t\v")]
        [TestMethod]
        public void Given_ASectionNameWithWhitespacesWithinTheBracket_When_TheValueIsRead_Then_TheWhitespaceIsIgnored()
        {
            foreach (var blank in new[] { " ", "\t", "\v" })
            {
                EnsureASCII($"[{blank}x{blank}]\r\n{keyname}={inivalue}\r\n");

                var sb = DefaultStringBuilder();

                // Insight: Whitespace within a section are ignored
                var bytes = GetIniString_SB_Unicode("x", keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
                AssertASCIILength(inivalue, bytes);
                AssertSbEqual(inivalue, sb);
            }
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpAppName, " ")]
        [TestMethod]
        public void Given_ASectionParameterWithSpace_When_TheValueIsRead_Then_TheSpacesAreStripped()
        {
            foreach (var blank in new[] { " " })
            {
                EnsureDefaultContent_UsingFile();
                var sb = DefaultStringBuilder();
                // Insight: Spaces (only) in the parameter are ignored
                var bytes = GetIniString_SB_Unicode(blank + sectionname + blank, keyname, defaultvalue, sb, (uint)sb.Capacity,
                    FileName);
                AssertASCIILength(inivalue, bytes);
                AssertSbEqual(inivalue, sb);
            }
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpAppName, "\t\v")]
        [TestMethod]
        public void Given_ASectionParameterWithTabs_When_TheValueIsRead_Then_TheTabsAreNotStripped()
        {
            foreach (var blank in new[] { "\t", "\v" })
            {
                EnsureDefaultContent_UsingFile();
                var sb = DefaultStringBuilder();
                // Insight: you can't access with a tab in the request
                var bytes = GetIniString_SB_Unicode(blank + sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity,
                    FileName);
                AssertASCIILength(defaultvalue, bytes);
                AssertSbEqual(defaultvalue, sb);
            }
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(FileContent.lpKeyName, " \t\v")]
        [TestMethod]
        public void Given_AKeyWithSpacesBeforeAndAfter_When_TheValueIsRead_Then_TheWhitespacesAreStripped()
        {
            foreach (var blank in new[] { " ", "\t", "\v" })
            {
                EnsureASCII($"[{sectionname}]\r\n{blank}{keyname}{blank}={inivalue}\r\n");

                var sb = DefaultStringBuilder();

                // Insight: Spaces and tabs before and after a key are ignored when a file is read
                var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity,
                    FileName);
                AssertASCIILength(inivalue, bytes);
                AssertSbEqual(inivalue, sb);
            }
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, "\t\v\r\n")]
        [TestMethod]
        public void Given_AKeyParameterWithWhitespaces_When_TheValueIsRead_Then_TheKeyCannotBeFound()
        {
            foreach (var blank in new[] { "\t", "\v", "\r", "\n" })
            {
                EnsureDefaultContent_UsingFile();
                var sb = DefaultStringBuilder();

                // Insight: You can't query for a value with these whitespaces
                var bytes = GetIniString_SB_Unicode(sectionname, $"{keyname}{blank}", defaultvalue, sb,
                    (uint)sb.Capacity, FileName);
                AssertASCIILength(defaultvalue, bytes);
                AssertSbEqual(defaultvalue, sb);
            }
        }
    }
}