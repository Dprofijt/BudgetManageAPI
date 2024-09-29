using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestGenerator
{
    [Generator]
    public class SimpleTestGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // Generate a simple test class
            var testClassSource = GenerateSimpleTestClass();

            // Add the generated unit test source to the compilation
            context.AddSource("SimpleTest.g.cs", SourceText.From(testClassSource, Encoding.UTF8));

        }

        public void Initialize(GeneratorInitializationContext context)
        {
            
        }

        private string GenerateSimpleTestClass()
        {
            return @"

using Xunit;

namespace TestBudgetManageAPI
{

    public class SimpleTest
    {
        [Fact]
        public void Test_One_Equals_One()
        {
            Assert.Equal(1, 1);
        }
    }
}

        ";
        }
    }
}
