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

        public MLService()
        {
            _mlContext = new MLContext();
        }

        public void TrainModel(List<ExpenseData> trainingData)
        {
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Concatenate("Features", new[] { "Year", "Month" })
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
                MoneyOutcomeCategory = category.ToString()
        };

        var prediction = predictionEngine.Predict(input);

        return (prediction.Total, string.Empty);
        }
    }
}
