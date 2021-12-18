using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests
{
    [TestClass]
    public class IntendedUse_Reading_Tests : IniFileTestBase
    {
        [DoNotRename("Used in documentation")]
        [TestsApiParameter("lpAppName")]
        [TestsApiParameter("lpKeyName")]
        [TestsApiParameter("lpFileName")]
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

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("lpAppName", null)]
        [TestMethod]
        public void Given_AnIniFileWithKnownContent_When_NullIsUsedForSectionName_Then_WeGetAListOfZeroTerminatedSections()
        {
            EnsureDefaultContent_UsingAPI();
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(null, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);

            // Insight: when using the StringBuilder, we get only 1 section name,
            // although there are 2 section names inside.
            // This is probably more a problem of the C# method signature,
            // but it tells us what can go wrong with with implementations that do not have unit tests :-)
            AssertSbEqual(sectionname, sb);

            // Insight: the section names are (probably) delimited by \0. At least the length matches.
            var length = (uint)Encoding.ASCII.GetBytes(sectionname + '\0').Length;
            var length2 = (uint)Encoding.ASCII.GetBytes(sectionname2 + '\0').Length;
            Assert.AreEqual(length + length2, bytes);
        }

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("lpAppName", null)]
        [TestMethod]
        public void Given_AnIniFileWithDuplicateSections_When_NullIsUsedForSectionName_Then_WeGetDuplicateSectionsAsWell()
        {
            EnsureASCII($"[{sectionname}]\r\n[{sectionname2}]\r\n[{sectionname}]\r\n");
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(null, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);


            // Insight: the length matches if we consider one section twice
            var length = (uint)Encoding.ASCII.GetBytes(sectionname + '\0').Length;
            var length2 = (uint)Encoding.ASCII.GetBytes(sectionname2 + '\0').Length;
            Assert.AreEqual(length * 2 + length2, bytes);
        }

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("lpKeyName", null)]
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

        [DoNotRename("Used in documentation")]
        [TestsApiParameter("lpKeyName", null)]
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

        // TODO: Review
        [TestMethod]
        public void Given_AnIniFileWithKnownContent_When_ANonExistingSectionIsAccessed_Then_WeGetTheDefaultValue()
        {
            EnsureDefaultContent_UsingAPI();
            var sb = DefaultStringBuilder();

            // Insight: reading from a non-existing section gives the default value
            var bytes = GetIniString_SB_Unicode("NonExistingSection", keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            AssertASCIILength(defaultvalue, bytes);
            AssertSbEqual(defaultvalue, sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_NoIniFile_When_TheContentIsAccessed_Then_WeGetTheDefaultValue()
        {
            EnsureDeleted();
            var sb = DefaultStringBuilder();

            // Insight: if the INI file does not exist, we get the default value
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, defaultvalue, sb, (uint)sb.Capacity, FileName);
            Assert.AreEqual((uint)Encoding.ASCII.GetBytes(defaultvalue).Length, bytes);
            AssertSbEqual(defaultvalue, sb);
        }

        // TODO: Review
        [TestMethod]
        public void Given_NoIniFile_When_NullIsTheDefaultValue_Then_WeGetAnEmptyString()
        {
            EnsureDeleted();
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname.ToLower(), keyname, null, sb, (uint)sb.Capacity, FileName);
            AssertZero(bytes);
            // According the documentation, NULL should lead to an empty string
            AssertSbEqual(string.Empty, sb);
        }
    }
}