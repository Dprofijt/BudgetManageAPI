//using BudgetManageAPI.Data;
//using BudgetManageAPI.Models.ExpensePrediction;
//using BudgetManageAPI.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace BudgetManageAPI.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class PredictionController : ControllerBase
//    {
//        private readonly MLService _mlService;
//        private readonly DBContextBudgetManageAPI _context;

//        public PredictionController(MLService mlService, DBContextBudgetManageAPI context)
//        {
//            _mlService = mlService;
//            _context = context;
//        }

//        [HttpPost("train")]
//        public IActionResult TrainModel(string userId)
//        {
//            var expenses = _context.Outcomes
//                .Where(e => e.UserId == userId)
//                .GroupBy(e => new { e.Date.Year, e.Date.Month })
//                .Select(g => new ExpenseData
//                {
//                    Year = g.Key.Year,
//                    Month = g.Key.Month,
//                    Total = (float)g.Sum(e => e.Amount)
//                })
//                .ToList();

//            _mlService.TrainModel(expenses);

//            return Ok("Model trained successfully.");
//        }

//        [HttpGet("predict")]
//        public IActionResult PredictExpense(string userId, int year, int month)
//        {
//            // Optionally retrain the model before predicting
//            var expenses = _context.Outcomes
//                .Where(e => e.UserId == userId)
//                .GroupBy(e => new { e.Date.Year, e.Date.Month })
//                .Select(g => new ExpenseData
//                {
//                    Year = g.Key.Year,
//                    Month = g.Key.Month,
//                    Total = (float)g.Sum(e => e.Amount)
//                })
//                .ToList();

//            if (expenses.Count() == 0)
//            {
//                return BadRequest("Not enough data to make a prediction.");
//            }

//            _mlService.TrainModel(expenses);

//            var prediction = _mlService.PredictExpense(year, month, Enums.MoneyOutcomeCategory.Debt,expenses);
//            return Ok(prediction);
//        }
//    }
//}
