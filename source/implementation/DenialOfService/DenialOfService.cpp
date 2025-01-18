#include <iostream>
#include <fstream>
#include <windows.h>

class IniFile {
public:
    IniFile(std::wstring filePath) : filePath(std::move(filePath)), hFile(INVALID_HANDLE_VALUE) {
        WriteIniFile();
        OpenFileExclusive();
    }

    ~IniFile() {
        if (hFile != INVALID_HANDLE_VALUE) {
            CloseHandle(hFile);
        }
        DeleteFile(filePath.c_str());
    }

    IniFile(const IniFile&) = delete;
    IniFile& operator=(const IniFile&) = delete;

    IniFile(IniFile&& other) noexcept : filePath(std::move(other.filePath)), hFile(other.hFile) {
        other.hFile = INVALID_HANDLE_VALUE;
    }

    IniFile& operator=(IniFile&& other) noexcept {
        if (this != &other) {
            if (hFile != INVALID_HANDLE_VALUE) {
                CloseHandle(hFile);
            }
            filePath = std::move(other.filePath);
            hFile = other.hFile;
            other.hFile = INVALID_HANDLE_VALUE;
        }
        return *this;
    }

    LPCWSTR c_str() const
    {
        return filePath.c_str();
    }

private:
    std::wstring filePath;
    HANDLE hFile;

    void WriteIniFile() const {
        std::ofstream iniFile(filePath);
        iniFile << L"[Section]\n";
        iniFile << L"Key=Value\n";
        iniFile.close();
    }

    void OpenFileExclusive() {
        hFile = CreateFile(filePath.c_str(), GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
        if (hFile == INVALID_HANDLE_VALUE) {
            std::wcerr << L"Failed to open file exclusively. Error: " << GetLastError() << '\n';
        }
        else {
            std::wcout << L"File opened exclusively." << '\n';
        }
        SetLastError(ERROR_SUCCESS);
    }
};

void TestGetPrivateProfileString(IniFile& ini) {
    wchar_t buffer[256];
    DWORD charsRead = GetPrivateProfileString(L"Section", L"Key", L"Default", buffer, sizeof(buffer) / sizeof(wchar_t), ini.c_str());
    if (charsRead > 0) {
        std::wcout << L"GetPrivateProfileString read: " << buffer << ". Error: " << GetLastError() << '\n';
    }
    else {
        std::wcerr << L"GetPrivateProfileString failed. Error: " << GetLastError() << '\n';
    }
    SetLastError(ERROR_SUCCESS);
}

void TestWritePrivateProfileString(IniFile& ini) {
    BOOL result = WritePrivateProfileString(L"Section", L"Key", L"NewValue", ini.c_str());
    if (result) {
        std::wcout << L"WritePrivateProfileString succeeded." << '\n';
    }
    else {
        std::wcerr << L"WritePrivateProfileString failed. Error: " << GetLastError() << '\n';
    }
    SetLastError(ERROR_SUCCESS);
}

int main() {
    IniFile iniFile(L"b:\\test.ini");

    TestGetPrivateProfileString(iniFile);
    TestWritePrivateProfileString(iniFile);
}
