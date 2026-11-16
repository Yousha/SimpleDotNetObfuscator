/*
 * Name: SimpleDotNetObfuscator
 * Description: A small C# source-string encoder for .NET Framework projects.
 * Version: 1.0.1.0
 * Locale: en_International
 * Last update: 2026
 * Architecture: multi-arch
 * API: .Net Framework 4.8
 * Compiler: C# 7.3
 * Builder: MsBuild 16.9
 * Copyright: Copyright 2025 Yousha Aleayoub.
 */
namespace SimpleDotNetObfuscator
{
   using System;
   using System.IO;
   using SimpleDotNetObfuscator.Libraries;

   internal class Program
   {
      /// <summary>Processes the supplied source and assembly files.</summary>
      /// <param name="args">Input file paths.</param>
      /// <returns>Zero when every input succeeds; otherwise a nonzero exit code.</returns>
      internal static int Main(string[] args)
      {
         if (args.Length == 0)
         {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Constants.NAME + " " + Constants.VERSION);
            Console.WriteLine(Constants.DESCRIPTION);
            Console.WriteLine("Usage: " + Constants.NAME + " <input_file1> [<input_file2>] ...");
            Console.ResetColor();
            return 0;
         }

         int failures = 0;
         foreach (string filePath in args)
         {
            try
            {
               if (!File.Exists(filePath))
               {
                  Console.Error.WriteLine("File not found: " + filePath);
                  failures++;
                  continue;
               }

               string ext = Path.GetExtension(filePath).ToLowerInvariant();
               if (ext == ".cs")
                  Obfuscator.ObfuscateCSFile(filePath);
               else if (ext == ".dll" || ext == ".exe")
                  Obfuscator.ObfuscateAssembly(filePath);
               else
               {
                  Console.Error.WriteLine("Unsupported file type: " + filePath);
                  failures++;
               }
            }
            catch (Exception exception)
            {
               Console.Error.WriteLine("Failed to process '" + filePath + "': " + exception.Message);
               failures++;
            }
         }

         return failures == 0 ? 0 : 1;
      }
   }
}