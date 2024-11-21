using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VerifyLicenseApi;

namespace VerifyLicenseApi.Controllers
{
    public class LicenseInactiveException : Exception
    {
        public LicenseInactiveException(string message) : base(message) { }
    }

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
            try
            {
                // Fetch the customer and associated companies based on the license key
                var customerData = await _databaseService.GetCustomerAndCompaniesByLicenseKeyAsync(licenseKey);

                // If no customer data is found, return not found
                if (customerData == null)
                {
                    return BadRequest("Customer data is not available.");
                }

                // Return the customer data if the license is active
                return Ok(customerData);
            }
            catch (LicenseInactiveException ex)
            {
                // Catch the LicenseInactiveException and return a 200 OK response with the exception message
                return Ok(ex.Message); // Return 200 OK with the message indicating the license is inactive
            }
            catch (Exception ex)
            {
                // Catch other unexpected exceptions and return a generic error
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
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
