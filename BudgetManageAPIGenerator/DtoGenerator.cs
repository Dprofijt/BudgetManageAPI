using BudgetManageAPIGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Dynamic;
using BudgetManageAPI.Attributes;

namespace BudgetManageAPIGenerator
{
    [Generator]
    public class DtoGenerator : ISourceGenerator
    {

        public void Initialize(GeneratorInitializationContext context)
        {
            // Initialization logic if needed
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Get the syntax trees in the current compilation
            var syntaxTrees = context.Compilation.SyntaxTrees;

            foreach (var tree in syntaxTrees)
            {
                // Process each tree and extract class declarations
                var root = tree.GetRoot();
                var classDeclarations = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(c => c.BaseList != null && c.BaseList.Types.Any(t => t.ToString() == "ICashFlow"));

                foreach (var classDecl in classDeclarations)
                {
                    // Generate DTO for the class
                    var dtoSource = GenerateDtoSource(classDecl);
                    context.AddSource($"{classDecl.Identifier.Text}Dto.g.cs", dtoSource);
                }
            }
        }

        private string GenerateDtoSource(ClassDeclarationSyntax classDecl)
        {
            var className = classDecl.Identifier.Text;
            var properties = classDecl.Members.OfType<PropertyDeclarationSyntax>()
                .Select(p => new { Type = p.Type.ToString(), Name = p.Identifier.Text })
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine($"public class {className}Dto");
            sb.AppendLine("{");

            foreach (var prop in properties)
            {
                sb.AppendLine($"    public {prop.Type} {prop.Name} {{ get; set; }}");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        // Add a method to generate DTO from an instance
        public static object GenerateDto(object model, DtoFilter filter = DtoFilter.None)
        {

            var dto = new ExpandoObject() as IDictionary<string, object>; // Create a dynamic DTO

            var modelType = model.GetType();
            foreach (var property in modelType.GetProperties())
            {
                bool includeProperty = true;


                switch (filter)
                {
                    case DtoFilter.Sensitive:
                        // Exclude properties marked with [Sensitive]
                        includeProperty = property.GetCustomAttribute<SensitiveAttribute>() == null;
                        break;

                    case DtoFilter.Public:
                        // Add logic for public properties, if needed
                        // For now, we'll just include all properties
                        break;

                    case DtoFilter.None:
                    default:
                        // No filtering, include all properties
                        break;
                }

                if (includeProperty)
                {
                    dto[property.Name] = property.GetValue(model);
                }
            }

            return dto;

            //var modelType = model.GetType();
            //var dtoTypeName = $"{modelType.Name}Dto"; // e.g., "IncomeDto"

            //// Use reflection to create a new type for the DTO
            //var dtoTypeBuilder = CreateDtoType(modelType);

            //// Create an instance of the new DTO type
            //var dtoInstance = Activator.CreateInstance(dtoTypeBuilder.CreateType());

            //// Map properties from the model to the DTO
            //foreach (var property in modelType.GetProperties())
            //{
            //    var dtoProperty = dtoTypeBuilder.GetProperty(property.Name);
            //    if (dtoProperty != null && dtoProperty.CanWrite)
            //    {
            //        dtoProperty.SetValue(dtoInstance, property.GetValue(model));
            //    }
            //}

            //return dtoInstance;
        }

        private static TypeBuilder CreateDtoType(Type modelType)
        {
            var assemblyName = new AssemblyName("DynamicDtoAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var typeBuilder = moduleBuilder.DefineType($"{modelType.Name}Dto", TypeAttributes.Public);

            // Define properties for the DTO based on the model
            foreach (var property in modelType.GetProperties())
            {
                var propertyBuilder = typeBuilder.DefineProperty(
                    property.Name,
                    PropertyAttributes.HasDefault,
                    property.PropertyType,
                    null);

                // Define the 'get' accessor method
                var getMethodBuilder = typeBuilder.DefineMethod(
                    $"get_{property.Name}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    property.PropertyType,
                    Type.EmptyTypes);

                var getIL = getMethodBuilder.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ret);
                propertyBuilder.SetGetMethod(getMethodBuilder);

                // Define the 'set' accessor method
                var setMethodBuilder = typeBuilder.DefineMethod(
                    $"set_{property.Name}",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    null,
                    new Type[] { property.PropertyType });

                var setIL = setMethodBuilder.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Ret);
                propertyBuilder.SetSetMethod(setMethodBuilder);
            }

            return typeBuilder;

        }

    }
}
