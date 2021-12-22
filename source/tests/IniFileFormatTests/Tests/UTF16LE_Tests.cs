using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.Encodings
{
    [TestClass]
    public class UTF16LE_Tests : IniFileTestBase
    {
        [UsedInDocumentation]
        [TestMethod]
        public void Given_AFileWithUTF16BOM_When_ReadingTheContent_Then_WeHaveUnicodeSupport()
        {
            EnsureDeleted();
            EnsureDefaultContent_UsingFile(Encoding.Unicode); // UTF16-LE BOM

            var sb = DefaultStringBuilder();

            // Insight: A UTF-16 BOM is ok
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity,
                FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpFileName")]
        [TestMethod]
        public void Given_AFileWithUTF16Header_When_WritingToTheFile_Then_WeHaveUnicodeSupport()
        {
            EnsureEmptyUTF16();
            var unicodeCharacters = "§€°´²³";
            var result = WritePrivateProfileString(unicodeCharacters, keyname, inivalue, FileName);

            // Insight: Unicode characters are written when the file has a UTF16 BOM (...W() method)
            string unicodeContent = File.ReadAllText(FileName);
            Assert.AreEqual($"\r\n[{unicodeCharacters}]\r\n{keyname}={inivalue}\r\n", unicodeContent);
            Assert.IsTrue(result);
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)GetLastError.SUCCESS, error);

            // Insight: the same applies to the ...A() method, Unicode in an ...A() method!
            result = WritePrivateProfileStringA(unicodeCharacters, keyname, inivalue, FileName);
            unicodeContent = File.ReadAllText(FileName);
            Assert.AreEqual($"\r\n[{unicodeCharacters}]\r\n{keyname}={inivalue}\r\n", unicodeContent);
            Assert.IsTrue(result);
            error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)GetLastError.SUCCESS, error);
        }
    }
}