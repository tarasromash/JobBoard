using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Vacancies;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data;

namespace Api.Tests.Integration.Vacancies;

public class VacanciesControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string BaseRoute = "vacancies";

    private readonly Vacancy _firstVacancy;
    private readonly Vacancy _secondVacancy;
    private readonly Vacancy _notStoredVacancy;

    public VacanciesControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        _firstVacancy = VacanciesTestData.FirstTestVacancy();
        _secondVacancy = VacanciesTestData.SecondTestVacancy();
        _notStoredVacancy = VacanciesTestData.ThirdTestVacancy();
    }

    private static string ById(VacancyId id) => $"{BaseRoute}/{id.Value}";

    // ---------- POST /vacancies ----------

    [Fact]
    public async Task ShouldCreateVacancy()
    {
        // Arrange
        var request = new CreateVacancyDto(
            _notStoredVacancy.Title,
            _notStoredVacancy.Company,
            _notStoredVacancy.Salary,
            _notStoredVacancy.PublishedAt,
            _notStoredVacancy.IsRemote,
            _notStoredVacancy.Level);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var dto = await response.Content.ReadFromJsonAsync<VacancyDto>();
        dto.Should().NotBeNull();
        dto!.Id.Should().NotBeEmpty();

        var dbVacancy = await Context.Vacancies.AsNoTracking()
            .FirstAsync(x => x.Id == new VacancyId(dto.Id));
        dbVacancy.Title.Should().Be(request.Title);
        dbVacancy.Company.Should().Be(request.Company);
        dbVacancy.Salary.Should().Be(request.Salary);
        dbVacancy.PublishedAt.Should().Be(request.PublishedAt);
        dbVacancy.IsRemote.Should().Be(request.IsRemote);
        dbVacancy.Level.Should().Be(request.Level);
    }

    [Fact]
    public async Task ShouldNotCreateVacancyBecauseTitleAlreadyExists()
    {
        // Arrange
        var request = new CreateVacancyDto(
            _firstVacancy.Title,
            "Another Company",
            2000.00m,
            new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            false,
            VacancyLevel.Middle);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var withSameTitle = await Context.Vacancies.AsNoTracking()
            .Where(x => x.Title == _firstVacancy.Title)
            .ToListAsync();
        withSameTitle.Should().ContainSingle()
            .Which.Id.Should().Be(_firstVacancy.Id);
    }

    [Theory]
    [InlineData("", 1000)]
    [InlineData("ab", 1000)]
    [InlineData("Valid Vacancy Title", 0)]
    [InlineData("Valid Vacancy Title", 2000000)]
    public async Task ShouldNotCreateVacancyBecauseOfInvalidData(string title, decimal salary)
    {
        // Arrange
        var request = new CreateVacancyDto(
            title,
            "Invalid Data Company",
            salary,
            new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            true,
            VacancyLevel.Junior);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var exists = await Context.Vacancies.AsNoTracking()
            .AnyAsync(x => x.Company == "Invalid Data Company");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldNotCreateVacancyWithTooLongTitle()
    {
        // Arrange
        var request = new CreateVacancyDto(
            new string('a', 256),
            "Too Long Title Company",
            1000.00m,
            new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            true,
            VacancyLevel.Junior);

        // Act
        var response = await Client.PostAsJsonAsync(BaseRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var exists = await Context.Vacancies.AsNoTracking()
            .AnyAsync(x => x.Company == "Too Long Title Company");
        exists.Should().BeFalse();
    }

    // ---------- PUT /vacancies/{id} ----------

    [Fact]
    public async Task ShouldUpdateVacancy()
    {
        // Arrange
        var request = new UpdateVacancyDto(
            "Lead Platform Engineer",
            "Grammarly",
            7500.50m,
            new DateTime(2026, 4, 4, 0, 0, 0, DateTimeKind.Utc),
            !_firstVacancy.IsRemote,
            VacancyLevel.Lead);

        // Act
        var response = await Client.PutAsJsonAsync(ById(_firstVacancy.Id), request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var dto = await response.Content.ReadFromJsonAsync<VacancyDto>();
        dto!.Id.Should().Be(_firstVacancy.Id.Value);

        var dbVacancy = await Context.Vacancies.AsNoTracking().FirstAsync(x => x.Id == _firstVacancy.Id);
        dbVacancy.Title.Should().Be(request.Title);
        dbVacancy.Company.Should().Be(request.Company);
        dbVacancy.Salary.Should().Be(request.Salary);
        dbVacancy.PublishedAt.Should().Be(request.PublishedAt);
        dbVacancy.IsRemote.Should().Be(request.IsRemote);
        dbVacancy.Level.Should().Be(request.Level);
    }

    [Fact]
    public async Task ShouldUpdateVacancyKeepingItsOwnTitle()
    {
        // Arrange
        var request = new UpdateVacancyDto(
            _firstVacancy.Title,
            _firstVacancy.Company,
            _firstVacancy.Salary + 500m,
            _firstVacancy.PublishedAt,
            _firstVacancy.IsRemote,
            _firstVacancy.Level);

        // Act
        var response = await Client.PutAsJsonAsync(ById(_firstVacancy.Id), request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var dbVacancy = await Context.Vacancies.AsNoTracking().FirstAsync(x => x.Id == _firstVacancy.Id);
        dbVacancy.Title.Should().Be(_firstVacancy.Title);
        dbVacancy.Salary.Should().Be(request.Salary);
    }

    [Fact]
    public async Task ShouldNotUpdateVacancyBecauseTitleBelongsToAnotherVacancy()
    {
        // Arrange
        var request = new UpdateVacancyDto(
            _secondVacancy.Title,
            "Changed Company",
            9999.00m,
            _firstVacancy.PublishedAt,
            _firstVacancy.IsRemote,
            _firstVacancy.Level);

        // Act
        var response = await Client.PutAsJsonAsync(ById(_firstVacancy.Id), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await ShouldBeUnchanged(_firstVacancy);
    }

    [Fact]
    public async Task ShouldNotUpdateVacancyBecauseItDoesNotExist()
    {
        // Arrange
        var request = new UpdateVacancyDto(
            "Ghost Vacancy",
            "Nowhere Inc",
            1000.00m,
            new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            true,
            VacancyLevel.Junior);

        // Act
        var response = await Client.PutAsJsonAsync(ById(VacancyId.New()), request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotUpdateVacancyBecauseOfInvalidData()
    {
        // Arrange
        var request = new UpdateVacancyDto(
            "",
            "",
            -10m,
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            true,
            VacancyLevel.Junior);

        // Act
        var response = await Client.PutAsJsonAsync(ById(_firstVacancy.Id), request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await ShouldBeUnchanged(_firstVacancy);
    }

    // ---------- GET /vacancies, GET /vacancies/{id} ----------

    [Fact]
    public async Task ShouldGetAllVacancies()
    {
        // Act
        var response = await Client.GetAsync(BaseRoute);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var dtos = await response.Content.ReadFromJsonAsync<List<VacancyDto>>();
        dtos.Should().HaveCount(2);
        dtos!.Select(x => x.Id).Should()
            .BeEquivalentTo([_firstVacancy.Id.Value, _secondVacancy.Id.Value]);
    }

    [Fact]
    public async Task ShouldGetVacancyById()
    {
        // Act
        var response = await Client.GetAsync(ById(_secondVacancy.Id));

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var dto = await response.Content.ReadFromJsonAsync<VacancyDto>();

        var dbVacancy = await Context.Vacancies.AsNoTracking().FirstAsync(x => x.Id == _secondVacancy.Id);
        dto!.Id.Should().Be(dbVacancy.Id.Value);
        dto.Title.Should().Be(dbVacancy.Title);
        dto.Company.Should().Be(dbVacancy.Company);
        dto.Salary.Should().Be(dbVacancy.Salary);
        dto.PublishedAt.Should().Be(dbVacancy.PublishedAt);
        dto.IsRemote.Should().Be(dbVacancy.IsRemote);
        dto.Level.Should().Be(dbVacancy.Level.ToString());
        dto.CreatedAt.Should().Be(dbVacancy.CreatedAt);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForUnknownVacancy()
    {
        // Act
        var response = await Client.GetAsync(ById(VacancyId.New()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task ShouldBeUnchanged(Vacancy expected)
    {
        var dbVacancy = await Context.Vacancies.AsNoTracking().FirstAsync(x => x.Id == expected.Id);
        dbVacancy.Title.Should().Be(expected.Title);
        dbVacancy.Company.Should().Be(expected.Company);
        dbVacancy.Salary.Should().Be(expected.Salary);
        dbVacancy.PublishedAt.Should().Be(expected.PublishedAt);
        dbVacancy.IsRemote.Should().Be(expected.IsRemote);
        dbVacancy.Level.Should().Be(expected.Level);
    }

    public async Task InitializeAsync()
    {
        await Context.Vacancies.AddRangeAsync(_firstVacancy, _secondVacancy);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    public async Task DisposeAsync()
    {
        Context.Vacancies.RemoveRange(await Context.Vacancies.ToListAsync());
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }
}
