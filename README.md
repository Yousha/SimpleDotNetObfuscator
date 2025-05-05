# Simple .Net Obfuscator (Without making the code vulnerable)

A lightweight C# obfuscation tool for .Net applications.

*(It was created this for a proprietary project called Simorgh, a clone of Logicube Falcon device.)*

[![CodeQL](https://github.com/Yousha/SimpleDotNetObfuscator/actions/workflows/github-code-scanning/codeql/badge.svg?branch=main)](https://github.com/Yousha/SimpleDotNetObfuscator/actions/workflows/github-code-scanning/codeql)

## Overview

It supports obfuscating `.cs` source files, `.dll` libraries, and `.exe` executables to prevent reverse engineering and tampering.

## Features (basic)

-  **String encryption**: Encodes all string literals to make them unreadable without execution.
-  **Dead code injection**: Adds meaningless methods to distract reverse engineers.
-  **Anti-Debugging mechanism**: Detects attached debuggers and prevents execution.

## Installation

### Prerequisites

-  .Net Framework 4.8 Runtime (https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)
-  Mono.Cecil library (nuget Install-Package Mono.Cecil)

## Usage

Run the tool via command line:

```sh
SimpleDotNetObfuscator.exe <input_file1> [<input_file2>] ...
```

Example:

```sh
SimpleDotNetObfuscator.exe MyApp.dll MyApp.exe MySource.cs
```

### Output:

-  Obfuscated `.cs` files are saved as `.obf.cs`
-  Obfuscated assemblies (`.dll` and `.exe`) are renamed with `.obf.dll` and `.obf.exe`

## FAQs

-  What does this tool do? This tool obfuscates `.cs` source files, `.dll` libraries, and `.exe` executables to make them harder to reverse-engineer.

-  How does string encryption work? String literals in C# files are replaced with encrypted versions using Base64 encoding. At runtime, they are dynamically decrypted.

-  Will obfuscation break my application? It depends. Renaming public types and members might cause issues in applications relying on reflection or serialization. Testing is highly recommended.

-  How does the anti-debugging feature work? The tool checks if a debugger is attached or if profiling is enabled. If debugging is detected, the application exits to prevent reverse engineering.

-  Can I obfuscate multiple files at once? Yes. You can pass multiple file paths as arguments when running the tool.

-  Does this tool provide code virtualization? No.

-  How do I install dependencies? Install the required **Mono.Cecil** package via NuGet using:
   `Install-Package Mono.Cecil`

-  Is this tool safe to use? Yes, but always **test your obfuscated binaries** before deployment to avoid runtime errors.

## QA/QC

Unit tests are written using **MSTest**. To run the tests:

```sh
dotnet test
```

## License

This project is licensed under the **GPL-3.0**. See the [LICENSE](LICENSE) file for details.

## Contact

For questions or feedback, please use [issues](https://github.com/yousha/SimpleDotNetObfuscator/issues) section.
