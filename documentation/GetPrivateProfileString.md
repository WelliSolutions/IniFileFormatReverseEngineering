# GetPrivateProfileString()

## Differences

The documentation of [GetPrivateProfileString() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring) does not mention it, but the documentation of the [GetPrivateProfileStringA() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringa) and [GetPrivateProfileStringW() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringw) versions do:

> The winbase.h header defines GetPrivateProfileString as an alias which automatically selects the ANSI or Unicode version of this function based on the definition of the  UNICODE preprocessor constant.

Just to make sure neither me nor Microsoft made a mistake, I compared the MSDN contents in WinMerge, with the following results:

| GetPrivateProfileString()                                    | GetPrivateProfileStringA()                                   | GetPrivateProfileStringW()                                   |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| LPCTSTR                                                      | LPCSTR                                                       | LPCWSTR                                                      |
| In the event the initialization file specified by lpFileName is not found, or contains invalid values, **calling GetLastError will return** '0x2' (File Not Found). | In the event the initialization file specified by lpFileName is not found, or contains invalid values, **this function will set errorno with a value of** '0x2' (File Not Found). | In the event the initialization file specified by lpFileName is not found, or contains invalid values, **this function will set errorno with a value of** '0x2' (File Not Found). |

Other than that, the specification is identical, which is great.

## Parameters

    [in] lpAppName

> The name of the section containing the key name.

Note that this parameter is called "AppName" and not "Section". It seems like the original intended use was that one application accesses only one section of the INI file. However, this must already have been obsolete when Microsoft introduced this method. The idea of a "private" INI file already means that only one application is expected to access it, because no other application would be aware of the file existence.

Test Coverage: 

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_TheContentIsAccessed_Then_WeGetTheExpectedValue()`

Insights:

* Basically, this functionality works as expected

> If this parameter is **NULL**, the **GetPrivateProfileString** function copies all section names in the file to the supplied buffer.

With this special interpretation of the parameter, the function violates clean code principles, e.g. that a function should only do one thing (book: Clean Code, Robert C. Martin).

Test Coverage: 

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_NullIsUsedForSectionName_Then_WeGetAListOfZeroTerminatedSections()`
* `IntendedUse_Reading_Tests.Given_AnIniFileWithDuplicateSections_When_NullIsUsedForSectionName_Then_WeGetDuplicateSectionsAsWell()`

Insights:

* Basically, this functionality works as described
* The section names are copied without square brackets (as expected) 
* The section names are copied as zero-terminated strings. As such, you can't simply use the buffer as a string, but you need to consider the number of returned bytes and interpret the result.
* Duplicate sections are reported multiple times

```
[in] lpKeyName
```

> The name of the key whose associated string is to be retrieved.

Test Coverage: 

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_TheContentIsAccessed_Then_WeGetTheExpectedValue()`

Insights:

* Basically, this functionality works as expected.
> If this parameter is **NULL**, all key names in the section specified by the *lpAppName* parameter are copied to the buffer specified by the *lpReturnedString* parameter.

Test Coverage:

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_NullIsUsedAsTheKey_Then_WeGetAListOfKeysInTheSection()`
* `IntendedUse_Reading_Tests.Given_AnIniFileWithDuplicateKeys_When_NullIsUsedAsTheKey_Then_WeGetDuplicateKeysAsWell`

Insights:

* Basically, this functionality works as described
* The key names are copied as zero-terminated strings. As such, you can't simply use the buffer as a string, but you need to consider the number of returned bytes and interpret the result.
* Duplicate keys are reported multiple times

