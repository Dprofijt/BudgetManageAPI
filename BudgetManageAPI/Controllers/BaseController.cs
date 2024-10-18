using BudgetManageAPI.Dtos;
using BudgetManageAPI.Interfaces;
using CommonLibrary;
using Microsoft.AspNetCore.Mvc;
using GeneratedDto;
using BudgetManageAPI.Models;
using BudgetManageAPI.Attributes;

namespace BudgetManageAPI.Controllers
{
    public abstract class BaseController<T> : ControllerBase where T : class
    {
        private DtoGenerator_old _generator = new DtoGenerator_old();
        protected IActionResult OkWithDto(IEnumerable<T> items, DtoFilter filter)
        {
            var dtos = items.Select(item => {
                return MethodInvoker.InvokeWithLogging(_generator, nameof(DtoGenerator_old.GenerateDto), item, filter);
            }).ToList();
            return Ok(dtos);
        }
        protected IActionResult OkWithDto(T item, DtoFilter filter)
        {
            //Can be used like:
            //var dto2 = new Income { Amount = 10, Description = "testc"};
            //var dtoIncome = new IncomeDTO();
            //dtoIncome.Map(dto2);
            //var income = new Outcome.Builder();

            var dto = DtoGenerator_old.GenerateDto(item, filter);
            return Ok(dto);
        }
    }
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult OkWithDto(object item, DtoFilter filter)
        {
            var dto = DtoGenerator_old.GenerateDto(item, filter);
            return Ok(dto);
        }
    }

}
