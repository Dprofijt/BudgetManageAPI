using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BudgetManageAPIGenerator
{
    [Generator]
    public class PropertyGenerator : ISourceGenerator
    {
        private const decimal MAX_AMOUNT = 1000000; // Example maximum amount
        private const decimal MAX_VALUE = 1000000; // Example maximum amount
        private const int MIN_VALUE = 0; // Example minimum value for integers
        private const int MAX_LENGTH = 500; // Example maximum length for strings

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

            // Adding amount property
            //var amountProperty = "Amount:decimal:true"; // Required decimal property
            //if (!properties.Contains(amountProperty)) // Prevent duplicates
            //{
            //    properties.Add(amountProperty);
            //}

            foreach (var property in properties)
            {
                var parts = property.Split(':');
                var propertyName = parts[0];
                var propertyType = parts[1];
                var isRequired = bool.Parse(parts[2]);

                var validation = getValidateChecksForSetters(propertyType, propertyName);

                if (isRequired)
                {
                    validation.Add($"if (value == null) throw new ArgumentNullException(nameof({propertyName}));");
                }

                sb.AppendLine($@"
    public {propertyType} {propertyName}
    {{
        get => _{propertyName};
        set
        {{
            {string.Join("\r\n        ", validation)}
            _{propertyName} = value;
        }}
    }}
    private {propertyType} _{propertyName};");
            }

            // Add the Validate method after generating the properties
            sb.AppendLine($@"
public void Validate()
{{
    // Validation logic for each property
    {string.Join("\r\n" + "    ", properties.Select(p =>
            {
                var name = p.Split(':')[0];
                var type = p.Split(':')[1];
                var isRequired = bool.Parse(p.Split(':')[2]);

                var validateChecks = GetValidateChecksForValidateFunction(type, name);

                if (isRequired)
                {
                    validateChecks.Add($"if ({name} == null) throw new ArgumentNullException(nameof({name}));");
                }


                return string.Join("\r\n" + "    ", validateChecks).Trim();
            }))}
}}");


            return sb.ToString();
        }

        public List<string> getValidateChecksForSetters(string propertyType, string propertyName)
        {
            var validation = new List<string>();
            if (propertyType.Equals("int", StringComparison.OrdinalIgnoreCase))
            {
                validation.Add($"if (value < 0) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} must be non-negative.\");");

                validation.Add($"if (value < {MIN_VALUE} || value > {MAX_VALUE}) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} must be between {MIN_VALUE} and {MAX_VALUE}.\");");

            }
            else if (propertyType.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                validation.Add($"if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(\"{propertyName} cannot be null or empty.\", nameof({propertyName}));");

                validation.Add($"if (value.Length > {MAX_LENGTH}) throw new ArgumentException(\"{propertyName} cannot exceed {MAX_LENGTH} characters.\", nameof({propertyName}));");

            }

            else if (propertyType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
            {
                validation.Add($"if (value < 0) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} must be non-negative.\");");
                // Add maximum amount validation
                validation.Add($"if (value > {MAX_AMOUNT}) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} cannot exceed {MAX_AMOUNT}.\");");
                // Add precision check
                validation.Add($"if (Math.Round(value, 2) != value) throw new ArgumentException(\"{propertyName} must have at most two decimal places.\", nameof({propertyName}));");
            }

            return validation;
        }

        public List<string> GetValidateChecksForValidateFunction(string propertyType, string propertyName)
        {
            var validateChecks = new List<string>();
            if (propertyType.Equals("int", StringComparison.OrdinalIgnoreCase))
            {
                validateChecks.Add($"if ({propertyName} < {MIN_VALUE} || {propertyName} > {MAX_VALUE}) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} must be between {MIN_VALUE} and {MAX_VALUE}.\");");
            }
            else if (propertyType.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                validateChecks.Add($"if (string.IsNullOrWhiteSpace({propertyName})) throw new ArgumentException(\"{propertyName} cannot be null or empty.\", nameof({propertyName}));");
                validateChecks.Add($"if ({propertyName}.Length > {MAX_LENGTH}) throw new ArgumentException(\"{propertyName} cannot exceed {MAX_LENGTH} characters.\", nameof({propertyName}));");
            }
            else if (propertyType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
            {
                validateChecks.Add($"if ({propertyName} < 0) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} must be non-negative.\");");
                validateChecks.Add($"if ({propertyName} > {MAX_AMOUNT}) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} cannot exceed {MAX_AMOUNT}.\");");
                validateChecks.Add($"if (Math.Round({propertyName}, 2) != {propertyName}) throw new ArgumentException(\"{propertyName} must have at most two decimal places.\", nameof({propertyName}));");
            }
            return validateChecks;
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
