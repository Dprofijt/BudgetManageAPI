using Microsoft.CodeAnalysis; // Provides access to Roslyn APIs
using Microsoft.CodeAnalysis.CSharp.Syntax; // For C# syntax nodes
using System; // Basic types
using System.Collections.Generic; // For collections
using System.Linq; // For LINQ methods
using System.Text; // For StringBuilder
using System.Xml.Linq; // Unused in this code, can be removed

namespace BudgetManageAPIGenerator.Generators // Namespace for the source generator
{
    // The PropertyGenerator class implements ISourceGenerator to generate properties dynamically
    [Generator]
    public class PropertyGenerator : ISourceGenerator
    {
        // Constants for validation
        private const decimal MAX_AMOUNT = 1000000; // Maximum allowed amount for decimal properties
        private const decimal MAX_VALUE = 1000000; // Maximum value for integer properties
        private const int MIN_VALUE = 0; // Minimum value for integers
        private const int MAX_LENGTH = 500; // Maximum length for string properties

        // Diagnostic descriptor for existing properties
        private static readonly DiagnosticDescriptor ExistingPropertyDescriptor = new DiagnosticDescriptor(
            id: "BGA001",
            title: "Property Already Defined",
            messageFormat: "The property '{0}' is already defined in the class '{1}'. Skipping generation.",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        // This method is called once when the generator is initialized
        public void Initialize(GeneratorInitializationContext context)
        {
            // No specific initialization is needed for this example
        }

        // This method is called to generate code
        public void Execute(GeneratorExecutionContext context)
        {
            // Iterate through each syntax tree in the compilation
            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                // Get the root node of the syntax tree
                var root = syntaxTree.GetRoot();
                // Find all class declarations with attributes
                var classDeclarations = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(c => c.AttributeLists.Count > 0);

                // Define the namespace for models
                var modelsNamespace = "BudgetManageAPI.Models";

                // Process each class declaration
                foreach (var classDeclaration in classDeclarations)
                {
                    // Get the semantic model for the class
                    var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
                    // Get the symbol for the class
                    var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

                    // Skip classes that are not in the specified namespace
                    if (classSymbol == null || !classSymbol.ContainingNamespace.ToDisplayString().StartsWith(modelsNamespace))
                        continue;

                    // List to hold properties that need to be generated
                    var propertiesToGenerate = new List<string>();

                    // Get properties from base class attributes
                    var baseProperties = GetPropertiesFromBaseClass(classSymbol);
                    propertiesToGenerate.AddRange(baseProperties);

                    // Check for AutoGeneratePropertiesAttribute on the derived class
                    var classAttributes = classSymbol.GetAttributes()
                        .Where(ad => ad.AttributeClass?.ToDisplayString() == "CommonLibrary.Attributes.AutoGeneratePropertiesAttribute");

                    // Add properties specified in the attribute
                    foreach (var classAttribute in classAttributes)
                    {
                        propertiesToGenerate.AddRange(classAttribute.ConstructorArguments[0]
                            .Values.Select(arg => arg.Value?.ToString()));
                    }

                    // Check for existing properties in the class to avoid duplicates
                    var existingProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    existingProperties.UnionWith(classSymbol.GetMembers()
                        .OfType<IPropertySymbol>()
                        .Select(p => p.Name));

                    // Filter out properties that already exist and report diagnostics for them
                    foreach (var prop in propertiesToGenerate.ToList())
                    {
                        var propName = prop.Split(':')[0];
                        if (existingProperties.Contains(propName))
                        {
                            // Report a diagnostic message for the existing property
                            context.ReportDiagnostic(Diagnostic.Create(
                                ExistingPropertyDescriptor,
                                Location.None,
                                propName,
                                classSymbol.Name));

                            // Remove the existing property from the list of properties to generate
                            propertiesToGenerate.Remove(prop);
                        }
                    }

                    // Generate code only if there are new properties to add
                    if (propertiesToGenerate.Count > 0)
                    {
                        var generatedProperties = GenerateProperties(propertiesToGenerate);
                        // Construct the generated code for the class
                        var generatedCode = $@"
namespace {classSymbol.ContainingNamespace}
{{
    public partial class {classSymbol.Name}
    {{
        {generatedProperties}
    }}
}}";

                        // Add the generated source to the compilation
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

            // Loop through base classes to get properties
            while (baseClass != null)
            {
                var baseAttributes = baseClass.GetAttributes()
                    .FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == "CommonLibrary.Attributes.AutoGeneratePropertiesAttribute");

                // If the base class has the attribute, get its properties
                if (baseAttributes != null)
                {
                    properties.AddRange(baseAttributes.ConstructorArguments[0]
                        .Values.Select(arg => arg.Value?.ToString()));
                }

                // Move to the next base class
                baseClass = baseClass.BaseType;
            }

            return properties; // Return all collected properties
        }

        // Helper method to generate the properties as code
        public string GenerateProperties(List<string> properties)
        {
            var sb = new StringBuilder();

            // Generate each property based on the list
            foreach (var property in properties)
            {
                var parts = property.Split(':'); // Split the property definition
                var propertyName = parts[0]; // Get the property name
                var propertyType = parts[1]; // Get the property type
                var isRequired = bool.Parse(parts[2]); // Determine if the property is required
                
                // Determine the property type and whether it's required
                string propertyTypeWithNullability = isRequired ? propertyType : $"{propertyType}?";
                string requiredModifier = isRequired ? "required " : "";

                // Get validation checks for setters
                var validation = getValidateChecksForSetters(propertyType, propertyName);

                // Add validation for required properties
                if (isRequired && !propertyType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
                {
                    validation.Add($"if (value == null) throw new ArgumentNullException(nameof({propertyName}));");
                }

                // Initialize the backing field for non-nullable types (e.g., int, decimal, string)
                string defaultValue = propertyType switch
                {
                    "decimal" => " = 0m", // Use the 'm' suffix for decimal literals
                    "int" => " = 0",
                    "string" => " = string.Empty", // Initialize strings to an empty string
                    _ => "" // Default case, no initialization
                };


                // Append the generated property code to the StringBuilder
                sb.AppendLine($@"
    public {requiredModifier}{propertyType} {propertyName}
    {{
        get => _{propertyName}; // Getter for the property
        set
        {{
            {string.Join("\r\n        ", validation)} // Validation checks in the setter
            _{propertyName} = value; // Set the property value
        }}
    }}
    private {propertyType} _{propertyName}{defaultValue};"); // Backing field for the property
            }

            // Add the Validate method after generating the properties
            sb.AppendLine($@"
public void Validate()
{{
    // Validation logic for each property
    {string.Join("\r\n" + "    ", properties.Select(p =>
            {
                var name = p.Split(':')[0]; // Get the property name
                var type = p.Split(':')[1]; // Get the property type
                var isRequired = bool.Parse(p.Split(':')[2]); // Check if it's required

                // Get validation checks for the Validate method
                var validateChecks = GetValidateChecksForValidateFunction(type, name);

                // Add required property check
                if (isRequired && !type.Equals("decimal", StringComparison.OrdinalIgnoreCase))
                {
                    validateChecks.Add($"if ({name} == null) throw new ArgumentNullException(nameof({name}));");
                }

                return string.Join("\r\n" + "    ", validateChecks).Trim(); // Return all validation checks
            }))}
}}");

            return sb.ToString(); // Return the generated code as a string
        }

        // Generate validation checks for setters
        public List<string> getValidateChecksForSetters(string propertyType, string propertyName)
        {
            var validation = new List<string>();
            if (propertyType.Equals("int", StringComparison.OrdinalIgnoreCase))
            {
                // Validation checks for integers
                validation.Add($"if (value < 0) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} must be non-negative.\");");
                validation.Add($"if (value < {MIN_VALUE} || value > {MAX_VALUE}) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} must be between {MIN_VALUE} and {MAX_VALUE}.\");");
            }
            else if (propertyType.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                // Validation checks for strings
                validation.Add($"if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(\"{propertyName} cannot be null or empty.\", nameof({propertyName}));");
                validation.Add($"if (value.Length > {MAX_LENGTH}) throw new ArgumentException(\"{propertyName} cannot exceed {MAX_LENGTH} characters.\", nameof({propertyName}));");
            }
            else if (propertyType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
            {
                // Validation checks for decimals
                validation.Add($"if (value < 0) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} must be non-negative.\");");
                validation.Add($"if (value > {MAX_AMOUNT}) throw new ArgumentOutOfRangeException(nameof({propertyName}), \"{propertyName} cannot exceed {MAX_AMOUNT}.\");");
                validation.Add($"if (Math.Round(value, 2) != value) throw new ArgumentException(\"{propertyName} must have at most two decimal places.\", nameof({propertyName}));");
            }

            return validation; // Return all validation checks
        }

        // Generate validation checks for the Validate method
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
            return validateChecks; // Return all validation checks for the Validate method
        }
    }

    // Extension methods for symbols
    public static class SymbolExtensions
    {
        // Helper method to check if a symbol is derived from a base class
        public static bool IsDerivedFrom(this INamedTypeSymbol symbol, INamedTypeSymbol baseClass)
        {
            while (symbol != null)
            {
                // Check if the symbol matches the base class
                if (symbol.Equals(baseClass, SymbolEqualityComparer.Default))
                    return true;

                // Move to the next base class
                symbol = symbol.BaseType;
            }
            return false; // Return false if not derived from the specified base class
        }
    }
}
