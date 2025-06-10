using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.IntendedUse
{
    [TestClass]
    public class Reading_Tests : IniFileTestBase
    {
        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpAppName)]
        [Checks(Parameter.lpKeyName)]
        [Checks(Parameter.lpFileName)]
        [Checks(Parameter.lpReturnedString)]
        [TestMethod]
        public void Given_AnIniFileWithKnownContent_When_TheContentIsAccessed_Then_WeGetTheExpectedValue()
        {
            EnsureDefaultContent_UsingAPI();
            var sb = DefaultStringBuilder();

            // Insight: basically, it works as expected, at least for trivial cases ;-)
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpAppName, null)]
        [TestMethod]
        public void
            Given_AnIniFileWithKnownContent_When_NullIsUsedForSectionName_Then_WeGetAListOfZeroTerminatedSections()
        {
            EnsureDefaultContent_UsingAPI();
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(null, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);

            // Insight: when using the StringBuilder, we get only 1 section name,
            // although there are 2 section names inside.
            // This is probably more a problem of the C# method signature,
            // but it tells us what can go wrong with implementations that do not have unit tests :-)
            AssertSbEqual(sectionname, sb);

            // Insight: the section names are (probably) delimited by \0. At least the length matches.
            var length = (uint)Encoding.ASCII.GetBytes(sectionname + '\0').Length;
            var length2 = (uint)Encoding.ASCII.GetBytes(sectionname2 + '\0').Length;
            Assert.AreEqual(length + length2, bytes);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpAppName, null)]
        [TestMethod]
        public void
            Given_AnIniFileWithDuplicateSections_When_NullIsUsedForSectionName_Then_WeGetDuplicateSectionsAsWell()
        {
            EnsureASCII($"[{sectionname}]\r\n[{sectionname2}]\r\n[{sectionname}]\r\n");
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(null, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);


            // Insight: the length matches if we consider one section twice
            var length = (uint)Encoding.ASCII.GetBytes(sectionname + '\0').Length;
            var length2 = (uint)Encoding.ASCII.GetBytes(sectionname2 + '\0').Length;
            Assert.AreEqual(length * 2 + length2, bytes);
        }



        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpAppName, null)]
        [TestMethod]
        public void Given_AKnownIniFile_When_NullIsUsedForSectionName_Then_SeparatorCharacterIsNul()
        {
            EnsureDefaultContent_UsingAPI();
            var buffer = new char[40];
            GetIniString_ChArr_Unicode(null, keyname, defaultvalue, buffer, (uint)buffer.Length, FileName);

            // Insight: The separator character is NUL \0
            Assert.AreEqual('\0', buffer[sectionname.Length]);
            Assert.AreEqual('\0', buffer[sectionname.Length + 1 + sectionname2.Length]);
        }
        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpAppName, null)]
        [TestMethod]
        public void Given_ATooSmallBuffer_When_NullIsUsedForSectionName_Then_SizeIsBytesMinusTwo()
        {
            EnsureDefaultContent_UsingAPI();
            var buffer = new char[10]; // StringBuilder can't be smaller than 16
            var bytes = GetIniString_ChArr_Unicode(null, keyname, defaultvalue, buffer, (uint)buffer.Length, FileName);

            // Insight: The result is nSize -2
            Assert.AreEqual((uint)buffer.Length - 2, bytes);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpAppName, null)]
        [TestMethod]
        public void Given_ATooSmallBuffer_When_NullIsUsedForKeyName_Then_SizeIsBytesMinusTwo()
        {
            EnsureDefaultContent_UsingAPI();
            var buffer = new char[10]; // StringBuilder can't be smaller than 16
            var bytes = GetIniString_ChArr_Unicode(sectionname, null, defaultvalue, buffer, (uint)buffer.Length,
                FileName);

            // Insight: The result is nSize -2
            Assert.AreEqual((uint)buffer.Length - 2, bytes);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, null)]
        [TestMethod]
        public void Given_AKnownIniFile_When_NullIsUsedForKeyName_Then_SeparatorCharacterIsNul()
        {
            EnsureDefaultContent_UsingAPI();
            var buffer = new char[40];
            GetIniString_ChArr_Unicode(sectionname, null, defaultvalue, buffer, (uint)buffer.Length,
                FileName);

            // Insight: The separator character is NUL \0
            Assert.AreEqual('\0', buffer[keyname.Length]);
            Assert.AreEqual('\0', buffer[keyname.Length + 1 + keyname2.Length]);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, null)]
        [TestMethod]
        public void Given_AnIniFileWithKnownContent_When_NullIsUsedAsTheKey_Then_WeGetAListOfKeysInTheSection()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}=value\r\n{keyname2}=value2");
            var sb = DefaultStringBuilder();

            var bytes = GetIniString_SB_Unicode(sectionname, null, defaultvalue, sb, (uint)sb.Capacity, FileName);

            // Insight: when using the StringBuilder, we get only 1 key name,
            // although there are 2 keys inside.
            // This is probably more a problem of the C# method signature
            AssertSbEqual(keyname, sb);

            // Insight: the key names are (probably) delimited by \0. At least the length matches.
            var length = (uint)Encoding.ASCII.GetBytes(keyname + '\0').Length;
            var length2 = (uint)Encoding.ASCII.GetBytes(keyname2 + '\0').Length;
            Assert.AreEqual(length + length2, bytes);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, null)]
        [TestMethod]
        public void Given_AnIniFileWithDuplicateKeys_When_NullIsUsedAsTheKey_Then_WeGetDuplicateKeysAsWell()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}=value\r\n{keyname2}=value2\r\n{keyname}=value3");
            var sb = DefaultStringBuilder();

            // Insight: the length matches if we consider one key twice.
            var bytes = GetIniString_SB_Unicode(sectionname, null, defaultvalue, sb, (uint)sb.Capacity, FileName);
            var length = (uint)Encoding.ASCII.GetBytes(keyname + '\0').Length;
            var length2 = (uint)Encoding.ASCII.GetBytes(keyname2 + '\0').Length;
            Assert.AreEqual(length * 2 + length2, bytes);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, null)]
        [TestMethod]
        public void Given_AnIniFileWithDuplicateKeys_When_TheKeyIsRead_Then_WeGetTheFirstOccurrence()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}={inivalue}\r\n{keyname}={inivalue2}");
            var sb = DefaultStringBuilder();

            // Insight: we get the first occurrence
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            AssertSbEqual(inivalue, sb);
        }


        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpKeyName, null)]
        [TestMethod]
        public void Given_AnIniFileWithDuplicateSections_When_TheKeyIsRead_Then_OnlyTheFirstSectionIsConsidered()
        {
            EnsureASCII($"[{sectionname}]\r\n[{sectionname}]\r\n{keyname}={inivalue}");
            var sb = DefaultStringBuilder();

            // Insight: we get the default value
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }


        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpDefault)]
        [TestMethod]
        public void Given_AnIniFileWithKnownContent_When_ANonExistingKeyIsAccessed_Then_WeGetTheDefaultValue()
        {
            EnsureDefaultContent_UsingAPI();
            var sb = DefaultStringBuilder();

            // Insight: reading a non-existing key gives the default value
            var bytes = GetIniString_SB_Unicode(sectionname, "NonExistingKey", defaultvalue, sb, (uint)sb.Capacity,
                FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpDefault)]
        [TestMethod]
        public void Given_AnIniFileWithKnownContent_When_ANonExistingSectionIsAccessed_Then_WeGetTheDefaultValue()
        {
            EnsureDefaultContent_UsingAPI();
            var sb = DefaultStringBuilder();

            // Insight: reading from a non-existing section gives the default value
            var bytes = GetIniString_SB_Unicode("NonExistingSection", keyname, defaultvalue, sb, (uint)sb.Capacity,
                FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpDefault)]
        [TestMethod]
        public void Given_NoIniFile_When_TheContentIsAccessed_Then_WeGetTheDefaultValue()
        {
            EnsureDeleted();
            var sb = DefaultStringBuilder();

            // Insight: if the INI file does not exist, we get the default value
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            Assert.AreEqual((uint)Encoding.ASCII.GetBytes(defaultvalue).Length, bytes);
            AssertSbEqual(defaultvalue, sb);

            // Insight: we get the FileNotFound error as described for the return value
            Assert.AreEqual((int)GetLastError.ERROR_FILE_NOT_FOUND, Marshal.GetLastWin32Error());
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpDefault)]
        [TestMethod]
        public void Given_AnIniFileWithKnownContent_When_NullIsTheDefaultValue_Then_WeGetAnEmptyString()
        {
            EnsureDefaultContent_UsingAPI();
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, "NonExistingKey", null, sb, (uint)sb.Capacity, FileName);
            AssertZero(bytes);
            // Insight: According the documentation, NULL should lead to an empty string
            AssertSbEqual("", sb);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.nSize)]
        [Checks(Paragraph.returnValue)]
        [TestMethod]
        public void Given_ASmallBuffer_When_WeTryToGetTheValue_Then_TheValueIsTruncated()
        {
            EnsureDefaultContent_UsingAPI();
            var buffer = new char[5]; // StringBuilder does not support lengths < 16
            var bytes = GetIniString_ChArr_Unicode(sectionname, keyname, defaultvalue, buffer, (uint)buffer.Length,
                FileName);

            // Insight: is works as documented
            // In C#, strings are not zero-terminated. Using the whole buffer will include that character.
            Assert.AreEqual((uint)(buffer.Length - 1), bytes);
            Assert.AreEqual(inivalue.Substring(0, 4) + '\0', new string(buffer));
            // Insight: the last error gives an indication that more data is available
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)GetLastError.ERROR_MORE_DATA, error);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.nSize)]
        [Checks(Paragraph.returnValue)]
        [TestMethod]
        public void Given_AZeroBuffer_When_WeTryToGetTheValue_Then_NothingCanBeReturned()
        {
            EnsureDefaultContent_UsingAPI();
            var buffer = new char[0]; // StringBuilder does not support lengths < 16

            // Insight: a zero length buffer will not fit anything (as expected)
            var bytes = GetIniString_ChArr_Unicode(sectionname, keyname, defaultvalue, buffer, (uint)buffer.Length,
                FileName);
            AssertZero(bytes);
            // Insight: the last error gives an indication that more data is available
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)GetLastError.ERROR_MORE_DATA, error);
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpFileName)]
        [TestMethod]
        public void Given_AFileNameWithArbitraryExtension_When_ReadingFromTheFile_Then_WeGetTheValue()
        {
            FileName = Path.Combine(Path.GetTempPath(), "FileWithExtension.ext");
            EnsureDefaultContent_UsingAPI();

            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            Assert.AreEqual(0, Marshal.GetLastWin32Error());
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpFileName)]
        [TestMethod]
        public void Given_AFileNameWithoutExtension_When_ReadingFromTheFile_Then_WeGetTheValue()
        {
            FileName = Path.Combine(Path.GetTempPath(), "FileWithoutExtension");
            EnsureDefaultContent_UsingAPI();

            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(inivalue, bytes);
            Assert.AreEqual(0, Marshal.GetLastWin32Error());
        }

        [UsedInDocumentation("GetPrivateProfileString.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Parameter.lpFileName)]
        [TestMethod]
        public void Given_AnInvalidFileName_When_ReadingFromTheFile_Then_WeGetAnError()
        {
            var sb = DefaultStringBuilder();
            var invalids = new Dictionary<string, GetLastError>();
            invalids.Add("PRN", GetLastError.ERROR_FILE_NOT_FOUND);
            invalids.Add("COM9", GetLastError.ERROR_FILE_NOT_FOUND);
            invalids.Add("LPT", GetLastError.ERROR_FILE_NOT_FOUND);
            invalids.Add(@"C:\C:\", GetLastError.ERROR_INVALID_NAME);
            invalids.Add(@"*", GetLastError.ERROR_INVALID_NAME);
            invalids.Add(@"?", GetLastError.ERROR_INVALID_NAME);
            invalids.Add(@"", GetLastError.ERROR_ACCESS_DENIED);
            invalids.Add(@".", GetLastError.ERROR_ACCESS_DENIED);
            invalids.Add(@"..", GetLastError.ERROR_ACCESS_DENIED);
            invalids.Add(Path.GetTempPath(), GetLastError.ERROR_ACCESS_DENIED);
            foreach (var invalid in invalids)
            {
                var bytes = GetIniString_SB_Unicode(null, null, null, sb, (uint)sb.Capacity, invalid.Key);
                AssertZero(bytes);
                Assert.AreEqual((int)invalid.Value, Marshal.GetLastWin32Error());
            }
        }

    }
}
