using CashFlow.Domain.Models;
using CashFlow.Infra.Services;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CashController : ControllerBase
    {
        private readonly ICashPostingService _service;

        public CashController(ICashPostingService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CashPostingRequest request)
        {
            var cashPosting = await _service.CreateCashPostingAsync(request);
            return CreatedAtAction(nameof(Post), new { id = cashPosting.Id }, cashPosting);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var cashPostings = await _service.GetAllAsync();
            return Ok(cashPostings);
        }
    }
}
