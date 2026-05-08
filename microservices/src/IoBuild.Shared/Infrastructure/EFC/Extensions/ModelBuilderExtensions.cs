using Microsoft.EntityFrameworkCore;
using Humanizer;

namespace IoBuild.Shared.Infrastructure.EFC.Extensions;

public static class ModelBuilderExtensions
{
    public static void UseSnakeCaseNamingConvention(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var clrType = entity.ClrType;
            var entityBuilder = builder.Entity(clrType);

            var tableName = entityBuilder.Metadata.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
                entityBuilder.ToTable(tableName.Pluralize().Underscore());

            foreach (var property in entity.GetProperties())
            {
                var columnName = property.Name.Underscore();
                entityBuilder.Property(property.Name).HasColumnName(columnName);
            }
        }
    }
}
