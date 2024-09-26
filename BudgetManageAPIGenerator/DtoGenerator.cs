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
            //TODO no clue about this
        }

        // Add a method to generate DTO from an instance
        public static object GenerateDto(object model, DtoFilter filter = DtoFilter.None)
        {
            var dto = new ExpandoObject() as IDictionary<string, object>; // Create a dynamic DTO

            var modelType = model.GetType();

            var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var filteredProperties = properties.Where(property =>
            {
                switch (filter)
                {
                    case DtoFilter.Sensitive:
                        // Exclude properties marked with [Sensitive]
                        return property.GetCustomAttribute<SensitiveAttribute>() == null;

                    case DtoFilter.Public:
                        // Add logic for public properties, if needed
                        // For now, we'll just include all properties
                        return true;

                    case DtoFilter.None:
                    default:
                        // No filtering, include all properties
                        return true;
                }
            });
            foreach (var property in filteredProperties)
            {
                dto[property.Name] = property.GetValue(model);
            }
        
            return dto;
        }

    }
}
