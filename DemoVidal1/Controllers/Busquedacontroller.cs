using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DemoVidal1.Modelo;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;



namespace DemoVidal1.Controllers
{
    /// <summary>
    /// Controlador que realiza búsquedas en diferentes APIs y devuelve los resultados ordenados.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BuscarInformacionController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;

        /// <summary>
        /// Constructor que recibe una fábrica de clientes HTTP.
        /// </summary>
        /// <param name="clientFactory">Fábrica de clientes HTTP.</param>
        public BuscarInformacionController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        /// <summary>
        /// Realiza la búsqueda en diferentes APIs y devuelve los resultados ordenados por nombre.
        /// </summary>
        /// <param name="Busquedaparametro">El término de búsqueda a consultar en las APIs.</param>
        /// <returns>Una lista de resultados de la búsqueda ordenada por nombre.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search([FromQuery] string Busquedaparametro)
        {
            var results = new List<ModeloBusqueda>();

            // Lógica para buscar en cada API
            results.AddRange(await BusquedaItunes(Busquedaparametro));
            results.AddRange(await BusquedaTVMaze(Busquedaparametro));
            results.AddRange(await BusquedaPersonas(Busquedaparametro));

            // Ordenar resultados por título
            var orderedResults = results.OrderBy(r => r.Name).ToList();

            return Ok(orderedResults);
        }
        /// <summary>
        /// Realiza una búsqueda en la API de iTunes y devuelve los resultados.
        /// </summary>
        /// <param name="Busquedaparametro">El término de búsqueda a consultar en iTunes.</param>
        /// <returns>Una lista de resultados de la búsqueda en iTunes.</returns>
        private async Task<IEnumerable<ModeloBusqueda>> BusquedaItunes(string Busquedaparametro)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"https://itunes.apple.com/search?term={Busquedaparametro}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var itunesResponse = JsonConvert.DeserializeObject<ItunesSearchResponse>(content);

                // Mapeo de datos del JSON a tu modelo
                var resultados = itunesResponse.Results.Select(r => new ModeloBusqueda
                {
                    ID = r.artistId.ToString(),
                    Name = r.artistName,
                    DATE = r.releaseDate.ToString(),
                    Description = r.collectionName,
                    TypeSearch = "Itunes"
                });

                return resultados;
            }
            else
            {
                return Enumerable.Empty<ModeloBusqueda>();
            }
        }
        /// <summary>
        /// Realiza una búsqueda en la API de TVMaze y devuelve los resultados.
        /// </summary>
        /// <param name="Busquedaparametro">El término de búsqueda a consultar en TVMaze.</param>
        /// <returns>Una lista de resultados de la búsqueda en TVMaze.</returns>
        private async Task<IEnumerable<ModeloBusqueda>> BusquedaTVMaze(string Busquedaparametro)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"http://api.tvmaze.com/search/shows?q={Busquedaparametro}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var tvMazeResults = JsonConvert.DeserializeObject<List<TVMazeResponse>>(content);

                // Mapeo de datos del JSON a tu modelo
                var resultados = tvMazeResults.Select(r => new ModeloBusqueda
                {
                    ID = r.Show.id.ToString(),
                    Name = r.Show.name,
                    DATE = r.Show.premiered,
                    Description = r.Show.summary,
                    TypeSearch = "TVMaze"
                });

                return resultados;
            }
            else
            {
                return Enumerable.Empty<ModeloBusqueda>();
            }
        }

        /// <summary>
        /// Realiza una búsqueda en la API de Personas y devuelve los resultados.
        /// </summary>
        /// <param name="Busquedaparametro">El término de búsqueda a consultar en Personas.</param>
        /// <returns>Una lista de resultados de la búsqueda en Personas.</returns>
        private async Task<IEnumerable<ModeloBusqueda>> BusquedaPersonas(string Busquedaparametro)
        {
            var client = _clientFactory.CreateClient();
            var response = await client.GetAsync($"https://www.crcind.com/csp/samples/SOAP.Demo.cls?soap_method=QueryByName&name={Busquedaparametro}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var modeloBusquedas = new List<ModeloBusqueda>();

                // Cargar el XML en XDocument
                var xmlDoc = XDocument.Parse(content);

                // Encontrar los elementos QueryByName en el XML
                var queryResults = xmlDoc.Descendants(XName.Get("QueryByName", "http://tempuri.org/QueryByName_DataSet"));

                foreach (var result in queryResults)
                {
                    var id = result.Element(XName.Get("ID", "http://tempuri.org/QueryByName_DataSet"))?.Value;
                    var name = result.Element(XName.Get("Name", "http://tempuri.org/QueryByName_DataSet"))?.Value;
                    var dob = result.Element(XName.Get("DOB", "http://tempuri.org/QueryByName_DataSet"))?.Value;
                    var ssn = result.Element(XName.Get("SSN", "http://tempuri.org/QueryByName_DataSet"))?.Value;

                    if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
                    {
                        modeloBusquedas.Add(new ModeloBusqueda
                        {
                            ID = $" {id}",
                            Name = $"{name}",
                            DATE = $" {dob}",
                            Description = $" {ssn}",
                            TypeSearch = "Persona"
    });
                    }
                }

                return modeloBusquedas;
            }
            else
            {
                return Enumerable.Empty<ModeloBusqueda>();
            }
        }

    }
}

