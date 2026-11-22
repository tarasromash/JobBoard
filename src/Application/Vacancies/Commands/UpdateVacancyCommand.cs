using Application.Common.Interfaces.Repositories;
using Application.Vacancies.Exceptions;
using Domain.Vacancies;
using LanguageExt;
using MediatR;
using Unit = LanguageExt.Unit;

namespace Application.Vacancies.Commands;

public record UpdateVacancyCommand : IRequest<Either<VacancyException, Vacancy>>
{
    public required Guid VacancyId { get; init; }
    public required string Title { get; init; }
    public required string Company { get; init; }
    public required decimal Salary { get; init; }
    public required DateTime PublishedAt { get; init; }
    public required bool IsRemote { get; init; }
    public required VacancyLevel Level { get; init; }
}

public class UpdateVacancyCommandHandler(IVacancyRepository vacancyRepository)
    : IRequestHandler<UpdateVacancyCommand, Either<VacancyException, Vacancy>>
{
    public async Task<Either<VacancyException, Vacancy>> Handle(
        UpdateVacancyCommand request, CancellationToken cancellationToken)
    {
        var vacancyId = new VacancyId(request.VacancyId);
        var vacancy = await vacancyRepository.GetById(vacancyId, cancellationToken);

        return await vacancy.MatchAsync(
            v => CheckDuplicates(v.Id, request.Title, cancellationToken)
                .BindAsync(_ => UpdateEntity(v, request, cancellationToken)),
            () => new VacancyNotFoundException(vacancyId));
    }

    private async Task<Either<VacancyException, Unit>> CheckDuplicates(
        VacancyId currentId, string title, CancellationToken cancellationToken)
    {
        var existing = await vacancyRepository.GetByTitle(title, cancellationToken);

        return existing.Match<Either<VacancyException, Unit>>(
            v => v.Id == currentId ? Unit.Default : new VacancyAlreadyExistsException(v.Id),
            () => Unit.Default);
    }

    private async Task<Either<VacancyException, Vacancy>> UpdateEntity(
        Vacancy vacancy, UpdateVacancyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            vacancy.UpdateDetails(
                request.Title, request.Company, request.Salary,
                request.PublishedAt, request.IsRemote, request.Level);

            return await vacancyRepository.Update(vacancy, cancellationToken);
        }
        catch (Exception exception)
        {
            return new VacancyUnhandledException(vacancy.Id, exception);
        }
    }
}
