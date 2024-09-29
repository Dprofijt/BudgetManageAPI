using BudgetManageAPI.Dtos;
using BudgetManageAPI.Interfaces;
using CommonLibrary;
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
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult OkWithDto(object item, DtoFilter filter)
        {
            var dto = DtoGenerator.GenerateDto(item, filter);
            return Ok(dto);
        }
    }

}
