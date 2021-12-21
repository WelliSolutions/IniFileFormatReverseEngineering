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
        [DoNotRename("Used in documentation")]
        [TestMethod]
        public void Given_AnIniFileWrittenWithSemicolonAtBeginOfKey_When_TheContentIsAccessed_Then_WeGetTheDefaultValue()
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

        [DoNotRename("Used in documentation")]
        [TestMethod]
        public void Given_AnIniFileWithASemicolonAtBeginOfKey_When_AllKeysAreRetrieved_Then_WeDontGetTheComment()
        {
            EnsureDeleted();
            WritePrivateProfileString(sectionname, ";key", inivalue, FileName);
            WritePrivateProfileString(sectionname, keyname, inivalue, FileName);

            var buffer = new char[1000];
            var bytes = GetIniString_ChArr_Unicode(sectionname, null, defaultvalue, buffer, (uint)buffer.Length, FileName);
            AssertASCIILength(keyname + '\0', bytes);
        }

        [DoNotRename("Used in documentation")]
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

        [DoNotRename("Used in documentation")]
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

        [DoNotRename("Used in documentation")]
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
        }
    }
}