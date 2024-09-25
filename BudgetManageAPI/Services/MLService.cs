using BudgetManageAPI.Enums;
using BudgetManageAPI.Models.ExpensePrediction;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BudgetManageAPI.Services
{
    public class MLService
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private List<ExpenseData> _historicalData;
        private Dictionary<string, ITransformer> _userModels = new Dictionary<string, ITransformer>();


        public MLService()
        {
            _mlContext = new MLContext();
        }

        public void TrainModelForUser(List<ExpenseData> userHistoricalData, string userId)
        {
            var enrichedData = AddRecurringFeatures(userHistoricalData, userId);

            var transformedData = enrichedData
                .Select(data => new TransformedExpenseData
                {
                    Year = data.Year,
                    Month = data.Month,
                    Total = data.Total,
                    Description = data.Description,
                    MoneyOutcomeCategory = data.MoneyOutcomeCategory.ToString()
                }).ToList();

            var dataView = _mlContext.Data.LoadFromEnumerable(transformedData);

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("CategoryKey", nameof(TransformedExpenseData.MoneyOutcomeCategory))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("CategoryEncoded", "CategoryKey"))
                .Append(_mlContext.Transforms.Concatenate("Features", new[] { "Year", "Month", "CategoryEncoded" }))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: "Total", maximumNumberOfIterations: 100));

            var model = pipeline.Fit(dataView);

            // Store the trained model for the user (optional, depending on your application architecture)
            // You can store it in memory, a database, or serialize it to disk
            _userModels[userId] = model; // Assuming _userModels is a dictionary<string, ITransformer> to store models per user
        }

        public (List<ExpensePrediction> predictions, float total) PredictExpensesForUser(int year, int month, string userId)
        {
            if (!_userModels.ContainsKey(userId))
            {
                throw new ArgumentException($"Model for user {userId} has not been trained. Please train the model first.");
            }

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TransformedExpenseData, ExpensePrediction>(_userModels[userId]);

            var predictions = new List<ExpensePrediction>();
            float total = 0;

            var userHistoricalData = _historicalData[int.Parse(userId)]; // Assuming _historicalData is a dictionary<string, List<ExpenseData>> storing historical data per user

            var inputList = Enum.GetValues<MoneyOutcomeCategory>()
                                .Select(category => new TransformedExpenseData
                                {
                                    Year = year,
                                    Month = month,
                                    MoneyOutcomeCategory = category.ToString()
                                })
                                .ToList();

            foreach (var expense in inputList)
            {
                var prediction = predictionEngine.Predict(expense);

                var predictedExpense = new ExpensePrediction
                {
                    Year = expense.Year,
                    Month = expense.Month,
                    Description = expense.Description,
                    MoneyOutcomeCategory = expense.MoneyOutcomeCategory,
                    Total = prediction.Total
                };

                predictions.Add(predictedExpense);
                total += predictedExpense.Total;
            }

            var orderedPredictions = predictions.OrderBy(p => p.MoneyOutcomeCategory).ToList();
            return (orderedPredictions, total);
        }

        private List<ExpenseData> AddRecurringFeatures(List<ExpenseData> historicalData, string userId)
        {
            var enrichedData = new List<ExpenseData>();

            // Add original historical data
            enrichedData.AddRange(historicalData);

            // Retrieve user-specific recurring expenses and intervals (you need to implement this)
            var recurringExpenses = GetRecurringExpensesForUser(userId);

            foreach (var recurringExpense in recurringExpenses)
            {
                // Generate recurring entries based on intervals
                var lastMonthWithData = historicalData.Max(d => d.Month);
                var currentMonth = DateTime.Now.Month;
                for (int month = (int)(lastMonthWithData + recurringExpense.Interval); month <= currentMonth; month += recurringExpense.Interval)
                {
                    enrichedData.Add(new ExpenseData
                    {
                        Year = DateTime.Now.Year, // or use a specific year logic
                        Month = month,
                        Total = recurringExpense.Amount,
                        Description = recurringExpense.Description,
                        MoneyOutcomeCategory = recurringExpense.MoneyOutcomeCategory
                    });
                }
            }

            return enrichedData;
        }

        // Example method to retrieve user-specific recurring expenses (you need to implement this)
        private List<RecurringExpense> GetRecurringExpensesForUser(string userId)
        {
            // Implement logic to fetch recurring expenses for the user from a database or other storage
            // Example implementation:
            //return dbContext.RecurringExpenses.Where(re => re.UserId == userId).ToList();
        }


        #region "old" way 
        public void TrainModel(List<ExpenseData> trainingData)
        {
            _historicalData = trainingData;

            var enrichedData = AddRecurringFeatures(trainingData)
                       .Select(data => new TransformedExpenseData
                       {
                           Year = data.Year,
                           Month = data.Month,
                           Total = data.Total,
                           Description = data.Description,
                           MoneyOutcomeCategory = data.MoneyOutcomeCategory.ToString()
                       }).ToList();
            var dataView = _mlContext.Data.LoadFromEnumerable(enrichedData);

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("CategoryKey", nameof(TransformedExpenseData.MoneyOutcomeCategory))
        .Append(_mlContext.Transforms.Categorical.OneHotEncoding("CategoryEncoded", "CategoryKey"))
        .Append(_mlContext.Transforms.Concatenate("Features", new[] { "Year", "Month", "CategoryEncoded" }))
        .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
        .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: "Total", maximumNumberOfIterations: 100));




            _model = pipeline.Fit(dataView);
        }

        public (float prediction, string message) PredictExpense(int year, int month, MoneyOutcomeCategory category, List<ExpenseData> historyData)
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ExpenseData, ExpensePrediction>(_model);

            var input = new ExpenseData
            {
                Year = year,
                Month = month,
                MoneyOutcomeCategory = category
        };

        var prediction = predictionEngine.Predict(input);

        return (prediction.Total, string.Empty);
        }

        public (List<ExpensePrediction> predictions, float total) PredictExpenses(int year, int month)
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TransformedExpenseData, ExpensePrediction>(_model);

            var predictions = new List<ExpensePrediction>();
            float total = 0;

            var inputList = Enum.GetValues<MoneyOutcomeCategory>()
                            .Select(category => new TransformedExpenseData
                            {
                                Year = year,
                                Month = month,
                                MoneyOutcomeCategory = category.ToString()
                            })
                            .ToList();

            foreach (var expense in inputList)
            {
                var prediction = predictionEngine.Predict(expense);

                // Check historical data for the category
                var hasHistoricalData = _historicalData.Any(data => data.MoneyOutcomeCategory.ToString() == expense.MoneyOutcomeCategory);

                var predictedExpense = new ExpensePrediction
                {
                    Year = expense.Year,
                    Month = expense.Month,
                    Description = expense.Description,
                    MoneyOutcomeCategory = expense.MoneyOutcomeCategory,
                    Total = hasHistoricalData ? prediction.Total : 0
                };
                predictions.Add(predictedExpense);
                total += predictedExpense.Total;
            }

            var orderedPredictions = predictions.OrderBy(p => p.MoneyOutcomeCategory).ToList();
            return (orderedPredictions, total);
        }

        private List<ExpenseDataWithRecurring> AddRecurringFeatures(List<ExpenseData> data)
        {
            var enrichedData = new List<ExpenseDataWithRecurring>();
            var groupedData = data.GroupBy(e => new { e.Description, e.MoneyOutcomeCategory });

            foreach (var group in groupedData)
            {
                var expenses = group.OrderBy(e => new DateTime((int)e.Year, (int)e.Month, 1)).ToList();
                var periods = new Dictionary<int, int>(); // period in months, frequency

                for (int i = 1; i < expenses.Count; i++)
                {
                    var previousExpense = expenses[i - 1];
                    var currentExpense = expenses[i];

                    var period = ((int)currentExpense.Year - (int)previousExpense.Year) * 12 + ((int)currentExpense.Month - (int)previousExpense.Month);

                    if (periods.ContainsKey(period))
                    {
                        periods[period]++;
                    }
                    else
                    {
                        periods[period] = 1;
                    }
                }

                var recurringPeriod = periods.OrderByDescending(p => p.Value).FirstOrDefault().Key;

                foreach (var expense in expenses)
                {
                    var isRecurring = recurringPeriod > 0 && expenses.Any(e =>
                        e.Description == expense.Description &&
                        e.MoneyOutcomeCategory == expense.MoneyOutcomeCategory &&
                        ((int)expense.Year - (int)e.Year) * 12 + ((int)expense.Month - (int)e.Month) % recurringPeriod == 0);

                    enrichedData.Add(new ExpenseDataWithRecurring
                    {
                        Year = expense.Year,
                        Month = expense.Month,
                        Total = expense.Total,
                        Description = expense.Description,
                        MoneyOutcomeCategory = expense.MoneyOutcomeCategory,
                        IsRecurring = isRecurring ? 1 : 0
                    });
                }
            }

            return enrichedData;
        }

        #endregion
    }
}
