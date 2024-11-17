using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace VerifyLicenseApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        // Constructor to inject DatabaseService
        public CustomerController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // Get Customer and Companies by LicenseKey
        [HttpGet("GetCustomerWithCompanies/{licenseKey}")]
        public async Task<IActionResult> GetCustomerWithCompanies(string licenseKey)
        {
            var customerData = await _databaseService.GetCustomerAndCompaniesByLicenseKeyAsync(licenseKey);

            if (customerData == null)
            {
                return NotFound("Customer with the given LicenseKey not found.");
            }

            return Ok(customerData);
        }

        // Get a company by name (company name)
        [HttpGet("GetCompanyDetailsByName/{companyName}")]
        public async Task<IActionResult> GetCompanyDetailsByName(string companyName)
        {
            var companyDetails = await _databaseService.GetCompanyByNameAsync(companyName);

            if (companyDetails == null)
            {
                return NotFound("Company not found.");
            }

            return Ok(companyDetails);
        }

        // Get connection string for a specific company and connection type (Online or Offline)
        [HttpGet("GetCompanyConnectionString/{companyId}/{connectionType}")]
        public async Task<IActionResult> GetCompanyConnectionString(int companyId, string connectionType)
        {
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
