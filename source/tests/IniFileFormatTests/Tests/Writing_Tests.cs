using System;
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
        [UsedInDocumentation]
        [TestsApiParameter("lpAppName")]
        [TestsApiParameter("lpKeyName")]
        [TestsApiParameter("lpString")]
        [TestsApiParameter("lpFileName")]
        [TestMethod]
        public void Given_AnExistingEmptyFile_When_AValueIsWritten_Then_TheFileContainsSectionKeyAndValue()
        {
            EnsureEmptyASCII();
            var result = WritePrivateProfileString(sectionname, keyname, inivalue, FileName);

            // Insight: the section is enclosed in square brackets
            // Insight: Windows line endings are used
            // Insight: Key and value are separated by an equal sign
            // Insight: The file has an empty line at the end
            // Insight: the section name is written as given (regarding casing)
            AssertFileEqualASCII($"[{sectionname}]\r\n{keyname}={inivalue}\r\n", FileName);
            Assert.IsTrue(result);
            Assert.AreEqual((int)GetLastError.SUCCESS, Marshal.GetLastWin32Error());
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpFileName")]
        [TestMethod]
        public void Given_ANonExistingFile_When_AValueIsWritten_Then_TheFileIsCreated()
        {
            EnsureDeleted();
            var result = WritePrivateProfileString(sectionname, keyname, inivalue, FileName);

            // Insight: The file is created
            Assert.IsTrue(File.Exists(FileName));
            Assert.IsTrue(result);
            Assert.AreEqual((int)GetLastError.ERROR_FILE_NOT_FOUND, Marshal.GetLastWin32Error());
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpFileName")]
        [TestMethod]
        public void Given_ANonExistingFileInWindowsDirectory_When_AValueIsWritten_Then_WeGetAFileNotFoundError()
        {
            FileName = "createme.ini";
            var result = WritePrivateProfileString(sectionname, keyname, inivalue, FileName);

            // Insight: The file is not created
            var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var destination = Path.Combine(windir, FileName);
            Assert.IsFalse(File.Exists(destination));

            Assert.IsFalse(result);

            // Insight: the error code is FILE_NOT_FOUND and not e.g. ACCESS_DENIED
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)GetLastError.ERROR_FILE_NOT_FOUND, error);
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpFileName")]
        [TestMethod]
        public void Given_AFileInANonExistingDirectory_When_AValueIsWritten_Then_WeGetAPathNotFoundError()
        {
            FileName = Path.Combine(Path.GetTempPath(), @"subdir\file.ini");
            var result = WritePrivateProfileString(sectionname, keyname, inivalue, FileName);

            // Insight: the subdirectory (and the file) is not created
            Assert.IsFalse(File.Exists(FileName));

            Assert.IsFalse(result);

            // Insight: the error message is PATH_NOT_FOUND
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)GetLastError.ERROR_PATH_NOT_FOUND, error);
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpAppName")]
        [TestMethod]
        public void Given_ASectionNameNotOnlyLetters_When_WritingTheSection_Then_ItsAccepted()
        {
            EnsureEmptyASCII();
            var sectionNameNonLetter = "1234567890!$%&/()=?*+#-_<>.,:;@~\"\'|`\\ \t\v";
            WritePrivateProfileString(sectionNameNonLetter, keyname, inivalue, FileName);

            // Insight: a lot of non-letters can be used for the section name as well
            AssertFileEqualASCII($"[{sectionNameNonLetter}]\r\n{keyname}={inivalue}\r\n", FileName);
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpAppName")]
        [TestMethod]
        public void Given_ASectionNameContainingAParagraph_When_WritingTheSection_Then_ItBecomesAQuestionmark()
        {
            EnsureEmptyASCII();
            var result = WritePrivateProfileString("§€°´²³", keyname, inivalue, FileName);

            // Insight: a few characters are not accepted in the section name
            // Insight: they are replaced by the Unicode replacement character
            string unicodeContent = File.ReadAllText(FileName);
            Assert.AreEqual($"[������]\r\n{keyname}={inivalue}\r\n", unicodeContent);

            // Insight: we don't get an error in case of such a substitution
            Assert.IsTrue(result);
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual((int)GetLastError.SUCCESS, error);
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpKeyName")]
        [TestMethod]
        public void Given_AKeyNameNotOnlyLetters_When_WritingTheSection_Then_ItsAccepted()
        {
            EnsureEmptyASCII();
            var keyNameNonLetter = "1234567890!$%&/()=?*+#-_<>.,:;@~\"\'|`\\ \t\v";
            WritePrivateProfileString(sectionname, keyNameNonLetter, inivalue, FileName);

            // Insight: a lot of non-letters can be used for the section name as well
            AssertFileEqualASCII($"[{sectionname}]\r\n{keyNameNonLetter}={inivalue}\r\n", FileName);
        }

        [UsedInDocumentation]
        [TestMethod]
        public void Given_AnEmptyIniFile_When_WritingKeys_Then_TheyAreWrittenInChronologicalOrder()
        {
            EnsureEmptyASCII();
            WritePrivateProfileString(sectionname, "z", "", FileName);
            WritePrivateProfileString(sectionname, "a", "", FileName);
            WritePrivateProfileString(sectionname, "y", "", FileName);
            WritePrivateProfileString(sectionname, "b", "", FileName);

            // Insight: values are written in chronological order
            // This might depend on whether or not keys already exist, so we need more tests...
            Assert.AreEqual($"[{sectionname}]\r\nz=\r\na=\r\ny=\r\nb=\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation]
        [TestMethod]
        public void Given_AnIniFileWithExistingKeys_When_WritingKeys_Then_TheyAreKeptInOriginalOrder()
        {
            EnsureASCII($"[{sectionname}]\r\nb=value\r\na=value\r\n");
            WritePrivateProfileString(sectionname, "z", "", FileName);
            WritePrivateProfileString(sectionname, "b", "", FileName);
            WritePrivateProfileString(sectionname, "y", "", FileName);
            WritePrivateProfileString(sectionname, "a", "", FileName);

            // Insight: values that already exist are kept in their order
            // Insight: New values are written in chronological order after the existing values
            Assert.AreEqual($"[{sectionname}]\r\nb=\r\na=\r\nz=\r\ny=\r\n", File.ReadAllText(FileName));
        }
    }
}
