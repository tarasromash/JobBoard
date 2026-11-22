using Domain.Vacancies;

namespace Api.Dtos;

public record VacancyDto(
    Guid Id, string Title, string Company, decimal Salary,
    DateTime PublishedAt, bool IsRemote, string Level, DateTime CreatedAt)
{
    public static VacancyDto FromDomainModel(Vacancy vacancy)
        => new(vacancy.Id.Value, vacancy.Title, vacancy.Company, vacancy.Salary,
            vacancy.PublishedAt, vacancy.IsRemote, vacancy.Level.ToString(), vacancy.CreatedAt);
}

public record CreateVacancyDto(
    string Title, string Company, decimal Salary,
    DateTime PublishedAt, bool IsRemote, VacancyLevel Level);

public record UpdateVacancyDto(
    string Title, string Company, decimal Salary,
    DateTime PublishedAt, bool IsRemote, VacancyLevel Level);

public record SearchVacancyDto
{
    public string? Title { get; init; }
    public string? Company { get; init; }
    public decimal? SalaryFrom { get; init; }
    public decimal? SalaryTo { get; init; }
    public DateTime? PublishedAtFrom { get; init; }
    public DateTime? PublishedAtTo { get; init; }
    public bool? IsRemote { get; init; }
    public string? Level { get; init; }
    public DateTime? CreatedAtFrom { get; init; }
    public DateTime? CreatedAtTo { get; init; }
}
