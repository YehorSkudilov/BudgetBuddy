namespace BudgetBuddyAPI;

public class PlaidInstitutionResponse
{
    public Institution institution { get; set; }

    public class Institution
    {
        public string name { get; set; }
    }
}