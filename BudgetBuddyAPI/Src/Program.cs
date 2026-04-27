using BudgetBuddyAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppServices(builder);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();

app.Run();