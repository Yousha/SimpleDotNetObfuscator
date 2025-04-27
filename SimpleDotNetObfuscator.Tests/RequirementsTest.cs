#define TEST

namespace SimpleDotNetObfuscator.Tests
{
   using System;
   using Microsoft.CSharp;
   using Microsoft.VisualStudio.TestTools.UnitTesting;

   [TestClass]
   [TestCategory("Medium"), TestCategory("Requirements")]
   public sealed class RequirementsTest
   {
      #region Fields

      private const int TIMEOUT_METHOD = 1000;

      #endregion Fields

      /// <summary>
      /// Runs once before all tests in this assembly are executed. (Optional)
      /// </summary>
      [AssemblyInitialize()]
      public static void AssemblyInit(TestContext context)
      {
         Environment.ExitCode = 0;
         Environment.SetEnvironmentVariable("SIMPLEDOTNETOBFUSCATOR_ENVIRONMENT", "Test");
      }

      /// <summary>
      /// Runs once after all tests in this class are executed. (Optional)
      /// </summary>
      [ClassCleanup()]
      [Timeout(TestTimeout.Infinite)]
      public static void ClassCleanup()
      {
         GC.Collect();
      }

      [TestMethod()]
      [Timeout(TIMEOUT_METHOD)]
      [Priority(2)]
      public void TestLoadedReferences()
      {
         // Arrange/Action
         var _csharp = new CSharpCodeProvider();
         // Assert
         Assert.IsNotNull(_csharp);
      }
   }
}
