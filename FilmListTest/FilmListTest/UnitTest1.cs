using Xunit;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using FilmList.Services;

public class UnitTest1
{
    private readonly MovieService _movieService;
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;

    public UnitTest1()
    {
        _mockLocalStorage = new Mock<ILocalStorageService>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"results\":[]}")
            });

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _movieService = new MovieService(_httpClient, _mockLocalStorage.Object);
    }

    [Fact]
    public async Task Test_AddMovieToFavorites()
    {
        // Arrange
        int movieId = 1;
        var favorites = new HashSet<int> { movieId };

        _mockLocalStorage.Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<HashSet<int>>(), default))
                 .Returns(new ValueTask());


        _mockLocalStorage.Setup(x => x.GetItemAsync<HashSet<int>>(It.IsAny<string>(), default))
                         .ReturnsAsync(favorites);

        // Act
        await _movieService.ToggleFavoriteAsync(movieId);

        // Assert
        _mockLocalStorage.Verify(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<HashSet<int>>(), default), Times.Once);
    }

    [Fact]
    public async Task Test_GetMovies_ReturnsEmptyListWhenApiFails()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Erreur de connexion"));

        // Act
        var movies = await _movieService.GetMovieListAsync();

        // Assert
        Assert.Empty(movies);
    }
}
