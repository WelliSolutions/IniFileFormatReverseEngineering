using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.SpecialCharacters
{
    /// <summary>
    /// Checking the characters which have special meaning in INI files.
    /// This time: semicolons (for comments)
    /// </summary>
    [TestClass]
    public class Semicolon_Tests : IniFileTestBase
    {
        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, ";")]
        [Checks(FileContent.lpKeyName, ";")]
        [TestMethod]
        public void
            Given_AnIniFileWithSemicolonAtBeginOfKey_When_TheValueIsRead_Then_WeGetTheDefaultValue()
        {
            EnsureASCII($"[{sectionname}]\r\n;key={inivalue}\r\n");

            // Insight: Semicolon at the beginning of the key is a comment
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, ";key", defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);

            bytes = GetIniString_SB_Unicode(sectionname, "key", defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, ";")]
        [TestMethod]
        public void
            Given_AKeyParameterWithSemicolonAtBeginning_When_TheValueIsWritten_Then_TheFileContainsTheSemicolon()
        {
            EnsureDeleted();
            WritePrivateProfileStringW(sectionname, ";key", inivalue, FileName);

            // Insight: the comment is written to the file
            Assert.AreEqual($"[{sectionname}]\r\n;key={inivalue}\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, ";")]
        [TestMethod]
        public void Given_AnIniFileWithSpacesBeforeTheSemicolon_When_TheContentIsAccessed_Then_ItsStillAComment()
        {
            foreach (var whitespace in new[] { ' ', '\t', '\v' })
            {
                EnsureASCII($"[{sectionname}]\r\n{whitespace};{keyname}={inivalue}\r\n");
                var sb = DefaultStringBuilder();

                // Insight: cannot be accessed by keyname
                var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity,
                    FileName);
                AssertASCIILength(defaultvalue, bytes);
                AssertSbEqual(defaultvalue, sb);

                // Insight: cannot be accessed by semicolon + keyname
                bytes = GetIniString_SB_Unicode(sectionname, ";" + keyname, defaultvalue, sb, (uint)sb.Capacity,
                    FileName);
                AssertASCIILength(defaultvalue, bytes);
                AssertSbEqual(defaultvalue, sb);

                // Insight: cannot be accessed as written in the file
                bytes = GetIniString_SB_Unicode(sectionname, $"{whitespace};" + keyname, defaultvalue, sb,
                    (uint)sb.Capacity,
                    FileName);
                AssertASCIILength(defaultvalue, bytes);
                AssertSbEqual(defaultvalue, sb);
            }
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpKeyName)]
        [TestMethod]
        public void Given_AnIniFileWithASemicolonAtBeginOfKey_When_AllKeysAreRetrieved_Then_WeDontGetTheComment()
        {
            // keyname is a comment, keyname2 isn't
            EnsureASCII($"[{sectionname}]\r\n;{keyname}={inivalue}\r\n{keyname2}={inivalue2}\r\n");

            var buffer = new char[1000];
            var bytes = GetIniString_ChArr_Unicode(sectionname, null, defaultvalue, buffer, (uint)buffer.Length,
                FileName);
            AssertASCIILength(keyname2 + '\0', bytes);
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(FileContent.lpString, ";")]
        [TestMethod]
        public void Given_AnIniFileWrittenWithSemicolonInValue_When_TheValueIsRead_Then_WeGetTheSemicolon()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}=;nocomment\r\n");

            // Insight: Semicolon in value is not a comment
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(";nocomment", bytes);
            AssertSbEqual(";nocomment", sb);
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpString, ";")]
        [TestMethod]
        public void Given_AValueParameterWithSemicolon_When_TheValueIsWritten_Then_TheSemicolonIsPartOfTheFile()
        {
            EnsureDeleted();
            WritePrivateProfileStringW(sectionname, keyname, ";nocomment", FileName);
            Assert.AreEqual($"[{sectionname}]\r\n{keyname}=;nocomment\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, ";")]
        [TestMethod]
        public void Given_AKeyParameterWithSemicolon_When_TheValueIsWritten_Then_TheSemicolonIsPartOfTheFile()
        {
            EnsureDeleted();
            WritePrivateProfileStringW(sectionname, "key ;nocomment", inivalue, FileName);

            // Insight: Semicolon in key is not a comment
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, "key ;nocomment", null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpAppName, ";")]
        [TestMethod]
        public void Given_AnIniFileWrittenWithSemicolonInSection_When_TheContentIsAccessed_Then_WeGetTheSemicolon()
        {
            EnsureDeleted();
            WritePrivateProfileStringW(";section", keyname, inivalue, FileName);

            // Insight: Semicolon in key is not a comment
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(";section", keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);

            // Insight: the semicolon is written inside the square brackets
            Assert.AreEqual($"[;section]\r\n{keyname}={inivalue}\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, "")]
        [Checks(Parameter.lpString, ";")]
        [TestMethod]
        public void Given_AnIniFile_When_ACommentIsWrittenViaTheValueAndAnEmptyKey_Then_ItsNotAComment()
        {
            EnsureDeleted();
            WritePrivateProfileStringW(sectionname, "", ";comment", FileName);

            // Insight: it's written to the file
            Assert.AreEqual($"[{sectionname}]\r\n=;comment\r\n", File.ReadAllText(FileName));

            // Insight: it can still be accessed
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, "", null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(";comment", bytes);
            AssertSbEqual(";comment", sb);
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, ";")]
        [TestMethod]
        public void Given_AnIniFile_When_ACommentIsWrittenTwice_Then_TheFileContainsBoth()
        {
            EnsureDeleted();
            WritePrivateProfileStringW(sectionname, ";key", inivalue, FileName);
            WritePrivateProfileStringW(sectionname, ";key", inivalue, FileName);

            // Insight: the comment is written twice. There's no magic to detect identical comments
            Assert.AreEqual($"[{sectionname}]\r\n;key={inivalue}\r\n;key={inivalue}\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, ";")]
        [TestMethod]
        public void Given_AnIniFile_When_ATwoCommentsAreWritten_Then_TheLatterOneIsFirst()
        {
            EnsureEmptyASCII();
            WritePrivateProfileStringW(sectionname, "z", "", FileName);
            WritePrivateProfileStringW(sectionname, ";y", "", FileName);
            WritePrivateProfileStringW(sectionname, "a", "", FileName);
            WritePrivateProfileStringW(sectionname, ";b", "", FileName);
            WritePrivateProfileStringW(sectionname, ";c", "", FileName);

            // Insight: comments are moved to the end of the section
            // Insight: the comments are in reverse order
            Assert.AreEqual($"[{sectionname}]\r\nz=\r\na=\r\n;c=\r\n;b=\r\n;y=\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, ";")]
        [Checks(FileContent.lpKeyName, ";")]
        [TestMethod]
        public void Given_AnIniFileWithExistingComments_When_Writing_Then_TheyAreKeptInOrder()
        {
            EnsureASCII($";comment0\r\n[{sectionname}]\r\n;comment1\r\nb=value\r\n;comment2\r\na=value\r\n");
            WritePrivateProfileStringW(sectionname, "z", "", FileName);
            WritePrivateProfileStringW(sectionname, ";x", "", FileName);
            WritePrivateProfileStringW(sectionname, "y", "", FileName);
            WritePrivateProfileStringW(sectionname, "a", "", FileName);
            WritePrivateProfileStringW(sectionname, "b", "", FileName);

            // Insight: like values, comment that already exist are kept in their order
            // Insight: New values are written in chronological order after the existing values (not alphabetically)
            // Insight: new comments are moved to the end of the section

            var expected = $";comment0\r\n[{sectionname}]\r\n;comment1\r\n";
            expected += "b=\r\n"; // b will stay here, although a is written later
            expected += ";comment2\r\n"; // comment will stay in place
            expected += "a=\r\n"; // a will stay here
            expected += "z=\r\n"; // z being written first
            expected += "y=\r\n";  // y is the last non-existing value written
            expected += ";x=\r\n"; // new comments go to the end
            Assert.AreEqual(expected, File.ReadAllText(FileName));
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, null)]
        [TestMethod]
        public void Given_AnIniFileWithExistingComments_When_DeletingASection_Then_TheyAreNotDeleted()
        {
            EnsureASCII($";comment0\r\n[{sectionname}]\r\n;comment1\r\n[{sectionname2}]\r\n;comment2\r\n");
            WritePrivateProfileStringW(sectionname, null, "", FileName); // Delete section
            WritePrivateProfileStringW(sectionname2, null, "", FileName); // Delete section

            // Insight: comments are not deleted at all
            // Insight: the comments will still be in order
            Assert.AreEqual($";comment0\r\n;comment1\r\n;comment2\r\n", File.ReadAllText(FileName));
        }
    }
}