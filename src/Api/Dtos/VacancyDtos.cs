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

// Control work 2: filter request DTO.
public record SearchVacancyDto
{
}
