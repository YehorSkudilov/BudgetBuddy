namespace BudgetBuddy;

public partial class TopBar : ContentView
{
    public static readonly BindableProperty PageNameProperty =
    BindableProperty.Create(
        nameof(PageName),
        typeof(string),
        typeof(TopBar),
        default(string));

    public string PageName
    {
        get => (string)GetValue(PageNameProperty);
        set => SetValue(PageNameProperty, value);
    }

    public TopBar()
	{
		InitializeComponent();
		MainGrid.BindingContext = this;
	}
}