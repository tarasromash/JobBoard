using Domain.Vacancies;

namespace Tests.Data;

// Values are chosen so that every filter parameter splits the set:
//   title "developer"  -> First, Third        company "soft"   -> First
//   salary >= 3000      -> First, Third        salary <= 1200   -> Second, Fourth
//   published >= 20 May -> Third, Fourth       published <= 10 Mar -> First, Second
//   isRemote = true     -> First, Third        level = Junior   -> Second
public static class VacanciesTestData
{
    public static Vacancy FirstTestVacancy()
        => Vacancy.New(
            VacancyId.New(),
            "Senior .NET Backend Developer",
            "SoftServe",
            5000.00m,
            new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            isRemote: true,
            VacancyLevel.Senior);

    public static Vacancy SecondTestVacancy()
        => Vacancy.New(
            VacancyId.New(),
            "Junior QA Engineer",
            "EPAM",
            1200.00m,
            new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            isRemote: false,
            VacancyLevel.Junior);

    public static Vacancy ThirdTestVacancy()
        => Vacancy.New(
            VacancyId.New(),
            "Middle Frontend Developer",
            "GlobalLogic",
            3000.00m,
            new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc),
            isRemote: true,
            VacancyLevel.Middle);

    public static Vacancy FourthTestVacancy()
        => Vacancy.New(
            VacancyId.New(),
            "Data Analyst Intern",
            "Ajax Systems",
            600.00m,
            new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
            isRemote: false,
            VacancyLevel.Intern);
}
