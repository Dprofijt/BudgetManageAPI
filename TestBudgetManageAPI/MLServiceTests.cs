using BudgetManageAPI.Enums;
using BudgetManageAPI.Models.ExpensePrediction;
using BudgetManageAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBudgetManageAPI
{
    public class MLServiceTests
    {
        [Fact]
        public void TrainModel_ShouldTrainModelWithValidData()
        {
            // Arrange
            var mlService = new MLService();
            var data = new List<ExpenseData>
            {
                new ExpenseData { Year = 2022, Month = 1, Total = 200, Description="TEST", MoneyOutcomeCategory = MoneyOutcomeCategory.Food.ToString() },
                new ExpenseData { Year = 2022, Month = 2, Total = 300, Description="TEST",MoneyOutcomeCategory = MoneyOutcomeCategory.Food.ToString() },
                new ExpenseData { Year = 2022, Month = 3, Total = 250, Description="TEST",MoneyOutcomeCategory = MoneyOutcomeCategory.Food.ToString() },
            };

            // Act
            mlService.TrainModel(data);

            // Assert
            var (prediction, message) = mlService.PredictExpense(2022, 4, MoneyOutcomeCategory.Food, data);
            Assert.InRange(prediction, 0, 1000); // Assuming the predicted value should be within this range
        }

        [Theory]
        [InlineData(2022, 4, MoneyOutcomeCategory.Food, 150, 300)]
        [InlineData(2022, 5, MoneyOutcomeCategory.Food, 200, 350)]
        [InlineData(2022, 6, MoneyOutcomeCategory.Food, 180, 320)]
        public void PredictExpense_ShouldReturnPrediction(int year, int month, MoneyOutcomeCategory category, int expectedMin, int expectedMax)
        {
            // Arrange
            var mlService = new MLService();
            var data = new List<ExpenseData>
            {
                new ExpenseData { Year = 2022, Month = 1, Total = 200, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 2, Total = 300, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 3, Total = 250, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 4, Total = 270, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 5, Total = 260, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 6, Total = 280, MoneyOutcomeCategory = category.ToString() }
            };
            mlService.TrainModel(data);

            // Act
            var (prediction, message) = mlService.PredictExpense(year, month, category, data);

            // Assert
            Assert.InRange(prediction, expectedMin, expectedMax);
            Assert.True(string.IsNullOrEmpty(message));
        }


        [Theory]
        [InlineData(2022, 1, MoneyOutcomeCategory.Entertainment, 150, 300)]  // Regular month with moderate expenses
        [InlineData(2022, 3, MoneyOutcomeCategory.Entertainment, 400, 600)]  // Month with recurring big payment
        [InlineData(2022, 6, MoneyOutcomeCategory.Entertainment, 400, 600)]  // Month with recurring big payment
        [InlineData(2023, 2, MoneyOutcomeCategory.Entertainment, 150, 300)]  // Future year, regular month
        [InlineData(2023, 3, MoneyOutcomeCategory.Entertainment, 400, 600)]  // Future year, month with recurring big payment
        public void PredictExpenseWithRecurringBigPayment_ShouldReturnPrediction(int year, int month, MoneyOutcomeCategory category, int expectedMin, int expectedMax)
        {
            // Arrange
            var mlService = new MLService();
            var data = new List<ExpenseData>
            {
                new ExpenseData { Year = 2022, Month = 1, Total = 200, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 2, Total = 300, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 3, Total = 500, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 4, Total = 200, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 5, Total = 250, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 6, Total = 500, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 7, Total = 200, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 8, Total = 300, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 9, Total = 500, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 10, Total = 200, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 11, Total = 250, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2022, Month = 12, Total = 500, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 1, Total = 200, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 2, Total = 300, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 3, Total = 500, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 4, Total = 200, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 5, Total = 250, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 6, Total = 500, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 7, Total = 200, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 8, Total = 300, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 9, Total = 500, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 10, Total = 200, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 11, Total = 250, MoneyOutcomeCategory = category.ToString() },
                new ExpenseData { Year = 2023, Month = 12, Total = 500, MoneyOutcomeCategory = category.ToString() }
            };
            mlService.TrainModel(data);

            // Act
            var (prediction, message) = mlService.PredictExpense(year, month, category, data);

            // Assert
            Assert.InRange(prediction, expectedMin, expectedMax);
            Assert.True(string.IsNullOrEmpty(message));
        }
    }
}
