using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using User.BlazorClient;
using User.BlazorClient.Services;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthState>();

builder.Services.AddHttpClient<UserApiClient>(client =>
{
    var baseUrl = cfg["UserService:BaseUrl"] ?? "http://localhost:8081";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
