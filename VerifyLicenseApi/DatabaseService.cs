using Azure.Core;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VerifyLicenseApi.Controllers;
using static VerifyLicenseApi.ValidationModel;

namespace VerifyLicenseApi
{
    public class DatabaseService
    {
        private readonly IDbConnection _dbConnection;

        // Constructor to initialize database connection
        public DatabaseService(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Existing method to get customer and associated companies
        public async Task<CustomerWithCompanies> GetCustomerAndCompaniesByLicenseKeyAsync(string licenseKey)
        {
            var query = @"
                SELECT 
                    ch.CustomerID,
                    ch.CustomerName,
                    ch.Country,
                    ch.CustomerAddress,
                    ch.LicenseKey,
                    ch.Status,
                    cd.CompanyId,
                    cd.CompanyName,
                    cd.ConnectionStringOnline,
                    cd.ConnectionStringOffline,
                    cd.Comment
                FROM 
                    Customer_Header ch
                JOIN 
                    Customer_Details cd ON ch.CustomerID = cd.CustomerID
                WHERE 
                    ch.LicenseKey = @LicenseKey;
            ";

            var customerData = await _dbConnection.QueryAsync<CustomerHeader, CustomerDetails, CustomerWithCompanies>(
                query,
                (customer, company) =>
                {
                    return new CustomerWithCompanies
                    {
                        Customer = customer,
                        Companies = new List<CustomerDetails> { company }
                    };
                },
                new { LicenseKey = licenseKey },
                splitOn: "CompanyId"
            );

            // If no customer data is found, return null
            if (customerData == null || !customerData.Any())
            {
                return null;
            }

            // Correctly declare and initialize 'result' by getting the first customer
            var result = customerData.FirstOrDefault();

            // Check if the Status is not "Active" and return null if it's inactive
            if (result.Customer.Status.Trim() != "Active")
            {
                throw new LicenseInactiveException("The license is inactive. Please check your license status.");
            }


            // Log License Access Details into the LogFile table (newly added)
            await LogLicenseAccessAsync(result.Customer.LicenseKey, result.Customer.CustomerId);

            // If the user is active, add additional companies (if any)
            foreach (var company in customerData.Skip(1))
            {
                result.Companies.Add(company.Companies.First());
            }

            return result; // Return the active customer with associated companies
        }

        // New method to log LicenseKey access details into the LogFile table
        public async Task LogLicenseAccessAsync(string licenseKey, long customerId)
        {
            var query = @"
                INSERT INTO LicenseAccessLog (LicenseKey, CustomerId)
                VALUES (@LicenseKey,  @CustomerId);
            ";

            var parameters = new
            {
                LicenseKey = licenseKey,
                 // Current date and time
                CustomerId = customerId
            };

            // Execute the query to insert the log entry
            await _dbConnection.ExecuteAsync(query, parameters);
        }

        // Get a company by name (company name)
        public async Task<CustomerDetails> GetCompanyByNameAsync(string companyName)
        {
            var query = "SELECT * FROM Customer_Details WHERE CompanyName = @CompanyName";
            return await _dbConnection.QueryFirstOrDefaultAsync<CustomerDetails>(query, new { CompanyName = companyName });
        }

        // Get the connection string for a specific company and connection type (Online or Offline)
        public async Task<string> GetConnectionStringAsync(int companyId, string connectionType)
        {
            var query = "SELECT " + connectionType + " AS ConnectionString FROM Customer_Details WHERE CompanyId = @CompanyId";
            return await _dbConnection.QueryFirstOrDefaultAsync<string>(query, new { CompanyId = companyId });
        }

        // Get full details of a specific company by CompanyId
        public async Task<CustomerDetails> GetCompanyDetailsAsync(int companyId)
        {
            var query = "SELECT * FROM Customer_Details WHERE CompanyId = @CompanyId";
            return await _dbConnection.QueryFirstOrDefaultAsync<CustomerDetails>(query, new { CompanyId = companyId });
        }
    }

    // Define a model to return both customer and associated companies
    public class CustomerWithCompanies
    {
        public CustomerHeader Customer { get; set; }
        public string Status { get; set; }
        public List<CustomerDetails> Companies { get; set; } = new List<CustomerDetails>();
    }
}
