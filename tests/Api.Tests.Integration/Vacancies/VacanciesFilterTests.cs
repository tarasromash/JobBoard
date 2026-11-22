using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Vacancies;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data;

namespace Api.Tests.Integration.Vacancies;

public class VacanciesFilterTests : BaseIntegrationTest, IAsyncLifetime
{
    private const string FilterRoute = "vacancies/filter";

    private readonly Vacancy _first;
    private readonly Vacancy _second;
    private readonly Vacancy _third;
    private readonly Vacancy _fourth;
    private readonly Vacancy[] _all;

    // CreatedAt values as stored in PostgreSQL (microsecond precision), ordered ascending.
    private List<Vacancy> _storedByCreatedAt = [];

    public VacanciesFilterTests(IntegrationTestWebFactory factory) : base(factory)
    {
        // CreatedAt is set by the factory method; the short pauses guarantee
        // that the four records get different creation times even after
        // PostgreSQL truncates them to microseconds.
        _first = VacanciesTestData.FirstTestVacancy();
        Thread.Sleep(5);
        _second = VacanciesTestData.SecondTestVacancy();
        Thread.Sleep(5);
        _third = VacanciesTestData.ThirdTestVacancy();
        Thread.Sleep(5);
        _fourth = VacanciesTestData.FourthTestVacancy();

        _all = [_first, _second, _third, _fourth];
    }

    // ---------- one parameter per test ----------

    [Fact]
    public Task ShouldFilterByTitle()
        => ShouldReturn(new SearchVacancyDto { Title = "developer" }, _first, _third);

    [Fact]
    public Task ShouldFilterByCompany()
        => ShouldReturn(new SearchVacancyDto { Company = "soft" }, _first);

    [Fact]
    public Task ShouldFilterBySalaryFrom()
        => ShouldReturn(new SearchVacancyDto { SalaryFrom = 3500m }, _first);

    [Fact]
    public Task ShouldFilterBySalaryTo()
        => ShouldReturn(new SearchVacancyDto { SalaryTo = 1000m }, _fourth);

    [Fact]
    public Task ShouldFilterByPublishedAtFrom()
        => ShouldReturn(
            new SearchVacancyDto { PublishedAtFrom = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc) },
            _third, _fourth);

    [Fact]
    public Task ShouldFilterByPublishedAtTo()
        => ShouldReturn(
            new SearchVacancyDto { PublishedAtTo = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc) },
            _first);

    [Fact]
    public Task ShouldFilterByIsRemote()
        => ShouldReturn(new SearchVacancyDto { IsRemote = false }, _second, _fourth);

    [Fact]
    public Task ShouldFilterByLevel()
        => ShouldReturn(new SearchVacancyDto { Level = nameof(VacancyLevel.Junior) }, _second);

    [Fact]
    public Task ShouldFilterByCreatedAtFrom()
    {
        // From = creation time of the third stored record -> the last two records.
        var from = _storedByCreatedAt[2].CreatedAt;
        return ShouldReturn(
            new SearchVacancyDto { CreatedAtFrom = from },
            _storedByCreatedAt[2], _storedByCreatedAt[3]);
    }

    [Fact]
    public Task ShouldFilterByCreatedAtTo()
    {
        // To = creation time of the first stored record -> only that record.
        var to = _storedByCreatedAt[0].CreatedAt;
        return ShouldReturn(
            new SearchVacancyDto { CreatedAtTo = to },
            _storedByCreatedAt[0]);
    }

    // ---------- additional scenarios ----------

    [Fact]
    public Task ShouldFilterByTitleIgnoringCase()
        => ShouldReturn(new SearchVacancyDto { Title = "DEVELOPER" }, _first, _third);

    [Fact]
    public Task ShouldIncludeSalaryBoundaries()
        => ShouldReturn(new SearchVacancyDto { SalaryFrom = 1200m, SalaryTo = 3000m }, _second, _third);

    [Fact]
    public Task ShouldIncludePublishedAtBoundaries()
        => ShouldReturn(
            new SearchVacancyDto
            {
                PublishedAtFrom = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
                PublishedAtTo = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            _second, _third);

    [Fact]
    public Task ShouldFilterByRangeWithBothBounds()
        => ShouldReturn(new SearchVacancyDto { SalaryFrom = 1000m, SalaryTo = 4000m }, _second, _third);

    [Fact]
    public Task ShouldIncludeCreatedAtBoundaries()
        => ShouldReturn(
            new SearchVacancyDto
            {
                CreatedAtFrom = _storedByCreatedAt[1].CreatedAt,
                CreatedAtTo = _storedByCreatedAt[2].CreatedAt
            },
            _storedByCreatedAt[1], _storedByCreatedAt[2]);

    [Fact]
    public Task ShouldCombineParametersWithAnd()
        => ShouldReturn(
            new SearchVacancyDto { Title = "developer", IsRemote = true, Level = "Middle" },
            _third);

    [Fact]
    public async Task ShouldReturnAllVacanciesWithoutParameters()
    {
        // Act
        var response = await Client.PostAsJsonAsync(FilterRoute, new SearchVacancyDto());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dtos = await response.Content.ReadFromJsonAsync<List<VacancyDto>>();
        dtos!.Select(x => x.Id).Should().BeEquivalentTo(_all.Select(x => x.Id.Value));
    }

    [Fact]
    public async Task ShouldReturnEmptyListWhenNothingMatches()
    {
        // Act
        var response = await Client.PostAsJsonAsync(
            FilterRoute, new SearchVacancyDto { Title = "developer", Level = "Intern" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dtos = await response.Content.ReadFromJsonAsync<List<VacancyDto>>();
        dtos.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task ShouldRejectUnknownLevel()
    {
        // Act
        var response = await Client.PostAsJsonAsync(FilterRoute, new SearchVacancyDto { Level = "Wizard" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task ShouldReturn(SearchVacancyDto filter, params Vacancy[] expected)
    {
        // A filter test is meaningful only if it keeps some records and drops others.
        expected.Should().NotBeEmpty();
        expected.Length.Should().BeLessThan(_all.Length);

        // Act
        var response = await Client.PostAsJsonAsync(FilterRoute, filter);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dtos = await response.Content.ReadFromJsonAsync<List<VacancyDto>>();
        dtos!.Select(x => x.Id).Should().BeEquivalentTo(expected.Select(x => x.Id.Value));
    }

    public async Task InitializeAsync()
    {
        await Context.Vacancies.AddRangeAsync(_all);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        _storedByCreatedAt = await Context.Vacancies.AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task DisposeAsync()
    {
        Context.Vacancies.RemoveRange(await Context.Vacancies.ToListAsync());
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }
}
