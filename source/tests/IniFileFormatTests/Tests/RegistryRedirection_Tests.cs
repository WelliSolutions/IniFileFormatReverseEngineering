using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using IniFileFormatTests.Limits;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests
{
    [TestClass]
    public class RegistryRedirection_Tests : IniFileTestBase
    {
        [TestInitialize]
        public void SetupRegistryTests()
        {
            // Make sure the file name is not a GUID
            FileName = Path.Combine(Path.GetDirectoryName(FileName), "test.ini");
            DeletePreviousContent();
        }

        private void MakeRedirectionEffective()
        {
            // Let the Registry mapping become effective without a reboot
            WritePrivateProfileStringW(null, null, null, "test.ini");
        }

        private void DeletePreviousContent()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\WelliSolutions", true))
            {
                key?.DeleteSubKeyTree("Test", false);
            }
        }

        private void SetupRedirection(RedirectionPolicies redirectionPoliciesType)
        {
            var map = new Dictionary<RedirectionPolicies, string>
            {
                { RedirectionPolicies.None, "" },
                { RedirectionPolicies.PreventReadsFromFile, "@" },
                { RedirectionPolicies.WriteThrough, "!" },
                { RedirectionPolicies.SetAtFirstLogon, "#" }
            };

            try
            {
                var redirection = $@"{map[redirectionPoliciesType]}USR:Software\WelliSolutions\Test\SectionName";
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\IniFileMapping",
                               true))
                    {
                        if (key == null)
                        {
                            Console.WriteLine(
                                "Uncertain whether Registry redirection has been set up correctly. Run these tests as administrator.");
                            Assert.Inconclusive(
                                "Uncertain whether Registry redirection has been set up correctly. Run these tests as administrator.");
                            return;
                        }

                        RegistryKey testini = null;
                        try
                        {
                            if (key.GetSubKeyNames().Contains("test.ini"))
                            {
                                testini = key.OpenSubKey("test.ini", true);
                            }
                            else
                            {
                                testini = key.CreateSubKey("test.ini", true);
                            }

                            if (testini.GetValueNames().Contains(sectionname))
                            {
                                if ((string)testini.GetValue(sectionname) == redirection)
                                {
                                    // Stuff is set up correctly
                                    return;
                                }
                            }

                            // Write the redirection
                            testini.SetValue(sectionname, redirection);
                        }
                        finally
                        {
                            testini?.Dispose();
                            MakeRedirectionEffective();
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                Assert.Inconclusive("This test must be run as administrator due to modifications in the Registry.");
            }
        }


        enum RedirectionPolicies
        {
            None = 1,
            WriteThrough = 2,
            SetAtFirstLogon = 4,
            PreventReadsFromFile = 8
        }

        private RedirectionPolicies[] AllPolicies =
        {
            RedirectionPolicies.None, RedirectionPolicies.PreventReadsFromFile,
            RedirectionPolicies.SetAtFirstLogon, RedirectionPolicies.WriteThrough
        };

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_ARegistryMapping_When_AValueIsWritten_Then_TheFileIsNotCreated()
        {
            foreach (var policy in new[]
                     {
                         RedirectionPolicies.None, RedirectionPolicies.PreventReadsFromFile,
                         RedirectionPolicies.SetAtFirstLogon
                     })
            {
                SetupRedirection(policy);
                WritePrivateProfileStringW(sectionname, keyname, inivalue, FileName);

                // Insight: the file is not created with @, # or no special policy
                Assert.IsFalse(File.Exists(FileName));
            }
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_AWriteThroughMapping_When_AValueIsWritten_Then_TheFileIsCreated()
        {
            SetupRedirection(RedirectionPolicies.WriteThrough);
            EnsureDeleted();
            WritePrivateProfileStringW(sectionname, keyname, inivalue, FileName);

            // Insight: the file is created with the ! policy
            Assert.IsTrue(File.Exists(FileName));
            // Insight: The file content is like without Registry Mapping
            Assert.AreEqual($"[{sectionname}]\r\n{keyname}={inivalue}\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_ARegistryMapping_When_AValueIsWritten_Then_TheValueIsInRegistry()
        {
            foreach (var policy in AllPolicies)
            {
                SetupRedirection(policy);
                WritePrivateProfileStringW(sectionname, keyname, inivalue, FileName);
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\WelliSolutions\Test"))
                {
                    Assert.IsTrue(key.GetSubKeyNames().Contains(sectionname));

                    using (var sectionKey = key.OpenSubKey(sectionname))
                    {
                        // Insight: the value is indeed in the Registry
                        Assert.IsTrue((string)sectionKey.GetValue(keyname) == inivalue);
                    }
                }
            }
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_ARegistryMappingAndNoFile_When_AValueIsRead_Then_TheValueIsFromRegistry()
        {
            foreach (var policy in AllPolicies)
            {
                SetupRedirection(policy);
                WritePrivateProfileStringW(sectionname, keyname, inivalue, FileName);
                EnsureDeleted(); // Note this is after writing, so there will be no file

                var sb = DefaultStringBuilder();
                // Insight: the value is read from Registry
                GetIniString_SB_Ansi(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
                AssertSbEqual(inivalue, sb);
            }
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_ARegistryMappingAndAFile_When_AValueIsInTheRegistry_Then_TheValueIsFromRegistry()
        {
            foreach (var policy in AllPolicies)
            {
                SetupRedirection(policy);
                WritePrivateProfileStringW(sectionname, keyname, inivalue, FileName);
                EnsureASCII($"[{sectionname}]\r\n{keyname}={inivalue2}\r\n");

                var sb = DefaultStringBuilder();
                // Insight: the value is read from Registry
                GetIniString_SB_Ansi(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
                AssertSbEqual(inivalue, sb);
            }
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_ARegistryMapping_When_TheSectionIsMapped_Then_TheValueIsReadFromRegistryOnly()
        {
            foreach (var policy in AllPolicies)
            {
                EnsureDefaultContent_UsingFile();
                SetupRedirection(policy);

                var sb = DefaultStringBuilder();
                // Insight: for values that have a mapping set up, the default value is returned
                GetIniString_SB_Ansi(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
                AssertSbEqual(defaultvalue, sb);
            }
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_ARegistryMapping_When_TheSectionIsNotMapped_Then_TheValueIsReadFromTheFile()
        {
            foreach (var policy in new[]
                     {
                         RedirectionPolicies.None, RedirectionPolicies.WriteThrough,
                         RedirectionPolicies.SetAtFirstLogon
                     })
            {
                EnsureDefaultContent_UsingFile();
                SetupRedirection(policy);

                var sb = DefaultStringBuilder();
                // Insight: for values that have a no mapping set up, the value from the file is returned
                GetIniString_SB_Ansi(sectionname2, keyname2, defaultvalue, sb, (uint)sb.Capacity, FileName);
                AssertSbEqual(inivalue2, sb);
            }
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_ARegistryMapping_When_AValueIsWritten_Then_ItCanContainNewlines()
        {
            SetupRedirection(RedirectionPolicies.WriteThrough);
            WritePrivateProfileStringW(sectionname, keyname, "a\r\nb", FileName);
            using (var key = Registry.CurrentUser.OpenSubKey($@"Software\WelliSolutions\Test\{sectionname}"))
            {
                // Insight: the value in the Registry contains the newline.
                // Note that this is different from a newline in a file where a new line is related to a new key.
                Assert.AreEqual((string)key.GetValue(keyname), "a\r\nb");
            }

            // Insight: The content in the file is written the same way (but reading will stop at the \r\n)
            Assert.AreEqual($"[{sectionname}]\r\n{keyname}=a\r\nb\r\n", File.ReadAllText(FileName));
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [TestMethod]
        public void Given_ARegistryMapping_When_AValueIsRead_Then_ItCanContainNewlines()
        {
            SetupRedirection(RedirectionPolicies.PreventReadsFromFile);
            WritePrivateProfileStringW(sectionname, keyname, "a\r\nb", FileName);
            EnsureDeleted();

            var sb = DefaultStringBuilder();
            // Insight: Reading the value from Registry returns the newline.
            // Note that this is different from reading from a file where a newline is related to a new key.
            GetIniString_SB_Ansi(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, "test.ini");
            AssertSbEqual("a\r\nb", sb);
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_ARegistryMapping_When_ASectionIsWrittenUpperCase_Then_CasingOfTheMappingIsUsed()
        {
            SetupRedirection(RedirectionPolicies.None);
            WritePrivateProfileStringW(sectionname, keyname, inivalue2, FileName);
            WritePrivateProfileStringW(sectionname.ToUpper(), keyname, inivalue, FileName);
            Assert.IsFalse(File.Exists(FileName));
            var sb = DefaultStringBuilder();
            // Insight: writing section in upper case, reading in normal case is possible
            GetIniString_SB_Ansi(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, "test.ini");
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_ARegistryMapping_When_AKeyIsWrittenUpperCase_Then_CasingOfTheKeyIsUsed()
        {
            SetupRedirection(RedirectionPolicies.None);
            WritePrivateProfileStringW(sectionname, keyname.ToUpper(), inivalue, FileName);
            Assert.IsFalse(File.Exists(FileName));
            using (var key = Registry.CurrentUser.OpenSubKey($@"Software\WelliSolutions\Test\{sectionname}"))
            {
                // Insight: it's written to the Registry using uppercase
                Assert.IsTrue(key.GetValueNames().Contains(keyname.ToUpper()));
            }
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_ARegistryKeyInUppercase_When_ReadingInNormalCase_Then_TheValueCanBeRead()
        {
            SetupRedirection(RedirectionPolicies.None);
            WritePrivateProfileStringW(sectionname, keyname.ToUpper(), inivalue, FileName);
            Assert.IsFalse(File.Exists(FileName));

            var sb = DefaultStringBuilder();
            // Insight: writing key in upper case, reading in normal case is possible
            GetIniString_SB_Ansi(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, "test.ini");
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [Checks(Paragraph.remarks)]
        [TestMethod]
        public void Given_AnExistingKeyInRegistry_When_AKeyIsWrittenUpperCase_Then_CasingOfTheExistingKeyIsUsed()
        {
            SetupRedirection(RedirectionPolicies.None);
            WritePrivateProfileStringW(sectionname, keyname, inivalue2, FileName);
            WritePrivateProfileStringW(sectionname, keyname.ToUpper(), inivalue, FileName);
            Assert.IsFalse(File.Exists(FileName));

            var sb = DefaultStringBuilder();
            // Insight: writing key in upper case, reading in normal case is possible
            GetIniString_SB_Ansi(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, "test.ini");
            AssertSbEqual(inivalue, sb);
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [TestMethod]
        public void Given_ARegistryMapping_When_AValueIsRead_Then_ItCanContainWhiteSpace()
        {
            SetupRedirection(RedirectionPolicies.PreventReadsFromFile);
            WritePrivateProfileStringW(sectionname, keyname, " \t\va\t\v ", FileName);
            EnsureDeleted();

            var sb = DefaultStringBuilder();
            // Insight: Reading the value from Registry returns the whitespace.
            // Note that this is different from reading from a file where whitespace is stripped when reading.
            GetIniString_SB_Ansi(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, "test.ini");
            AssertSbEqual(" \t\va\t\v ", sb);
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [TestMethod]
        public void Given_ARegistryMapping_When_AValueInQuotesIsWritten_Then_QuotesAreStoredInRegistry()
        {
            SetupRedirection(RedirectionPolicies.PreventReadsFromFile);
            foreach (var quote in new[] { "\"", "'" })
            {
                WritePrivateProfileStringW(sectionname, keyname, $"{quote}a{quote}", FileName);

                using (var key = Registry.CurrentUser.OpenSubKey($@"Software\WelliSolutions\Test\{sectionname}"))
                {
                    var value = (string)key.GetValue(keyname);
                    // Insight: it's written to the Registry with quotes
                    Assert.AreEqual($"{quote}a{quote}", value);
                }
            }
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.GetPrivateProfileStringW)]
        [TestMethod]
        public void Given_ARegistryMapping_When_AValueIsRead_Then_QuotesAreStripped()
        {
            SetupRedirection(RedirectionPolicies.PreventReadsFromFile);
            foreach (var outer in new[] { "\"", "'" })
            {
                foreach (var inner in new[] { "\"", "'" })
                {
                    WritePrivateProfileStringW(sectionname, keyname, $"{outer}{inner}a{inner}{outer}", FileName);

                    var sb = DefaultStringBuilder();
                    // Insight: Reading the value from Registry, quotes are stripped
                    GetIniString_SB_Ansi(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, "test.ini");
                    AssertSbEqual($"{inner}a{inner}", sb);
                }
            }
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Parameter.nSize)]
        [TestMethod]
        public void Given_TheUnicodeNatureOfRegistry_When_ReadingAValue_Then_WeGetMaximum32767Characters()
        {
            SetupRedirection(RedirectionPolicies.PreventReadsFromFile);
            var large = Limits_Tests.LargeString.Substring(0, 65535);
            WritePrivateProfileStringW(sectionname, keyname, large, FileName);

            var sb = new StringBuilder(65535 + 2);
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual(0, error);
            // Insight: The length of the string is different, because we have Unicode
            Assert.AreEqual((uint)65535 / 2, bytes);
            Assert.AreEqual((uint)sb.Length, bytes);
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Parameter.nSize)]
        [TestMethod]
        public void Given_AValueOfLength65537_When_ReadingTheValue_Then_WeGetModuloBehavior()
        {
            SetupRedirection(RedirectionPolicies.PreventReadsFromFile);
            var large = Limits_Tests.LargeString.Substring(0, 65537);
            WritePrivateProfileStringW(sectionname, keyname, large, FileName);

            var sb = new StringBuilder(65537 + 2);
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            var error = Marshal.GetLastWin32Error();
            Assert.AreEqual(0, error);
            // Insight: 65537 overflows modulo 65536
            Assert.AreEqual((uint)1, bytes);
            Assert.AreEqual((uint)sb.Length, bytes);
        }

        [UsedInDocumentation("RegistryRedirection.md")]
        [Checks(Method.WritePrivateProfileStringW)]
        [TestMethod]
        public void Given_ARegistryMapping_When_AKeyWithSemicolonIsWritten_Then_TheSemicolonIsInRegistry()
        {
            SetupRedirection(RedirectionPolicies.PreventReadsFromFile);

            WritePrivateProfileStringW(sectionname, ";" + keyname, inivalue, FileName);

            using (var key = Registry.CurrentUser.OpenSubKey($@"Software\WelliSolutions\Test\{sectionname}"))
            {
                var value = (string)key.GetValue(";" + keyname);
                // Insight: it's written to the Registry with quotes
                Assert.AreEqual(inivalue, value);
            }

        }
    }
}