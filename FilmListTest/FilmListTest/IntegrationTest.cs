using Xunit;
using Bunit;
using FilmList.Pages;
using FilmList.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.LocalStorage;

public class IntegrationTest : TestContext
{
    [Fact]
    public void HomeComponent_RendersCorrectly()
    {
        var mockLocalStorage = new Mock<ILocalStorageService>();
        var mockMovieService = new Mock<MovieService>(new HttpClient(), mockLocalStorage.Object);

        mockMovieService.Setup(s => s.GetMovieListAsync()).ReturnsAsync(new List<MovieData>
        {
            new MovieData { Id = 1, Title = "Test Movie", Overview = "Test Description", PosterPath = "/test.jpg" }
        });

        Services.AddSingleton(mockMovieService.Object);

        var component = RenderComponent<Home>();
        Assert.Contains("Films Populaires", component.Markup);
        Assert.Contains("Test Movie", component.Markup);
    }
}
