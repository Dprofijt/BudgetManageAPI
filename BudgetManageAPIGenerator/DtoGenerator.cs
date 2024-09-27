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

        /// <summary>
        /// Generates a dynamic Data Transfer Object (DTO) from a given model object,
        /// applying optional filtering based on the specified DtoFilter.
        /// </summary>
        /// <param name="model">The source object from which to create the DTO.</param>
        /// <param name="filter">The filter to apply when generating the DTO.</param>
        /// <returns>An ExpandoObject representing the filtered properties of the model.</returns>
        public static object GenerateDto(object model, DtoFilter filter = DtoFilter.Default)
        {
            var dto = new ExpandoObject() as IDictionary<string, object>;

            var modelType = model.GetType();

            var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var filteredProperties = properties.Where(property =>
            {
                switch (filter)
                {
                    //also returns sensitive information
                    case DtoFilter.ShowSensitive:
                        return true;
                    //the default, will filter out sensitive
                    case DtoFilter.Default:
                        return property.GetCustomAttribute<SensitiveAttribute>() == null;

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
