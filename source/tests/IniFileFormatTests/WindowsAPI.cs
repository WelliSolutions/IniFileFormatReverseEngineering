using System;
using System.Runtime.InteropServices;
using System.Text;

namespace IniFileFormatTests
{
    static class WindowsAPI
    {
        internal enum GetLastError
        {
            /// <summary>
            /// The operation completed successfully.
            /// </summary>
            SUCCESS = 0,
            /// <summary>
            /// The system cannot find the file specified.
            /// </summary>
            ERROR_FILE_NOT_FOUND = 2,
            /// <summary>
            /// The system cannot find the path specified.
            /// </summary>
            ERROR_PATH_NOT_FOUND = 3,
            /// <summary>
            /// Access is denied.
            /// </summary>
            ERROR_ACCESS_DENIED = 5,
            /// <summary>
            /// The filename, directory name, or volume label syntax is incorrect.
            /// </summary>
            ERROR_INVALID_NAME = 123,
            /// <summary>
            /// More data is available.
            /// </summary>
            ERROR_MORE_DATA = 234,

        }

        // TODO: Copy documentation from MSDN
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpAppName">The name of the section containing the key name. If this parameter is NULL, the GetPrivateProfileString function copies all section names in the file to the supplied buffer.</param>
        /// <param name="lpKeyName">The name of the key whose associated string is to be retrieved. If this parameter is NULL, all key names in the section specified by the lpAppName parameter are copied to the buffer specified by the lpReturnedString parameter.</param>
        /// <param name="lpDefault">A default string. If the lpKeyName key cannot be found in the initialization file, GetPrivateProfileString copies the default string to the lpReturnedString buffer.
        /// If this parameter is NULL, the default is an empty string, "".
        /// Avoid specifying a default string with trailing blank characters. The function inserts a null character in the lpReturnedString buffer to strip any trailing blanks.</param>
        /// <param name="lpReturnedString">A pointer to the buffer that receives the retrieved string.</param>
        /// <param name="nSize">The size of the buffer pointed to by the lpReturnedString parameter, in characters.</param>
        /// <param name="lpFileName">The name of the initialization file. If this parameter does not contain a full path to the file, the system searches for the file in the Windows directory.</param>
        /// <returns>The return value is the number of characters copied to the buffer, not including the terminating null character.
        /// If neither lpAppName nor lpKeyName is NULL and the supplied destination buffer is too small to hold the requested string, the string is truncated and followed by a null character, and the return value is equal to nSize minus one.
        /// If either lpAppName or lpKeyName is NULL and the supplied destination buffer is too small to hold all the strings, the last string is truncated and followed by two null characters. In this case, the return value is equal to nSize minus two.
        /// In the event the initialization file specified by lpFileName is not found, or contains invalid values, calling GetLastError will return '0x2' (File Not Found). To retrieve extended error information, call GetLastError.</returns>
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetIniString_SB_Unicode(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedString,
            uint nSize,
            string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetIniString_ChArr_Unicode(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            [In, Out] char[] lpReturnedString,
            uint nSize,
            string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern uint GetIniString_ByteArr_Ansi(
            string lpAppName,
            string lpKeyName,
            string lpDefault,
            [In, Out] byte[] lpReturnedString,
            uint nSize,
            string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint GetPrivateProfileInt(
            string lpAppName,
            string lpKeyName,
            int nDefault,
            string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint GetPrivateProfileSection(
            string lpAppName,
            IntPtr lpszReturnBuffer,
            uint nSize,
            string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint GetPrivateProfileSectionNames(
            IntPtr lpszReturnBuffer,
            uint nSize,
            string lpFileName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lpAppName">The name of the section to which the string will be copied. If the section does not exist, it is created. The name of the section is case-independent; the string can be any combination of uppercase and lowercase letters.</param>
        /// <param name="lpKeyName">The name of the key to be associated with a string. If the key does not exist in the specified section, it is created. If this parameter is NULL, the entire section, including all entries within the section, is deleted.</param>
        /// <param name="lpString">A null-terminated string to be written to the file. If this parameter is NULL, the key pointed to by the lpKeyName parameter is deleted.</param>
        /// <param name="lpFileName">The name of the initialization file.
        /// If the file was created using Unicode characters, the function writes Unicode characters to the file. Otherwise, the function writes ANSI characters.</param>
        /// <returns>If the function successfully copies the string to the initialization file, the return value is nonzero.
        /// If the function fails, or if it flushes the cached version of the most recently accessed initialization file, the return value is zero.To get extended error information, call GetLastError.</returns>
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileStringW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WritePrivateProfileString(
            string lpAppName,
            string lpKeyName,
            string lpString,
            string lpFileName);
    }
}
