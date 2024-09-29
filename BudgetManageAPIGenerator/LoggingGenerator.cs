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
             //Iterate over all the syntax trees in the project
            var syntaxTrees = context.Compilation.SyntaxTrees;
            foreach (var syntaxTree in syntaxTrees)
            {
                var root = syntaxTree.GetRoot();

                // Find method declarations in the syntax tree
                var methods = root.DescendantNodes()
                                  .OfType<MethodDeclarationSyntax>()
                                  .Where(m => m.Body != null); // Only methods with bodies

                // For each method, generate logging-wrapped methods
                foreach (var method in methods)
                {
                    var classDeclaration = method.Parent as ClassDeclarationSyntax;
                    if (classDeclaration != null)
                    {
                        var className = classDeclaration.Identifier.Text;
                        var methodName = method.Identifier.Text;

                        // Generate the logging method
                        var sourceText = GenerateLoggingMethod(className, methodName);
                        context.AddSource($"{className}_{methodName}_Logging.cs", SourceText.From(sourceText, Encoding.UTF8));
                    }
                }
            }
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
