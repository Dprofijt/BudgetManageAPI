
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BudgetManageAPIGenerator.Generators
{
    [Generator]
    public class DocumentationGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization needed for this example
            if (!Debugger.IsAttached)
            {
                //Debugger.Launch();
            }
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Iterate over all syntax trees in the project
            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                // Process each class
                foreach (var classDeclaration in classes)
                {
                    var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();

                    // Process each method in the class
                    foreach (var method in methods)
                    {
                        // Extract XML documentation from leading trivia (comments before the method)
                        var leadingTrivia = method.GetLeadingTrivia();

                        Debug.WriteLine("Leading Trivia:");
                        foreach (var trivia in leadingTrivia)
                        {
                            Debug.WriteLine(trivia.ToFullString());
                        }

                        // Find the XML documentation comment (not working)
                        //var xmlComment = leadingTrivia
                        //    .Where(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                        //    .Select(trivia => trivia.GetStructure())
                        //    .OfType<DocumentationCommentTriviaSyntax>()
                        //    .FirstOrDefault();

                        // Debug output for the extracted xmlComment
                        Debug.WriteLine("Extracted XML Comment:");
                        //Debug.WriteLine(xmlComment);

                        // Combine the trivia into a single string for easier processing
                        var xmlDocumentation = string.Join("\r\n", leadingTrivia.Select(trivia => trivia.ToFullString()));

                        // Parse the XML documentation directly
                        string exampleCode = ExtractExampleCode(xmlDocumentation);

                        string methodSummary = ExtractMethodSummary(xmlDocumentation); // New method to extract summary


                        Debug.WriteLine(exampleCode);

                        // Handle the case where exampleCode is found
                        if (!string.IsNullOrEmpty(exampleCode))
                        {
                            // Get the full class name (including any parent classes)
                            var fullClassName = GetFullClassName(classDeclaration);
                            var methodName = method.Identifier.Text;

                            var namespaceDeclaration = classDeclaration.Ancestors()
                                .OfType<NamespaceDeclarationSyntax>()
                                .FirstOrDefault();

                            var testSourceCode = GenerateTestClass(namespaceDeclaration.Name.ToString(), fullClassName, methodName, exampleCode);


                            context.AddSource($"{fullClassName}_{methodName}_GeneratedTests.cs", SourceText.From(testSourceCode, Encoding.UTF8));

                            // Generate the Markdown content
                            var markdownContent = GenerateMarkdown(methodSummary, exampleCode, methodName, fullClassName);
                            //context.AddSource($"{fullClassName}_{methodName}_Documentation.md", SourceText.From(markdownContent, Encoding.UTF8));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursively builds the full class name, including any outer (parent) classes.
        /// </summary>
        private string GetFullClassName(ClassDeclarationSyntax classDeclaration)
        {
            var className = classDeclaration.Identifier.Text;

            //// Check if the class is nested inside another class
            //var parentClass = classDeclaration.Parent as ClassDeclarationSyntax;
            //if (parentClass != null)
            //{
            //    // If nested, prepend the parent class name recursively
            //    return GetFullClassName(parentClass) + "." + className;
            //}

            return className;
        }

        private string ExtractExampleCode(string xmlDocumentation)
        {
            // Parse the XML documentation
            try
            {
                var xDoc = XDocument.Parse("<root>" + xmlDocumentation + "</root>");

                // Extract the content of the <code> tag within <example>
                var codeNode = xDoc.Descendants("code").FirstOrDefault();
                if (codeNode != null)
                {
                    // Remove XML comment markers and clean up the code
                    var cleanCode = codeNode.Value
                        .Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries) // Split by new lines
                        .Select(line => line.Trim().TrimStart('/')) // Trim whitespace and remove leading slashes
                        .Where(line => !string.IsNullOrWhiteSpace(line)) // Remove empty lines
                        .ToArray();

                    return string.Join("\r\n", cleanCode); // Join cleaned lines back into a single string
                }
            }
            catch
            {
                // Handle parsing errors (in case XML is malformed or missing)
            }

            return null;
        }

        private string GenerateTestClass(string namespaceDeclaration, string className, string methodName, string exampleCode)
        {
            var builder = new StringBuilder();

            builder.AppendLine("using Xunit;");
            builder.AppendLine($"using {namespaceDeclaration};");
            builder.AppendLine($"namespace TestBudgetManageAPI.Tests");
            builder.AppendLine("{");
            //builder.AppendLine($"    [TestFixture]");
            builder.AppendLine($"    public class {className}_{methodName}_Tests");
            builder.AppendLine("    {");

            builder.AppendLine($"        [Fact]");
            builder.AppendLine($"        public void {methodName}_ExampleTest()");
            builder.AppendLine("        {");

            // Insert the extracted example code into the test
            builder.AppendLine($"            {exampleCode}");

            builder.AppendLine("        }");

            builder.AppendLine("    }");
            builder.AppendLine("}");

            return builder.ToString();
        }

        private string ExtractMethodSummary(string xmlDocumentation)
        {
            // Parse the XML documentation and extract the summary
            try
            {
                var xDoc = XDocument.Parse("<root>" + xmlDocumentation + "</root>");
                var summaryNode = xDoc.Descendants("summary").FirstOrDefault();
                return summaryNode?.Value.Trim() ?? string.Empty; // Trim whitespace and return summary
            }
            catch
            {
                return string.Empty; // Return empty if parsing fails
            }
        }

        private string GenerateMarkdown(string summary, string exampleCode, string methodName, string className)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"# {className}.{methodName}");
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine(summary);
            builder.AppendLine();
            builder.AppendLine("## Example Usage");
            builder.AppendLine("```csharp");
            builder.AppendLine(exampleCode);
            builder.AppendLine("```");
            return builder.ToString();
        }
    }
}

