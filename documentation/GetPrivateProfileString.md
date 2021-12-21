# GetPrivateProfileString()

Navigate to [lpAppName](#lpAppName), [lpKeyName](#lpKeyName), [lpDefault](#lpDefault), [lpReturnedString](#lpReturnedString), [nSize](#nSize), [lpFileName](#lpFileName), [return value](#returnValue), [remarks](#remarks)

## Documentation at MSDN

The documentation of [GetPrivateProfileString() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring) does not mention it, but the documentation of the [GetPrivateProfileStringA() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringa) and [GetPrivateProfileStringW() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringw) versions do:

> The winbase.h header defines GetPrivateProfileString as an alias which automatically selects the ANSI or Unicode version of this function based on the definition of the  UNICODE preprocessor constant.

Just to make sure neither me nor Microsoft made a mistake, I compared the MSDN contents in WinMerge, with the following results:

| GetPrivateProfileString()                                    | GetPrivateProfileStringA()                                   | GetPrivateProfileStringW()                                   |
| ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| LPCTSTR                                                      | LPCSTR                                                       | LPCWSTR                                                      |
| In the event the initialization file specified by lpFileName is not found, or contains invalid values, **calling GetLastError will return** '0x2' (File Not Found). | In the event the initialization file specified by lpFileName is not found, or contains invalid values, **this function will set errorno with a value of** '0x2' (File Not Found). | In the event the initialization file specified by lpFileName is not found, or contains invalid values, **this function will set errorno with a value of** '0x2' (File Not Found). |

Other than that, the specification is identical, which is great.

This page has the same layout as the MSDN documentation, because I'll go through each of the sentences, cite them and check whether that statement is true or not.

<a name="syntax"></a>

## Syntax

```C++
DWORD GetPrivateProfileString(
  [in]  LPCTSTR lpAppName,
  [in]  LPCTSTR lpKeyName,
  [in]  LPCTSTR lpDefault,
  [out] LPTSTR  lpReturnedString,
  [in]  DWORD   nSize,
  [in]  LPCTSTR lpFileName
);
```

## Parameters

<a name="lpAppName"></a>

    [in] lpAppName

> The name of the section containing the key name.

Note that this parameter is called "AppName" and not "Section". It seems like the original intended use was that one application accesses only one section of the INI file. However, this must already have been obsolete when Microsoft introduced this method. The idea of a "private" INI file already means that only one application is expected to access it, because no other application would be aware of the file existence.

Test Coverage: 

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_TheContentIsAccessed_Then_WeGetTheExpectedValue()`
* `Casing_Tests.Given_AnSectionWithUpperCaseLetters_When_TheContentIsAccessedWithLowerCase_Then_WeGetTheExpectedValue()`
* `SquareBracket_Tests.Given_ASectionNameWithOpeningBracket_When_TheValueIsAccessed_Then_WeGetTheExpectedValue()`
* `SquareBracket_Tests.Given_ASectionNameWithClosingBracket_When_TheContentIsAccessed_Then_WeDontGetTheValue()`
* `SquareBracket_Tests.Given_AnEmptySectionName_When_WeAccessAKey_Then_WeGetTheValue()` 
* `SquareBracket_Tests.Given_ASectionNameWithMissingClosingBracket_When_WeAccessAKey_Then_WeGetTheValue()`
* `SquareBracket_Tests.Given_ASectionNameWithMissingOpeningBracket_When_WeAccessAKey_Then_WeDontGetTheValue()`

Insights:

* Basically, this functionality works as expected
* The section can be accessed case-insensitive
* The section name can be an empty string
* The section name can contain an opening square bracket. It will be part of the section name.
* The section name must not contain a closing square bracket. Parsing of the section name stops at the first closing square bracket.
* The section in the file needn't have a closing square bracket. Parsing of the section name will also end at the linebreak.

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

<a name="lpKeyName"></a>
```
[in] lpKeyName
```

> The name of the key whose associated string is to be retrieved.

Test Coverage: 

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_TheContentIsAccessed_Then_WeGetTheExpectedValue()`
* `Casing_Tests.Given_AnEntryWithUpperCaseLetter_When_TheContentIsAccessedWithLowerCase_Then_WeGetTheExpectedValue()`

Insights:

* Basically, this functionality works as expected.
* The key can be accessed case-insensitive
> If this parameter is **NULL**, all key names in the section specified by the *lpAppName* parameter are copied to the buffer specified by the *lpReturnedString* parameter.

Test Coverage:

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_NullIsUsedAsTheKey_Then_WeGetAListOfKeysInTheSection()`
* `IntendedUse_Reading_Tests.Given_AnIniFileWithDuplicateKeys_When_NullIsUsedAsTheKey_Then_WeGetDuplicateKeysAsWell`

Insights:

* Basically, this functionality works as described
* The key names are copied as zero-terminated strings. As such, you can't simply use the buffer as a string, but you need to consider the number of returned bytes and interpret the result.
* Duplicate keys are reported multiple times
<a name="lpDefault"></a>

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

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_NullIsTheDefaultValue_Then_WeGetAnEmptyString()`

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
<a name="lpReturnedString"></a>
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
  <a name="nSize"></a>
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
<a name="lpFileName"></a>
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
<a name="returnValue"></a>
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
<a name="remarks"></a>

## Remarks

My feeling is that the remarks on this function do not give us additional information. It's mostly a repetition of what has been said before when describing the parameters.

> The **GetPrivateProfileString** function searches the specified initialization file for a key that matches the name specified by the *lpKeyName* parameter under the section heading specified by the *lpAppName* parameter.

Test Coverage:

* `Casing_Tests.Given_AnSectionWithUpperCaseLetters_When_TheContentIsAccessedWithLowerCase_Then_WeGetTheExpectedValue()`
* `Casing_Tests.Given_AnEntryWithUpperCaseLetter_When_TheContentIsAccessedWithLowerCase_Then_WeGetTheExpectedValue()`

Insights:

* The section and the key can be accessed case independent

> If it finds the key, the function copies the corresponding  string to the buffer. 

Test Coverage:

* `IntendedUse_Reading_Tests.Given_ASmallBuffer_When_WeTryToGetTheValue_Then_TheValueIsTruncated()`
* `IntendedUse_Reading_Tests.Given_AZeroBuffer_When_WeTryToGetTheValue_Then_NothingCanBeReturned()`
* `Limits_Tests.Given_AValueOfLength65534_When_AccessingIt_Then_WeGetTheFullValue()`
* `Limits_Tests.Given_AValueOfLength65535_When_AccessingIt_Then_WeGetTheFullValueAndAnError()`
* `Limits_Tests.Given_AValueOfLength65536_When_AccessingIt_Then_WeGetNothingAndNoError()`
* `Limits_Tests.Given_AValueOfLength65537_When_AccessingIt_Then_WeGetModuloBehavior()`

Insights:

* Basically the statement is correct if we consider the limitations of the buffer size and modulo behavior. See [nSize](#nSize) for details.

> If the key does not exist, the function copies the default character string specified by the *lpDefault* parameter. 

Test Cases:

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_ANonExistingSectionIsAccessed_Then_WeGetTheDefaultValue()`
* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_ANonExistingKeyIsAccessed_Then_WeGetTheDefaultValue()`
* `IntendedUse_Reading_Tests.Given_NoIniFile_When_TheContentIsAccessed_Then_WeGetTheDefaultValue()`
* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_TheDefaultValueHasTrailingBlanks_Then_TheseBlanksAreStripped()` 
* `WhiteSpace_Tests.Given_ADefaultValueWithTrailingWhitespace_When_TheDefaultValueIsReturned_Then_OnlySpacesAreStripped()`

Insights:

* The default value is used when the key is missing, section is missing or file is missing.
* The default value is copied without trailing spaces.
* The default value is also limited by the size of the buffer.

> A section in the initialization file must have the following form:
>
> ​    [section]
> ​    key=string

Test Cases:

* Probably all tests

Insights:

* Yes, that's how an INI file looks like if we don't consider any of the special cases.

> If *lpAppName* is **NULL**, **GetPrivateProfileString** copies all section names in the specified file to the supplied buffer. If *lpKeyName* is **NULL**, the function copies all key names in the specified section to the supplied buffer. An application can use  this method to enumerate all of the sections and keys in a file.

Test Cases:

* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_NullIsUsedForSectionName_Then_WeGetAListOfZeroTerminatedSections()`
* `IntendedUse_Reading_Tests.Given_AnIniFileWithDuplicateSections_When_NullIsUsedForSectionName_Then_WeGetDuplicateSectionsAsWell()`
* `IntendedUse_Reading_Tests.Given_AnIniFileWithKnownContent_When_NullIsUsedAsTheKey_Then_WeGetAListOfKeysInTheSection()`
* `IntendedUse_Reading_Tests.Given_AnIniFileWithDuplicateKeys_When_NullIsUsedAsTheKey_Then_WeGetDuplicateKeysAsWell`

Insights:

* We can enumerate sections and keys that way
* An INI file which contains the same section multiple times or the same key multiple times will result in duplicate entries in the return value.

> In either case, each string is followed by a **null** character and the final string is followed by a second **null** character.

Test Coverage:

* `IntendedUse_Reading_Tests.Given_AKnownIniFile_When_NullIsUsedForSectionName_Then_SeparatorCharacterIsNul()`
* `IntendedUse_Reading_Tests.Given_AKnownIniFile_When_NullIsUsedForKeyName_Then_SeparatorCharacterIsNul()`

Insights:

* The separator is the NUL character `\0`.

> If the string associated with *lpKeyName* is enclosed in single or double quotation marks, the marks are discarded when the **GetPrivateProfileString** function retrieves the string.

Test Coverage:

* `IntendedUse_Reading_Tests.Given_AValueWithDoubleQuotationMarks_When_TheValueIsRetrieved_Then_TheQuotesAreStripped()`
* `IntendedUse_Reading_Tests.Given_AValueWithSingleQuotationMarks_When_TheValueIsRetrieved_Then_TheQuotesAreStripped()`
* `IntendedUse_Reading_Tests.Given_AValueWithQuotesInQuotes_When_TheValueIsRetrieved_Then_TheOutermostQuotesAreStripped()`
* `IntendedUse_Reading_Tests.Given_AValueWithDifferentQuotes_When_TheValueIsRetrieved_Then_NoQuotesAreStripped()`
* `IntendedUse_Reading_Tests.Given_AValueWithQuotesInWrongOrder_When_TheValueIsRetrieved_Then_NoQuotesAreStripped()`

Insights:

* Spaces outside the quotes are stripped
* Double quotes are stripped
* Single quotes are stripped
* Spaces inside quotes are not stripped
* Only the outermost quotes are stripped
* If the quotes do not match, they will not be stripped

Now, this is very interesting for a test that writes to an INI file. If the quotes are stripped when reading, they must be escaped when writing.

> The **GetPrivateProfileString** function is not case-sensitive; the strings can be a combination of uppercase and lowercase letters.

Test Coverage:

* `Casing_Tests.Given_AnSectionWithUpperCaseLetters_When_TheContentIsAccessedWithLowerCase_Then_WeGetTheExpectedValue()`
* `Casing_Tests.Given_AnEntryWithUpperCaseLetter_When_TheContentIsAccessedWithLowerCase_Then_WeGetTheExpectedValue()`

Insights:

* This statement is a bit misleading. The section name and the key name are not case sensitive. But the value is returned with the same casing as in the INI file.

