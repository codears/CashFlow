using CashFlow.Infra.Services;
using CashReportApi.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CashReportApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("{date}")]
        public async Task<ActionResult<Response>> Get(string date)
        {
            if (!DateOnly.TryParse(date, out var formatDate))
                return BadRequest("Invalid date. Use date format: yyyy-MM-dd.");

            var response = await _reportService.GetBalanceByDateAsync(formatDate);
            return response.Error is not null
                ? this.BadRequest(response) :
                Ok(response?.Value);
        }
    }
}
