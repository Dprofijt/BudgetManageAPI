using BudgetManageAPI.Interfaces;
using BudgetManageAPIGenerator;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManageAPI.Controllers
{
    public abstract class BaseController<T> : ControllerBase where T : class
    {
        protected IActionResult OkWithDto(IEnumerable<T> items, DtoFilter filter)
        {
            var dtos = items.Select(item => DtoGenerator.GenerateDto(item, filter)).ToList();
            return Ok(dtos);
        }
        protected IActionResult OkWithDto(T item, DtoFilter filter)
        {
            var dto = DtoGenerator.GenerateDto(item, filter);
            return Ok(dto);
        }
    }

}
