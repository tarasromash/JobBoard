using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Vacancies.Commands;
using Domain.Vacancies;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("vacancies")]
[ApiController]
public class VacanciesController(ISender sender, IVacancyQueries vacancyQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VacancyDto>>> GetAll(CancellationToken cancellationToken)
    {
        var vacancies = await vacancyQueries.GetAll(cancellationToken);
        return vacancies.Select(VacancyDto.FromDomainModel).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VacancyDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var vacancy = await vacancyQueries.GetById(new VacancyId(id), cancellationToken);

        return vacancy.Match<ActionResult<VacancyDto>>(
            v => VacancyDto.FromDomainModel(v),
            () => NotFound());
    }

    [HttpPost("filter")]
    public async Task<ActionResult<IReadOnlyList<VacancyDto>>> Filter(
        [FromBody] SearchVacancyDto request, CancellationToken cancellationToken)
    {
        VacancyLevel? level = null;
        if (!string.IsNullOrWhiteSpace(request.Level))
        {
            if (!Enum.TryParse<VacancyLevel>(request.Level, ignoreCase: true, out var parsed)
                || !Enum.IsDefined(parsed))
            {
                ModelState.AddModelError(nameof(request.Level),
                    $"Unknown level '{request.Level}'. Allowed: {string.Join(", ", Enum.GetNames<VacancyLevel>())}");
                return ValidationProblem(ModelState);
            }

            level = parsed;
        }

        var vacancies = await vacancyQueries.Filter(
            request.Title,
            request.Company,
            request.SalaryFrom,
            request.SalaryTo,
            ToUtc(request.PublishedAtFrom),
            ToUtc(request.PublishedAtTo),
            request.IsRemote,
            level,
            ToUtc(request.CreatedAtFrom),
            ToUtc(request.CreatedAtTo),
            cancellationToken);

        return vacancies.Select(VacancyDto.FromDomainModel).ToList();
    }

    // Filter dates are interpreted as UTC: a value without an offset is treated as UTC as-is.
    private static DateTime? ToUtc(DateTime? value)
        => value switch
        {
            null => null,
            { Kind: DateTimeKind.Unspecified } v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
            { } v => v.ToUniversalTime()
        };

    [HttpPost]
    public async Task<ActionResult<VacancyDto>> Create(
        [FromBody] CreateVacancyDto request, CancellationToken cancellationToken)
    {
        var command = new CreateVacancyCommand
        {
            Title = request.Title,
            Company = request.Company,
            Salary = request.Salary,
            PublishedAt = request.PublishedAt,
            IsRemote = request.IsRemote,
            Level = request.Level
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<VacancyDto>>(
            v => CreatedAtAction(nameof(GetById), new { id = v.Id.Value }, VacancyDto.FromDomainModel(v)),
            e => e.ToObjectResult());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VacancyDto>> Update(
        Guid id, [FromBody] UpdateVacancyDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateVacancyCommand
        {
            VacancyId = id,
            Title = request.Title,
            Company = request.Company,
            Salary = request.Salary,
            PublishedAt = request.PublishedAt,
            IsRemote = request.IsRemote,
            Level = request.Level
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<VacancyDto>>(
            v => VacancyDto.FromDomainModel(v),
            e => e.ToObjectResult());
    }
}
