using Domain.Vacancies;
using Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VacancyConfiguration : IEntityTypeConfiguration<Vacancy>
{
    public void Configure(EntityTypeBuilder<Vacancy> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => new VacancyId(x))
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(x => x.Title).HasColumnType("varchar(255)").HasMaxLength(255).IsRequired();
        builder.HasIndex(x => x.Title).IsUnique();

        builder.Property(x => x.Company).HasColumnType("varchar(255)").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Salary).HasColumnType("numeric(10,2)").IsRequired();

        builder.Property(x => x.PublishedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.IsRemote).HasColumnType("boolean").IsRequired();

        builder.Property(x => x.Level)
            .HasConversion<string>()
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasConversion(new DateTimeUtcConverter())
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
    }
}
