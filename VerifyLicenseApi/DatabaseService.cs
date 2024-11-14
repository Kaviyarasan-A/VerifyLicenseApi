using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static VerifyLicenseApi.ValidationModel;

namespace VerifyLicenseApi
{
    public class DatabaseService
    {
        private readonly IDbConnection _dbConnection;

        public DatabaseService(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Get Customer Header and all companies by CustomerId in a single query
        public async Task<CustomerWithCompanies> GetCustomerAndCompaniesAsync(long customerId)
        {
            // Query to fetch customer details
            var customerQuery = "SELECT * FROM Customer_Header WHERE CustomerId = @CustomerId";

            // Query to fetch associated companies
            var companiesQuery = "SELECT * FROM Customer_Details WHERE CustomerId = @CustomerId";

            using (var multi = await _dbConnection.QueryMultipleAsync(customerQuery + ";" + companiesQuery, new { CustomerId = customerId }))
            {
                // Fetch Customer details
                var customer = await multi.ReadFirstOrDefaultAsync<CustomerHeader>();

                // Fetch associated companies
                var companies = await multi.ReadAsync<CustomerDetails>();

                if (customer == null)
                {
                    return null; // If no customer is found
                }

                // Return customer with their companies
                return new CustomerWithCompanies
                {
                    Customer = customer,
                    Companies = companies
                };
            }
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
        public IEnumerable<CustomerDetails> Companies { get; set; }
    }
}
