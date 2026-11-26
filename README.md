# Simple .NET Obfuscator

A small C# source-string encoder for .NET Framework projects. It writes a new `.obf.cs` file and leaves the input unchanged.

*(It was created for a proprietary project called Simorgh, a clone of Logicube Falcon device.)*

[![Build status](https://ci.appveyor.com/api/projects/status/lmntqv7h9061ly30?svg=true)](https://ci.appveyor.com/project/Yousha/simpledotnetobfuscator)
[![Dependabot Updates](https://github.com/Yousha/SimpleDotNetObfuscator/actions/workflows/dependabot/dependabot-updates/badge.svg?branch=main)](https://github.com/Yousha/SimpleDotNetObfuscator/actions/workflows/dependabot/dependabot-updates)
[![CodeQL](https://github.com/Yousha/SimpleDotNetObfuscator/actions/workflows/github-code-scanning/codeql/badge.svg?branch=main)](https://github.com/Yousha/SimpleDotNetObfuscator/actions/workflows/github-code-scanning/codeql)

## Overview

It rewrites eligible string literals in `.cs` source files. `.dll` and `.exe` files are copied unchanged; they are not currently obfuscated.

## What it does

- Rewrites eligible C# string literals as fully qualified `System.String.Intern(new System.String(new char[] { ... }))` expressions.
- Uses the Roslyn C# syntax parser, so comments and character literals are not mistaken for strings, and C# escape sequences are interpreted before transformation.
- Preserves compile-time constant contexts, expression-tree structure, string interning, and source-file encoding.
- Copies `.dll` and `.exe` inputs unchanged. Assembly metadata rewriting and dead-code injection are disabled because the implementation cannot safely preserve reflection, references, or virtual/interface contracts.

This transformation is reversible and is not encryption. Interpolated-string text and compile-time constant strings (for example `const` values, attributes, and optional parameter defaults) are left unchanged to keep generated source valid. Ordinary string literals inside interpolation expressions are still processed. This tool does not add anti-debugging code to output applications.

## Requirements

- .NET Framework 4.8 Runtime and Developer Pack.
- NuGet for restoring the Roslyn and MSTest dependencies.
- Visual Studio 2022 Build Tools or Visual Studio with MSBuild and VSTest for building and testing.

## Usage

Run the tool via command line:

```powershell
SimpleDotNetObfuscator.exe <input_file1> [<input_file2>] ...
```

Example:

```powershell
SimpleDotNetObfuscator.exe MyApp.dll MyApp.exe MySource.cs
```

### Output

- `.cs` files are saved as `.obf.cs`.
- Assembly inputs are copied unchanged with `.obf.dll` or `.obf.exe` suffixes.

## Build and test

Run these commands from the repository root in a Visual Studio Developer PowerShell:

```powershell
nuget restore SimpleDotNetObfuscator.sln -NonInteractive
msbuild SimpleDotNetObfuscator.sln /m /p:Configuration=Release
vstest.console.exe .\SimpleDotNetObfuscator.Tests\Artifacts\bin\Release\SimpleDotNetObfuscator.Tests.dll
```

AppVeyor performs the same restore/build sequence and explicitly runs the Release test assembly.

## Limitations

- Base64 is not used as a security boundary; this tool only changes the representation of eligible source literals.
- Assemblies are copied byte-for-byte and are not protected against reverse engineering.
- The tool does not provide code virtualization or anti-debugging.

## License

This project is licensed under GPL-3.0. See [LICENSE](LICENSE).

## Contact

For questions or feedback, use the [GitHub issues](https://github.com/Yousha/SimpleDotNetObfuscator/issues).
