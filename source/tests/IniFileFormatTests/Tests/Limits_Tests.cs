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

        private Tuple<uint, int> GetHugeValue(int valueLength, int bufferSize)
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}=");
            File.AppendAllText(FileName, LargeString.Substring(0, valueLength));
            File.AppendAllText(FileName, "\r\n");
            var buffer2 = new char[bufferSize];
            var bytes = WindowsAPI.GetIniString_ChArr_Unicode(sectionname, keyname, defaultvalue, buffer2,
                (uint)buffer2.Length, FileName);
            var error = Marshal.GetLastWin32Error();
            return new Tuple<uint, int>(bytes, error);
        }

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("nSize")]
        [TestMethod]
        public void Given_AValueOfLength65534_When_AccessingIt_Then_WeGetTheFullValue()
        {
            var result = GetHugeValue(65534, 65534 + 2);

            // Insight: 65534 bytes can be returned without an error
            Assert.AreEqual(0, result.Item2);
            Assert.AreEqual((uint)65534, result.Item1);
        }

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("nSize")]
        [TestMethod]
        public void Given_AValueOfLength65535_When_AccessingIt_Then_WeGetTheFullValueAndAnError()
        {
            var result = GetHugeValue(65535, 65535 + 2);

            // Insight: 65535 bytes are returned with an error, although there is no more data
            Assert.AreEqual(0, result.Item2);
            Assert.AreEqual((uint)65535, result.Item1);
        }

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("nSize")]
        [TestMethod]
        public void Given_AValueOfLength65536_When_AccessingIt_Then_WeGetNothingAndAnError()
        {
            var result = GetHugeValue(65536, 65536 + 2);

            // Insight: 65536 bytes cannot be read and there's no error either
            Assert.AreEqual((uint)0, result.Item1);
            Assert.AreEqual(0, result.Item2);
        }
    }
}