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

So, the Write... methods have more information and I'll follow that.

## Remarks

> The system keeps a cached version of the most recent registry file mapping to improve performance. If all parameters are **NULL**, the function flushes the cache. 

The order of explanations is weird: why does it explain caching before it explains what Registry File Mapping is?

> While the system is editing the cached  version of the file, processes that edit the file itself will use the  original file until the cache has been cleared.

That's always the hard thing about caches and sort of expected.

What does Microsoft mean by "cleared"? Is "cleared" the same thing as "flushed"?

> The system maps most .ini file references to the registry, using the mapping defined under the following registry key:
> `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\IniFileMapping`

"Most .INI files" seems to refer to the next sentence about Windows INI files. Again, the order of explanations is weird.

> This mapping is likely if an application modifies system-component  initialization files, such as Control.ini, System.ini, and Winfile.ini.

IMHO, such a mapping is not "likely". Either a mapping entry exists or it doesn't. As a programmer you don't want to rely on the likeliness of something.

> In this case, the function writes information to the registry, not to the initialization  file; 

It's unclear to me what "in this case" means? Until now, it wasn't explained that you could create such a mapping yourself. Does that mean it applies to "most .INI files" or "the system" only? No, it doesn't.

Test Coverage:

* `RegistryRedirection_Tests.Given_ARegistryMapping_When_AValueIsWritten_Then_TheFileIsNotCreated()` 
* `RegistryRedirection_Tests.Given_AWriteThroughMapping_When_AValueIsWritten_Then_TheFileIsCreated()`
* `RegistryRedirection_Tests.Given_ARegistryMapping_When_AValueIsWritten_Then_TheValueIsInRegistry()` 
* `RegistryRedirection_Tests.Given_ARegistryMappingAndNoFile_When_AValueIsRead_Then_TheValueIsFromRegistry()` 
* `RegistryRedirection_Tests.Given_ARegistryMappingAndAFile_When_AValueIsInTheRegistry_Then_TheValueIsFromRegistry()`
* `RegistryRedirection_Tests.Given_ARegistryMapping_When_TheSectionIsMapped_Then_TheValueIsReadFromRegistryOnly()`
* `RegistryRedirection_Tests.Given_ARegistryMapping_When_TheSectionIsNotMapped_Then_TheValueIsReadFromTheFile()`
* `RegistryRedirection_Tests.Given_ARegistryMapping_When_ASectionIsWrittenUpperCase_Then_CasingOfTheMappingIsUsed()`
* `RegistryRedirection_Tests.Given_ARegistryMapping_When_AKeyIsWrittenUpperCase_Then_CasingOfTheKeyIsUsed()`
* `RegistryRedirection_Tests.Given_ARegistryKeyInUppercase_When_ReadingInNormalCase_Then_TheValueCanBeRead()`
* `RegistryRedirection_Tests.Given_AnExistingKeyInRegistry_When_AKeyIsWrittenUpperCase_Then_CasingOfTheExistingKeyIsUsed()`

Insights:

* Basically this works as expected: it redirects reading and writing to the Registry. 
* This also applies to INI mappings that you have created yourself (not only to system .INI files).
* A file is not created if the mapping uses  no special policy, `@` or `#`.
* A file is created if the mapping uses the `!` policy ("write through").
* Values of a mapped section that exist in the file but not in the Registry will return the default value.
* Values of a section that is not mapped will be read from the file.
* Casing of the section at writing is ignored. It will use the casing of the defined redirection.
* Casing of the key is ignored. The key will be written as given the first time.

> the change in the storage location has no effect on the function's behavior.

Actually, the behavior is quite different:

Test Coverage:

* `RegistryRedirection_Tests.Given_ARegistryMapping_When_AValueIsWritten_Then_ItCanContainNewlines()`
* `RegistryRedirection_Tests.Given_ARegistryMapping_When_AValueIsRead_Then_ItCanContainNewlines()` 
* `RegistryRedirection_Tests.Given_ARegistryMapping_When_AValueIsRead_Then_ItCanContainWhiteSpace()`
* `RegistryRedirection_Tests.Given_TheUnicodeNatureOfRegistry_When_ReadingAValue_Then_WeGetMaximum32767Characters()`
* `RegistryRedirection_Tests.Given_AValueOfLength65537_When_ReadingTheValue_Then_WeGetModuloBehavior()`
* `RegistryRedirection_Tests.Given_ARegistryMapping_When_AKeyWithSemicolonIsWritten_Then_TheSemicolonIsInRegistry()`

Insights:

* In contrast to the file, a single Registry value can contain newlines.
* In contrast of reading a file, whitespace will not be stripped when reading from the Registry.
* The Registry is always stored in Unicode. Given the modulo 65536 overflow, you can only read values of 32767 characters, whereas you could read values of length 65535 from ASCII files. Although the Registry would be capable of storing 1 MB of data.
* In contrast to a file, keys with a semicolon in front can be read.  They will not turn into a comment.

Functionality that is identical:

Test Coverage:

* `RegistryRedirection_Tests.Given_ARegistryMapping_When_AValueInQuotesIsWritten_Then_QuotesAreStoredInRegistry()` 
* `RegistryRedirection_Tests.Given_ARegistryMapping_When_AValueIsRead_Then_QuotesAreStripped()` 

Insights:

* Single and double quotes are written to the Registry.
* Quotes are stripped when reading.

> The profile functions use the following steps to locate initialization information:
>
> 1. Look in the registry for the name of the initialization file  under the IniFileMapping key.
> 2. Look for the section name specified by *lpAppName*. This will  be a named value under the key that has the name of the initialization  file, or a subkey with this name, or the name will not exist as either a value or subkey.
> 3. If the section name specified by *lpAppName* is a named value, then that value specifies where in the registry you will find the keys for the section.
> 4. If the section name specified by *lpAppName* is a subkey, then  named values under that subkey specify where in the registry you will  find the keys for the section. If the key you are looking for does not  exist as a named value, then there will be an unnamed value (shown as **<No Name>**) that specifies the default location in the registry where you will find the key.
> 5. If the section name specified by *lpAppName* does not exist as a named value or as a subkey, then there will be an unnamed value (shown as **<No Name>**) that specifies the default location in the registry where you will find the keys for the section.
> 6. If there is no subkey or entry for the section name, then look for  the actual initialization file on the disk and read its contents.

Test Coverage:

* `RegistryRedirection_Tests.Given_ARegistryMapping_When_TheSectionIsMapped_Then_TheValueIsReadFromRegistryOnly()`

Insights:

* If the section is defined for redirection and a key does not exist, it will not be read from the file.

> When looking at values in the registry that specify other registry  locations, there are several prefixes that change the behavior of the  .ini file mapping: 			 		
>
> - ! - this character forces all writes to go both to the registry and to the .ini file on disk.
> - \# - this character causes the registry value to be set to the value  in the Windows 3.1 .ini file when a new user logs in for the first time  after setup.
> - @ - this character prevents any reads from going to the .ini file on disk if the requested data is not found in the registry.
> - USR: - this prefix stands for HKEY_CURRENT_USER, and the text after the prefix is relative to that key.
> - SYS: - this prefix stands for HKEY_LOCAL_MACHINE\SOFTWARE, and the text after the prefix is relative to that key.



> An application using the WritePrivateProfileString function to enter .ini file information into the registry should follow these guidelines:

These guidelines do not only apply to writing to the Registry. They also apply if you read from the Registry only and e.g. use a setup procedure (installer) to add the values to the Registry (without using WritePrivateProfileString).

> - Ensure that no .ini file of the specified name exists on the system.

Note that you can hardly make such a guarantee. There might not be such a file now, but it might be added by the installation of an additional application.

The Registry Keys that you need to create cannot contain a backslash, so you can not specify full file names including the path. Thus, only the filename itself will be part of the Registry and *all* access will be redirected, independent of the directory that is specified by *lpFileName*. This is potentially dangerous and could affect other applications.

> - Ensure that there is a key entry in the registry that specifies the .ini file. This entry should be under the path HKEY_LOCAL_MACHINE\SOFTWARE \Microsoft\Windows NT\CurrentVersion\IniFileMapping.

The name "key entry" might be misleading. Here, they are talking about a Registry Key, not a key of an INI file.

> - Specify a value for that .ini file key entry that specifies a  section. That is to say, an application must specify a section name, as  it would appear within an .ini file or registry entry. Here is an  example: [My Section].

By "specify a value" Microsoft means a "create a REG_SZ". The name of that REG_SZ must not include the square brackets.

> - For system files, specify SYS for an added value.

IMHO it's not a matter of whether the file is a system file or not. It's a matter of where you want the values to be stored. SYS will store in HKLM, USR will store in HKCU.

> - For application files, specify USR within the added value. Here is  an example: "My Section: USR: App Name\Section". And, since USR  indicates a mapping under HKEY_CURRENT_USER, the application should also create a key under HKEY_CURRENT_USER that specifies the application name listed in the added value. For the example just given, that would be "App Name".

The example is misleading. "My Section" is the name of the REG_SZ and "USR:App Name\Section" is the value of the REG_SZ. It seems to be best practice to use "USR:Software\App Name\Section".

> - After following the preceding steps, an application setup program should call WritePrivateProfileString with the first three parameters set to NULL, and the fourth parameter set to the INI file name. For example: 
>
>   `WritePrivateProfileString( NULL, NULL, NULL, L"appname.ini" );`
>
> - Such a call causes the mapping of an .ini file to the registry to  take effect before the next system reboot. The system rereads the  mapping information into shared memory. A user will not have to reboot  their computer after installing an application in order to have future  invocations of the application see the mapping of the .ini file to the  registry.

