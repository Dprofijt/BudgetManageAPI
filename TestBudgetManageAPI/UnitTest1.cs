using BudgetManageAPI.Examples;

namespace TestBudgetManageAPI
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var calc = new DocumentationExample.Calculator();
            int result = calc.Add(2, 3);
            Assert.Equal(5, result);
        }
    }
}