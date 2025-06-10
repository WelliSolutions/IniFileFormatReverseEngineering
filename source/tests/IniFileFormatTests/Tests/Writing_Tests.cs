﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.IntendedUse
{
    [TestClass]
    public class Writing_Tests : IniFileTestBase
    {
        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpAppName)]
        [Checks(Parameter.lpKeyName)]
        [Checks(Parameter.lpString)]
        [Checks(Parameter.lpFileName)]
        [TestMethod]
        public void Given_AnExistingEmptyFile_When_AValueIsWritten_Then_TheFileContainsSectionKeyAndValue()
        {
            EnsureEmptyASCII();
            var result = WritePrivateProfileStringW(sectionname, keyname, inivalue, FileName);

            // Insight: the section is enclosed in square brackets
            // Insight: Windows line endings are used
            // Insight: Key and value are separated by an equal sign
            // Insight: The file has an empty line at the end
            // Insight: the section name is written as given (regarding casing)
            AssertFileEqualASCII($"[{sectionname}]\r\n{keyname}={inivalue}\r\n", FileName);
            Assert.IsTrue(result);
            Assert.AreEqual((int)GetLastError.SUCCESS, Marshal.GetLastWin32Error());
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpFileName)]
        [TestMethod]
        public void Given_ANonExistingFile_When_AValueIsWritten_Then_TheFileIsCreated()
        {
            EnsureDeleted();
            var result = WritePrivateProfileStringW(sectionname, keyname, inivalue, FileName);

            // Insight: The file is created
            Assert.IsTrue(File.Exists(FileName));
            Assert.IsTrue(result);
            Assert.AreEqual((int)GetLastError.ERROR_FILE_NOT_FOUND, Marshal.GetLastWin32Error());
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpFileName)]
        [TestMethod]
        public void Given_ANonExistingFileInWindowsDirectory_When_AValueIsWritten_Then_WeGetAFileNotFoundError()
        {
            using (var limitedPrivileges = ImpersonationUtils.ImpersonateCurrentUser())
            {
                FileName = "createme.ini";
                var result = WritePrivateProfileStringW(sectionname, keyname, inivalue, FileName);

                // Insight: The file is not created
                var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                var destination = Path.Combine(windir, FileName);
                if (File.Exists(destination))
                {
                    try
                    {
                        Assert.Fail("The file was created");
                    }
                    finally
                    {
                        File.Delete(destination);
                    }
                }

                Assert.IsFalse(result);

                // Insight: the error code is FILE_NOT_FOUND and not e.g. ACCESS_DENIED
                var error = Marshal.GetLastWin32Error();
                Assert.AreEqual((int)GetLastError.ERROR_FILE_NOT_FOUND, error);
            }
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpFileName)]
        [TestMethod]
        public void Given_AFileInANonExistingDirectory_When_AValueIsWritten_Then_WeGetAPathNotFoundError()
        {
            FileName = Path.Combine(Path.GetTempPath(), @"subdir\file.ini");
            var result = WritePrivateProfileStringW(sectionname, keyname, inivalue, FileName);

            // Insight: the subdirectory (and the file) is not created
            Assert.IsFalse(File.Exists(FileName));

            Assert.IsFalse(result);

            // Insight: the error message is PATH_NOT_FOUND
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)GetLastError.ERROR_PATH_NOT_FOUND, error);
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpAppName)]
        [TestMethod]
        public void Given_ASectionNameNotOnlyLetters_When_WritingTheSection_Then_ItsAccepted()
        {
            EnsureEmptyASCII();
            var sectionNameNonLetter = "1234567890!$%&/()=?*+#-_<>.,:;@~\"\'|`\\ \t\v";
            WritePrivateProfileStringW(sectionNameNonLetter, keyname, inivalue, FileName);

            // Insight: a lot of non-letters can be used for the section name as well
            AssertFileEqualASCII($"[{sectionNameNonLetter}]\r\n{keyname}={inivalue}\r\n", FileName);
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpAppName)]
        [TestMethod]
        public void Given_ASectionNameContainingAParagraph_When_WritingTheSection_Then_ItBecomesAReplacementCharacter()
        {
            EnsureEmptyASCII();
            var result = WritePrivateProfileStringW("§€°´²³", keyname, inivalue, FileName);

            // Insight: a few characters are not accepted in the section name
            // Insight: they are replaced by the Unicode replacement character
            string unicodeContent = File.ReadAllText(FileName);
            Assert.AreEqual($"[������]\r\n{keyname}={inivalue}\r\n", unicodeContent);

            // Insight: we don't get an error in case of such a substitution
            Assert.IsTrue(result);
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)GetLastError.SUCCESS, error);
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpKeyName)]
        [TestMethod]
        public void Given_AKeyNameNotOnlyLetters_When_WritingTheSection_Then_ItsAccepted()
        {
            EnsureEmptyASCII();
            var keyNameNonLetter = "1234567890!$%&/()=?*+#-_<>.,:;@~\"\'|`\\ \t\v";
            WritePrivateProfileStringW(sectionname, keyNameNonLetter, inivalue, FileName);

            // Insight: a lot of non-letters can be used for the section name as well
            AssertFileEqualASCII($"[{sectionname}]\r\n{keyNameNonLetter}={inivalue}\r\n", FileName);
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [TestMethod]
        public void Given_AnEmptyIniFile_When_WritingKeys_Then_TheyAreWrittenInChronologicalOrder()
        {
            EnsureEmptyASCII();
            WritePrivateProfileStringW(sectionname, "z", "", FileName);
            WritePrivateProfileStringW(sectionname, "a", "", FileName);
            WritePrivateProfileStringW(sectionname, "y", "", FileName);
            WritePrivateProfileStringW(sectionname, "b", "", FileName);

            // Insight: values are written in chronological order
            // This might depend on whether or not keys already exist, so we need more tests...
            Assert.AreEqual($"[{sectionname}]\r\nz=\r\na=\r\ny=\r\nb=\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [TestMethod]
        public void Given_AnIniFileWithExistingKeys_When_WritingKeys_Then_TheyAreKeptInOriginalOrder()
        {
            EnsureASCII($"[{sectionname}]\r\nb=value\r\na=value\r\n");
            WritePrivateProfileStringW(sectionname, "z", "", FileName);
            WritePrivateProfileStringW(sectionname, "b", "", FileName);
            WritePrivateProfileStringW(sectionname, "y", "", FileName);
            WritePrivateProfileStringW(sectionname, "a", "", FileName);

            // Insight: values that already exist are kept in their order
            // Insight: New values are written in chronological order after the existing values
            Assert.AreEqual($"[{sectionname}]\r\nb=\r\na=\r\nz=\r\ny=\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpString, null)]
        [TestMethod]
        public void Given_NullAsTheStringParameter_When_WritingTheValue_Then_TheKeyIsDeleted()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}={inivalue}\r\n");

            WritePrivateProfileStringW(sectionname, keyname, null, FileName);

            // Insight: the key and the value are deleted
            // Insight: an empty section is not deleted
            Assert.AreEqual($"[{sectionname}]\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation("WritePrivateProfileString.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Parameter.lpString, null)]
        [TestMethod]
        public void Given_NullAsTheStringParameter_When_UsingACommentAsTheKey_Then_TheCommentIsNotDeleted()
        {
            var contents = $"[{sectionname}]\r\n;key={inivalue}\r\n";
            EnsureASCII(contents);

            WritePrivateProfileStringW(sectionname, ";key", null, FileName);

            // Insight: the comment is not deleted
            Assert.AreEqual(contents, File.ReadAllText(FileName));
        }
    }
}
