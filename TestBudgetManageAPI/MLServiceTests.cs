//using BudgetManageAPI.Enums;
//using BudgetManageAPI.Models.ExpensePrediction;
//using BudgetManageAPI.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TestBudgetManageAPI
//{
//    public class MLServiceTests
//    {
//        private Dictionary<(int year, int month), List<ExpenseData>> GetTestData()
//        {
//            return new Dictionary<(int year, int month), List<ExpenseData>>
//        {
//            { (2022, 1), new List<ExpenseData>
//                {
//                    new ExpenseData { Year = 2022, Month = 1, Total = 200, Description = "Groceries", MoneyOutcomeCategory = MoneyOutcomeCategory.Food },
//                    new ExpenseData { Year = 2022, Month = 1, Total = 500, Description = "Rent", MoneyOutcomeCategory = MoneyOutcomeCategory.Housing }
//                }
//            },
//            { (2022, 2), new List<ExpenseData>
//                {
//                    new ExpenseData { Year = 2022, Month = 2, Total = 300, Description = "Groceries", MoneyOutcomeCategory = MoneyOutcomeCategory.Food }
//                }
//            },
//            { (2022, 3), new List<ExpenseData>
//                {
//                    new ExpenseData { Year = 2022, Month = 3, Total = 250, Description = "Groceries", MoneyOutcomeCategory = MoneyOutcomeCategory.Food },
//                    new ExpenseData { Year = 2022, Month = 3, Total = 500, Description = "Rent", MoneyOutcomeCategory = MoneyOutcomeCategory.Housing }
//                }
//            },
//            { (2022, 4), new List<ExpenseData>
//                {
//                    new ExpenseData { Year = 2022, Month = 4, Total = 270, Description = "Groceries", MoneyOutcomeCategory = MoneyOutcomeCategory.Food }
//                }
//            },
//            { (2022, 5), new List<ExpenseData>
//                {
//                    new ExpenseData { Year = 2022, Month = 5, Total = 260, Description = "Groceries", MoneyOutcomeCategory = MoneyOutcomeCategory.Food },
//                    new ExpenseData { Year = 2022, Month = 5, Total = 500, Description = "Rent", MoneyOutcomeCategory = MoneyOutcomeCategory.Housing }
//                }
//            },
//            { (2022, 6), new List<ExpenseData>
//                {
//                    new ExpenseData { Year = 2022, Month = 6, Total = 280, Description = "Groceries", MoneyOutcomeCategory = MoneyOutcomeCategory.Food }
//                }
//            }
//        };
//        }
//        [Fact]
//        public void TrainModel_ShouldTrainModelWithValidData()
//        {
//            // Arrange
//            var mlService = new MLService();
//            var data = GetTestData().SelectMany(kv => kv.Value).ToList();

//            // Act
//            mlService.TrainModel(data);

//            // Assert
//            var (predictions, total) = mlService.PredictExpenses(2022, 4);
//            Assert.InRange(total, 0, 1000); // Assuming the predicted value should be within this range
//        }

//        [Theory]
//        [InlineData(2022, 7, MoneyOutcomeCategory.Housing, 450, 550)]
//        [InlineData(2022, 9, MoneyOutcomeCategory.Housing, 450, 550)]
//        [InlineData(2022, 8, MoneyOutcomeCategory.Housing, 0, 0)]
//        public void PredictExpense_ShouldReturnPrediction(int year, int month, MoneyOutcomeCategory category, int expectedMin, int expectedMax)
//        {
//            // Arrange
//            var mlService = new MLService();
//            var data = GetTestData().SelectMany(kv => kv.Value).ToList();
//            mlService.TrainModel(data);

//            // Act
//            var (predictions, total) = mlService.PredictExpenses(year, month);

//            // Assert
//            Assert.InRange(total, expectedMin, expectedMax);
//            Assert.True(predictions.OrderBy(p => p.MoneyOutcomeCategory).SequenceEqual(predictions));
//            foreach (var prediction in predictions)
//            {
//                Assert.NotNull(prediction.Description);
//                Assert.NotNull(prediction.MoneyOutcomeCategory);

//                if (prediction.MoneyOutcomeCategory.Equals(category))
//                {
//                    Assert.InRange(prediction.Total, expectedMin, expectedMax);
//                }
//            }
//        }
    



//        //[Theory]
//        //[InlineData(2022, 1, MoneyOutcomeCategory.Entertainment, 150, 300)]  // Regular month with moderate expenses
//        //[InlineData(2022, 3, MoneyOutcomeCategory.Entertainment, 400, 600)]  // Month with recurring big payment
//        //[InlineData(2022, 6, MoneyOutcomeCategory.Entertainment, 400, 600)]  // Month with recurring big payment
//        //[InlineData(2023, 2, MoneyOutcomeCategory.Entertainment, 150, 300)]  // Future year, regular month
//        //[InlineData(2023, 3, MoneyOutcomeCategory.Entertainment, 400, 600)]  // Future year, month with recurring big payment
//        //public void PredictExpenseWithRecurringBigPayment_ShouldReturnPrediction(int year, int month, MoneyOutcomeCategory category, int expectedMin, int expectedMax)
//        //{
//        //    // Arrange
//        //    var mlService = new MLService();
//        //    var data = new List<ExpenseData>
//        //    {
//        //        new ExpenseData { Year = 2022, Month = 1, Total = 200, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2022, Month = 2, Total = 300, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2022, Month = 3, Total = 500, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2022, Month = 4, Total = 200, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2022, Month = 5, Total = 250, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2022, Month = 6, Total = 500, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2022, Month = 7, Total = 200, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2022, Month = 8, Total = 300, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2022, Month = 9, Total = 500, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2022, Month = 10, Total = 200, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2022, Month = 11, Total = 250, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2022, Month = 12, Total = 500, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 1, Total = 200, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 2, Total = 300, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 3, Total = 500, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 4, Total = 200, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 5, Total = 250, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 6, Total = 500, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 7, Total = 200, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 8, Total = 300, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 9, Total = 500, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 10, Total = 200, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 11, Total = 250, MoneyOutcomeCategory = category },
//        //        new ExpenseData { Year = 2023, Month = 12, Total = 500, MoneyOutcomeCategory = category }
//        //    };
//        //    mlService.TrainModel(data);

//        //    // Act
//        //    var (prediction, message) = mlService.PredictExpense(year, month, category, data);

//        //    // Assert
//        //    Assert.InRange(prediction, expectedMin, expectedMax);
//        //    Assert.True(string.IsNullOrEmpty(message));
//        //}
//    }
//}
