# Simple .NET Obfuscator

A small C# source-string encoder for .NET Framework projects. It writes a new `.obf.cs` file and leaves the input unchanged.

*(It was created for a proprietary project called Simorgh, a clone of Logicube Falcon device.)*

[![Dependabot Updates](https://github.com/Yousha/SimpleDotNetObfuscator/actions/workflows/dependabot/dependabot-updates/badge.svg?branch=main)](https://github.com/Yousha/SimpleDotNetObfuscator/actions/workflows/dependabot/dependabot-updates) [![CodeQL](https://github.com/Yousha/SimpleDotNetObfuscator/actions/workflows/github-code-scanning/codeql/badge.svg?branch=main)](https://github.com/Yousha/SimpleDotNetObfuscator/actions/workflows/github-code-scanning/codeql)

## Overview

It rewrites eligible string literals in `.cs` source files. `.dll` and `.exe` files are copied unchanged; they are not currently obfuscated.

## What it does

- Rewrites eligible C# string literal expressions as fully qualified Base64/UTF-8 decode expressions.
- Uses the C# syntax parser, so comments and character literals are not mistaken for strings, and C# escape sequences are decoded before encoding.
- Copies `.dll` and `.exe` inputs to `.obf.dll` / `.obf.exe` unchanged. Assembly identifier rewriting and dead-code injection are disabled because the current implementation cannot safely preserve reflection, references, or virtual/interface contracts.

Base64 is reversible encoding, not encryption, and provides no meaningful secrecy. Interpolated-string text and compile-time constant strings (for example `const` values, attributes, and optional parameter defaults) are left unchanged to keep the generated source valid. Ordinary string literals inside interpolation expressions are still processed. This tool does not add anti-debugging code to output applications.

## Requirements

### Prerequisites

- .NET Framework 4.8 Runtime (https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)
- NuGet restore for the Roslyn C# parser dependency (Microsoft.CodeAnalysis.CSharp)

## Usage

Run the tool via command line:

```sh
SimpleDotNetObfuscator.exe <input_file1> [<input_file2>] ...
```

Example:

```sh
SimpleDotNetObfuscator.exe MyApp.dll MyApp.exe MySource.cs
```

### Output

- `.cs` files are saved as `.obf.cs`
- Assembly inputs are copied unchanged with `.obf.dll` or `.obf.exe` suffixes

## FAQs

- What does this tool do? It applies a lightweight, reversible Base64 encoding to eligible source string literals. It currently does not obfuscate assembly metadata.

- Is Base64 string encoding encryption? No. Anyone can decode it; it only changes how literals appear in the source file.

- Are all string literals transformed? No. Interpolated strings and compile-time constant contexts are preserved.

- Are assemblies renamed or protected? No. They are copied byte-for-byte so signatures and runtime metadata remain intact.

-  Can I obfuscate multiple files at once? Yes. You can pass multiple file paths as arguments when running the tool.

-  Does this tool provide code virtualization? No.

- How do I install dependencies? Restore the NuGet packages for the solution.

- Does this provide strong protection against reverse engineering? No. It is a small source transformation utility, not a security boundary or a full obfuscator.

## QA/QC

Unit tests are written using **MSTest**. To run the tests:

```sh
dotnet test
```

## License

This project is licensed under the **GPL-3.0**. See the [LICENSE](LICENSE) file for details.

## Contact

For questions or feedback, please use [issues](https://github.com/yousha/SimpleDotNetObfuscator/issues) section.

