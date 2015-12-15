using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace SeriesRatings.Data
{
    internal class Utils
    {
        public static async Task<T> RequestData<T>(IRestRequest request, CancellationToken cancellationToken)
            where T : class
        {
            var client = new RestClient("http://www.omdbapi.com");
            var response = await client.ExecuteTaskAsync<T>(request, cancellationToken);
            return response.Data;
        }

        public static double ParseImdbRating(string imdbRating)
        {
            return imdbRating == "N/A"
                ? double.NaN
                : double.Parse(imdbRating, NumberStyles.Any, CultureInfo.InvariantCulture);
        }
    }
}