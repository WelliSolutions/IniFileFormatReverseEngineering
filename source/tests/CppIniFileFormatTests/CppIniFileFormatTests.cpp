#include "pch.h"
#include "CppUnitTest.h"
#include <Windows.h>
#include <vector>
#include <string>
#include <print>
#include <format>
#include <filesystem>
#include <fstream>
#include <numeric>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace PrivateProfileStructTests
{
	TEST_CLASS(Checksum)
	{
		const std::wstring section{ L"TestSection" };
		const std::wstring key{ L"TestKey" };
		const std::wstring filename{ (std::filesystem::current_path() / L"test.ini").c_str() };

		/// <summary>
		/// Writes a binary structure to a test section and a test key of an INI file using WritePrivateProfileStruct().
		/// </summary>
		/// <param name="data">Binary data as a replacement for a real struct</param>
		void write_struct(const std::vector<std::byte>& data) const
		{
			auto pStruct = const_cast<std::byte*>(data.data());
			DWORD size = static_cast<DWORD>(data.size());
			auto result = WritePrivateProfileStruct(section.data(), key.data(), pStruct, size, filename.data());
			if (!result) {
				auto error = GetLastError();
				throw std::exception(std::format("WritePrivateProfileStruct failed. Error {}", error).c_str());
			}
		}

		/// <summary>
		/// Reads a string from the test section and test key in an INI file using GetPrivateProfileString().
		/// </summary>
		/// <returns>Value of the test section and test key</returns>
		std::wstring read_string() const
		{
			std::wstring buffer(65536, L'\0');
			DWORD const size = static_cast<DWORD>(buffer.size());
			DWORD const len = GetPrivateProfileStringW(section.data(), key.data(), nullptr, buffer.data(), size, filename.data());
			buffer.resize(len);
			return buffer;
		}

		/// <summary>
		/// Calculates the checksum: sum of all bytes in the data structure, modulo 256.
		/// </summary>
		/// <param name="data">Binary data as a replacement for a real struct</param>
		/// <returns>Checksum</returns>
		unsigned char compute_checksum(const std::vector<std::byte>& data) const
		{
			return std::accumulate(
				data.begin(), data.end(), 0,
				[](unsigned char acc, std::byte b) -> unsigned char {
					return acc + static_cast<unsigned char>(b);
				}
			);
		}

		/// <summary>
		/// Converts binary data into a nicely formatted hex string.
		/// </summary>
		/// <param name="data">Binary data as a replacement for a real struct</param>
		/// <returns>Human readable hex string</returns>
		std::wstring to_hex_string(const std::vector<std::byte>& data) const
		{
			std::wstring hex;
			hex.reserve(data.size() * 2);
			for (std::byte b : data) {
				hex += std::format(L"{:02X}", std::to_integer<unsigned char>(b));
			}
			return hex;
		}

	public:
		
		TEST_METHOD(StructChecksumAlgorithm)
		{
			std::vector<std::vector<std::byte>> test_cases = {
				{std::byte{0x01}, std::byte{0x02}, std::byte{0x03}, std::byte{0x04}}, // sum=10
				{std::byte{0x00}, std::byte{0x00}, std::byte{0x00}, std::byte{0x01}}, // sum=1
				{std::byte{0xFF}, std::byte{0xFF}, std::byte{0xFF}, std::byte{0xFF}}, // sum=1020 % 256
				{std::byte{0x10}, std::byte{0x20}, std::byte{0x30}, std::byte{0x40}}, // sum=160
				{std::byte{0xDE}, std::byte{0xAD}, std::byte{0xBE}, std::byte{0xEF}}  // sum=0x38A
			};

			for (const auto& data : test_cases) {
				write_struct(data);
				std::wstring ini_line = read_string();
				Assert::IsTrue(ini_line.starts_with(to_hex_string(data)), L"INI line does not start with the correct bytes.");

				unsigned char expected_checksum = compute_checksum(data);
				std::wstring expected_checksum_str = std::format(L"{:02X}", expected_checksum);
				Assert::IsTrue(ini_line.ends_with(expected_checksum_str), L"INI line does not end with the expected checksum.");
			}
		}
	};
}
