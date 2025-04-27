/*
 * Name: SimpleDotNetObfuscator
 * Description: A lightweight C# obfuscation tool for .Net applications.
 * Version: 1.0.0.1
 * Locale: en_International
 * Last update: 2025
 * Architecture: multi-arch
 * API: .Net Framework 4.8
 * Compiler: C# 7.3
 * Builder: MsBuild 16.9
 * License:
 * Copyright: Copyright (c) 2025 Yousha Aleayoub.
 * Producer: Yousha Aleayoub
 * Maintainer: Yousha Aleayoub
 * Contact: yousha.a@hotmail.com
 * Link: https://yousha.blog.ir
 */

#define PRODUCTION

#if DEBUG
#warning Debug mode is active.
#endif

namespace SimpleDotNetObfuscator
{
   using System;
   using System.IO;
   using MySoftwarePackager.Libraries;
   using SimpleDotNetObfuscator.Libraries;

   internal class Program
   {
      /// <summary>
      /// Main entry point for the obfuscation tool.
      /// </summary>
      /// <param name="args">Command-line arguments specifying file paths to obfuscate.</param>
      internal static void Main(string[] args)
      {
         Obfuscator.CheckForDebugging(); // Check for debuggers before running.

         if (args.Length == 0)
         {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Constants.NAME + " " + Constants.VERSION);
            Console.WriteLine(Constants.DESCRIPTION);
            Console.WriteLine("Usage: " + Constants.NAME + "<input_file1> [<input_file2>] ...");
            Console.ResetColor();
            return;
         }

         foreach (var filePath in args)
         {
            if (!File.Exists(filePath))
            {
               Console.WriteLine("File not found: " + filePath);
               continue;
            }
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".cs")
            {
               Obfuscator.ObfuscateCSFile(filePath);
            }
            else if (ext == ".dll" || ext == ".exe")
            {
               Obfuscator.ObfuscateAssembly(filePath);
            }
            else
            {
               Console.WriteLine("Unsupported file type: " + filePath);
            }
         }
      }
   }

}
