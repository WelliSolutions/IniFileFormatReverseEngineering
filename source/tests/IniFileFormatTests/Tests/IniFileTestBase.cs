using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static IniFileFormatTests.WindowsAPI;

namespace IniFileFormatTests
{
    public abstract class IniFileTestBase
    {
        internal IniFileTestBase()
        {
            FileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ini");
        }

        ~IniFileTestBase()
        {
            EnsureDeleted();
        }

        protected string FileName;

        protected void EnsureASCII(string contents)
        {
            EnsureFileContent(contents, Encoding.ASCII);
        }

        private void EnsureFileContent(string contents, Encoding encoding)
        {
            File.WriteAllText(FileName, contents, encoding);
        }

        protected string ReadIniFile()
        {
            return File.ReadAllText(FileName);
        }

        protected void EnsureDefaultContent_UsingAPI()
        {
            EnsureDeleted();
            WritePrivateProfileStringW(sectionname, keyname, inivalue, FileName);
            WritePrivateProfileStringW(sectionname2, keyname2, inivalue2, FileName);
        }

        protected void EnsureDefaultContent_UsingFile()
        {
            EnsureASCII(DefaultContent);
        }

        protected void EnsureDefaultContent_UsingFile(Encoding encoding)
        {
            EnsureFileContent(DefaultContent, encoding);
        }

        private string DefaultContent => $"[{sectionname}]\r\n{keyname}={inivalue}\r\n[{sectionname2}]\r\n{keyname2}={inivalue2}\r\n";


        protected void EnsureDeleted()
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
        }

        protected void EnsureEmptyASCII()
        {
            EnsureASCII("");
        }

        protected void EnsureEmptyUTF16()
        {
            EnsureUTF16("");
        }

        protected void EnsureUTF16(string content)
        {
            File.WriteAllText(FileName, content, Encoding.Unicode);
        }

        protected string sectionname = "SectionName";
        protected string keyname = "TestKey";
        protected string inivalue = "TestValue";

        protected string sectionname2 = "SectionName2";
        protected string keyname2 = "TestKey2";
        protected string inivalue2 = "TestValue2";

        protected string defaultvalue = "defaultValue";

        protected static StringBuilder DefaultStringBuilder()
        {
            var sb = new StringBuilder(1024);
            sb.EnsureCapacity(1024);
            return sb;
        }

        [TestCleanup]
        public void TearDown()
        {
            EnsureDeleted();
        }
    }
}