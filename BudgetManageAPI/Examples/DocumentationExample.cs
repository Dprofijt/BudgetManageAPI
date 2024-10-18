// Your main project file
namespace BudgetManageAPI.Examples
{
    public class DocumentationExample
    {
        public class Calculator
        {
            /// <summary>
            /// Adds two integers and returns the result.
            /// </summary>
            /// <example>
            /// <code>
            /// var calc = new DocumentationExample.Calculator();
            /// int result = calc.Add(2, 3);
            /// Assert.Equal(5, result);
            /// </code>
            /// </example>
            public int Add(int a, int b)
            {
                return a + b;
            }
        }
    }
}
