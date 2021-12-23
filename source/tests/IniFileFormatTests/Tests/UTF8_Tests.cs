using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.Encodings
{
    [TestClass]
    public class UTF8_Tests : IniFileTestBase
    {
        [UsedInDocumentation]
        [Checks(Parameter.lpFileName)]
        [TestMethod]
        public void Given_AFileWithUTF8BOM_When_WritingToTheFile_Then_WeGetReplacementCharacters()
        {
            File.WriteAllText(FileName, "", Encoding.UTF8);
            var unicodeCharacters = "§€°´²³";
            var result = WritePrivateProfileStringW(unicodeCharacters, keyname, inivalue, FileName);

            // Insight: UTF-8 does not help
            string unicodeContent = File.ReadAllText(FileName);
            Assert.AreEqual($"\r\n[������]\r\n{keyname}={inivalue}\r\n", unicodeContent);

            Assert.IsTrue(result);
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)WindowsAPI.GetLastError.SUCCESS, error);

            // Same for the ...A() method
            result = WritePrivateProfileStringA(unicodeCharacters, keyname, inivalue, FileName);
            unicodeContent = File.ReadAllText(FileName);
            Assert.AreEqual($"\r\n[������]\r\n{keyname}={inivalue}\r\n", unicodeContent);
            Assert.IsTrue(result);
            error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)WindowsAPI.GetLastError.SUCCESS, error);
        }

        [UsedInDocumentation]
        [TestMethod]
        public void Given_AFileWithUTF8BOM_When_ReadingTheContent_Then_TheFirstLineIsBroken()
        {
            EnsureDeleted();
            EnsureDefaultContent_UsingFile(Encoding.UTF8); // With BOM

            var sb = DefaultStringBuilder();

            // Insight: The BOM makes the file unreadable?
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity,
                FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);

            // Insight: not fully, though, just the first section seems to be affected
            bytes = GetIniString_SB_Unicode(sectionname2, keyname2, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue2, bytes);
            AssertSbEqual(inivalue2, sb);
        }
    }
}