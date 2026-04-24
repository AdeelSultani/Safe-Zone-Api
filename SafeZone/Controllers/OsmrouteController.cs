using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Text.Json;
namespace SafeZone.Controllers
{
    public class OsmrouteController : ApiController
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private async Task<List<object>> GetRouteWaypoints(
            double startLon, double startLat, double endLon, double endLat)
        {
            string url = $"http://router.project-osrm.org/route/v1/driving/" +
                         $"{startLon},{startLat};{endLon},{endLat}" +
                         $"?overview=full&geometries=geojson";

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;

                var routes = root.GetProperty("routes");
                if (routes.GetArrayLength() == 0)
                    throw new Exception("No routes found");

                var geometry = routes[0].GetProperty("geometry");
                var coordinates = geometry.GetProperty("coordinates");

                var waypoints = new List<object>();

                foreach (var coord in coordinates.EnumerateArray())
                {
                    double lon = coord[0].GetDouble();
                    double lat = coord[1].GetDouble();

                    waypoints.Add(new
                    {
                        Latitude = lat,
                        Longitude = lon
                    });
                }

                return waypoints;
            }
        }



        [HttpGet]
      //  [Route("api/osmroute/getmultipleroutes")]
        public async Task<IHttpActionResult> GetMultipleRouteWaypoints(
     double startLon, double startLat, double endLon, double endLat, int alternatives = 0)
        {
            //string url = $"http://router.project-osrm.org/route/v1/driving/" +
            //             $"{startLon},{startLat};{endLon},{endLat}" +
            //             $"?overview=full&geometries=geojson" +
            //             (alternatives > 0 ? $"&alternatives={alternatives}" : "");

            string url = $"http://router.project-osrm.org/route/v1/driving/" +
             $"{startLon},{startLat};{endLon},{endLat}" +
             $"?overview=full&geometries=geojson&alternatives=true";

            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            var routes = root.GetProperty("routes");

            var allRoutesWaypoints = new List<List<object>>();

            foreach (var route in routes.EnumerateArray())
            {
                var geometry = route.GetProperty("geometry");
                var coordinates = geometry.GetProperty("coordinates");

                var waypoints = new List<object>();

                foreach (var coord in coordinates.EnumerateArray())
                {
                    double lon = coord[0].GetDouble();
                    double lat = coord[1].GetDouble();

                    waypoints.Add(new
                    {
                        Latitude = lat,
                        Longitude = lon
                    });
                }

                allRoutesWaypoints.Add(waypoints);
            }

            return Ok(allRoutesWaypoints);
        }
    }
}