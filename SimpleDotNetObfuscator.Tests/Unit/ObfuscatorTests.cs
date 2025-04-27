#define TEST

namespace SimpleDotNetObfuscator.Tests.Unit
{
   using System.IO;
   using Microsoft.VisualStudio.TestTools.UnitTesting;
   using SimpleDotNetObfuscator.Libraries;

   [TestClass]
   [TestCategory("Small")]
   public class ObfuscatorTests
   {
      #region Fields

      private const int TIMEOUT_METHOD = 3000;
      // Temporary test file paths
      private string testCsFile;
      private string obfuscatedCsFile;

      #endregion Fields

      [TestInitialize]
      public void Setup()
      {
         // Create a test C# source file with known contents.
         testCsFile = Path.Combine(Path.GetTempPath(), "test.cs");
         obfuscatedCsFile = Path.ChangeExtension(testCsFile, ".obf.cs");
         File.WriteAllText(testCsFile, @"
                using System;
                class HelloWorld {
                    static void Main() {
                        Console.WriteLine(""Hello, World!"");
                    }
                }
            ");
      }

      [TestCleanup]
      public void Teardown()
      {
         // Clean up generated files.
         if (File.Exists(testCsFile)) File.Delete(testCsFile);
         if (File.Exists(obfuscatedCsFile)) File.Delete(obfuscatedCsFile);
      }

      [TestMethod()]
      [Timeout(TIMEOUT_METHOD)]
      public void TestStringEncryptionAndDecryption()
      {
         // Arrange
         string original = "SecretData123";
         string encrypted = Obfuscator.EncryptString(original);
         string decrypted = Obfuscator.DecryptString(encrypted);
         // Action/Assert
         Assert.AreNotEqual(original, encrypted, "Encrypted string should be different");
         Assert.AreEqual(original, decrypted, "Decrypted string should match original");
      }

      [TestMethod()]
      [Timeout(TIMEOUT_METHOD)]
      public void TestObfuscationCreatesNewFile()
      {
         // Arrange
         Obfuscator.ObfuscateCSFile(testCsFile);
         // Action/Assert
         Assert.IsTrue(File.Exists(obfuscatedCsFile), "Obfuscated file should be created");

         // Arrange
         string originalContent = File.ReadAllText(testCsFile);
         string obfuscatedContent = File.ReadAllText(obfuscatedCsFile);
         // Action/Assert
         Assert.AreNotEqual(originalContent, obfuscatedContent, "Obfuscated content should be different");
      }

      [TestMethod()]
      [Timeout(TIMEOUT_METHOD)]
      public void TestAntiDebuggingDetection()
      {
         // Arrange
         bool isDebugging = Obfuscator.IsDebugging();
         // Action/Assert
         Assert.IsFalse(isDebugging, "Debugger detection should return false in normal execution");
      }
   }
}
