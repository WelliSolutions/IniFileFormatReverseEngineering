# INI file-format reverse engineering
This place is intended to provide details of the INI file format and INI file APIs **as supported by Microsoft**.

## What is the INI file format?

Basically it's a key-value-store with a few limitations. The intended use looks like this:

```ini
[section1]
key1=value1
;next line defines key2
key2=value2
[section2]
;keys can repeat in another section
key1=value1
key2=value2
```

Straight-forward you may think. But it is a file format that has no specification - which is unfortunate. You can [read more on Wikipedia](https://en.wikipedia.org/wiki/INI_file). I will dissect the statements from there as soon as I have enough evidence.

File formats without a real specification seem to be popular again recently (like JSON, Markdown), after we went through a period of potentially over-specified file formats (like XML, of course with DTD only).

## Why?

At least to my experience (working full time for three companies), there are still a lot of applications out there that store configuration information in INI files.

Parsing INI files seems trivial and I have written at least three INI file parsers in my life already - and probably none of them was 100% compatible to the INI file format of the Windows API - at least when it comes to humans editing the file in a text editor. They all "worked", sort of.

So, before I implement the next INI file parser, I want to make sure I understand what Microsoft does and provide a compatible implementation, and maybe a configurable one in order to be able to convert files from one INI dialect into another.

## How?

For the moment I'll follow the law of the instrument and make progress using the tools I'm familiar with. As there are

* Visual Studio
* .NET Framework
* Unit Tests

This approach should quickly give me some insights.

Later, I could try a few things I'm not overly comfortable with, like

* C++

* Reverse Engineering using disassembly in WinDbg

* Reverse Engineering using IDA Free

at which point I'd certainly appreciate someone of the RCE community. At least I hope that I have found enough evidence before, so that I can always confirm my reverse engineering against the results of the unit tests.

## What's the problem?

The problem? [Many problems ;-)](documentation/Problems%20of%20INI%20files.md)

## Documentation

Analysis of [GetPrivateProfileString()](documentation/GetPrivateProfileString.md)

Analysis of [WritePrivateProfileString()](documentation/WritePrivateProfileString.md)

Analysis of [Comments](documentation/Comments.md)

Analysis of [Registry Redirection](documentation/RegistryRedirection.md)

## References

* [English Wikipedia about the INI-file](https://en.wikipedia.org/wiki/INI_file)
* [Question "Did INI files work in a different way on Windows 3.x than today?" on Retrocomputing](https://retrocomputing.stackexchange.com/questions/23503/did-ini-files-work-in-a-different-way-on-windows-3-x-than-today)
* [Raymond Chen in "The old new thing": Why are INI files deprecated in favor of the Registry?](https://devblogs.microsoft.com/oldnewthing/?p=24383) Raymond Chen is a Microsoft employee writing a lot of blog posts
* [Michael S. Kaplan in "Unicode INI function; Unicode INI file?"](http://archives.miloush.net/michkap/archive/2006/09/15/754992.html) Michael S. Kaplan was a Microsoft employee writing blog posts until he died

Top questions on Stack Overflow regarding INI files:

* [Reading/writing an INI file](https://stackoverflow.com/questions/217902/reading-writing-an-ini-file)
* [Do Standard INI files allow comments?](https://stackoverflow.com/questions/1378219/do-standard-windows-ini-files-allow-comments)
* [What is the easiest way to parse an INI File in C++?](https://stackoverflow.com/questions/12633/what-is-the-easiest-way-to-parse-an-ini-file-in-c) (closed as off-topic, because "easiest" is opinion based, the Microsoft way might not be the easiest, but probably quite compatible)

Methods for reading INI files, focusing on the "private" ones. The non-private ones will only read from `c:\windows\win.ini`:

* Reading text
  * [GetPrivateProfileString() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring)
  * [GetPrivateProfileStringA() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringa)
  * [GetPrivateProfileStringW() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringw)
  
* Reading structs
  * [GetPrivateProfileStruct() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestruct)
  * [GetPrivateProfileStructA() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestructa)
  * [GetPrivateProfileStructW() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestructw)
  
* Reading numbers
  * [GetPrivateProfileInt() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofileint)
  * [GetPrivateProfileIntA() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofileinta)
  * [GetPrivateProfileIntW() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofileintw)
  
* Reading sections
  * [GetPrivateProfileSection() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilesection)
  * [GetPrivateProfileSectionA() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilesectiona)
  * [GetPrivateProfileSectionW() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilesectionw)
  * [GetPrivateProfileSectionNames() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilesectionnames)
  * [GetPrivateProfileSectionNamesA() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilesectionnamesa)
  * [GetPrivateProfileSectionNamesW() [MSDN]](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilesectionnamesw)
  


The Registry key that maps INI files is at

    HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\IniFileMapping

## Implementations

Of course, people have implemented INI parsers already. My implementations are not published, luckily :-)

* [NuGet: INI-Parser](https://www.nuget.org/packages/ini-parser/) License: MIT
* [NuGet: Peanut Butter.INI](https://www.nuget.org/packages/PeanutButter.INI/) License: BSD 3-clause
* [CodeProject: An INI file handling class using C#](https://www.codeproject.com/Articles/1966/An-INI-file-handling-class-using-C) License: unclear. Problems: has a limit of 254 characters for reading. 
* [Bytes: Reading and parsing an INI file in C#](https://bytes.com/topic/net/insights/797169-reading-parsing-ini-file-c). License: unclear. Problems: does not strip spaces from the values
* [Multipedros: Confing.dll](http://www.multipetros.gr/public-projects/libraries/confing-dll/) has a `SimpleIni` and a `Ini` class. License: FreeBSD
* [Github: Madmilkman.ini](https://github.com/MarioZ/MadMilkman.Ini) License: MIT.
* [Github: simpleini](https://github.com/brofield/simpleini). License: MIT.
* [SourceForge: libini](https://sourceforge.net/projects/libini/) License: unclear.
* [Github: inih](https://github.com/benhoyt/inih) License: New BSD.
* [Github: inipp](https://github.com/mcmtroffaes/inipp) License: MIT.
* [Maybe a dozen implementations answering this Stack Overflow question](https://stackoverflow.com/questions/217902/reading-writing-an-ini-file)

## Giving back to the community

As a result of my research I came across a few things and I can hopefully give back to the community, to whomever is interested. I left my traces here:

* Stack Overflow: [Do Standard INI files allow comments?](https://stackoverflow.com/a/70432963/480982)
* Stack Overflow: [How do WritePrivateProfileStringA() and Unicode go together?](https://stackoverflow.com/questions/70438143/how-do-writeprivateprofilestringa-and-unicode-go-together)
* Stack Overflow: [Why does File.ReadAllText() also recognize UTF-16 encodings?](https://stackoverflow.com/questions/70445598/why-does-file-readalltext-also-recognize-utf-16-encodings)
* Stack Overflow: [What are the differences in functionality when INI files are mapped to the Registry?](https://stackoverflow.com/questions/70500724/what-are-the-differences-in-functionality-when-ini-files-are-mapped-to-the-regis)
* Super User: [What's the largest REG_SZ value that Regedit can edit?](https://superuser.com/questions/1696002/whats-the-largest-reg-sz-value-that-regedit-can-edit)
