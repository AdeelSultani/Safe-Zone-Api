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

        // API: api/osmroute/getroute?startLat=..&startLon=..&endLat=..&endLon=..
        [HttpGet]
        [Route("api/osmroute/getroute")]
        public async Task<IHttpActionResult> GetRoute(double startLat, double startLon, double endLat, double endLon)
        {
            try
            {
                var waypoints = await GetRouteWaypoints(startLon, startLat, endLon, endLat);
                return Ok(waypoints);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

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
    }
}