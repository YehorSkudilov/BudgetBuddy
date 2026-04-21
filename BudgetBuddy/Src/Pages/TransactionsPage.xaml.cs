using AppSkeleton;
using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class TransactionsPage : ContentView
{
    public ObservableCollection<TransactionGroup> TransactionsGroups { get; set; }

    public static readonly BindableProperty ValuesProperty =
    BindableProperty.Create(
        nameof(Values),
        typeof(ObservableCollection<CChartEntry>),
        typeof(HomePage),
        new ObservableCollection<CChartEntry>());

    public ObservableCollection<CChartEntry> Values
    {
        get => (ObservableCollection<CChartEntry>)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }
    public TransactionsPage()
    {
        InitializeComponent();


        Values = new ObservableCollection<CChartEntry>
        {
            new CChartEntry { Label = "Income", Value = 1200, Color = Colors.Green },
            new CChartEntry { Label = "Spending", Value = 500, Color = Colors.Red },
            new CChartEntry { Label = "Left", Value = 700, Color = Colors.DodgerBlue }
        };

        var transactions = new List<TransactionItem>
        {
          new TransactionItem
{
    Title = "Netflix",
    Subtitle = "Subscription",
    Amount = -16.99m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-9).AddHours(20)
},
new TransactionItem
{
    Title = "Stock Sale",
    Subtitle = "AAPL Profit",
    Amount = 320,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-9).AddHours(11)
},
new TransactionItem
{
    Title = "Electricity",
    Subtitle = "BC Hydro",
    Amount = -92.40m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-9).AddHours(9)
},
new TransactionItem
{
    Title = "Side Job",
    Subtitle = "Logo Design",
    Amount = 150,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-9).AddHours(15)
},
new TransactionItem
{
    Title = "Amazon",
    Subtitle = "Order #1123",
    Amount = -67.89m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-13).AddHours(18)
},
new TransactionItem
{
    Title = "Spotify",
    Subtitle = "Monthly Plan",
    Amount = -11.99m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-14).AddHours(8)
},
new TransactionItem
{
    Title = "Bonus",
    Subtitle = "Performance Bonus",
    Amount = 500,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-15).AddHours(10)
},
new TransactionItem
{
    Title = "Dining",
    Subtitle = "Restaurant",
    Amount = -58.20m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-16).AddHours(19)
},
new TransactionItem
{
    Title = "Transfer",
    Subtitle = "To Savings",
    Amount = -300,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-17).AddHours(14)
},
new TransactionItem
{
    Title = "Savings Interest",
    Subtitle = "Bank Interest",
    Amount = 12.35m,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-18).AddHours(6)
},
new TransactionItem
{
    Title = "Gas",
    Subtitle = "Petrol Station",
    Amount = -75.10m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-19).AddHours(17)
},
new TransactionItem
{
    Title = "Freelance",
    Subtitle = "Website Fix",
    Amount = 220,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-20).AddHours(13)
},
new TransactionItem
{
    Title = "Insurance",
    Subtitle = "Car Insurance",
    Amount = -110,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-21).AddHours(9)
},
new TransactionItem
{
    Title = "Refund",
    Subtitle = "Amazon Refund",
    Amount = 45.99m,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-22).AddHours(16)
},
new TransactionItem
{
    Title = "Movie",
    Subtitle = "Cinema Ticket",
    Amount = -14.50m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-23).AddHours(20)
},
new TransactionItem
{
    Title = "Freelance",
    Subtitle = "App UI Work",
    Amount = 600,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-24).AddHours(11)
},
new TransactionItem
{
    Title = "Groceries",
    Subtitle = "Costco",
    Amount = -180,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-25).AddHours(10)
},
new TransactionItem
{
    Title = "Rent Refund",
    Subtitle = "Overpayment Return",
    Amount = 200,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-26).AddHours(15)
},
new TransactionItem
{
    Title = "Clothing",
    Subtitle = "H&M",
    Amount = -95,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-27).AddHours(12)
},
new TransactionItem
{
    Title = "Uber Eats",
    Subtitle = "Dinner Delivery",
    Amount = -32.40m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-28).AddHours(21)
},
new TransactionItem
{
    Title = "Dividend",
    Subtitle = "ETF Income",
    Amount = 42.10m,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-29).AddHours(8)
},
new TransactionItem
{
    Title = "Phone Bill",
    Subtitle = "Rogers",
    Amount = -85,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-30).AddHours(18)
},
new TransactionItem
{
    Title = "Bonus",
    Subtitle = "Referral Bonus",
    Amount = 75,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-31).AddHours(10)
},
new TransactionItem
{
    Title = "Coffee",
    Subtitle = "Tim Hortons",
    Amount = -4.50m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-32).AddHours(9)
},
new TransactionItem
{
    Title = "Gym",
    Subtitle = "Monthly Membership",
    Amount = -45,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-33).AddHours(7)
},
new TransactionItem
{
    Title = "Freelance",
    Subtitle = "Bug Fixing",
    Amount = 180,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-34).AddHours(14)
},
new TransactionItem
{
    Title = "Amazon",
    Subtitle = "Electronics",
    Amount = -220,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-35).AddHours(16)
},
new TransactionItem
{
    Title = "Gift",
    Subtitle = "Family Support",
    Amount = 300,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-36).AddHours(12)
},
new TransactionItem
{
    Title = "Transport",
    Subtitle = "Monthly Transit Pass",
    Amount = -110,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-37).AddHours(8)
},
new TransactionItem
{
    Title = "Freelance",
    Subtitle = "Landing Page",
    Amount = 450,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-38).AddHours(13)
},
new TransactionItem
{
    Title = "Water Bill",
    Subtitle = "City Utilities",
    Amount = -38.75m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-39).AddHours(10)
},
new TransactionItem
{
    Title = "Stock Dividend",
    Subtitle = "TSX ETF",
    Amount = 28.90m,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-40).AddHours(9)
},
new TransactionItem
{
    Title = "Shopping",
    Subtitle = "Walmart",
    Amount = -76.20m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-41).AddHours(17)
},
new TransactionItem
{
    Title = "Freelance",
    Subtitle = "API Integration",
    Amount = 700,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-42).AddHours(15)
},
new TransactionItem
{
    Title = "Restaurant",
    Subtitle = "Dinner Out",
    Amount = -64.30m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-43).AddHours(19)
},
new TransactionItem
{
    Title = "Bonus",
    Subtitle = "Yearly Bonus",
    Amount = 1200,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-44).AddHours(11)
},
new TransactionItem
{
    Title = "Internet",
    Subtitle = "Monthly Bill",
    Amount = -79.99m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-45).AddHours(8)
},
new TransactionItem
{
    Title = "Coffee",
    Subtitle = "Starbucks",
    Amount = -5.75m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-46).AddHours(10)
},
new TransactionItem
{
    Title = "Freelance",
    Subtitle = "Dashboard Work",
    Amount = 520,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-47).AddHours(14)
},
new TransactionItem
{
    Title = "Insurance Refund",
    Subtitle = "Overpayment",
    Amount = 60,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-48).AddHours(12)
},
new TransactionItem
{
    Title = "Groceries",
    Subtitle = "No Frills",
    Amount = -98.10m,
    IsIncome = false,
    DateTime = DateTime.Now.AddDays(-49).AddHours(18)
},
new TransactionItem
{
    Title = "Dividend",
    Subtitle = "Monthly Yield",
    Amount = 33.40m,
    IsIncome = true,
    DateTime = DateTime.Now.AddDays(-50).AddHours(9)
}
        };

        TransactionsGroups = new ObservableCollection<TransactionGroup>(
            transactions
                .GroupBy(t => t.DateTime.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new TransactionGroup(
                    g.Key,
                    g.OrderByDescending(x => x.DateTime)
                ))
        );

        MainGrid.BindingContext = this;
    }
}