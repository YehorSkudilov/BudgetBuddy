namespace BudgetBuddy;

public partial class TopBar : ContentView
{
    // -------------------------
    // PAGE NAME
    // -------------------------
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

    // -------------------------
    // SETTINGS ACTION
    // -------------------------
    public static readonly BindableProperty SettingsActionProperty =
        BindableProperty.Create(
            nameof(SettingsAction),
            typeof(Action),
            typeof(TopBar),
            default(Action));

    public Action SettingsAction
    {
        get => (Action)GetValue(SettingsActionProperty);
        set => SetValue(SettingsActionProperty, value);
    }

    // -------------------------
    // NOTIFICATIONS ACTION
    // -------------------------
    public static readonly BindableProperty NotificationsActionProperty =
        BindableProperty.Create(
            nameof(NotificationsAction),
            typeof(Action),
            typeof(TopBar),
            default(Action));

    public Action NotificationsAction
    {
        get => (Action)GetValue(NotificationsActionProperty);
        set => SetValue(NotificationsActionProperty, value);
    }

    public void Deselect()
    {
        NotificationsButton.Select(false);
        SettingsButton.Select(false);
    }

    public TopBar()
    {
        InitializeComponent();
        MainGrid.BindingContext = this;
    }
}