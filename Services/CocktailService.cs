using ErrorOr;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using UltimateCocktails.Models;

namespace UltimateCocktails.Services
{
    public class CocktailService
    {
        public Drinks CocktailSearch { get; set; } = new();
        public string SearchError { get; set; } = string.Empty;
        private readonly HttpClient _httpClient;

        public CocktailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public event Action OnSearchPerformed;
        public event Action OnShowSpinner;
        public event Action OnHideSpinner;

        public async Task PerformSearch(string searchQuery)
        {
            SearchError = string.Empty;

            try
            {
                CocktailSearch = await _httpClient.GetFromJsonAsync<Drinks>($"search.php?s={ searchQuery }");
                
                if (CocktailSearch?.drinks?.Any() is not true)
                {
                    SearchError = $"No Cocktails were found matching your search term!";
                }
            }
            catch (Exception ex)
            {
                SearchError = ex.Message;
            }

            NotifyStateChanged();
        }

        public async Task PerformFilter(string filterBy, string filterValue)
        {
            SearchError = string.Empty;
            
            try
            {
                CocktailSearch = await _httpClient.GetFromJsonAsync<Drinks>($"filter.php?{ filterBy }={ filterValue }");
                if (CocktailSearch?.drinks?.Any() is not true)
                {
                    SearchError = $"No { filterValue } Cocktails were found!";
                }
            }
            catch (Exception ex)
            {
                SearchError = ex.Message;
            }

            NotifyStateChanged();
        }

        public async Task<ErrorOr<Drinks>> GetRandomDrinks(int count)
        {
            try
            {
                Drinks result = new Drinks();

                for (int i = 0; i < count; i++)
                {
                    var drink = await _httpClient.GetFromJsonAsync<Drinks>("random.php");
                    result.drinks.Add(drink.drinks.FirstOrDefault());
                }

                return result;
            }
            catch (Exception ex)
            {
                return Error.Failure(description: ex.Message);
            }
        }

        public async Task<ErrorOr<List<Drinks>>> GetLists()
        {
            try
            {
                List<Drinks> result = new List<Drinks>();

                foreach (string endpoint in new string[] { "g", "c", "i" })
                {
                    var drink = await _httpClient.GetFromJsonAsync<Drinks>($"list.php?{endpoint}=list");
                    result.Add(drink);
                }

                return result;
            }
            catch (Exception ex)
            {
                return Error.Failure(description: ex.Message);
            }
        }

        private void NotifyStateChanged() => OnSearchPerformed?.Invoke();
    }
}
