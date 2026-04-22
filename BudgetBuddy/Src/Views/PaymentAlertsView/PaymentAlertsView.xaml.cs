using System.Collections.ObjectModel;

namespace BudgetBuddy;

public partial class PaymentAlertsView : ContentView
{
    public ObservableCollection<PaymentAlert> Alerts { get; set; }

    public PaymentAlertsView()
    {
        InitializeComponent();

        Alerts = new ObservableCollection<PaymentAlert>
        {
            new PaymentAlert
            {
                Icon = "💳",
                Title = "Visa Payment Due",
                Description = "Due in 3 days",
                Amount = "$120.00"
            },
            new PaymentAlert
            {
                Icon = "⚠️",
                Title = "Low Balance Warning",
                Description = "RBC Chequing below $100",
                Amount = ""
            },
            new PaymentAlert
            {
                Icon = "🏠",
                Title = "Rent Scheduled",
                Description = "Auto-payment tomorrow",
                Amount = "$1200.00"
            },
            new PaymentAlert
            {
                Icon = "🔔",
                Title = "Subscription Renewal",
                Description = "Spotify renews today",
                Amount = "$9.99"
            }
        };

        BindingContext = this;
    }
}