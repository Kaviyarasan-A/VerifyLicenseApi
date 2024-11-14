using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace VerifyLicenseApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public CustomerController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // Get Customer and Companies by CustomerId
        [HttpGet("GetCustomerWithCompanies/{customerId}")]
        public async Task<IActionResult> GetCustomerWithCompanies(long customerId)
        {
            var customerData = await _databaseService.GetCustomerAndCompaniesAsync(customerId);

            if (customerData == null)
            {
                return NotFound("Customer not found.");
            }

            // Return Customer Name and Companies (for dropdown)
            return Ok(customerData);
        }

        // Get full details of a selected company by CompanyName
        [HttpGet("GetCompanyDetailsByName/{companyName}")]
        public async Task<IActionResult> GetCompanyDetailsByName(string companyName)
        {
            var companyDetails = await _databaseService.GetCompanyByNameAsync(companyName);

            if (companyDetails == null)
            {
                return NotFound("Company not found.");
            }

            // Return company details
            return Ok(companyDetails);
        }

        // Get connection string for a specific company and connection type (Online or Offline)
        [HttpGet("GetCompanyConnectionString/{companyId}/{connectionType}")]
        public async Task<IActionResult> GetCompanyConnectionString(int companyId, string connectionType)
        {
            // Ensure connectionType is either "Online" or "Offline"
            if (connectionType != "Online" && connectionType != "Offline")
            {
                return BadRequest("Invalid connection type. It should be either 'Online' or 'Offline'.");
            }

            var connectionString = await _databaseService.GetConnectionStringAsync(companyId, connectionType);

            if (string.IsNullOrEmpty(connectionString))
            {
                return NotFound("Connection string not found for this company.");
            }

            return Ok(connectionString);
        }
    }
}
