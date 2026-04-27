namespace BudgetBuddy;

public static class ApiCommunicators
{
    //public const string BaseUrl = "http://10.0.2.2:5107";
    //public const string BaseUrl = "http://localhost:5107";
    public const string BaseUrl = "http://192.168.1.68:5107";



    public static Plaid Plaid { get; private set; } = new Plaid();

    public static Finance Finance { get; private set; } = new Finance();

}