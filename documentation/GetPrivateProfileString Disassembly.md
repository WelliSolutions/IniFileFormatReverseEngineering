# GetPrivateProfileString Disassembly

## How to disassemble using WinDbg

1. Use [Search Everything](https://www.voidtools.com/) and find a `kernel32.dll` of reasonable size, like 600 kB or more.
2. Install [WinDbg Preview](https://www.microsoft.com/en-us/p/windbg-preview/9pgjgd53tn86#activetab=pivot:overviewtab) and run it. 
3. When you see an empty command window, drag'n'drop the `kernel32.dll` onto that window. This will treat the DLL like a crash dump file, which is ok.
4. Check whether the methods exist, using `x *!GetPrivateProfile*`. It should list the `...A()` and `...W()` methods. The neutral ones only exist for the compiler.
5. Disassemble a function using `uf kernel32!GetPrivateProfileStringA` and similar.

## How to disassemble using IDA Free

1. Use [Search Everything](https://www.voidtools.com/) and find a `kernel32.dll` of reasonable size, like 600 kB or more.
2. Install [IDA Free 7.6](https://hex-rays.com/ida-free/#download) and run it

## How to disassemble using Ghidra

1. Install [Ghidra](https://ghidra-sre.org/)

## Default value handling

The following C++ code snippet was created with Ghidra. Ghidra will not detect all variable names, so I renamed some of them.

```C++
// Calculation of the default value length
defaultValueLength = 0xffffffffffffffff;     // -1
do {
   defaultValueLength = defaultValueLength + 1;
} while (lpDefaultValue[defaultValueLength] != '\0');
uVar2 = (uint)defaultValueLength;

// Stripping whitespace: space only
_MaxCount = defaultValueLength & 0xffffffff;
while ((nCharsCopiedToBuffer = (DWORD)defaultValueLength, uVar2 != 0 &&
       (uVar2 = nCharsCopiedToBuffer - 1, lpDefaultValue[uVar2] == ' '))) {
   _MaxCount = (ulonglong)uVar2;
   defaultValueLength = (ulonglong)uVar2;
}

if (size == 0) {
   BaseSetLastNTError();
   return 0;
}
if (size <= (uint)_MaxCount) {
   nCharsCopiedToBuffer = size - 1;
   _MaxCount = (ulonglong)nCharsCopiedToBuffer;
}
uVar2 = (uint)_MaxCount;

// Copy the default value into the return buffer
strncpy_s(lpReturnedString,_SizeInBytes,lpDefaultValue,_MaxCount);
```

