using DataAccessLayer.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using System;
using System.ComponentModel;
using System.Linq;

namespace DataAccessLayer
{
    public class EfCoreContext : DbContext
    {
        private readonly string _connectionString;

        public DbSet<BookingDto> Bookings { get; set; }
        public DbSet<ProductDto> Products { get; set; }

        public EfCoreContext() { }
        public EfCoreContext(DbContextOptions<EfCoreContext> options) : base(options)
        {
            _connectionString = ((SqlServerOptionsExtension)options.Extensions.Last()).ConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // temp solution
            optionsBuilder.UseSqlServer("Server=localhost;Database=BookShopTest;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True");

            //if (!optionsBuilder.IsConfigured)
            //{
            //    optionsBuilder.UseSqlServer(_connectionString);
            //}
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
