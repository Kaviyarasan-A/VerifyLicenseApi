using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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

        // Get Customer Header and all companies by LicenseKey in a single query
        public async Task<CustomerWithCompanies> GetCustomerAndCompaniesByLicenseKeyAsync(string licenseKey)
        {
            // SQL query to fetch customer and associated companies in a single query using JOIN
            var query = @"
                SELECT 
                    ch.CustomerID,
                    ch.CustomerName,
                    ch.Country,
                    ch.CustomerAddress,
                    ch.LicenseKey,
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

            // Execute the query with the provided LicenseKey
            var customerData = await _dbConnection.QueryAsync<CustomerHeader, CustomerDetails, CustomerWithCompanies>(
                query,
                (customer, company) =>
                {
                    // Return the combined customer and associated companies
                    return new CustomerWithCompanies
                    {
                        Customer = customer,
                        Companies = new List<CustomerDetails> { company } // Start with the first company
                    };
                },
                new { LicenseKey = licenseKey },
                splitOn: "CompanyId" // Split based on the CompanyId field in the result set
            );

            // If no customer is found, return null
            if (customerData == null || !customerData.Any())
            {
                return null;
            }

            // Return the first matched customer (since it's a 1-to-many relationship)
            var result = customerData.FirstOrDefault();

            // If the customer has multiple companies, append them
            foreach (var company in customerData.Skip(1))
            {
                result.Companies.Add(company.Companies.First());
            }

            return result;
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
        public List<CustomerDetails> Companies { get; set; } = new List<CustomerDetails>();
    }
}