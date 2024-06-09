using BudgetManageAPI.Interfaces;
using BudgetManageAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManageAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenericCashFlowController<T> : ControllerBase where T : class,
        ICashFlow
    {

        [HttpGet]
        public OkResult Get()
        {
            Console.Write("TEst");
            return Ok();
        }
    
    }
}
