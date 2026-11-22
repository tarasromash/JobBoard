using FluentValidation;

namespace Application.Vacancies.Commands;

public class CreateVacancyCommandValidator : AbstractValidator<CreateVacancyCommand>
{
    public CreateVacancyCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(3).MaximumLength(255);
        RuleFor(x => x.Company).NotEmpty().MinimumLength(2).MaximumLength(255);
        RuleFor(x => x.Salary).InclusiveBetween(1m, 1_000_000m);
        RuleFor(x => x.PublishedAt)
            .GreaterThanOrEqualTo(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            .Must(d => d <= DateTime.UtcNow.AddYears(1))
            .WithMessage("PublishedAt cannot be more than one year in the future.");
        RuleFor(x => x.Level).IsInEnum();
    }
}
