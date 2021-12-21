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
* `Hashtag_Tests.Given_AnIniFileWrittenWithHashtagInValue_When_TheContentIsAccessed_Then_WeGetTheHashtag()`
* `Hashtag_Tests.Given_AnIniFileWrittenWithHashtagInKey_When_TheContentIsAccessed_Then_WeGetTheHashtag()`

Insights:

* A semicolon in front of the key will make it a comment.
* Comment can even be written to the INI file using the API, prepending the semicolon to the key.
* A semicolon as part of the section will make it part of the section
* A semicolon in the middle of the key will make it part of the key.
* A semicolon in the middle of a value will make it part of the value, i.e. you can't have comments at the end of a line.
* Hashtags cannot be used for comments

Examples:

(Note that the Markdown syntax highlighter might provide a wrong highlighting considering the Microsoft rules)

```ini
[;this is a section]
;this=is a comment
this;is=not a comment
this is=not a ;comment
#this is=not a comment
this is=#not a comment
```

