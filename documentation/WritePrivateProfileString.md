# WritePrivateProfileString()

Navigate to [lpAppName](#lpAppName), [lpKeyName](#lpKeyName), [lpString](#lpString), [lpFileName](#lpFileName), [return value](#returnValue)

## Documentation at MSDN

In contrast to [GetPrivateProfileString()](GetPrivateProfileString.md), MSDN does not have an extra page for the neutral version, although such a definition exists.

> The winbase.h header defines WritePrivateProfileString as an alias which automatically selects the ANSI or Unicode version of this function  based on the definition of the UNICODE preprocessor constant. 

Just to make sure neither me nor Microsoft made a mistake, I compared the MSDN contents in WinMerge, with the following results:

| WritePrivateProfileString() | WritePrivateProfileStringA() | WritePrivateProfileStringW() |
| --------------------------- | ---------------------------- | ---------------------------- |
| -- missing --               | LPCSTR                       | LPCWSTR                      |

Other than that, the specification is identical, which is great.

This page has the same layout as the MSDN documentation, because I'll go through each of the sentences, cite them and check whether that statement is true or not.

<a name="syntax"></a>

## Syntax

There is no definition in `winbase.h` which uses `LPCTSTR` as the type, so we have the two definitions for ANSI and Unicode:

```c++
BOOL WritePrivateProfileStringA(
  [in] LPCSTR lpAppName,
  [in] LPCSTR lpKeyName,
  [in] LPCSTR lpString,
  [in] LPCSTR lpFileName
);
```

```c++
BOOL WritePrivateProfileStringW(
  [in] LPCWSTR lpAppName,
  [in] LPCWSTR lpKeyName,
  [in] LPCWSTR lpString,
  [in] LPCWSTR lpFileName
);
```

## Parameters

<a name="lpAppName"></a>

    [in] lpAppName

> The name of the section to which the string will be copied. If the section does not exist, it is created

Test Coverage:

* `Writing_Tests.Given_AnExistingEmptyFile_When_AValueIsWritten_Then_TheFileContainsSectionKeyAndValue()`

Insights:

* The section name is enclosed in square brackets
* Windows line endings are used
* Key and value are separated by an equal sign
* If the file was created from scratch, it has an empty line at the end

> The name of the section is case-independent; the string can be any combination of uppercase and lowercase letters.

The case independency is probably more a matter of reading than writing.

Test Coverage:

* `Writing_Tests.Given_AnExistingEmptyFile_When_AValueIsWritten_Then_TheFileContainsSectionKeyAndValue()`
* `Writing_Tests.Given_ASectionNameNotOnlyLetters_When_WritingTheSection_Then_ItsAccepted()`
* `Writing_Tests.Given_ASectionNameContainingAParagraph_When_WritingTheSection_Then_ItBecomesAQuestionmark`
* `WhiteSpace_Tests.Given_AnIniFileWrittenWithSpaces_When_TheContentIsWritten_Then_SpacesAreStripped()`
* `Semicolon_Tests.Given_AnIniFileWrittenWithSemicolonInSection_When_TheContentIsAccessed_Then_WeGetTheSemicolon()`

Insights:

* If the section is created from scratch, it will use the casing as given.
* The section name cannot be letters only. It can also contain numbers and many special characters. Allowed characters: at least `1234567890!$%&/()=?*+#-_<>.,:;@~|\` , double quotes, single quotes, space, tab and vertical tab.
* While the section name can be passed with leading or trailing spaces, these spaces will not be written to the file.
* Special characters that are not accepted will result in Unicode replacement marks. Affected characters: at least `§€°´²³` 
* Semicolons will be part of the key. They will be written inside the square brackets. Semicolons do not turn the section into a comment.

<a name="lpKeyName"></a>

```
[in] lpKeyName
```

> The name of the key to be associated with a string. If the key does not exist in the specified section, it is created.

Test Coverage:

* `Writing_Tests.Given_AnExistingEmptyFile_When_AValueIsWritten_Then_TheFileContainsSectionKeyAndValue()`
* `EdgeCases_Tests.Given_AnEmptySectionName_When_WritingAValue_Then_ASectionWithoutNameIsCreated()`
* `Writing_Tests.Given_AKeyNameNotOnlyLetters_When_WritingTheSection_Then_ItsAccepted()`
* `Semicolon_Tests.Given_AnIniFileWrittenWithSemicolonAtBeginOfKey_When_TheContentIsAccessed_Then_WeGetTheDefaultValue()`
* `Semicolon_Tests.Given_AnIniFileWithASemicolonAtBeginOfKey_When_AllKeysAreRetrieved_Then_WeDontGetTheComment()`
* `Semicolon_Tests.Given_AnIniFileWrittenWithSemicolonInKey_When_TheContentIsAccessed_Then_WeGetTheSemicolon()`
* `WhiteSpace_Tests.Given_AnIniFileWrittenWithSpaces_When_TheContentIsWritten_Then_SpacesAreStripped()`

Insights:

* Basically works as expected
* The section name can also be an empty string.
* Like section name, the key can also consist of special characters
* The key must not start with a semicolon, otherwise it will turn into a comment. There will be an equal sign in the comment.
* A semicolon in the middle of the key becomes part of the key.
* While the key may be passed with leading or trailing spaces, these spaces will not be written to the file.

> If this parameter is **NULL**, the entire section, including all entries within the section, is deleted.

With this special interpretation of the parameter, the function violates clean code principles, e.g. that a function should only do one thing (book: Clean Code, Robert C. Martin).

Test Coverage:

* `Semicolon_Tests.Given_AnIniFileWithExistingComments_When_DeletingASection_Then_TheyAreNotDeleted()`

Insights:

* Comments will not be deleted when a section is deleted

<a name="lpString"></a>

```
[in] lpString
```

> A **null**-terminated string to be written to the file. 

Test Coverage:

* `Semicolon_Tests.Given_AnIniFileWrittenWithSemicolonInValue_When_TheContentIsAccessed_Then_WeGetTheSemicolon()`
* `Semicolon_Tests.Given_AnIniFile_When_ACommentIsWrittenViaTheValueAndAnEmptyKey_Then_ItsNotAComment()`

Insights:

* Values starting with a semicolon are not comments. The value starting with the semicolon will be written (and returned when reading). Even when the key is an empty string.

> If this parameter is **NULL**, the key pointed to by the *lpKeyName* parameter is deleted.

Test Coverage:

* 

Insights:

* 

<a name="lpFileName"></a>

```
[in] lpFileName
```

> The name of the initialization file.

Test Coverage:

* `Writing_Tests.Given_AnExistingEmptyFile_When_AValueIsWritten_Then_TheFileContainsSectionKeyAndValue()`
* `Writing_Tests.Given_ANonExistingFile_When_AValueIsWritten_Then_TheFileIsCreated()`
* `Writing_Tests.Given_ANonExistingFileInWindowsDirectory_When_AValueIsWritten_Then_WeGetAFileNotFoundError()` 
* `Writing_Tests.Given_AFileInANonExistingDirectory_When_AValueIsWritten_Then_WeGetAPathNotFoundError()`

Insights:

* Basically it works as expected
* If the file does not exist, it will be created (if possible). In that case, the return value is `true` and `GetLastError()` is `ERROR_FILE_NOT_FOUND`.
* Files cannot be created in the Windows directory with normal permissions. In that case, the return value is `false` and the error code of `GetLastError()` is `ERROR_FILE_NOT_FOUND`.
* Any non-existing subdirectories will not be created. In that case, the return value is `false` and `GetLastError()` is `ERROR_PATH_NOT_FOUND`.

> If the file was created using Unicode characters, the function writes Unicode characters to the file. Otherwise, the function writes ANSI characters.

Test Coverage:

* `UTF16LE_Tests.Given_AFileWithUTF16Header_When_WritingToTheFile_Then_WeHaveUnicodeSupport()`
* `UTF8_Tests.Given_AFileWithUTF8BOM_When_WritingToTheFile_Then_WeGetReplacementCharacters()`
* `UTF16BE_Tests.Given_UTF16BEBOM_When_WritingToTheFile_Then_ContentIsANSI()`

Insights:

* A UTF-8 BOM will break the first line of the file. If it e.g. contains a section, that section cannot be accessed. Other than that, the content will be treated as ANSI.
* A UTF-16 Little Endian BOM will bring Unicode support to both writing methods, `WritePrivateProfileStringA()` and `WritePrivateProfileStringW()` as well as both reading methods `GetPrivateProfileStringA()` and `GetPrivateProfileStringW()`.
* A UTF-16 Big Endian BOM will result in ANSI characters being written.

<a name="returnValue"></a>

## Return Value

>  If the function successfully copies the string to the initialization file, the return value is nonzero.

Test Coverage:

* `Writing_Tests.Given_AnExistingEmptyFile_When_AValueIsWritten_Then_TheFileContainsSectionKeyAndValue()`
* `Writing_Tests.Given_ANonExistingFile_When_AValueIsWritten_Then_TheFileIsCreated()`

Insights:

* A return value of `true` may still give a `GetLastError()` of `ERROR_FILE_NOT_FOUND` if the file did not exist but was created.

> If the function fails, or if it flushes the cached version of the most  recently accessed initialization file, the return value is zero. To get  extended error information, call [GetLastError](https://docs.microsoft.com/en-us/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror).

What is that "flushing"? When will it happen? If it happens randomly, will we get random errors?

Test Coverage:

* 

Insights:

* 

<a name="remarks"></a>

## Remarks

> A section in the initialization file must have the following form:
>
>  [section]
>  key=string

Test Coverage:

* 

Insights:

* 

> If the *lpFileName* parameter does not contain a full path and file name for the file, **WritePrivateProfileString** searches the Windows directory for the  file. If the file does not exist, this function creates the file in the  Windows directory.

Probably: it will attempt to create the file and succeed if the user has enough permissions.

Test Coverage:

* 

Insights:

* 

> If *lpFileName* contains a full path and file name and the file does not exist, **WritePrivateProfileString** creates the file. The specified directory must already exist.

Test Coverage:

* 

Insights:

* 