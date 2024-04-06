namespace MauiNotifications.Model
{

    public class ReverseGeolocationResult
    {
        public Summary summary { get; set; }
        public ReverseAddress[] addresses { get; set; }
    }

    public class Summary
    {
        public int queryTime { get; set; }
        public int numResults { get; set; }
    }

    public class ReverseAddress
    {
        public Address1 address { get; set; }
        public string position { get; set; }
        public Datasources dataSources { get; set; }
        public string entityType { get; set; }
    }

    public class Address1
    {
        public object[] routeNumbers { get; set; }
        public string countryCode { get; set; }
        public string countrySubdivision { get; set; }
        public string municipality { get; set; }
        public string country { get; set; }
        public string countryCodeISO3 { get; set; }
        public string freeformAddress { get; set; }
        public Boundingbox boundingBox { get; set; }
    }

}
