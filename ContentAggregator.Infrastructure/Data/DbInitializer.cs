using ContentAggregator.Core.Entities;

namespace ContentAggregator.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(DatabaseContext context)
        {
            context.Database.EnsureCreated();

            // Look for any features.
            if (context.Features.Any())
            {
                return; // DB has been seeded
            }

            var features = new Feature[]
            {
                new Feature{ FirstNameEng="Irakli", LastNameEng="Gogava", FirstNameGeo="ირაკლი", LastNameGeo="გოგავა" },
                new Feature{ FirstNameEng="Soso", LastNameEng="Manjavidze", FirstNameGeo="სოსო", LastNameGeo="მანჯავიძე" }
            };

            foreach (Feature feature in features)
            {
                context.Features.Add(feature);
            }

            context.SaveChanges();


            var ytChannels = new YTChannel[]
            {
                new YTChannel{ Name="Salte", Id="UCIblVXoJdqdkIf694p3R6Wg", Url=new Uri("https://www.youtube.com/@salte1481"),  }
            };

            foreach (YTChannel ytChannel in ytChannels)
            {
                context.YTChannels.Add(ytChannel);
            }

            context.SaveChanges();
        }
    }
}
