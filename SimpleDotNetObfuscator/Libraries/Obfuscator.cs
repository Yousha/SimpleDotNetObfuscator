namespace SimpleDotNetObfuscator.Libraries
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.IO;
   using System.Text.RegularExpressions;
   using Mono.Cecil;

   public sealed class Obfuscator
   {
      /// <summary>
      /// Random number generator instance for obfuscation.
      /// </summary>
      public static Random rand = new Random();

      /// <summary>
      /// Stores mapping of original identifiers to obfuscated names.
      /// </summary>
      public static Dictionary<string, string> identifierMap = new Dictionary<string, string>();

      /// <summary>
      /// Generates a random string for obfuscation.
      /// </summary>
      /// <param name="length">Length of the random identifier.</param>
      /// <returns>A randomly generated identifier string.</returns>
      public static string GenerateRandomIdentifier(int length = 8)
      {
         const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
         char[] buffer = new char[length];
         for (int i = 0; i < length; i++)
         {
            buffer[i] = chars[rand.Next(chars.Length)];
         }
         return new string(buffer);
      }

      /// <summary>
      /// Encrypts a string using Base64 encoding (basic transformation).
      /// </summary>
      /// <param name="input">String to encrypt.</param>
      /// <returns>Encrypted Base64 string.</returns>
      public static string EncryptString(string input)
      {
         byte[] data = System.Text.Encoding.UTF8.GetBytes(input);
         return Convert.ToBase64String(data);
      }

      /// <summary>
      /// Decrypts a Base64-encoded string.
      /// </summary>
      /// <param name="input">Encrypted Base64 string.</param>
      /// <returns>Original decrypted string.</returns>
      public static string DecryptString(string input)
      {
         byte[] data = Convert.FromBase64String(input);
         return System.Text.Encoding.UTF8.GetString(data);
      }

      /// <summary>
      /// Obfuscates a C# source file by encrypting string literals.
      /// </summary>
      /// <param name="filePath">Path to the C# source file.</param>
      public static void ObfuscateCSFile(string filePath)
      {
         Console.WriteLine("Obfuscating C# source file: " + filePath);
         string code = File.ReadAllText(filePath);

         var regex = new Regex(@"""([^""]+)"""); // Matches string literals.
         string obfuscatedCode = regex.Replace(code, match =>
         {
            string originalString = match.Groups[1].Value;
            string encryptedString = EncryptString(originalString);
            return $"DecryptString(\"{encryptedString}\")"; // Replace plaintext strings with encrypted versions.
         });

         string newFilePath = Path.Combine(Path.GetDirectoryName(filePath),
             Path.GetFileNameWithoutExtension(filePath) + ".obf.cs");
         File.WriteAllText(newFilePath, obfuscatedCode);
         Console.WriteLine("Obfuscated file saved: " + newFilePath);
      }

      /// <summary>
      /// Injects dead code into a type definition to confuse reverse engineers.
      /// </summary>
      /// <param name="type">The target type definition for dead code injection.</param>
      public static void InjectDeadCode(TypeDefinition type)
      {
         var newMethod = new MethodDefinition(
             GenerateRandomIdentifier(),
             Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static,
             type.Module.TypeSystem.Void);

         var processor = newMethod.Body.GetILProcessor();
         processor.Append(processor.Create(Mono.Cecil.Cil.OpCodes.Nop));
         processor.Append(processor.Create(Mono.Cecil.Cil.OpCodes.Ret));

         type.Methods.Add(newMethod);
         Console.WriteLine($"Injected dead code into type {type.Name}");
      }

      /// <summary>
      /// Detects if a debugger is attached.
      /// </summary>
      /// <returns>True if a debugger is detected, otherwise false.</returns>
      public static bool IsDebugging()
      {
         return Debugger.IsAttached || Environment.GetEnvironmentVariable("COR_ENABLE_PROFILING") == "1";
      }

      /// <summary>
      /// Checks for active debugging and exits if detected.
      /// </summary>
      public static void CheckForDebugging()
      {
         if (IsDebugging())
         {
            Console.WriteLine("Debugger detected! Exiting...");
            Environment.Exit(-1);
         }
      }

      /// <summary>
      /// Obfuscates a .NET assembly (.dll or .exe) by renaming types and members.
      /// </summary>
      /// <param name="filePath">Path to the assembly file.</param>
      public static void ObfuscateAssembly(string filePath)
      {
         Console.WriteLine("Obfuscating Assembly: " + filePath);

         var assembly = AssemblyDefinition.ReadAssembly(filePath);
         foreach (var module in assembly.Modules)
         {
            foreach (var type in module.Types)
            {
               if (type.Name == "<Module>")
                  continue;

               string newTypeName = GenerateRandomIdentifier();
               type.Name = newTypeName;
               Console.WriteLine($"Renamed type -> {newTypeName}");

               foreach (var method in type.Methods)
               {
                  if (method.IsSpecialName)
                     continue;
                  method.Name = GenerateRandomIdentifier();
               }

               foreach (var field in type.Fields)
               {
                  if (field.IsSpecialName)
                     continue;
                  field.Name = GenerateRandomIdentifier();
               }

               foreach (var prop in type.Properties)
               {
                  prop.Name = GenerateRandomIdentifier();
               }

               // Apply Dead Code Injection.
               InjectDeadCode(type);
            }
         }

         string newFilePath = Path.Combine(Path.GetDirectoryName(filePath),
             Path.GetFileNameWithoutExtension(filePath) + ".obf" + Path.GetExtension(filePath));
         assembly.Write(newFilePath);
         Console.WriteLine("Obfuscated assembly saved at: " + newFilePath);
      }
   }
}
