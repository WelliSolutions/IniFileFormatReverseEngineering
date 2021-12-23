using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.Encodings
{
    /// <summary>
    /// These tests are to verify the statement of Michael S. Kaplan
    /// http://archives.miloush.net/michkap/archive/2006/09/15/754992.html
    /// "Just for fun, you can even reverse the BOM bytes and WritePrivateProfileString
    /// will write to it as a UTF-16 BE (Big Endian) file!"
    /// </summary>
    [TestClass]
    public class UTF16BE_Tests : IniFileTestBase
    {
        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [TestMethod]
        public void Given_UTF16BEBOM_When_ReadingTheContent_Then_WeGetTheDefaultValue()
        {
            EnsureDeleted();
            EnsureDefaultContent_UsingFile(Encoding.BigEndianUnicode); // UTF16-BE BOM

            var sb = DefaultStringBuilder();

            // Insight: A UTF-16 BOM is ok
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity,
                FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }

        [UsedInDocumentation]
        [Checks(Method.GetPrivateProfileStringW)]
        [TestMethod]
        public void Given_UTF16BEBOMAndLineBreak_When_ReadingTheContent_Then_WeGetTheDefaultValue()
        {
            EnsureDeleted();
            File.WriteAllText(FileName, $"\r\n[{sectionname}]\r\n{keyname}={inivalue}\r\n", Encoding.BigEndianUnicode); // UTF16-BE BOM

            var sb = DefaultStringBuilder();

            // Insight: UTF16-BE does not work ...
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity,
                FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }

        [UsedInDocumentation]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpFileName)]
        [TestMethod]
        public void Given_UTF16BEBOM_When_WritingToTheFile_Then_ContentIsANSI()
        {
            File.WriteAllText(FileName, "\r\n", Encoding.BigEndianUnicode);
            var result = WritePrivateProfileStringW(sectionname, keyname, inivalue, FileName);

            // Insight: That does not work with ...W()
            var bytes = File.ReadAllBytes(FileName); // Note: File.ReadAllText() has BOM detection
            var ascii = Encoding.ASCII.GetString(bytes);
            Assert.AreEqual($"??\0\r\0\n[{sectionname}]\r\n{keyname}={inivalue}\r\n", ascii);
            Assert.IsTrue(result);
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)GetLastError.SUCCESS, error);

            // Insight: the same applies to the ...A() method
            result = WritePrivateProfileStringA(sectionname, keyname, inivalue, FileName);
            bytes = File.ReadAllBytes(FileName);
            ascii = Encoding.ASCII.GetString(bytes);
            Assert.AreEqual($"??\0\r\0\n[{sectionname}]\r\n{keyname}={inivalue}\r\n", ascii);
            Assert.IsTrue(result);
            error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)GetLastError.SUCCESS, error);
        }
    }
}