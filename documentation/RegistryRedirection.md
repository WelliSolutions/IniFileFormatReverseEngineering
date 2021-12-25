# Registry Redirection

## Documentation at MSDN

The Registry Redirection is documented in several places:

* [GetPrivateProfileStringW [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringw), [GetPrivateProfileStringA [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringa) and [GetPrivateProfileString [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring), which are almost identical
* [WritePrivateProfileStringA [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-writeprivateprofilestringa) and [WritePrivateProfileStringW [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-writeprivateprofilestringw) which are also almost identical

In all these pages, the Registry Redirection is handled under the *Remarks* section.

Comparing the Get... against the Write... methods in WinMerge, we find:

| Get... methods                                               | Write... methods                                             |
| ------------------------------------------------------------ | ------------------------------------------------------------ |
| -- missing --                                                | The system keeps a cached version of the most recent registry file mapping to improve performance. If all parameters are NULL, the function flushes the cache. While the system is editing the cached version of the file, processes that edit the file itself will use the original file until the cache has been cleared. |
| This mapping is likely if an application modifies system-component initialization files, such as Control.ini, System.ini, and Winfile.ini. In **these** case**s**, the function **retrieves** information **from** the registry, not **from** the initialization file; the change in the storage location has no effect on the function's behavior. | This mapping is likely if an application modifies system-component initialization files, such as Control.ini, System.ini, and Winfile.ini. In **this** case, the function **writes** information **to** the registry, not **to** the initialization file; the change in the storage location has no effect on the function's behavior. |
| -- missing --                                                | An application using the WritePrivateProfileString function to enter .ini file information into the registry should follow these guidelines:<br />- Ensure that no .ini file of the specified name exists on the system.<br />- Ensure that there is a key entry in the registry that specifies the .ini file. This entry should be under the path HKEY_LOCAL_MACHINE\SOFTWARE \Microsoft\Windows NT\CurrentVersion\IniFileMapping.<br />- Specify a value for that .ini file key entry that specifies a  section. That is to say, an application must specify a section name, as  it would appear within an .ini file or registry entry. Here is an  example: [My Section].<br />- For system files, specify SYS for an added value.<br />- For application files, specify USR within the added value. Here is  an example: "My Section: USR: App Name\Section". And, since USR  indicates a mapping under HKEY_CURRENT_USER, the application should also create a key under HKEY_CURRENT_USER that specifies the application name listed in the added value. For the example just given, that would be "App Name".<br />- After following the preceding steps, an application setup program should call WritePrivateProfileString with the first three parameters set to NULL, and the fourth parameter set to the INI file name. For example: `WritePrivateProfileString( NULL, NULL, NULL, L"appname.ini" );`<br />- Such a call causes the mapping of an .ini file to the registry to  take effect before the next system reboot. The system rereads the  mapping information into shared memory. A user will not have to reboot  their computer after installing an application in order to have future  invocations of the application see the mapping of the .ini file to the  registry. |

So, the Write... methods have more information.

## Remarks
