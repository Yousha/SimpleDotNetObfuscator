namespace SimpleDotNetObfuscator.Libraries
{
   using System;
   using System.Collections.Generic;
   using System.IO;
   using System.Linq;
   using System.Text;
   using System.Text.RegularExpressions;
   using Microsoft.CodeAnalysis;
   using Microsoft.CodeAnalysis.CSharp;
   using Microsoft.CodeAnalysis.CSharp.Syntax;

   /// <summary>Provides reversible source-string transformation helpers.</summary>
   public sealed class Obfuscator
   {
      /// <summary>Encodes the exact UTF-16 code units of a string as Base64.</summary>
      /// <param name="input">String to encode.</param>
      /// <returns>Base64-encoded UTF-16 code units.</returns>
      public static string EncryptString(string input)
      {
         if (input == null) throw new ArgumentNullException(nameof(input));
         byte[] data = new byte[input.Length * 2];
         for (int i = 0; i < input.Length; i++)
         {
            data[i * 2] = (byte)input[i];
            data[i * 2 + 1] = (byte)(input[i] >> 8);
         }
         return Convert.ToBase64String(data);
      }

      /// <summary>Decodes Base64-encoded UTF-16 code units without replacing unpaired surrogates.</summary>
      /// <param name="input">Base64-encoded UTF-16 code units.</param>
      /// <returns>The original string.</returns>
      public static string DecryptString(string input)
      {
         if (input == null) throw new ArgumentNullException(nameof(input));
         byte[] data = Convert.FromBase64String(input);
         if ((data.Length & 1) != 0) throw new FormatException("The encoded UTF-16 data has an invalid length.");
         char[] chars = new char[data.Length / 2];
         for (int i = 0; i < chars.Length; i++)
            chars[i] = (char)(data[i * 2] | (data[i * 2 + 1] << 8));
         return new string(chars);
      }

      /// <summary>
      /// Rewrites eligible C# string literals while preserving compile-time constants,
      /// expression-tree structure, string interning, and source-file encoding.
      /// </summary>
      /// <param name="filePath">Path to the C# source file.</param>
      public static void ObfuscateCSFile(string filePath)
      {
         if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("A source path is required.", nameof(filePath));
         Console.WriteLine("Encoding C# source string literals: " + filePath);

         Encoding sourceEncoding;
         string code = ReadSource(filePath, out sourceEncoding);
         var replacements = CollectLiteralReplacements(code);
         var result = new StringBuilder(code);

         foreach (var replacement in replacements.OrderByDescending(item => item.Key))
         {
            LiteralExpressionSyntax literal = replacement.Value;
            if (IsCompileTimeConstantContext(literal) || IsExpressionTreeContext(literal))
               continue;

            string value = literal.Token.ValueText;
            string characters = string.Join(", ", value.Select(character => "(char)0x" + ((int)character).ToString("X4")));
            string expression = "global::System.String.Intern(new global::System.String(new char[] { " + characters + " }))";
            result.Remove(literal.SpanStart, literal.Span.Length);
            result.Insert(literal.SpanStart, expression);
         }

         string newFilePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(filePath)),
             Path.GetFileNameWithoutExtension(filePath) + ".obf.cs");
         WriteSource(newFilePath, result.ToString(), sourceEncoding);
         Console.WriteLine("Encoded source saved: " + newFilePath);
      }

      private static string ReadSource(string path, out Encoding encoding)
      {
         byte[] bytes = File.ReadAllBytes(path);
         if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00)
            encoding = new UTF32Encoding(false, true);
         else if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF)
            encoding = new UTF32Encoding(true, true);
         else if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            encoding = new UTF8Encoding(true, true);
         else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            encoding = new UnicodeEncoding(false, true, true);
         else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            encoding = new UnicodeEncoding(true, true, true);
         else
         {
            try
            {
               encoding = new UTF8Encoding(false, true);
               return encoding.GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
               encoding = Encoding.Default;
            }
         }

         int preambleLength = encoding.GetPreamble().Length;
         return encoding.GetString(bytes, preambleLength, bytes.Length - preambleLength);
      }

      private static void WriteSource(string path, string content, Encoding encoding)
      {
         byte[] preamble = encoding.GetPreamble();
         byte[] body = encoding.GetBytes(content);
         byte[] output = new byte[preamble.Length + body.Length];
         Buffer.BlockCopy(preamble, 0, output, 0, preamble.Length);
         Buffer.BlockCopy(body, 0, output, preamble.Length, body.Length);
         File.WriteAllBytes(path, output);
      }

      private static Dictionary<int, LiteralExpressionSyntax> CollectLiteralReplacements(string code)
      {
         var symbolSet = new HashSet<string>(StringComparer.Ordinal);
         foreach (Match directive in Regex.Matches(code, @"(?m)^[ \t]*#\s*(?:if|elif)\s+([^\r\n]+)"))
         {
            foreach (Match identifier in Regex.Matches(directive.Groups[1].Value, @"\b[A-Za-z_][A-Za-z0-9_]*\b"))
            {
               string symbol = identifier.Value;
               if (symbol != "true" && symbol != "false")
                  symbolSet.Add(symbol);
            }
         }
         string[] symbols = symbolSet.ToArray();

         if (symbols.Length > 12)
            throw new InvalidOperationException("More than 12 conditional-compilation symbols were found; refusing an incomplete transformation.");

         var literals = new Dictionary<int, LiteralExpressionSyntax>();
         int configurations = 1 << symbols.Length;
         for (int mask = 0; mask < configurations; mask++)
         {
            var defined = new List<string>();
            for (int bit = 0; bit < symbols.Length; bit++)
               if ((mask & (1 << bit)) != 0) defined.Add(symbols[bit]);

            var options = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: defined);
            var root = CSharpSyntaxTree.ParseText(code, options).GetRoot();
            foreach (var literal in root.DescendantNodes().OfType<LiteralExpressionSyntax>())
               if (literal.IsKind(SyntaxKind.StringLiteralExpression) && !literals.ContainsKey(literal.SpanStart))
                  literals.Add(literal.SpanStart, literal);
         }
         return literals;
      }

      private static bool IsExpressionTreeContext(SyntaxNode node)
      {
         for (SyntaxNode current = node.Parent; current != null; current = current.Parent)
         {
            if (current is LambdaExpressionSyntax || current is AnonymousMethodExpressionSyntax ||
                current is QueryExpressionSyntax)
               return true;
            if (current is StatementSyntax || current is MemberDeclarationSyntax)
               break;
         }
         return false;
      }

      private static bool IsCompileTimeConstantContext(SyntaxNode node)
      {
         for (SyntaxNode current = node.Parent; current != null; current = current.Parent)
         {
            if (current is AttributeArgumentSyntax || current is CaseSwitchLabelSyntax ||
                current is ConstantPatternSyntax || current is CasePatternSwitchLabelSyntax)
               return true;

            var gotoStatement = current as GotoStatementSyntax;
            if (gotoStatement != null && gotoStatement.CaseOrDefaultKeyword.IsKind(SyntaxKind.CaseKeyword) &&
                gotoStatement.Expression != null && gotoStatement.Expression.Span.Contains(node.Span))
               return true;

            var parameter = current as ParameterSyntax;
            if (parameter != null && parameter.Default != null && parameter.Default.Value.Span.Contains(node.Span))
               return true;

            var equalsValue = current as EqualsValueClauseSyntax;
            if (equalsValue != null)
            {
               var variable = equalsValue.Parent as VariableDeclaratorSyntax;
               var declaration = variable == null ? null : variable.Parent as VariableDeclarationSyntax;
               var field = declaration == null ? null : declaration.Parent as FieldDeclarationSyntax;
               var local = declaration == null ? null : declaration.Parent as LocalDeclarationStatementSyntax;
               if ((field != null && field.Modifiers.Any(SyntaxKind.ConstKeyword)) ||
                   (local != null && local.Modifiers.Any(SyntaxKind.ConstKeyword)))
                  return true;
            }

            if (current is StatementSyntax || current is MemberDeclarationSyntax)
               break;
         }
         return false;
      }

      /// <summary>Copies a .NET assembly without changing its metadata.</summary>
      /// <param name="filePath">Path to the assembly file.</param>
      public static void ObfuscateAssembly(string filePath)
      {
         Console.WriteLine("Copying assembly unchanged: " + filePath);
         string newFilePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(filePath)),
             Path.GetFileNameWithoutExtension(filePath) + ".obf" + Path.GetExtension(filePath));
         File.Copy(filePath, newFilePath, true);
         Console.WriteLine("Assembly copied unchanged (metadata rewriting is disabled): " + newFilePath);
      }
   }
}