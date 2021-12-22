# Comments

I have read the following Microsoft documentation carefully and I did not come across a definition of comments in INI files:

* [GetPrivateProfileString() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring) 
* [GetPrivateProfileStringA() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringa)
* [GetPrivateProfileStringW() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringw)

Still, [Wikipedia claims](https://en.wikipedia.org/wiki/INI_file#Comments):

> Semicolons (*;*) at the beginning of the line indicate a comment. Comment lines are ignored.

And [later](https://en.wikipedia.org/wiki/INI_file#Comments_2):

> Some software supports the use of the [number sign](https://en.wikipedia.org/wiki/Number_sign) (#) as an alternative to the semicolon for indicating comments, especially under Unix, where it mirrors [shell](https://en.wikipedia.org/wiki/Bourne_shell) comments. The number sign might be included in the key name in other  dialects and ignored as such. For instance, the following line may be  interpreted as a comment in one dialect, but create a variable named  "#var" in another dialect. If the "#var" value is ignored, it would form a pseudo-implementation of a comment.

and

> In some implementations, a comment may begin anywhere on a line after a  space (inline comments), including on the same line after properties or  section declarations.

Similar for [German Wikipedia](https://de.wikipedia.org/wiki/Initialisierungsdatei):

> Außerdem erlaubt das Dateiformat Kommentarzeilen, diese beginnen mit einem Semikolon.
>
> `; Kommentar`    

So, what is true for the Microsoft implementation?

Test Cases:

* `Semicolon_Tests.Given_AnIniFileWrittenWithSemicolonAtBeginOfKey_When_TheContentIsAccessed_Then_WeGetTheDefaultValue()`
* `Semicolon_Tests.Given_AnIniFileWrittenWithSemicolonInValue_When_TheContentIsAccessed_Then_WeGetTheSemicolon()`
* `Semicolon_Tests.Given_AnIniFileWrittenWithSemicolonInKey_When_TheContentIsAccessed_Then_WeGetTheSemicolon()`
* `Semicolon_Tests.Given_AnIniFileWrittenWithSemicolonInSection_When_TheContentIsAccessed_Then_WeGetTheSemicolon()`
* `Semicolon_Tests.Given_AnIniFileWithASemicolonAtBeginOfKey_When_AllKeysAreRetrieved_Then_WeDontGetTheComment()`
* `SquareBracket_Tests.Given_KeyValueBehindClosingSquareBracket_When_WeTryToAccessTheValue_Then_WeDontGetTheValue()`
* `SquareBracket_Tests.Given_AValueWithoutAnySection_When_WeTryToAccessIt_Then_WeDontGetTheValue()`
* `Semicolon_Tests.Given_AnIniFileWithSpacesBeforeTheSemicolon_When_TheContentIsAccessed_Then_ItsStillAComment()`

Insights:

* A semicolon in front of the key will make it a comment. It can be preceded by space, tab or vertical tab.
* Comment can even be written to the INI file using the API, prepending the semicolon to the key.
* A semicolon as part of the section will make it part of the section
* A semicolon in the middle of the key will make it part of the key.
* A semicolon in the middle of a value will make it part of the value, i.e. you can't have comments at the end of a line.
* The way sections are parsed makes text after the closing square bracket also a comment, even without a semicolon
* Values without any section are ignored by the parser, effectively making text before the first section a comment

Due to the overflow at 65536 characters, everything beyond that will not be part of the value either. I'd not call that a comment, though. Microsoft could change the implementation at any time in order to allow longer values.

Examples:

(Note that the Markdown syntax highlighter might provide a wrong highlighting considering the Microsoft rules)

```ini
this is=a comment
[;this is a section]
;this is=a comment
      ;this is=also a comment
this;is=not a comment
this is=not a ;comment
#this is=not a comment
this is=#not a comment
[section]this is=a comment
```

## Hashtags and other comment specifiers

Test coverage:

* `Hashtag_Tests.Given_AnIniFileWrittenWithHashtagInValue_When_TheContentIsAccessed_Then_WeGetTheHashtag()`
* `Hashtag_Tests.Given_AnIniFileWrittenWithHashtagInKey_When_TheContentIsAccessed_Then_WeGetTheHashtag()`

Insights:

* Hashtags cannot be used for comments

While hashtags are not a comment as per Microsoft, you'll still find that they serve as comments pretty well. Why is that?

Most applications will probably access a configuration value using hardcoded values for the file name, section name and key name. In that case, the key will not be found, because the application is looking for a key without the hashtag.

This applies for all other characters as well, so you could try C++ comments (`//`), Visual Basic comments (`'`) , Batch comments (`REM`) and in many cases you'll find that the application starts using the default value.

If, however, the application queries all keys by [specifying `null` for `lpKeyName`](documentation/GetPrivateProfileString.md#lpKeyName), your value will be found unless it uses a semicolon (`;`).

## Where do comments belong to?

Another potential issue with comments is, where they belong to. We don't have inline comments, so we have to choose whether to write comments above or below the line the comment belongs to. This might be important to know when keys or sections are deleted.

Example:

```ini
;Is this a comment for the whole INI file (outside a section) or does it belong to Section1?
[Section1]
;Does this comment belong to Section1 or to Key1?
Key1=Value1
;Does this comment belong to Section2 or to Key1?
[Section2]
```

