using CommonLibrary.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BudgetManageAPIGenerator.Generators
{
    [Generator]
    public class BuilderGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(postInitContext =>
            {
                // This will allow us to create the Builder class after properties are generated
            });
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var nodes = context.Compilation.SyntaxTrees
                .SelectMany(s => s.GetRoot().DescendantNodes());

            var classDeclarationSyntaxes = nodes
                .Select(s => s as ClassDeclarationSyntax)
                .Where(s => s != null);

            foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
            {
                var attributes = classDeclarationSyntax.AttributeLists
                    .SelectMany(s => s.Attributes);

                var hasGenerateBuilderAttribute = attributes.FirstOrDefault(attribute => attribute.Name.ToString() == nameof(GenerateBuilder));
                if (hasGenerateBuilderAttribute == null)
                {
                    continue;
                }

                var className = classDeclarationSyntax.Identifier.ToString();

                //TODO ignore builder item?

                var properties = classDeclarationSyntax.ChildNodes()
                    .Select(s => s as PropertyDeclarationSyntax)
                    .Where(s => s != null);

                if (properties.Count() <= 0)
                {
                    continue;
                }

                var builderLines = CreateBuilderLines(className, properties);

                // Find the namespace declaration that contains the class
                var namespaceDeclaration = classDeclarationSyntax.Ancestors()
                    .OfType<NamespaceDeclarationSyntax>()
                    .FirstOrDefault();

                var generatedClass = GenerateClass(className, namespaceDeclaration.Name.ToString(), builderLines);

                context.AddSource($"{className}_Builder.Generated.cs", generatedClass);
            }
        }

        private StringBuilder CreateBuilderLines(string className, IEnumerable<PropertyDeclarationSyntax> properties)
        {
            var builderName = $"Builder";
            var builderLines = new StringBuilder();

            builderLines.AppendLine($"public class {builderName}");
            builderLines.AppendLine("{");
            builderLines.Append($"public {builderName}(){{}}");

            foreach (var property in properties)
            {
                var propertyName = property.Identifier.Text;
                var propertyType = property.Type.ToString();

                builderLines.AppendLine($@"
    private {propertyType} _{propertyName};

    public {builderName} With{propertyName}({propertyType} value)
    {{
        _{propertyName} = value;
        return this;
    }}");
            }

            builderLines.AppendLine($@"
    public {className} Build()
    {{
        return new {className}()
        {{
");
            var propertyList = properties.ToList(); // Convert to list to use index
            for (int i = 0; i < propertyList.Count; i++)
            {
                var propertyName = propertyList[i].Identifier.Text;
                builderLines.AppendLine($"          {propertyName} = _{propertyName}{(i < propertyList.Count - 1 ? "," : "")}");

            }

            builderLines.AppendLine($@"
            }};
        }}
    }}");
            return builderLines;
        }

        private string GenerateClass(string className, string namespaceDeclaration, StringBuilder builderLines)
        {
            var classBuilder = new StringBuilder();

            // Ensure the namespace declaration is correctly formatted
            classBuilder.AppendLine($"using BudgetManageAPI.Enums;");
            classBuilder.AppendLine($"namespace {namespaceDeclaration}");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine($@"
    public partial class {className}
    {{
        // Nested builder class
        {builderLines.ToString()}
    }}
}}");

            return classBuilder.ToString();
        }

    }
}
