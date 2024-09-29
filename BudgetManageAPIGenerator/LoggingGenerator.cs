using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BudgetManageAPIGenerator
{
   

    [Generator]
    public class MyCombinedSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Initialization if needed
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Specify the folders you want to include
            var targetFolders = new[] { "Controllers", "Services" }; // Add the folders you want to target here

            //Iterate over all the syntax trees in the project
            var syntaxTrees = context.Compilation.SyntaxTrees;
            //foreach (var syntaxTree in syntaxTrees)
            //{

            //    var filePath = syntaxTree.FilePath;

            //    // Filter for specific folders
            //    if (!targetFolders.Any(folder => filePath.IndexOf(folder, StringComparison.OrdinalIgnoreCase) >= 0))
            //    {
            //        continue; // Skip files not in the specified folders
            //    }

            //    var root = syntaxTree.GetRoot();

            //    // Find method declarations in the syntax tree
            //    var methods = root.DescendantNodes()
            //                      .OfType<MethodDeclarationSyntax>()
            //                      .Where(m => m.Body != null); // Only methods with bodies

            //    // For each method, generate logging-wrapped methods
            //    foreach (var method in methods)
            //    {
            //        var classDeclaration = method.Parent as ClassDeclarationSyntax;
            //        if (classDeclaration != null)
            //        {
            //            var className = classDeclaration.Identifier.Text;
            //            var methodName = method.Identifier.Text;

            //            // Get parameter types for unique naming
            //            var parameterTypes = string.Join("_", method.ParameterList.Parameters
            //                                                .Select(p => p.Type.ToString()));

            //            // Generate the logging method with a unique hint name
            //            var sourceText = GenerateLoggingMethod(className, methodName);
            //            var uniqueHintName = $"{className}_{methodName}_{parameterTypes}_Logging.cs".Replace("<", "").Replace(">", ""); // Remove special characters for the filename
            //            context.AddSource(uniqueHintName, SourceText.From(sourceText, Encoding.UTF8));
            //        }
            //    }
            //}
        }

        private string GenerateLoggingMethod(string className, string methodName)
        {
            return $@"
using System;
namespace {className}
{{
    public partial class {className}
    {{
        public void {methodName}_WithLogging()
        {{
            Console.WriteLine(""Entering method {methodName}"");
            {methodName}();  // Call the original method
            Console.WriteLine(""Exiting method {methodName}"");
        }}
    }}
}}
";
        }
    }

}
