namespace VerifyLicenseApi
{
    public class ValidationModel
    {
        public class CustomerHeader
        {
            public long CustomerId { get; set; }
            public string CustomerName { get; set; }
            public string CustomerCountry { get; set; }
            public string CustomerAddress { get; set; }
        }

        public class CustomerDetails
        {
            public long CustomerId { get; set; }
            public int CompanyId { get; set; }
            public string CompanyName { get; set; }
            public string ConnectionstringOnline { get; set; }
            public string ConnectionstringOffline { get; set; }
            public string Comment { get; set; }
            public string LicenseKey { get; set; }
        }

    }
}
