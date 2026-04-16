namespace BudgetBuddyAPI;

public class PlaidItemResponse
{
    public Item item { get; set; }

    public class Item
    {
        public string institution_id { get; set; }
    }
}