using Domain.Vacancies;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IVacancyRepository
{
    Task<Option<Vacancy>> GetById(VacancyId id, CancellationToken cancellationToken);
    Task<Option<Vacancy>> GetByTitle(string title, CancellationToken cancellationToken);
    Task<Vacancy> Add(Vacancy vacancy, CancellationToken cancellationToken);
    Task<Vacancy> Update(Vacancy vacancy, CancellationToken cancellationToken);
}
