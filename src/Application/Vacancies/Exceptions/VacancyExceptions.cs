using Domain.Vacancies;

namespace Application.Vacancies.Exceptions;

public abstract class VacancyException(VacancyId vacancyId, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public VacancyId VacancyId { get; } = vacancyId;
}

public class VacancyAlreadyExistsException(VacancyId vacancyId)
    : VacancyException(vacancyId, $"Vacancy already exists under id {vacancyId}");

public class VacancyNotFoundException(VacancyId vacancyId)
    : VacancyException(vacancyId, $"Vacancy not found under id {vacancyId}");

public class VacancyUnhandledException(VacancyId vacancyId, Exception? innerException = null)
    : VacancyException(vacancyId, $"Unhandled exception for vacancy {vacancyId}", innerException);
