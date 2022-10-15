using ErrorOr;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Security.Cryptography;
using UltimateCocktails.Models;

namespace UltimateCocktails.Services
{
    public class CocktailService
    {
        public Drinks CocktailSearch { get; set; } = new();
        public Drinks IngredientsList = new();
        public Drinks GlassesList = new();
        public Drinks CategoriesList = new();
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
                var result = await _httpClient.GetFromJsonAsync<Drinks>($"search.php?s={ searchQuery }");
                
                if (result?.drinks?.Any() is not true)
                {
                    SearchError = $"No Cocktails were found matching your search term!";
                }
                else
                {
                    CocktailSearch.drinks = result.drinks.OrderBy(x => x.strDrink).ToList();
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
                var result = await _httpClient.GetFromJsonAsync<Drinks>($"filter.php?{ filterBy }={ filterValue }");
                if (result?.drinks?.Any() is not true)
                {
                    SearchError = $"No { filterValue } Cocktails were found!";
                }
                else
                {
                    CocktailSearch.drinks = result.drinks.OrderBy(x => x.strDrink).ToList();
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

        public List<string> GetRandomIngredients(int count)
        {
            List<string> result = new();
            int max = IngredientsList.drinks.Count;
            if (max > 0)
            {
                int[] ints = new int[count];
                int i = 0;

                while (i < count)
                {
                    int randomInt = RandomNumberGenerator.GetInt32(0, max - 1);
                    if (!ints.Contains(randomInt))
                    {
                        result.Add(IngredientsList.drinks.ElementAt<Drink>(randomInt).strIngredient1);
                        ints[i] = randomInt;
                        i++;
                    }
                }
            }

            return result;
        }
        public async Task <ErrorOr<bool>> LoadLists()
        {
            try
            {
                IngredientsList = await _httpClient.GetFromJsonAsync<Drinks>("list.php?i=list");
                GlassesList = await _httpClient.GetFromJsonAsync<Drinks>("list.php?g=list");
                CategoriesList = await _httpClient.GetFromJsonAsync<Drinks>("list.php?c=list");
                return true;
            }
            catch (Exception ex)
            {
                return Error.Failure(description: ex.Message);
            }
        }

        public async Task<ErrorOr<Drink>> GetDrink(string id)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<Drinks>($"lookup.php?i={ id }");
                if (result?.drinks?.Any() is true)
                {
                    return result.drinks.First();
                }

                return Error.Validation(description: $"Could not load details for Cocktail with ID: {id}");
            }
            catch (Exception ex)
            {
                return Error.Failure(description: ex.Message);
            }
        }

        public async Task<ErrorOr<Ingredient>> GetIngredient(string ingredient)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<Ingredients>($"search.php?i={ingredient}");
                if (result?.ingredients?.Any() is true)
                {
                    return result.ingredients.First();
                }

                return Error.Validation(description: $"Could not load details for Ingredient: {ingredient}");
            }
            catch (Exception ex)
            {
                return Error.Failure(description: ex.Message);
            }
        }

        public string GetIngredientImage(string ingredient)
        {
            return $"https://www.thecocktaildb.com/images/ingredients/{ingredient}-Medium.png";
        }

        private void NotifyStateChanged() => OnSearchPerformed?.Invoke();
    }
}
