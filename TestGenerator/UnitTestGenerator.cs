using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Generator]
public class UnitTestGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization needed for this example
    }

    //TODO: not yet working
    public void Execute(GeneratorExecutionContext context)
    {
        // Access the current compilation (this includes the main project)
        var compilation = context.Compilation;

        // HashSet to track generated source names
        var generatedSourceNames = new HashSet<string>();

        // Define the target namespace to include
        string targetNamespace = "BudgetManageAPI"; // Adjust this to your main namespace

        // Get all public classes within the target namespace and sub-namespaces
        var publicClasses = compilation.GlobalNamespace
            .GetMembers()
            .OfType<INamespaceSymbol>()
            .Where(ns => ns.ToDisplayString().StartsWith(targetNamespace))
            .SelectMany(ns => ns.GetMembers())
            .OfType<INamedTypeSymbol>()
            .Where(t =>
                t.TypeKind == TypeKind.Class &&
                t.DeclaredAccessibility == Accessibility.Public);


        foreach (var cls in publicClasses)
        {
            var className = cls.Name;
            var classNamespace = cls.ContainingNamespace.ToDisplayString();
            var testClassName = $"{className}Tests";

            // Create a unique hint name by combining namespace and class name
            var uniqueHintName = $"{classNamespace}.{testClassName}.g.cs";

            // Check if the source has already been generated
            if (generatedSourceNames.Contains(uniqueHintName))
            {
                continue; // Skip if already generated
            }

            var sb = new StringBuilder();

            // Start generating the test class
            sb.AppendLine($"using Xunit;");
            sb.AppendLine($"namespace {classNamespace}.Tests;");
            sb.AppendLine($"public class {testClassName} {{");

            // Generate test methods for each public method
            var publicMethods = cls.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.DeclaredAccessibility == Accessibility.Public && !m.IsImplicitlyDeclared);

            foreach (var method in publicMethods)
            {
                var methodName = method.Name;

                // Create a test method for the public method
                sb.AppendLine($"    [Fact]");
                sb.AppendLine($"    public void {methodName}_Test() {{");
                sb.AppendLine($"        // Arrange");
                sb.AppendLine($"        // Act");
                sb.AppendLine($"        // Assert");
                sb.AppendLine($"    }}");
            }

            sb.AppendLine($"}}"); // Close the test class

            // Add the generated test class to the compilation
            context.AddSource(uniqueHintName, SourceText.From(sb.ToString(), Encoding.UTF8));

            // Mark the source name as generated
            generatedSourceNames.Add(uniqueHintName);
        }
    }
}
