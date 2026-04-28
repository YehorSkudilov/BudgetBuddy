namespace BudgetBuddyAPI;
public static class DateFix
{
    public static DateTime ToUtc(DateTime dt)
        => dt.Kind == DateTimeKind.Utc
            ? dt
            : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    public static DateTime? ToUtc(DateTime? dt)
        => dt.HasValue ? ToUtc(dt.Value) : null;
}