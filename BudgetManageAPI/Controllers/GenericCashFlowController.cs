using BudgetManageAPI.Dtos;
using BudgetManageAPI.Interfaces;
using BudgetManageAPI.Models;
using BudgetManageAPI.Repositories;
using CommonLibrary;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManageAPI.Controllers
{
    [Route("api/[controller]")]
    public class IncomeController : GenericCashFlowController<Income>
    {
        public IncomeController(IRepository<Income> repository) : base(repository) { }
    }

    [Route("api/[controller]")]
    public class OutcomeController : GenericCashFlowController<Outcome>
    {
        public OutcomeController(IRepository<Outcome> repository) : base(repository) { }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class GenericCashFlowController<T> : BaseController<T> where T : class, ICashFlow
    {

        private readonly IRepository<T> _repository;

        public GenericCashFlowController(IRepository<T> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _repository.GetAllAsync();


            return OkWithDto(items, DtoFilter.Default);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();
            return OkWithDto(item, DtoFilter.Default);
        }

        [HttpPost]
        public async Task<IActionResult> Create(T item)
        {
            await _repository.AddAsync(item);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, T item)
        {
            if (id != item.Id) return BadRequest();

            await _repository.UpdateAsync(item);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
