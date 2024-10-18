using Xunit;
using BudgetManageAPI.Examples;
namespace TestBudgetManageAPI.Tests
{
    public class Calculator_Add_Tests
    {
        [Fact]
        public void Add_ExampleTest()
        {
             var calc = new DocumentationExample.Calculator();
 int result = calc.Add(2, 3);
 Assert.Equal(5, result);
        }
    }
}
