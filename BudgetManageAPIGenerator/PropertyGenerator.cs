using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BudgetManageAPIGenerator
{
    [Generator]
    public class PropertyGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // No need for initialization in this example.
        }

        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var classDeclarations = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(c => c.AttributeLists.Count > 0);

                var modelsNamespace = "BudgetManageAPI.Models";

                foreach (var classDeclaration in classDeclarations)
                {
                    var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
                    var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

                    if (classSymbol == null || !classSymbol.ContainingNamespace.ToDisplayString().StartsWith(modelsNamespace))
                        continue;

                    var propertiesToGenerate = new List<string>();

                    // Get properties from base class attributes
                    var baseProperties = GetPropertiesFromBaseClass(classSymbol);
                    propertiesToGenerate.AddRange(baseProperties);

                    // Check for AutoGeneratePropertiesAttribute on the derived class
                    var classAttributes = classSymbol.GetAttributes()
                        .Where(ad => ad.AttributeClass?.ToDisplayString() == "CommonLibrary.Attributes.AutoGeneratePropertiesAttribute");

                    foreach (var classAttribute in classAttributes)
                    {
                        propertiesToGenerate.AddRange(classAttribute.ConstructorArguments[0]
                            .Values.Select(arg => arg.Value?.ToString()));
                    }

                    // Check for existing properties in the class
                    var existingProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    existingProperties.UnionWith(classSymbol.GetMembers()
                        .OfType<IPropertySymbol>()
                        .Select(p => p.Name));

                    // Filter out properties that already exist
                    propertiesToGenerate = propertiesToGenerate
                        .Where(prop => !existingProperties.Contains(prop.Split(':')[0]))
                        .ToList();

                    // Generate code only if there are new properties to add
                    if (propertiesToGenerate.Count > 0)
                    {
                        var generatedProperties = GenerateProperties(propertiesToGenerate);
                        var generatedCode = $@"
namespace {classSymbol.ContainingNamespace}
{{
    public partial class {classSymbol.Name}
    {{
        {generatedProperties}
    }}
}}";

                        context.AddSource($"{classSymbol.Name}_Generated.cs", generatedCode);
                    }
                }
            }
        }

        // Helper method to recursively get properties from base class attributes
        private IEnumerable<string> GetPropertiesFromBaseClass(INamedTypeSymbol classSymbol)
        {
            var properties = new List<string>();
            var baseClass = classSymbol.BaseType;

            while (baseClass != null)
            {
                var baseAttributes = baseClass.GetAttributes()
                    .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == "CommonLibrary.Attributes.AutoGeneratePropertiesAttribute");

                if (baseAttributes != null)
                {
                    properties.AddRange(baseAttributes.ConstructorArguments[0]
                        .Values.Select(arg => arg.Value?.ToString()));
                }

                // Move to the next base class
                baseClass = baseClass.BaseType;
            }

            return properties;
        }




        // Helper method to generate the properties as code
        public string GenerateProperties(List<string> properties)
        {
            var sb = new StringBuilder();

            foreach (var property in properties)
            {
                var parts = property.Split(':');
                var propertyName = parts[0];
                var propertyType = parts[1];
                var isRequired = bool.Parse(parts[2]);

                var validation = isRequired
                    ? $"if (value == null) throw new ArgumentNullException(nameof({propertyName}));"
                    : string.Empty;

                sb.AppendLine($@"
    public {propertyType} {propertyName}
    {{
        get => _{propertyName};
        set
        {{
            {validation}
            _{propertyName} = value;
        }}
    }}
    private {propertyType} _{propertyName};");
            }

            return sb.ToString();
        }



    }
    public static class SymbolExtensions
    {
        // Helper method to check if a symbol is derived from a base class
        public static bool IsDerivedFrom(this INamedTypeSymbol symbol, INamedTypeSymbol baseClass)
        {
            while (symbol != null)
            {
                if (symbol.Equals(baseClass, SymbolEqualityComparer.Default))
                    return true;

                symbol = symbol.BaseType;
            }
            return false;
        }
    }
}
