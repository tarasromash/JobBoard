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
