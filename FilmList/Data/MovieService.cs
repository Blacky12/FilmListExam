using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using System.Text.Json.Serialization;

namespace FilmList.Services
{
    public class MovieService
    {
        private readonly HttpClient _httpClient; // P d envoyer des requettes HTTP à L'API TMDb
        private readonly ILocalStorageService _localStorage; // P stockage local
        private const string FavoriteMoviesKey = "favoriteMovieIds"; // P Clef des favories save nav>>>>>>>>>>>>
        private string apiKey = "8191666f6ae9b84932abee437bed7f18"; //  clé API TMDb 

        // Constructeur: stock HttpClient et LocalStorage pour les utiliser plus tard
        public MovieService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        private string listMoviesEndpoint => $"https://api.themoviedb.org/3/movie/popular?api_key={apiKey}&language=fr-FR";

        private string searchMoviesEndpoint => $"https://api.themoviedb.org/3/search/movie?api_key={apiKey}&language=fr-FR&query=";

        // Récupérer la liste des films populaires
        public virtual async Task<List<MovieData>> GetMovieListAsync()
        {
            // Envoie une requete à TDMb et retourne les resultat des films
            try
            {
                var response = await _httpClient.GetFromJsonAsync<MovieResponse>(listMoviesEndpoint);
                return response?.Results ?? new List<MovieData>();
            }
            // renvoie un message d erreur si sa foire
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des films : {ex.Message}");
                return new List<MovieData>();
            }
        }

        // Rechercher film
        public async Task<List<MovieData>> SearchMoviesAsync(string query)
        {
            // Envoie une requete à TDMb et ajoute le mot clef query et retourne le resultat
            try
            {
                var response = await _httpClient.GetFromJsonAsync<MovieResponse>($"{searchMoviesEndpoint}{Uri.EscapeDataString(query)}");
                return response?.Results ?? new List<MovieData>();
            }
            // "" "" si sa foire
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la recherche de films : {ex.Message}");
                return new List<MovieData>();
            }
        }

        // Ajouter/Supprimer un film des favoris
        public async Task ToggleFavoriteAsync(int movieId)
        {
            
            var favorites = await GetFavoritesAsync(); // Récupère les favoris enregistré
            // Ajoute en favories ou surpimme le film si il est déja en favoris
            if (favorites.Contains(movieId))
            {
                favorites.Remove(movieId);
                Console.WriteLine($"Film {movieId} à été suprimmer");
            }
            else
            {
                favorites.Add(movieId);
                Console.WriteLine($"Favorite add {movieId} ");
            }
            // Met a jour LocalStorage pour les films ajouté
            await _localStorage.SetItemAsync(FavoriteMoviesKey, favorites);
        }

        // Récupérer les films favoris
        public async Task<HashSet<int>> GetFavoritesAsync()
        {
            return await _localStorage.GetItemAsync<HashSet<int>>(FavoriteMoviesKey) ?? new HashSet<int>();
        }

        // Récupérer les détails des films par ID
        public async Task<List<MovieData>> GetMoviesByIdsAsync(IEnumerable<int> movieIds)
        {
            var movies = new List<MovieData>();
            foreach (var id in movieIds)
            {
                var endpoint = $"https://api.themoviedb.org/3/movie/{id}?api_key={apiKey}&language=fr-FR";
                try
                {
                    var movieResponse = await _httpClient.GetFromJsonAsync<MovieData>(endpoint);
                    if (movieResponse != null)
                    {
                        movies.Add(movieResponse);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la récupération du film avec ID {id} : {ex.Message}");
                }
            }
            return movies;
        }
    }

    // Modèle de réponse de l'API TMDb
    public class MovieResponse
    {
        [JsonPropertyName("results")]
        public List<MovieData> Results { get; set; }
    }

    // Modèle de film
    public class MovieData
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; }

        public bool IsFavorite { get; set; }

        public string FullPosterUrl => !string.IsNullOrEmpty(PosterPath)
            ? $"https://image.tmdb.org/t/p/w500{PosterPath}"
            : "https://via.placeholder.com/200x300?text=No+Image";
    }
}
