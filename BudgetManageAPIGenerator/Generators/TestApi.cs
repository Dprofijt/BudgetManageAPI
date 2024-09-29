using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetManageAPIGenerator.Generators
{
    [Generator]
    public class TestApi : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // Define the model name (you can customize this)
            string modelName = "Product"; // You could pass this dynamically later.

            // Generate the controller source code
            var source = GenerateController(modelName);

            // Add the generated controller to the compilation
            context.AddSource($"{modelName}Controller.g.cs", source);
        }

        public void Initialize(GeneratorInitializationContext context)
        {

            Console.WriteLine("Source generator initialized.");
        }

        private string GenerateController(string modelName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("");
            sb.AppendLine("namespace BudgetManageAPI.Controllers");
            sb.AppendLine("{");
            sb.AppendLine($"    [ApiController]");
            sb.AppendLine($"    [Route(\"api/[controller]\")]");
            sb.AppendLine($"    public class {modelName}Controller : ControllerBase");
            sb.AppendLine("    {");
            sb.AppendLine($"        [HttpGet]");
            sb.AppendLine($"        public ActionResult<IEnumerable<string>> Get()");
            sb.AppendLine("        {");
            sb.AppendLine("            // Example GET method returning a list of strings");
            sb.AppendLine($"            return new string[] {{ \"{modelName} item1\", \"{modelName} item2\" }};");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
