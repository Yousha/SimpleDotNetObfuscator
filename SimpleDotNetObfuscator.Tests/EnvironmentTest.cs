#define TEST

namespace SimpleDotNetObfuscator.Tests
{
   #region Refrences

   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Diagnostics.CodeAnalysis;
   using System.Net.NetworkInformation;
   using System.Text;
   using System.Threading;
   using Microsoft.VisualStudio.TestTools.UnitTesting;

   #endregion Refrences

   #region Classes

   [TestClass()]
   [TestCategory("Small"), TestCategory("Environment")]
   public sealed class EnvironmentTest
   {
      #region Properties

      private uint TestProperty { get; set; }

      #endregion Properties

      #region Fields

      private const int TIMEOUT_METHOD = 3000;

      #endregion Fields

      #region Methods

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
      [Priority(1)]
      [SuppressMessage("Roslynator", "RCS1118:Mark local variable as const")]
      [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value")]
      [SuppressMessage("Style", "IDE1006:Naming Styles")]
      public void TestCSharp()
      {
#pragma warning disable CS0219 // Variable is assigned but its value is never used.
         // AAA
         int _integer = 42;
         double _decimalNumber = 3.14;
         string _text = "Hello, world!";
         bool _boolean = true;
#pragma warning restore CS0219 // Variable is assigned but its value is never used.

         // AAA
         Assert.IsTrue(true);

         // AAA
         Assert.IsFalse(false);

         // AAA
         Assert.IsNull(null);

         // AAA
         Assert.IsNotNull(new object());

         // AAA
         Assert.AreEqual(1, 1);

         // AAA
         Assert.AreEqual("A", "A");

         // AAA
         Assert.AreNotEqual(1, -1);

         // AAA
         Assert.AreNotEqual("A", "");

         // Arrange

         const string _TEST_STRING = "Test content.";

         // Action
         var _result = _TEST_STRING.Contains("0123456789");
         // Assert
         Assert.IsFalse(_result);
         // Action
         _result = _TEST_STRING.Contains(_TEST_STRING);
         // Assert
         Assert.IsTrue(_result);

         // Arrange
         var _stringBuilder = new StringBuilder();
         // Action
         _stringBuilder.AppendFormat("{0}", _TEST_STRING);
         // Assert
         Assert.IsTrue(_stringBuilder.ToString() == _TEST_STRING);

         // Arrange
         var _value = Math.Min(12345, 123 + 1);
         // Action
         _result = _value == 124;
         // Assert
         Assert.IsTrue(_result);

         // Arrange
         var _ping = new Ping();
         // Action
         var _status = _ping.Send("localhost", 15).Status;
         // Assert
         Assert.IsTrue(_status == IPStatus.Success);
         // Cleanup
         _ping.Dispose();

         // Arrange
         var _hashSet = new HashSet<string>
         {
            "Test value.",
            _TEST_STRING
         };
         // Action
         _result = _hashSet.Count == 2;
         // Assert
         Assert.IsTrue(_result);
         // Action
         _result = _hashSet.Contains(_TEST_STRING);
         // Assert
         Assert.IsTrue(_result);
         // Action
         _hashSet.Clear();
         _result = _hashSet.Count == 0;
         // Assert
         Assert.IsTrue(_result);

         // Arrange & Action
         List<int> _emptyList = new List<int>();
         // Assert
         Assert.AreEqual(0, _emptyList.Count);

         // Arrange
         IDictionary<string, string> _testDictionary = new Dictionary<string, string>
         {
            { "Name1", "0123456789" },
            { "Name2", "Test." }
         };
         // Action
         _testDictionary.Remove("Name1");
         _result = _testDictionary.ContainsKey("Name1");
         // Assert
         Assert.IsFalse(_result);
         // Action
         _testDictionary.Clear();
         _result = _testDictionary.Count == 0;
         // Assert
         Assert.IsTrue(_result);

         // Arrange
         const int _NUMBER = 10;
         // Action & Assert
         if (_NUMBER > 5)
         {
            Assert.IsTrue(true);
         }
         else
         {
#pragma warning disable CS0162 // Unreachable code detected.
            Assert.Fail();
#pragma warning restore CS0162 // Unreachable code detected.
         }

         // Arrange
         this.TestProperty = 1234567890;
         // Assert
         Assert.IsTrue(this.TestProperty == 1234567890);

         // Arrange
         var _stackFrame = new StackTrace(true).GetFrame(1);
         // Action
         _result = string.IsNullOrEmpty(_stackFrame.GetFileName());
         // Assert
         Assert.IsTrue(_result);
         // Action
         _result = _stackFrame.GetFileLineNumber() == 0;
         // Assert
         Assert.IsTrue(_result);

         // AAA
         Thread.Sleep(TIMEOUT_METHOD / 2);
      }

      [TestMethod()]
      [Timeout(TIMEOUT_METHOD)]
      [Priority(1)]
      [ExpectedException(typeof(Exception))]
      public void TestThrowException()
      {
         throw new Exception();
      }

      #endregion Methods
   }

   #endregion Classes
}
