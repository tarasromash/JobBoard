using Domain.Vacancies;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IVacancyQueries
{
    Task<IReadOnlyList<Vacancy>> GetAll(CancellationToken cancellationToken);
    Task<Option<Vacancy>> GetById(VacancyId id, CancellationToken cancellationToken);
}
