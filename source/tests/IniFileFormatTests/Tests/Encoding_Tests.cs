using System.IO;
using System.Runtime.InteropServices;
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
        public void Given_AUnicodeStringToWrite_When_ItsWrittenToAnAnsiFile_Then_WeGetReplacementCharacters()
        {
            EnsureDeleted();
            var unicode = "Unicode❤︎";
            var result = WritePrivateProfileString(sectionname, keyname, unicode, FileName);
            Assert.IsTrue(result);

            // Insight: question marks have been written
            string unicodeContent = File.ReadAllText(FileName);
            Assert.AreEqual($"[{sectionname}]\r\n{keyname}=Unicode??\r\n", unicodeContent);
        }

        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWithUnicodeContent_When_TheContentIsAccessed_Then_WeGetTheExpectedValue_ByteArrayAnsi_ConvertManually_NoBOM()
        {
            EnsureDeleted();
            File.WriteAllText(FileName, $"[{sectionname}]\r\n{keyname}=Unicode❤︎\r\n"); // UTF-8 without BOM

            // Insight: The parser will simply read bytes. So we can decode the char[] using UTF-8
            var chars = new byte[1024];
            var bytes = GetIniString_ByteArr_Ansi(sectionname, keyname, null, chars, (uint)chars.Length, FileName);
            var x = Encoding.UTF8.GetString(chars, 0, (int)bytes);
            Assert.AreEqual("Unicode❤︎", x);

            // Insight: none of the StringBuilders work for UTF-8 content
            var sb = DefaultStringBuilder();
            bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            Assert.AreEqual("Unicodeâ¤ï¸Ž", sb.ToString());
            bytes = GetIniString_SB_Ansi(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            Assert.AreEqual("Unicodeâ¤ï¸Ž", sb.ToString());
        }
    }
}