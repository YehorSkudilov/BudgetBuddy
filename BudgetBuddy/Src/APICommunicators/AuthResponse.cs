using BudgetBuddy;

public class AuthResponse
{
    public string token { get; set; } = "";
    public User user { get; set; } = new();
}