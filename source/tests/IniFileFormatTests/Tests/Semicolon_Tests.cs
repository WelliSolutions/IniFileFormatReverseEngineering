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
        [TestMethod]
        public void
            Given_AnIniFileWrittenWithSemicolonAtBeginOfKey_When_TheContentIsAccessed_Then_WeGetTheDefaultValue()
        {
            EnsureDeleted();
            WritePrivateProfileString(sectionname, ";key", inivalue, FileName);

            // Insight: the comment is written to the file
            Assert.AreEqual($"[{sectionname}]\r\n;key={inivalue}\r\n", File.ReadAllText(FileName));

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
        [TestsApiParameter("lpKeyName")]
        [TestMethod]
        public void Given_AnIniFileWithASemicolonAtBeginOfKey_When_AllKeysAreRetrieved_Then_WeDontGetTheComment()
        {
            EnsureDeleted();
            WritePrivateProfileString(sectionname, ";key", inivalue, FileName);
            WritePrivateProfileString(sectionname, keyname, inivalue, FileName);

            var buffer = new char[1000];
            var bytes = GetIniString_ChArr_Unicode(sectionname, null, defaultvalue, buffer, (uint)buffer.Length,
                FileName);
            AssertASCIILength(keyname + '\0', bytes);
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpString")]
        [TestMethod]
        public void Given_AnIniFileWrittenWithSemicolonInValue_When_TheContentIsAccessed_Then_WeGetTheSemicolon()
        {
            EnsureDeleted();
            WritePrivateProfileString(sectionname, keyname, ";nocomment", FileName);

            // Insight: Semicolon in value is not a comment
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(";nocomment", bytes);
            AssertSbEqual(";nocomment", sb);
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpKeyName")]
        [TestMethod]
        public void Given_AnIniFileWrittenWithSemicolonInKey_When_TheContentIsAccessed_Then_WeGetTheSemicolon()
        {
            EnsureDeleted();
            WritePrivateProfileString(sectionname, "key ;nocomment", inivalue, FileName);

            // Insight: Semicolon in key is not a comment
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, "key ;nocomment", null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpAppName")]
        [TestMethod]
        public void Given_AnIniFileWrittenWithSemicolonInSection_When_TheContentIsAccessed_Then_WeGetTheSemicolon()
        {
            EnsureDeleted();
            WritePrivateProfileString(";section", keyname, inivalue, FileName);

            // Insight: Semicolon in key is not a comment
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(";section", keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);

            // Insight: the semicolon is written inside the square brackets
            Assert.AreEqual($"[;section]\r\n{keyname}={inivalue}\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation]
        [TestMethod]
        public void Given_AnIniFile_When_ACommentIsWrittenViaTheValueAndAnEmptyKey_Then_ItsNotAComment()
        {
            EnsureDeleted();
            WritePrivateProfileString(sectionname, "", ";comment", FileName);

            // Insight: it's not a comment
            Assert.AreEqual($"[{sectionname}]\r\n=;comment\r\n", File.ReadAllText(FileName));

            // Insight: it can still be accessed
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, "", null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(";comment", bytes);
            AssertSbEqual(";comment", sb);
        }

        [TestMethod]
        public void Given_AnIniFile_When_ACommentIsWrittenTwice_Then_TheFileContainsBoth()
        {
            EnsureDeleted();
            WritePrivateProfileString(sectionname, ";key", inivalue, FileName);
            WritePrivateProfileString(sectionname, ";key", inivalue, FileName);

            // Insight: the comment is written twice. There's no magic to detect identical comments
            Assert.AreEqual($"[{sectionname}]\r\n;key={inivalue}\r\n;key={inivalue}\r\n", File.ReadAllText(FileName));
        }

        [TestMethod]
        public void Given_AnIniFile_When_ATwoCommentsAreWritten_Then_TheLatterOneIsFirst()
        {
            EnsureEmptyASCII();
            WritePrivateProfileString(sectionname, "z", "", FileName);
            WritePrivateProfileString(sectionname, ";y", "", FileName);
            WritePrivateProfileString(sectionname, "a", "", FileName);
            WritePrivateProfileString(sectionname, ";b", "", FileName);
            WritePrivateProfileString(sectionname, ";c", "", FileName);

            // Insight: comments are moved to the end of the section
            // Insight: the comments are in reverse order
            Assert.AreEqual($"[{sectionname}]\r\nz=\r\na=\r\n;c=\r\n;b=\r\n;y=\r\n", File.ReadAllText(FileName));
        }

        [TestMethod]
        public void Given_AnIniFileWithExistingComments_When_Writing_Then_TheyAreKeptInOrder()
        {
            EnsureASCII($";comment0\r\n[{sectionname}]\r\n;comment1\r\nb=value\r\n;comment2\r\na=value\r\n");
            WritePrivateProfileString(sectionname, "z", "", FileName);
            WritePrivateProfileString(sectionname, ";x", "", FileName);
            WritePrivateProfileString(sectionname, "y", "", FileName);
            WritePrivateProfileString(sectionname, "a", "", FileName);
            WritePrivateProfileString(sectionname, "b", "", FileName);

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
        [TestsApiParameter("lpKeyName", null)]
        [TestMethod]
        public void Given_AnIniFileWithExistingComments_When_DeletingASection_Then_TheyAreNotDeleted()
        {
            EnsureASCII($";comment0\r\n[{sectionname}]\r\n;comment1\r\n[{sectionname2}]\r\n;comment2\r\n");
            WritePrivateProfileString(sectionname, null, "", FileName); // Delete section
            WritePrivateProfileString(sectionname2, null, "", FileName); // Delete section

            // Insight: comments are not deleted at all
            // Insight: the comments will still be in order
            Assert.AreEqual($";comment0\r\n;comment1\r\n;comment2\r\n", File.ReadAllText(FileName));
        }
    }
}