namespace SimpleDotNetObfuscator.Libraries
{
   using System;
   using System.IO;
   using System.Linq;
   using System.Text;
   using Microsoft.CodeAnalysis;
   using Microsoft.CodeAnalysis.CSharp;
   using Microsoft.CodeAnalysis.CSharp.Syntax;

   public sealed class Obfuscator
   {
      /// <summary>
      /// Encodes a string as Base64. This is reversible encoding, not encryption.
      /// </summary>
      /// <param name="input">String to encode.</param>
      /// <returns>Base64-encoded UTF-8 string.</returns>
      public static string EncryptString(string input)
      {
         byte[] data = System.Text.Encoding.UTF8.GetBytes(input);
         return Convert.ToBase64String(data);
      }

      /// <summary>
      /// Decodes a Base64-encoded UTF-8 string.
      /// </summary>
      /// <param name="input">Base64-encoded UTF-8 string.</param>
      /// <returns>The decoded string.</returns>
      public static string DecryptString(string input)
      {
         byte[] data = Convert.FromBase64String(input);
         return System.Text.Encoding.UTF8.GetString(data);
      }

      /// <summary>
      /// Rewrites non-constant C# string literals as fully qualified UTF-8 Base64 decode expressions.
      /// </summary>
      /// <param name="filePath">Path to the C# source file.</param>
      public static void ObfuscateCSFile(string filePath)
      {
         Console.WriteLine("Encoding C# source string literals: " + filePath);
         string code = File.ReadAllText(filePath);

         var tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(LanguageVersion.Latest));
         var rewriter = new StringLiteralRewriter();
         string obfuscatedCode = rewriter.Visit(tree.GetRoot()).ToFullString();

         string newFilePath = Path.Combine(Path.GetDirectoryName(filePath),
             Path.GetFileNameWithoutExtension(filePath) + ".obf.cs");
         File.WriteAllText(newFilePath, obfuscatedCode);
         Console.WriteLine("Encoded source saved: " + newFilePath);
      }

      private sealed class StringLiteralRewriter : CSharpSyntaxRewriter
      {
         public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
         {
            if (!node.IsKind(SyntaxKind.StringLiteralExpression) || IsCompileTimeConstantContext(node))
               return base.VisitLiteralExpression(node);

            // ValueText is the actual C# string value, with escapes and verbatim quoting resolved.
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(node.Token.ValueText));
            string expression = "global::System.Text.Encoding.UTF8.GetString(" +
               "global::System.Convert.FromBase64String(\"" + encoded + "\"))";
            return SyntaxFactory.ParseExpression(expression).WithTriviaFrom(node);
         }

         private static bool IsCompileTimeConstantContext(SyntaxNode node)
         {
            for (SyntaxNode current = node.Parent; current != null; current = current.Parent)
            {
               if (current is AttributeArgumentSyntax || current is CaseSwitchLabelSyntax ||
                   current is ConstantPatternSyntax || current is CasePatternSwitchLabelSyntax)
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
      }

      /// <summary>
      /// Copies a .NET assembly without changing metadata. Identifier rewriting is intentionally
      /// disabled because safe handling of reflection, external references, and virtual contracts
      /// requires more information than is available from a single assembly.
      /// </summary>
      /// <param name="filePath">Path to the assembly file.</param>
      public static void ObfuscateAssembly(string filePath)
      {
         Console.WriteLine("Copying assembly unchanged: " + filePath);

         string newFilePath = Path.Combine(Path.GetDirectoryName(filePath),
             Path.GetFileNameWithoutExtension(filePath) + ".obf" + Path.GetExtension(filePath));
         File.Copy(filePath, newFilePath, true);
         Console.WriteLine("Assembly copied unchanged (metadata rewriting is disabled): " + newFilePath);
      }
   }
}
