namespace Domain.Vacancies;

public class Vacancy
{
    public VacancyId Id { get; }
    public string Title { get; private set; }
    public string Company { get; private set; }
    public decimal Salary { get; private set; }
    public DateTime PublishedAt { get; private set; }
    public bool IsRemote { get; private set; }
    public VacancyLevel Level { get; private set; }
    public DateTime CreatedAt { get; }

    private Vacancy(
        VacancyId id, string title, string company, decimal salary,
        DateTime publishedAt, bool isRemote, VacancyLevel level, DateTime createdAt)
    {
        Id = id;
        Title = title;
        Company = company;
        Salary = salary;
        PublishedAt = publishedAt;
        IsRemote = isRemote;
        Level = level;
        CreatedAt = createdAt;
    }

    public static Vacancy New(
        VacancyId id, string title, string company, decimal salary,
        DateTime publishedAt, bool isRemote, VacancyLevel level)
        => new(id, title, company, salary, publishedAt, isRemote, level, DateTime.UtcNow);

    public void UpdateDetails(
        string title, string company, decimal salary,
        DateTime publishedAt, bool isRemote, VacancyLevel level)
    {
        Title = title;
        Company = company;
        Salary = salary;
        PublishedAt = publishedAt;
        IsRemote = isRemote;
        Level = level;
    }
}
