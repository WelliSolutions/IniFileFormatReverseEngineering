# Problems of INI files

Whenever you define a file format, you should think about the special characters you use. Remember the backslash `\` as the escape character in many programming languages?

For the INI file, we have:

* opening square bracket `[`
* closing square bracket `]`
* semicolon `;`
* equal sign `=`
* newlines, probably Windows-like `\r\n`

and IMHO there's no escape character. I can already imagine all the possible abuses :-)

And we potentially have other problems:

* Encoding issues
* Unicode issues
* Handling of empty lines
* Handling of duplicate sections
* Handling of duplicate keys
* Handling of spaces
* Handling of keys outside a section

Problems mentioned by [Raymond Chen](https://devblogs.microsoft.com/oldnewthing/?p=24383) in 2007:

* Unicode issues (16 bit Windows did not support Unicode)
* Security on file level only, not on key level
* Multithreading issues (16 bit Windows was cooperatively multitasked) resulting in data loss
* Denial of service attacks by opening a file in exclusive mode
* String content only. Binary data must be encoded yourself.
* Performance: each operation will parse the whole file
* Buggy programs dealing with INI files leading to a practical limit of 70 characters for a string
* Limitation of the file size at 32 kB (I can't confirm this: it's possible to read 65535 characters from a single entry)
* Default directory being the Windows directory
* No nesting. Only two levels of structure.
* No central administration

## Denial of service attack

If an INI file is opened by an external program exclusively, the Windows API returns the default value for `GetPrivateProfileString()` and `GetLastError()` is ERROR_SHARING_VIOLATION (32). Only ERROR_FILE_NOT_FOUND (2) is mentioned in the documentation at the time of writing. This means: getting the default value does not necessarily mean that this is a good value to use (it only is in case the file was not found and you actually want the default).

You can find the C++ code for this experiment in [source/implementation/DenialOfService](../source/implementation/DenialOfService).