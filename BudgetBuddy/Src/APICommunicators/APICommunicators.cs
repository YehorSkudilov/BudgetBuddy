namespace BudgetBuddy;

public static class ApiCommunicators
{
    //public const string BaseUrl = "http://10.0.2.2:5107";
    //public const string BaseUrl = "http://localhost:5107";
    //public const string BaseUrl = "http://172.21.48.1:5107";
    public const string BaseUrl = "https://budgetbuddyapi.yehorskudilov.com";
    //public const string BaseUrl = "http://192.168.1.68:10101";



    public static Plaid Plaid { get; } = new Plaid();
    public static Finance Finance { get; } = new Finance();
    public static User User { get; } = new User();
}