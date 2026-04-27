using Microsoft.EntityFrameworkCore;

namespace BudgetBuddyAPI.Src.DB;

[Owned]
public class PersonalFinanceCategory
{
    public string? primary { get; set; }
    public string? detailed { get; set; }
    public string? confidence_level { get; set; }
}