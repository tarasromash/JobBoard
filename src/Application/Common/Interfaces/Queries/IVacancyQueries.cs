using Domain.Vacancies;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IVacancyQueries
{
    Task<IReadOnlyList<Vacancy>> GetAll(CancellationToken cancellationToken);
    Task<Option<Vacancy>> GetById(VacancyId id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Vacancy>> Filter(
        string? title,
        string? company,
        decimal? salaryFrom,
        decimal? salaryTo,
        DateTime? publishedAtFrom,
        DateTime? publishedAtTo,
        bool? isRemote,
        VacancyLevel? level,
        DateTime? createdAtFrom,
        DateTime? createdAtTo,
        CancellationToken cancellationToken);
}
