using Application.Common.Interfaces.Repositories;
using Application.Vacancies.Exceptions;
using Domain.Vacancies;
using LanguageExt;
using MediatR;

namespace Application.Vacancies.Commands;

public record CreateVacancyCommand : IRequest<Either<VacancyException, Vacancy>>
{
    public required string Title { get; init; }
    public required string Company { get; init; }
    public required decimal Salary { get; init; }
    public required DateTime PublishedAt { get; init; }
    public required bool IsRemote { get; init; }
    public required VacancyLevel Level { get; init; }
}

public class CreateVacancyCommandHandler(IVacancyRepository vacancyRepository)
    : IRequestHandler<CreateVacancyCommand, Either<VacancyException, Vacancy>>
{
    public async Task<Either<VacancyException, Vacancy>> Handle(
        CreateVacancyCommand request, CancellationToken cancellationToken)
    {
        var existing = await vacancyRepository.GetByTitle(request.Title, cancellationToken);

        return await existing.MatchAsync(
            v => new VacancyAlreadyExistsException(v.Id),
            () => CreateEntity(request, cancellationToken));
    }

    private async Task<Either<VacancyException, Vacancy>> CreateEntity(
        CreateVacancyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var vacancy = Vacancy.New(
                VacancyId.New(), request.Title, request.Company, request.Salary,
                request.PublishedAt, request.IsRemote, request.Level);

            return await vacancyRepository.Add(vacancy, cancellationToken);
        }
        catch (Exception exception)
        {
            return new VacancyUnhandledException(VacancyId.Empty(), exception);
        }
    }
}
