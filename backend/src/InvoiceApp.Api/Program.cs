using System.Text.Json.Serialization;
using InvoiceApp.Api;
using InvoiceApp.Api.Auth;
using InvoiceApp.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.Configure<DemoUsersOptions>(builder.Configuration);
builder.Services.AddSingleton<SessionStore>();
builder.Services.AddSingleton<IInvoiceRepository, InMemoryInvoiceRepository>();
builder.Services.AddScoped<ApprovalService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseMiddleware<SessionAuthMiddleware>();
app.MapControllers();

app.Run();
