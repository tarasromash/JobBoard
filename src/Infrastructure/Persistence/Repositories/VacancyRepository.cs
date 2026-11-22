using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Vacancies;
using LanguageExt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class VacancyRepository(ApplicationDbContext context) : IVacancyRepository, IVacancyQueries
{
    public async Task<IReadOnlyList<Vacancy>> GetAll(CancellationToken cancellationToken)
    {
        return await context.Vacancies.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vacancy>> Filter(
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
        CancellationToken cancellationToken)
    {
        // Each condition is appended only when its parameter was provided; the whole
        // query is translated to one SQL statement and executed by ToListAsync.
        IQueryable<Vacancy> query = context.Vacancies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(title))
        {
            var pattern = title.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(pattern));
        }

        if (!string.IsNullOrWhiteSpace(company))
        {
            var pattern = company.Trim().ToLower();
            query = query.Where(x => x.Company.ToLower().Contains(pattern));
        }

        if (salaryFrom is not null)
        {
            query = query.Where(x => x.Salary >= salaryFrom.Value);
        }

        if (salaryTo is not null)
        {
            query = query.Where(x => x.Salary <= salaryTo.Value);
        }

        if (publishedAtFrom is not null)
        {
            var from = ToUtc(publishedAtFrom.Value);
            query = query.Where(x => x.PublishedAt >= from);
        }

        if (publishedAtTo is not null)
        {
            var to = ToUtc(publishedAtTo.Value);
            query = query.Where(x => x.PublishedAt <= to);
        }

        if (isRemote is not null)
        {
            query = query.Where(x => x.IsRemote == isRemote.Value);
        }

        if (level is not null)
        {
            query = query.Where(x => x.Level == level.Value);
        }

        if (createdAtFrom is not null)
        {
            var from = ToUtc(createdAtFrom.Value);
            query = query.Where(x => x.CreatedAt >= from);
        }

        if (createdAtTo is not null)
        {
            var to = ToUtc(createdAtTo.Value);
            query = query.Where(x => x.CreatedAt <= to);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private static DateTime ToUtc(DateTime value)
        => value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();

    public async Task<Option<Vacancy>> GetById(VacancyId id, CancellationToken cancellationToken)
    {
        var entity = await context.Vacancies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity ?? Option<Vacancy>.None;
    }

    public async Task<Option<Vacancy>> GetByTitle(string title, CancellationToken cancellationToken)
    {
        var entity = await context.Vacancies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Title == title, cancellationToken);
        return entity ?? Option<Vacancy>.None;
    }

    public async Task<Vacancy> Add(Vacancy vacancy, CancellationToken cancellationToken)
    {
        await context.Vacancies.AddAsync(vacancy, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return vacancy;
    }

    public async Task<Vacancy> Update(Vacancy vacancy, CancellationToken cancellationToken)
    {
        context.Vacancies.Update(vacancy);
        await context.SaveChangesAsync(cancellationToken);
        return vacancy;
    }
}
