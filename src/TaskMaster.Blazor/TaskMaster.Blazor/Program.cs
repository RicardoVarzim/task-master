using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskMaster.Blazor.Services;
using TaskMaster.Core.Data;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure Entity Framework Core with SQLite
var dbPath = DatabaseHelper.GetDatabasePath();
var connectionString = $"Data Source={dbPath}";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Register services
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<MetricsService>();
builder.Services.AddScoped<WeeklyReportService>();
builder.Services.AddScoped<HistoryService>();

// Configure SignalR Hub connection
builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.Client.HubConnection>(sp =>
{
    // Connect to the API's SignalR hub
    var hubUrl = new Uri("http://localhost:5000/syncHub");
    return new Microsoft.AspNetCore.SignalR.Client.HubConnectionBuilder()
        .WithUrl(hubUrl)
        .WithAutomaticReconnect()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // In development, show detailed error pages
    app.UseDeveloperExceptionPage();
}

// Only use HTTPS redirection if HTTPS is configured
// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
