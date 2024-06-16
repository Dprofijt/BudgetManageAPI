using BudgetManageAPI.Enums;
using Microsoft.ML.Data;

namespace BudgetManageAPI.Models.ExpensePrediction
{
    public class ExpensePrediction
    {
        public float Year { get; set; }
        public float Month { get; set; }
        public string Description { get; set; }
        public string MoneyOutcomeCategory { get; set; }
        [ColumnName("Score")]
        public float Total { get; set; }
    }
    public class ExpenseData
    {
        public float Year { get; set; }
        public float Month { get; set; }
        public float Total { get; set; }
        public string Description { get; set; }
        public MoneyOutcomeCategory MoneyOutcomeCategory { get; set; } // should be of value MoneyOutcomeCategory Enum
    }
    public class TransformedExpenseData
    {
        public float Year { get; set; }
        public float Month { get; set; }
        public float Total { get; set; }
        public string Description { get; set; }
        public string MoneyOutcomeCategory { get; set; }
    }


    public class ExpenseDataWithRecurring : ExpenseData
    {
        public float IsRecurring { get; set; }
    }

    public class ExpenseDataWithLags : ExpenseData
    {
        public float PreviousTotal1 { get; set; }
        public float PreviousTotal2 { get; set; }
        public float PreviousTotal3 { get; set; }
        public float PreviousTotal4 { get; set; }
    }

    public class ExpensePredictionResult
    {
        public float Year { get; set; }
        public float Month { get; set; }
        public MoneyOutcomeCategory MoneyOutcomeCategory { get; set; }
        public string Description { get; set; }
        public float Total { get; set; }
    }
}
