namespace BudgetBuddyAPI;

public class LinkTokenRequest
{
    public object user { get; set; }
    public string client_name { get; set; }
    public List<string> products { get; set; }
    public List<string> country_codes { get; set; }
    public string language { get; set; }
}