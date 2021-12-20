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

```
[in] lpDefault
```

> A default string. If the *lpKeyName* key cannot be found in the initialization file, **GetPrivateProfileString** copies the default string to the *lpReturnedString* buffer.

Test coverage:

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_ANonExistingSectionIsAccessed_Then_WeGetTheDefaultValue()`
* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_ANonExistingKeyIsAccessed_Then_WeGetTheDefaultValue()`
* `IntendedUse_Reading_Tests.Given_NoIniFile_When_TheContentIsAccessed_Then_WeGetTheDefaultValue()`

Insights:

* Basically, this functionality works as described
* This does not only work when the "key cannot be found", it also works if the section cannot be found
* This does not only work when the "key cannot be found", it also works if the file cannot be found

> If this parameter is **NULL**, the default is an empty string, "".

Test Coverage:

`IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_NullIsTheDefaultValue_Then_WeGetAnEmptyString()`

Insights:

* Basically, this functionality works as described

> Avoid specifying a default string with trailing blank characters. The function inserts a **null** character in the *lpReturnedString* buffer to strip any trailing blanks.

This stripping of "blanks" requires us to conduct a set of additional whitespace tests.

Test Coverage:

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_TheDefaultValueHasTrailingBlanks_Then_TheseBlanksAreStripped()` 
* `WhiteSpace_Tests.Given_ADefaultValueWithTrailingWhitespace_When_TheDefaultValueIsReturned_Then_OnlySpacesAreStripped()`

Insights:

* Basically, this functionality works as described.
* Leading spaces are not stripped for the default value.
* "Blank" refers to the space character only, not tab, vertical tab, carriage return and newline.

```
[out] lpReturnedString
```
For all the C# programmers out there: `[out]` is not identical to `out` parameters as in C#. Obviously this pointer must be valid and point to a buffer where the function writes the data to. I'm not sure whether these API calls are defined in MIDL, but at least the [MIDL definition of [out] [MSDN]](https://docs.microsoft.com/en-us/windows/win32/midl/out-idl#remarks) would match.

> A pointer to the buffer that receives the retrieved string.

Test Coverage:
* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_TheContentIsAccessed_Then_TrailingSpacesAreStripped()`
* `WhiteSpace_Tests.Given_AnIniFileWithKnownContent_When_TheContentIsAccessed_Then_BlanksAreStripped()`

Insights:

* From a developer's point of view, I'd say that this functionality does not work as expected. Yes, it reads a value from the file and the buffer is filled, but ...
* It does not *retrieve the received string*. Depending on the the special parameters, the buffer contains many *string**s***, which may be keys or sections. And there's no explanation what the string delimiter is.
* Leading blanks (space, tab and vertical tab) that exist in the file are stripped
* Trailing blanks (space, tab and vertical tab) that exist in the file are stripped

Why could whitespace stripping be done? Probably because some people formatted their INI files in columns for "readability" like so:
* ```ini
  [section]
  key=       value
  key2=      anothervalue
  longerkey= differentvalue
  ```

```
[in] nSize
```
> The size of the buffer pointed to by the *lpReturnedString* parameter, in characters.

This is a very interesting parameter, because it's potentially security relevant. If you manage to pass in a value larger than the buffer size, you'll get a buffer overflow with the typical consequences of undefined behavior.

My guess is that a lot of implementations will not consider the case when the buffer size is *nSize* and the return value is *nSize-1*. In that case you would need to increase the buffer size to get the full value - or you might decide that such a value is impossible and just go on with what you got. A human modifying the file manually might wonder why his value is cut off.

*nSize* is defined as a *DWORD*, which is *typedef*'d as an *unsigned long* (32 bit). So in theory, we could have a 4 GB buffer. But it turns out that it's limited to 65536 (16 bit) - well, this function is a relict of 16 bit Windows.

Test Coverage:

* `IntendedUse_Reading_Tests.Given_ASmallBuffer_When_WeTryToGetTheValue_Then_TheValueIsTruncated()`
* `IntendedUse_Reading_Tests.Given_AZeroBuffer_When_WeTryToGetTheValue_Then_NothingCanBeReturned()`
* `Limits_Tests.Given_AValueOfLength65534_When_AccessingIt_Then_WeGetTheFullValue()`
* `Limits_Tests.Given_AValueOfLength65535_When_AccessingIt_Then_WeGetTheFullValueAndAnError()`
* `Limits_Tests.Given_AValueOfLength65536_When_AccessingIt_Then_WeGetNothingAndNoError()`
* `Limits_Tests.Given_AValueOfLength65537_When_AccessingIt_Then_WeGetModuloBehavior()`

Insights:

* Values are truncated as described, resulting in a return value of *nSize-1*.
* A zero size buffer returns a zero bytes result (as expected).
* If the buffer is small and the value is truncated, `GetLastError()` returns `ERROR_MORE_DATA` (234) most of the times (exception see below). See the [list of error messages [MSDN]](https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-).
* The maximum length of a value that can be read without an error is 65534 bytes.
* The maximum length of a value that can be read is 65535 bytes in which case `GetLastError()` returns `ERROR_MORE_DATA` (234)  (although there is no more data)
* Values of *nSize*>=65535 will overflow modulo 65536 and there's no error from `GetLastError()`.
```
[in] lpFileName
```
> The name of the initialization file.

Test Coverage:

* `IntendedUse_Reading.Given_AnInvalidFileName_When_ReadingFromTheFile_Then_WeGetAnError()`
* `IntendedUse_Reading.Given_AFileNameWithoutExtension_When_ReadingFromTheFile_Then_WeGetTheValue()`
* `IntendedUse_Reading.Given_AFileNameWithArbitraryExtension_When_ReadingFromTheFile_Then_WeGetTheValue()`

Insights:

* Basically, the functionality works as described.
* Files do not need to have an extension.
* Devices such as `PRN`, `COM1` and `LPT` result in a `GetLastError()` of `ERROR_FILE_NOT_FOUND`.
* Invalid file names such as `*`, `?` and `C:\C:\` result in a `GetLastError()` of `ERROR_INVALID_NAME`.
* Special names such as an empty file name, `.` and `..` which effectively point to a directory result in a `GetLastError()` of `ERROR_ACCESS_DENIED`.

> If this parameter does not contain a full path to the file, the system searches for the file in the Windows  directory.

There are not many INI files in the Windows directory any more. There is still `system.ini` and `win.ini`, but they both have almost no content. Reading from those files is no issue. However, writing a file to the Windows directory requires elevated permissions.

Test Coverage:

* `Limits_Test.Given_ALongFileNameTooLong_When_ReadingFromTheFile_Then_ThePathIsNotFound()`

Insights:

* Basically the functionality works as described.
* The limitation for the full file name is `MAX_PATH` (260) and results in a `GetLastError()` of `ERROR_PATH_NOT_FOUND`.

## Return Value

> The return value is the number of characters copied to the buffer, not including the terminating **null** character.

TestCoverage:

* probably all test in `IntendedUse_Reading`
* `Limits_Tests.Given_AValueOfLength65534_When_AccessingIt_Then_WeGetTheFullValue()`
* `Limits_Tests.Given_AValueOfLength65535_When_AccessingIt_Then_WeGetTheFullValueAndAnError()`
* `Limits_Tests.Given_AValueOfLength65536_When_AccessingIt_Then_WeGetNothingAndNoError()`
* `Limits_Tests.Given_AValueOfLength65537_When_AccessingIt_Then_WeGetModuloBehavior()`

Insights:

* Basically the functionality works as described, but ...
* The number of characters copied to the buffer is not always the number you'd expect due to the 16 bit overflow.

> If neither *lpAppName* nor *lpKeyName* is **NULL** and the supplied destination buffer is too small to hold the requested string, the string is truncated and followed by a **null** character, and the return value is equal to *nSize* minus one.

Test Cases:

* `IntendedUse_Reading.Given_AZeroBuffer_When_WeTryToGetTheValue_Then_NothingCanBeReturned()`
* `IntendedUse_Reading.Given_ASmallBuffer_When_WeTryToGetTheValue_Then_TheValueIsTruncated()`

Insights:

* Basically works as expected

> If either *lpAppName* or *lpKeyName* is **NULL** and the supplied destination buffer is too small to hold all the strings, the last string is truncated and followed by two **null** characters. In this case, the return value is equal to *nSize* minus two.

Test Coverage:

* `IntendedUse_Reading.Given_ATooSmallBuffer_When_NullIsUsedForSectionName_Then_SizeIsBytesMinusTwo()`
* `IntendedUse_Reading.Given_ATooSmallBuffer_When_NullIsUsedForKeyName_Then_SizeIsBytesMinusTwo()`
* `Limits_Test.Given_ATooSmallBuffer_When_NullIsUsedForKeyName_Then_SizeIsNotNegative()` 

Insights:

* Basically it works as expected.
* If *nSize* is smaller than 2, the function returns 0

> In the event the initialization file specified by *lpFileName* is not found, or contains invalid values, calling **GetLastError** will return '0x2' (File Not Found). To retrieve extended error information, call [GetLastError](https://docs.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-getlasterror).

In C#, always use `Marshal.GetLastWin32Error()` and do not try to declare a P/Invoke method for `GetLastError()`. The .NET framework may have made WinAPI calls internally and you would get the result of such a last call. [Details on Stack Overflow](https://stackoverflow.com/questions/17918266/winapi-getlasterror-vs-marshal-getlastwin32error).

The interesting thing here is the statement "contains invalid values". That's not described in detail.

Test Coverage:

* `IntendedUse_Reading.Given_AnInvalidFileName_When_ReadingFromTheFile_Then_WeGetAnError()`
* `IntendedUse_Reading.Given_NoIniFile_When_TheContentIsAccessed_Then_WeGetTheDefaultValue()`

Insights:

* The "FileNotFound" case works as expected.

## Remarks
