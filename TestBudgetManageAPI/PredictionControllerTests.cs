using BudgetManageAPI.Controllers;
using BudgetManageAPI.Data;
using BudgetManageAPI.Models;
using BudgetManageAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBudgetManageAPI
{
    public class PredictionControllerTests
    {
        private DbContextOptions<DBContextBudgetManageAPI> CreateNewContextOptions()
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<DBContextBudgetManageAPI>();
            builder.UseInMemoryDatabase("InMemoryDb")
                   .UseInternalServiceProvider(serviceProvider);

            return builder.Options;
        }

        [Fact]
        public void TrainModel_ShouldReturnOk()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using (var context = new DBContextBudgetManageAPI(options))
            {
                context.Outcomes.AddRange(
                    //new Outcome { Id = 1, UserId = "user1", Description = "Groceries", Date = new DateTime(2022, 1, 1), Amount = 200 },
                    //new Outcome { Id = 2, UserId = "user1", Description = "Rent", Date = new DateTime(2022, 2, 1), Amount = 300 },
                    //new Outcome { Id = 3, UserId = "user1", Description = "Utilities", Date = new DateTime(2022, 3, 1), Amount = 250 }
                );
                context.SaveChanges();
            }

            using (var context = new DBContextBudgetManageAPI(options))
            {
                var mlService = new MLService();
                var controller = new PredictionController(mlService, context);

                // Act
                var result = controller.TrainModel("user1");

                // Assert
                Assert.IsType<OkObjectResult>(result);
            }
        }

        [Fact]
        public void PredictExpense_ShouldReturnPrediction()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using (var context = new DBContextBudgetManageAPI(options))
            {
                context.Outcomes.AddRange(
                    //new Outcome { Id = 1, UserId = "user1", Description = "Groceries", Date = new DateTime(2022, 1, 1), Amount = 200 },
                    //new Outcome { Id = 2, UserId = "user1", Description = "Rent", Date = new DateTime(2022, 2, 1), Amount = 300 },
                    //new Outcome { Id = 3, UserId = "user1", Description = "Utilities", Date = new DateTime(2022, 3, 1), Amount = 250 }
                );
                context.SaveChanges();
            }

            using (var context = new DBContextBudgetManageAPI(options))
            {
                var mlService = new MLService();
                var controller = new PredictionController(mlService, context);

                // Act
                var trainResult = controller.TrainModel("user1");
                var predictResult = controller.PredictExpense("user1", 2022, 4) as OkObjectResult;

                // Assert
                Assert.IsType<OkObjectResult>(predictResult);
                //Assert.InRange((float)predictResult.Value, 0, 1000); // Assuming the predicted value should be within this range
            }
        }
    }
}
