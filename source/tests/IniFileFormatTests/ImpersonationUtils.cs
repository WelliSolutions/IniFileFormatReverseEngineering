using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace IniFileFormatTests.IntendedUse
{
    public static class ImpersonationUtils
    {
        private const int SW_SHOW = 5;
        private const int TOKEN_QUERY = 0x0008;
        private const int TOKEN_DUPLICATE = 0x0002;
        private const int TOKEN_ASSIGN_PRIMARY = 0x0001;
        private const int STARTF_USESHOWWINDOW = 0x00000001;
        private const int STARTF_FORCEONFEEDBACK = 0x00000040;
        private const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        private const int TOKEN_IMPERSONATE = 0x0004;
        private const int TOKEN_QUERY_SOURCE = 0x0010;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const int TOKEN_ADJUST_GROUPS = 0x0040;
        private const int TOKEN_ADJUST_DEFAULT = 0x0080;
        private const int TOKEN_ADJUST_SESSIONID = 0x0100;
        private const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private const int TOKEN_ALL_ACCESS =
            STANDARD_RIGHTS_REQUIRED |
            TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE |
            TOKEN_IMPERSONATE |
            TOKEN_QUERY |
            TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES |
            TOKEN_ADJUST_GROUPS |
            TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID;

        [StructLayout(LayoutKind.Sequential)]
        private struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        private enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        private enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool DuplicateTokenEx(
            IntPtr hExistingToken,
            int dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            int ImpersonationLevel,
            int dwTokenType,
            ref IntPtr phNewToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(
            IntPtr ProcessHandle,
            int DesiredAccess,
            ref IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(
            IntPtr hObject);

        private static IDisposable Impersonate(IntPtr token)
        {
            var identity = new WindowsIdentity(token);
            return identity.Impersonate();
        }

        private static IntPtr GetPrimaryToken(Process process)
        {
            var token = IntPtr.Zero;
            var primaryToken = IntPtr.Zero;

            if (OpenProcessToken(process.Handle, TOKEN_DUPLICATE, ref token))
            {
                var sa = new SECURITY_ATTRIBUTES();
                sa.nLength = Marshal.SizeOf(sa);

                if (!DuplicateTokenEx(
                        token,
                        TOKEN_ALL_ACCESS,
                        ref sa,
                        (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                        (int)TOKEN_TYPE.TokenImpersonation, // .TokenPrimary,
                        ref primaryToken))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "DuplicateTokenEx failed");
                }

                CloseHandle(token);
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "OpenProcessToken failed");
            }

            return primaryToken;
        }


        public static IDisposable ImpersonateCurrentUser()
        {
            var process = Process.GetProcessesByName("explorer").FirstOrDefault();
            if (process != null)
            {
                var token = GetPrimaryToken(process);
                if (token != IntPtr.Zero)
                {
                    return Impersonate(token);
                }
            }

            throw new Exception("Could not find explorer.exe");
        }
    }
}