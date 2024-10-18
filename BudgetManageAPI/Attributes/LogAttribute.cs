namespace BudgetManageAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LogAttribute : Attribute
    {
        public void LogMethod(string methodName)
        {
            Console.WriteLine(methodName);
        }
        public void LogMethodStart(string methodName)
        {
            Console.WriteLine($"Method {methodName} started.");
        }

        public void LogMethodEnd(string methodName)
        {
            Console.WriteLine($"Method {methodName} completed.");
        }
    }
    // Helper Method Invoker
    public static class MethodInvoker
    {
        public static object InvokeWithLogging(object instance, string methodName, params object[] args)
        {
            var method = instance.GetType().GetMethod(methodName);
            var logAttribute = (LogAttribute)method.GetCustomAttributes(typeof(LogAttribute), false).FirstOrDefault();

            logAttribute?.LogMethodStart(methodName);

            var result = method.Invoke(instance, args);

            logAttribute?.LogMethodEnd(methodName);

            return result;
        }
    }
}
