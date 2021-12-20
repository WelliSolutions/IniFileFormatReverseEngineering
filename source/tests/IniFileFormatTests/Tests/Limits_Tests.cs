using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IniFileFormatTests
{
    [TestClass]
    public class Limits_Tests : IniFileTestBase
    {
        private static string LargeString
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
            var bytes = WindowsAPI.GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, buffer2,
                (uint)buffer2.Capacity, FileName);
            var error = Marshal.GetLastWin32Error();
            return new Result(bytes, error, buffer2.ToString().Length);
        }

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("nSize")]
        [TestMethod]
        public void Given_AValueOfLength65534_When_AccessingIt_Then_WeGetTheFullValue()
        {
            var result = GetHugeValue(65534, 65534 + 2);

            // Insight: 65534 bytes can be returned without an error
            Assert.AreEqual(0, result.Error);
            Assert.AreEqual((uint)65534, result.Bytes);
        }

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("nSize")]
        [TestMethod]
        public void Given_AValueOfLength65535_When_AccessingIt_Then_WeGetTheFullValueAndAnError()
        {
            var result = GetHugeValue(65535, 65535 + 2);

            // Insight: 65535 bytes are returned with an error, although there is no more data
            Assert.AreEqual(0, result.Error);
            Assert.AreEqual((uint)65535, result.Bytes);
        }

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("nSize")]
        [TestMethod]
        public void Given_AValueOfLength65536_When_AccessingIt_Then_WeGetNothingAndNoError()
        {
            var result = GetHugeValue(65536, 65536 + 2);

            // Insight: 65536 bytes cannot be read and there's no error either
            Assert.AreEqual(0, result.Error);
            Assert.AreEqual((uint)0, result.Bytes);
        }

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("nSize")]
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
    }
}