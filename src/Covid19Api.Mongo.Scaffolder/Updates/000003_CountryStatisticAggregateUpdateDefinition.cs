using System.Threading.Tasks;
using Covid19Api.Domain;
using Covid19Api.Mongo.Scaffolder.Abstractions;
using Covid19Api.Mongo.Scaffolder.Extensions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Covid19Api.Mongo.Scaffolder.Updates
{
    // ReSharper disable once UnusedType.Global
    public class CountryStatisticAggregateUpdateDefinition : DatabaseUpdateDefinition
    {
        private readonly Covid19ApiDbContext databaseContext;

        // ReSharper disable once SuggestBaseTypeForParameter
        public CountryStatisticAggregateUpdateDefinition(ILogger<CountryStatisticAggregateUpdateDefinition> logger,
            Covid19ApiDbContext databaseContext) : base(logger)
        {
            this.databaseContext = databaseContext;
        }

        public override int Version => 3;

        protected override async Task ExecuteAsync()
        {
            await this.databaseContext.Database.CreateCollectionIfNotExistsAsync(CollectionNames
                .CountryStatisticsAggregates);
            
            var collection =
                this.databaseContext.Database.GetCollection<CountryStatisticsAggregate>(CollectionNames
                    .CountryStatisticsAggregates);
            
            var countryIndex = Builders<CountryStatisticsAggregate>
                .IndexKeys
                .Ascending(statistics => statistics.Country);

            var countryIndexModel = new CreateIndexModel<CountryStatisticsAggregate>(countryIndex, new CreateIndexOptions
            {
                Name = $"{CollectionNames.CountryStatisticsAggregates}_country"
            });

            await collection.Indexes.CreateOneAsync(countryIndexModel);

            var monthIndex = Builders<CountryStatisticsAggregate>
                .IndexKeys
                .Descending(statistics => statistics.Month);

            var monthIndexModel = new CreateIndexModel<CountryStatisticsAggregate>(monthIndex, new CreateIndexOptions
            {
                Name = $"{CollectionNames.CountryStatisticsAggregates}_month_descending"
            });
            
            await collection.Indexes.CreateOneAsync(monthIndexModel);

            var yearIndex = Builders<CountryStatisticsAggregate>
                .IndexKeys
                .Descending(statistics => statistics.Year);

            var yearIndexModel = new CreateIndexModel<CountryStatisticsAggregate>(yearIndex, new CreateIndexOptions
            {
                Name = $"{CollectionNames.CountryStatisticsAggregates}_year_descending"
            });
            
            await collection.Indexes.CreateOneAsync(yearIndexModel);

            var yearMonthIndex = Builders<CountryStatisticsAggregate>
                .IndexKeys
                .Combine(yearIndex, monthIndex);

            var yearMonthIndexModel = new CreateIndexModel<CountryStatisticsAggregate>(yearMonthIndex,
                new CreateIndexOptions
                {
                    Name = $"{CollectionNames.CountryStatisticsAggregates}_year_month",
                });
            
            await collection.Indexes.CreateOneAsync(yearMonthIndexModel);
            
            var countryYearMonthIndex = Builders<CountryStatisticsAggregate>
                .IndexKeys
                .Combine(countryIndex, yearIndex, monthIndex);

            var countryYearMonthIndexModel = new CreateIndexModel<CountryStatisticsAggregate>(countryYearMonthIndex,
                new CreateIndexOptions
                {
                    Name = $"{CollectionNames.CountryStatisticsAggregates}_country_year_month",
                    Unique = true
                });

            await collection.Indexes.CreateOneAsync(countryYearMonthIndexModel);
        }
    }
}