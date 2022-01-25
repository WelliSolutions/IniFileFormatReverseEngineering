using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.WindowsAPI;
using static IniFileFormatTests.AssertionHelper;

namespace IniFileFormatTests.Limits
{
    [TestClass]
    public class Limits_Tests : IniFileTestBase
    {
        public static string LargeString
        {
            get
            {
                var sb = new StringBuilder(1 << 20); // 2^20 = 1 MB
                while (sb.Length < 1 << 20)
                    sb.Append("xxxxxxxx");
                return sb.ToString();
            }
        }

        struct Result
        {
            internal uint Bytes;
            internal int Error;
            internal int Length;

            public Result(uint bytes, int error, int length)
            {
                Bytes = bytes;
                Error = error;
                Length = length;
            }
        }

        private Result GetHugeValue(int valueLength, int bufferSize)
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}=");
            File.AppendAllText(FileName, LargeString.Substring(0, valueLength));
            File.AppendAllText(FileName, "\r\n");
            var buffer2 = new StringBuilder(bufferSize);
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, buffer2,
                (uint)buffer2.Capacity, FileName);
            var error = Marshal.GetLastWin32Error();
            return new Result(bytes, error, buffer2.ToString().Length);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Parameter.nSize)]
        [TestMethod]
        public void Given_AValueOfLength65534_When_AccessingIt_Then_WeGetTheFullValue()
        {
            var result = GetHugeValue(65534, 65534 + 2);

            // Insight: 65534 bytes can be returned without an error
            Assert.AreEqual(0, result.Error);
            Assert.AreEqual((uint)65534, result.Bytes);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Parameter.nSize)]
        [TestMethod]
        public void Given_AValueOfLength65535_When_AccessingIt_Then_WeGetTheFullValueAndAnError()
        {
            var result = GetHugeValue(65535, 65535 + 2);

            // Insight: 65535 bytes are returned with an error, although there is no more data
            Assert.AreEqual(0, result.Error);
            Assert.AreEqual((uint)65535, result.Bytes);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Parameter.nSize)]
        [TestMethod]
        public void Given_AValueOfLength65536_When_AccessingIt_Then_WeGetNothingAndNoError()
        {
            var result = GetHugeValue(65536, 65536 + 2);

            // Insight: 65536 bytes cannot be read and there's no error either
            Assert.AreEqual(0, result.Error);
            Assert.AreEqual((uint)0, result.Bytes);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Parameter.nSize)]
        [TestMethod]
        public void Given_AValueOfLength65537_When_AccessingIt_Then_WeGetModuloBehavior()
        {
            var result = GetHugeValue(65537, 65537 + 2);

            // Insight: 65537 overflows modulo 65536
            Assert.AreEqual(0, result.Error);
            Assert.AreEqual((uint)1, result.Bytes);
            // Insight: the value seems to overflow right at the beginning.
            // It's not that the buffer is filled and only the return value overflows.
            Assert.AreEqual(1, result.Length);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Parameter.lpFileName)]
        [TestMethod]
        public void Given_AFileNameTooLong_When_ReadingFromTheFile_Then_ThePathIsNotFound()
        {
            var sb = DefaultStringBuilder();
            // 245 + 4 + C:\Windows\ = 260 = MAX_PATH
            var longFileName = LargeString.Substring(0, 245) + ".ini";
            var bytes = GetIniString_SB_Unicode(null, null, null, sb, (uint)sb.Capacity, longFileName);
            AssertZero(bytes);
            // Insight: for too long file names, we get a path error
            Assert.AreEqual((int)GetLastError.ERROR_PATH_NOT_FOUND, Marshal.GetLastWin32Error());
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Parameter.lpAppName, null)]
        [TestMethod]
        public void Given_ATooSmallBuffer_When_NullIsUsedForKeyName_Then_SizeIsNotNegative()
        {
            EnsureDefaultContent_UsingAPI();
            foreach (var smallSize in new[] { 2, 1 })
            {
                var buffer = new char[smallSize]; // StringBuilder can't be smaller than 16
                var bytes = GetIniString_ChArr_Unicode(sectionname, null, defaultvalue, buffer, (uint)buffer.Length,
                    FileName);

                // Insight: The result will  not be negative (nSize-2)
                AssertZero(bytes);
            }
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpString)]
        [TestMethod]
        public void Given_ALargeString_When_TheValueIsWritten_Then_ItCanWriteMoreThan65536Characters()
        {
            EnsureEmptyASCII();
            var largeString = LargeString;
            var result = WritePrivateProfileStringW(sectionname, keyname, largeString, FileName);
            Assert.IsTrue(result);
            // Insight: the write function can write more than 65536 characters (as opposed to the read function)
            Assert.AreEqual($"[SectionName]\r\nTestKey={largeString}\r\n", ReadIniFile());
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Parameter.lpAppName, null)]
        [Checks(Parameter.lpReturnedString)]
        [Checks(Method.GetPrivateProfileStringW)]
        [TestMethod]
        public void Given_ManySections_When_GettingTheSectionNames_Then_ItCanReadMoreThan65536Characters()
        {
            // Generate 1 MB of section names
            var nBytes = 1000000;
            var length = 10;
            var sb = new StringBuilder();
            for (int i = 1; i <= nBytes / length; i++)
            {
                sb.Append($"[{i:D10}]\r\n");
            }
            EnsureASCII(sb.ToString());

            // Read the sections
            var buffer = new char[nBytes / length * (length + 1)];
            var bytesRead = GetIniString_ChArr_Unicode(null, null, "", buffer, (uint)buffer.Length, FileName);
            // Insight: the function can read a lot more sections than the 65536 characters value limit
            Assert.AreEqual((uint)(nBytes / length * (length + 1) - 2), bytesRead);
        }
    }
}