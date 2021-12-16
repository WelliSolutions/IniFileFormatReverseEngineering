using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests
{
    /// <summary>
    /// Here we have tests which deal with the fact that INI files were defined in a world
    /// without Unicode etc.
    /// </summary>
    [TestClass]
    public class Encoding_Tests : IniFileTestBase
    {
        // TODO: Review
        [TestMethod]
        public void Given_AFileWithBOM_When_TheContentIsAccessed_Then_TheFirstLineIsBroken()
        {
            EnsureDeleted();
            EnsureDefaultContent_UsingFile(Encoding.UTF8); // With BOM

            var sb = DefaultStringBuilder();

            // Insight: The BOM makes the file unreadable?
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);

            // Insight: not fully, though, just the first section seems to be affected
            bytes = GetIniString_SB_Unicode(sectionname2, keyname2, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue2, bytes);
            AssertSbEqual(inivalue2, sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_AUnicodeStringToWrite_When_TheContentIsAccessed_Then_WeGetReplacementCharacters()
        {
            EnsureDeleted();
            var unicode = "Unicode❤︎";
            var result = WritePrivateProfileString(sectionname, keyname, unicode, FileName);
            Assert.IsTrue(result);

            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            // Insight: question marks have been written
            AssertSbEqual("Unicode??", sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWithUnicodeContent_When_TheContentIsAccessed_Then_WeGetTheExpectedValue_ByteArrayAnsi_ConvertManually_NoBOM()
        {
            EnsureDeleted();
            File.WriteAllText(FileName, $"[{sectionname}]\r\n{keyname}=Unicode❤︎\r\n"); // does not write BOM

            var chars = new byte[1024];
            var bytes = GetIniString_ByteArr_Ansi(sectionname, keyname, null, chars, (uint)chars.Length, FileName);
            var x = Encoding.UTF8.GetString(chars, 0, (int)bytes);
            Assert.AreEqual("Unicode❤︎", x);
        }
    }
}