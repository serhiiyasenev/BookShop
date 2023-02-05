using DataAccessLayer.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DataAccessLayer
{
    public class EfCoreContext : DbContext
    {
        public DbSet<BookingDto> Bookings { get; set; }
        public DbSet<ProductDto> Products { get; set; }

        public EfCoreContext() { }
        public EfCoreContext(DbContextOptions<EfCoreContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // temp solution
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.Development.json").Build();
                var connectionString = config.GetSection("ConnectionStrings").GetValue<string>("Default");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            builder.Properties<DateOnly>()
                .HaveConversion<DateOnlyConverter>()
                .HaveColumnType("date");
        }

        /// <summary>
        /// Converts <see cref="DateOnly" /> to <see cref="DateTime"/> and vice versa.
        /// </summary>
        public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
        {
            /// <summary>
            /// Creates a new instance of this converter.
            /// </summary>
            public DateOnlyConverter() : base(
                    d => d.ToDateTime(TimeOnly.MinValue),
                    d => DateOnly.FromDateTime(d))
            { }
        }
    }
}
