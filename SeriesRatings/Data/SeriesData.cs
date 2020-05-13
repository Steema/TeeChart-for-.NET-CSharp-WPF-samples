using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;

namespace SeriesRatings.Data
{
    public class SeriesData
    {
        private static readonly string[] DefaultSeries =
        {
            "tt0903747", "tt0944947", "tt2802850", "tt0773262",
            "tt1442437", "tt0108778"
        };

        public string Title { get; set; }

        public string ImdbRating
        {
            get { return _imdbRating; }
            set
            {
                _imdbRating = value;
                Rating = Utils.ParseImdbRating(_imdbRating);
            }
        }

        public double Rating { get; private set; } = double.NaN;
        public List<SeasonData> Seasons;

        private string _imdbRating = "";

        public static async Task<SeriesData> GetSeriesData(string seriesId, CancellationToken cancellationToken)
        {
            try
            {
                var data = await GetSeries(seriesId, cancellationToken);

                data.Seasons = new List<SeasonData>();

                var seasonNumber = 1;
                var season = await GetSeason(seriesId, seasonNumber, cancellationToken);

                while (season.Episodes != null && !double.IsNaN(season.Episodes[0].Rating))
                {
                    data.Seasons.Add(season);

                    seasonNumber++;
                    season = await GetSeason(seriesId, seasonNumber, cancellationToken);
                }

                return data;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        public static async Task<List<SearchSeriesData>> Search(string query, CancellationToken cancellationToken)
        {
            var request = new RestRequest("/", Method.GET);
            request.AddParameter("s", query);
            request.AddParameter("type", "series");

            try
            {
                var searchData = await Utils.RequestData<SearchData>(request, cancellationToken);
                return searchData.Search;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        public static async Task<List<SearchSeriesData>> SearchDefaults(CancellationToken cancellationToken)
        {
            try
            {
                var results = new List<SearchSeriesData>();
                var request = new RestRequest("/", Method.GET);
                var idParameter = new Parameter("i", null, ParameterType.GetOrPost);
                request.AddParameter(idParameter);

                foreach (var imdbId in DefaultSeries)
                {
                    idParameter.Value = imdbId;
                    results.Add(await Utils.RequestData<SearchSeriesData>(request, cancellationToken));
                }

                return results;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        private static async Task<SeriesData> GetSeries(string seriesId, CancellationToken cancellationToken)
        {
            var request = new RestRequest("/", Method.GET);
            request.AddParameter("i", seriesId);

            var seriesData = await Utils.RequestData<SeriesData>(request, cancellationToken);
            return seriesData;
        }

        private static async Task<SeasonData> GetSeason(string seriesId, int season, CancellationToken cancellationToken)
        {
            var request = new RestRequest("/", Method.GET);
            request.AddParameter("i", seriesId);
            request.AddParameter("Season", season);

            var seasonData = await Utils.RequestData<SeasonData>(request, cancellationToken);
            return seasonData;
        }
    }

    public class SeasonData
    {
        public List<EpisodeData> Episodes { get; set; }
    }

    public class EpisodeData
    {
        public string Title { get; set; }
        public int Episode { get; set; }

        public string ImdbRating
        {
            get { return _imdbRating; }
            set
            {
                _imdbRating = value;
                Rating = Utils.ParseImdbRating(_imdbRating);
            }
        }

        public double Rating { get; private set; } = double.NaN;

        private string _imdbRating = "";
    }

    public class SearchData
    {
        public List<SearchSeriesData> Search { get; set; }
    }

    public class SearchSeriesData
    {
        public string Title { get; set; }
        public string Year { get; set; }

        [DeserializeAs(Name = "imdbID")]
        public string ImdbId { get; set; }

        public string Poster { get; set; }
    }
}
