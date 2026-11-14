#define TEST

namespace SimpleDotNetObfuscator.Tests.Unit
{
   using System;
   using System.IO;
   using System.Text;
   using Microsoft.VisualStudio.TestTools.UnitTesting;
   using SimpleDotNetObfuscator.Libraries;

   [TestClass]
   [TestCategory("Small")]
   public class ObfuscatorTests
   {
      #region Fields

      private const int TIMEOUT_METHOD = 5000;

      private string _tempDir;
      private string _testCsFile;
      private string _obfuscatedCsFile;
      private string _testAssemblyFile;
      private string _obfuscatedAssemblyFile;

      #endregion Fields

      #region Setup / Cleanup

      [TestInitialize]
      public void Setup()
      {
         // Unique folder per test run to avoid collisions
         _tempDir = Path.Combine(Path.GetTempPath(), "SimpleDotNetObfuscator_Tests_" + Guid.NewGuid().ToString("N"));
         Directory.CreateDirectory(_tempDir);

         _testCsFile = Path.Combine(_tempDir, "sample.cs");
         _obfuscatedCsFile = Path.Combine(_tempDir, "sample.obf.cs");

         _testAssemblyFile = Path.Combine(_tempDir, "sample.dll");
         _obfuscatedAssemblyFile = Path.Combine(_tempDir, "sample.obf.dll");
      }

      [TestCleanup]
      public void Teardown()
      {
         try
         {
            if (Directory.Exists(_tempDir))
            {
               Directory.Delete(_tempDir, recursive: true);
            }
         }
         catch
         {
            // Best-effort cleanup; do not fail the test suite on temp-file issues
         }
      }

      #endregion Setup / Cleanup

      #region EncryptString / DecryptString

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void EncryptDecrypt_RoundTrip_SimpleAscii()
      {
         string original = "SecretData123";
         string encoded = Obfuscator.EncryptString(original);
         string decoded = Obfuscator.DecryptString(encoded);

         Assert.AreNotEqual(original, encoded, "Encoded form must differ from plaintext");
         Assert.AreEqual(original, decoded, "Round-trip must restore original");
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void EncryptDecrypt_EmptyString()
      {
         string original = string.Empty;
         string encoded = Obfuscator.EncryptString(original);
         string decoded = Obfuscator.DecryptString(encoded);

         Assert.AreEqual(string.Empty, decoded);
         // Base64 of empty byte[] is empty string
         Assert.AreEqual(string.Empty, encoded);
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void EncryptDecrypt_UnicodeAndSpecialCharacters()
      {
         string original = "سلام دنیا 🌍 — line1\nline2\t\"quoted\"";
         string encoded = Obfuscator.EncryptString(original);
         string decoded = Obfuscator.DecryptString(encoded);

         Assert.AreEqual(original, decoded);
         Assert.IsFalse(encoded.Contains("سلام"), "Base64 must not contain original Unicode text");
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void EncryptString_Null_ThrowsArgumentNullException()
      {
         Assert.ThrowsException<ArgumentNullException>(() => Obfuscator.EncryptString(null));
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void DecryptString_InvalidBase64_ThrowsFormatException()
      {
         Assert.ThrowsException<FormatException>(() => Obfuscator.DecryptString("not-valid-base64!!!"));
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void EncryptString_ProducesValidBase64()
      {
         string encoded = Obfuscator.EncryptString("abc");
         // Must be decodable by Convert.FromBase64String without exception
         byte[] bytes = Convert.FromBase64String(encoded);
         Assert.IsTrue(bytes.Length > 0);
         Assert.AreEqual("abc", Encoding.UTF8.GetString(bytes));
      }

      #endregion EncryptString / DecryptString

      #region ObfuscateCSFile

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void ObfuscateCSFile_CreatesObfuscatedFile()
      {
         WriteSampleSource(_testCsFile, @"
using System;
class C {
    static void Main() {
        Console.WriteLine(""Hello, World!"");
    }
}
");

         Obfuscator.ObfuscateCSFile(_testCsFile);

         Assert.IsTrue(File.Exists(_obfuscatedCsFile), "Expected .obf.cs file to be created");
         Assert.IsTrue(File.Exists(_testCsFile), "Original source must remain untouched");
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void ObfuscateCSFile_OriginalFileContentUnchanged()
      {
         string source = @"
using System;
class C {
    static void Main() {
        string s = ""KeepMe"";
    }
}
";
         WriteSampleSource(_testCsFile, source);

         Obfuscator.ObfuscateCSFile(_testCsFile);

         string after = File.ReadAllText(_testCsFile);
         Assert.AreEqual(source, after, "Original file must not be modified");
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void ObfuscateCSFile_RewritesOrdinaryStringLiteral()
      {
         WriteSampleSource(_testCsFile, @"
using System;
class C {
    static void Main() {
        Console.WriteLine(""Hello, World!"");
    }
}
");

         Obfuscator.ObfuscateCSFile(_testCsFile);

         string result = File.ReadAllText(_obfuscatedCsFile);

         // Plain text should no longer appear as a string literal
         Assert.IsFalse(result.Contains("\"Hello, World!\""), "Plain string literal should have been rewritten");

         // The rewrite must use the documented fully-qualified expression
         Assert.IsTrue(result.Contains("FromBase64String"), "Expected Base64 decode expression");
         Assert.IsTrue(result.Contains("Encoding.UTF8.GetString"), "Expected UTF-8 GetString call");

         // The Base64 payload itself must decode back to the original text
         string expectedPayload = Obfuscator.EncryptString("Hello, World!");
         Assert.IsTrue(result.Contains(expectedPayload), "Encoded payload must be present in the output");
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void ObfuscateCSFile_PreservesConstString()
      {
         WriteSampleSource(_testCsFile, @"
using System;
class C {
    const string Message = ""DoNotTouch"";
    static void Main() {
        Console.WriteLine(Message);
    }
}
");

         Obfuscator.ObfuscateCSFile(_testCsFile);

         string result = File.ReadAllText(_obfuscatedCsFile);
         Assert.IsTrue(result.Contains("\"DoNotTouch\""), "const string must stay as a literal");
         Assert.IsFalse(result.Contains(Obfuscator.EncryptString("DoNotTouch")), "const value must not be Base64-encoded");
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void ObfuscateCSFile_PreservesAttributeArgument()
      {
         WriteSampleSource(_testCsFile, @"
using System;
[System.Obsolete(""Legacy"")]
class C {
    static void Main() { }
}
");

         Obfuscator.ObfuscateCSFile(_testCsFile);

         string result = File.ReadAllText(_obfuscatedCsFile);
         Assert.IsTrue(result.Contains("\"Legacy\""), "Attribute string argument must remain a literal");
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void ObfuscateCSFile_HandlesMultipleLiterals()
      {
         WriteSampleSource(_testCsFile, @"
using System;
class C {
    static void Main() {
        string a = ""Alpha"";
        string b = ""Beta"";
        Console.WriteLine(a + b);
    }
}
");

         Obfuscator.ObfuscateCSFile(_testCsFile);

         string result = File.ReadAllText(_obfuscatedCsFile);
         Assert.IsTrue(result.Contains(Obfuscator.EncryptString("Alpha")), "First literal should be encoded");
         Assert.IsTrue(result.Contains(Obfuscator.EncryptString("Beta")), "Second literal should be encoded");
         Assert.IsFalse(result.Contains("\"Alpha\""), "Plain Alpha literal should be gone");
         Assert.IsFalse(result.Contains("\"Beta\""), "Plain Beta literal should be gone");
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void ObfuscateCSFile_VerbatimStringIsProcessed()
      {
         // ValueText of a verbatim string still yields the real content
         WriteSampleSource(_testCsFile, @"
using System;
class C {
    static void Main() {
        string path = @""C:\Temp\file.txt"";
    }
}
");

         Obfuscator.ObfuscateCSFile(_testCsFile);

         string result = File.ReadAllText(_obfuscatedCsFile);
         string expectedPayload = Obfuscator.EncryptString(@"C:\Temp\file.txt");
         Assert.IsTrue(result.Contains(expectedPayload), "Verbatim string content must be Base64-encoded");
         Assert.IsFalse(result.Contains("@\"C:\\Temp\\file.txt\""), "Original verbatim literal should be rewritten");
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void ObfuscateCSFile_SourceWithNoStrings_StillProducesOutput()
      {
         WriteSampleSource(_testCsFile, @"
using System;
class C {
    static int Add(int x, int y) { return x + y; }
}
");

         Obfuscator.ObfuscateCSFile(_testCsFile);

         Assert.IsTrue(File.Exists(_obfuscatedCsFile));
         string result = File.ReadAllText(_obfuscatedCsFile);
         Assert.IsTrue(result.Contains("Add"), "Code without string literals must still be written out");
         Assert.IsFalse(result.Contains("FromBase64String"), "No Base64 expressions expected when there are no strings");
      }

      #endregion ObfuscateCSFile

      #region ObfuscateAssembly

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void ObfuscateAssembly_CreatesCopyWithObfSuffix()
      {
         // The method only performs a file copy; any content is fine
         byte[] payload = Encoding.UTF8.GetBytes("fake-assembly-content-for-test");
         File.WriteAllBytes(_testAssemblyFile, payload);

         Obfuscator.ObfuscateAssembly(_testAssemblyFile);

         Assert.IsTrue(File.Exists(_obfuscatedAssemblyFile), "Expected .obf.dll to be created");
         Assert.IsTrue(File.Exists(_testAssemblyFile), "Original assembly must remain");
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void ObfuscateAssembly_CopyIsByteIdentical()
      {
         byte[] payload = new byte[] { 0x4D, 0x5A, 0x00, 0x01, 0x02, 0xFF }; // minimal fake PE header start
         File.WriteAllBytes(_testAssemblyFile, payload);

         Obfuscator.ObfuscateAssembly(_testAssemblyFile);

         byte[] copied = File.ReadAllBytes(_obfuscatedAssemblyFile);
         CollectionAssert.AreEqual(payload, copied, "Copied assembly must be byte-identical to the original");
      }

      [TestMethod]
      [Timeout(TIMEOUT_METHOD)]
      public void ObfuscateAssembly_ExeExtensionProducesObfExe()
      {
         string exePath = Path.Combine(_tempDir, "tool.exe");
         string expectedObf = Path.Combine(_tempDir, "tool.obf.exe");
         File.WriteAllBytes(exePath, new byte[] { 0x00, 0x01 });

         Obfuscator.ObfuscateAssembly(exePath);

         Assert.IsTrue(File.Exists(expectedObf), "Expected .obf.exe suffix for .exe input");
      }

      #endregion ObfuscateAssembly

      #region Helpers

      private static void WriteSampleSource(string path, string content)
      {
         File.WriteAllText(path, content, Encoding.UTF8);
      }

      #endregion Helpers
   }
}
