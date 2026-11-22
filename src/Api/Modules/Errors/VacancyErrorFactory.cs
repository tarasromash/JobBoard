using Application.Vacancies.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class VacancyErrorFactory
{
    public static ObjectResult ToObjectResult(this VacancyException error)
        => new(error.Message)
        {
            StatusCode = error switch
            {
                VacancyAlreadyExistsException => StatusCodes.Status409Conflict,
                VacancyNotFoundException => StatusCodes.Status404NotFound,
                VacancyUnhandledException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("Vacancy error handler is not implemented")
            }
        };
}
