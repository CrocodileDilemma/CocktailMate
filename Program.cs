using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using UltimateCocktails;
using UltimateCocktails.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://www.thecocktaildb.com/api/json/v1/1/") });
builder.Services.AddMudServices();
builder.Services.AddScoped<CocktailService>();

await builder.Build().RunAsync();
