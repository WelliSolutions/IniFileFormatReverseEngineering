using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.AssertionHelper;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests.SpecialCharacters
{
    [TestClass]
    public class Quotes_Tests : IniFileTestBase
    {
        [UsedInDocumentation]
        [TestsApiParameter("lpReturnedString")]
        [TestMethod]
        public void Given_AValueWithDifferentQuotes_When_TheValueIsRetrieved_Then_NoQuotesAreStripped()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}=  \"   {inivalue}   \'  \r\n");
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);

            // Insight: no quotes are stripped
            AssertASCIILength("\"   " + inivalue + "   \'", bytes);
            Assert.AreEqual(0, Marshal.GetLastWin32Error());
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpReturnedString")]
        [TestMethod]
        public void Given_AValueWithDoubleQuotationMarks_When_TheValueIsRetrieved_Then_TheQuotesAreStripped()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}=  \"   {inivalue}   \"  \r\n");
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);

            // Insight: the quotes are stripped
            // Insight: the spaces inside the quotes are not stripped
            // Insight: the spaces outside the quotes are stripped
            AssertASCIILength("   " + inivalue + "   ", bytes);
            Assert.AreEqual(0, Marshal.GetLastWin32Error());
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpReturnedString")]
        [TestMethod]
        public void Given_AValueWithSingleQuotationMarks_When_TheValueIsRetrieved_Then_TheQuotesAreStripped()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}=  \'   {inivalue}   \'  \r\n");
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);

            // Insight: the quotes are stripped
            // Insight: the spaces inside the quotes are not stripped
            // Insight: the spaces outside the quotes are stripped
            AssertASCIILength("   " + inivalue + "   ", bytes);
            Assert.AreEqual(0, Marshal.GetLastWin32Error());
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpReturnedString")]
        [TestMethod]
        public void Given_AValueWithQuotesInQuotes_When_TheValueIsRetrieved_Then_TheOutermostQuotesAreStripped()
        {
            foreach (var outerquote in new[] { '\'', '\"' })
            {
                foreach (var innerquote in new[] { '\'', '\"' })
                {
                    EnsureASCII($"[{sectionname}]\r\n{keyname}=  " + outerquote + innerquote + $"   {inivalue}   " +
                                innerquote + outerquote + "  \r\n");
                    var sb = DefaultStringBuilder();
                    var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);

                    // Insight: only the outermost quotes are stripped
                    AssertASCIILength(innerquote + "   " + inivalue + "   " + innerquote, bytes);
                    Assert.AreEqual(0, Marshal.GetLastWin32Error());

                }
            }
        }

        [UsedInDocumentation]
        [TestsApiParameter("lpReturnedString")]
        [TestMethod]
        public void Given_AValueWithQuotesInWrongOrder_When_TheValueIsRetrieved_Then_NoQuotesAreStripped()
        {
            EnsureASCII($"[{sectionname}]\r\n{keyname}=  \'\"   {inivalue}   \'\"  \r\n");
            var sb = DefaultStringBuilder();
            var bytes = GetIniString_SB_Unicode(sectionname, keyname, null, sb, (uint)sb.Capacity, FileName);

            // Insight: no quotes are stripped
            AssertASCIILength("\'\"   " + inivalue + "   \'\"", bytes);
            Assert.AreEqual(0, Marshal.GetLastWin32Error());
        }
    }
}