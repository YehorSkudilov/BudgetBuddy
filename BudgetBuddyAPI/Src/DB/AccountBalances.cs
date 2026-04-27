using Microsoft.EntityFrameworkCore;

[Owned]
public class AccountBalances
{
    public decimal? available { get; set; }
    public decimal current { get; set; }
    public string? iso_currency_code { get; set; }
    public string? unofficial_currency_code { get; set; }
    public decimal? limit { get; set; }
}