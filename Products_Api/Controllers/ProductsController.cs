using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Products_Api.Model;
using Products_Api.Global;
using Microsoft.AspNetCore.Authorization;


namespace Products_Api.Controllers
{
    [Route("api/")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IHttpClientFactory httpClientFactory, ILogger<ProductsController> logger)
        {
            this._httpClientFactory = httpClientFactory;
            this._logger = logger;
        }

        [HttpGet("products")]
        [Authorize]

        public async Task<IActionResult> products(string? filter)
        {
            _logger.LogInformation("Products Controller execution Started");
            try
            {
                //Initialize variables
                int minPrice = 0;
                int maxPrice = 0;
                string[] sizeArray = [];
                string[] highlightArray = [];

                ProductsDetails jsonProductDetails = null;
                string[] filteredQueryString = [];
                if (filter != null)
                {
                    //Split Query Strings in Url
                    filteredQueryString = filter.Split("&");
                }

                if (filteredQueryString.Length > 0)
                {
                    _logger.LogInformation("Products Controller filtering Query String execution Started");

                    var minPriceExists = filteredQueryString.Where(x => x.Contains("minprice")).FirstOrDefault();
                    if (minPriceExists != null)
                    {
                        //Assign minPrice value from Query String
                        minPrice = Convert.ToInt32(minPriceExists.Split('=')[1]);
                    }

                    var maxPriceExists = filteredQueryString.Where(x => x.Contains("maxprice")).FirstOrDefault();
                    if (maxPriceExists != null)
                    {
                        //Assign maxPrice value from Query String
                        maxPrice = Convert.ToInt32(maxPriceExists.Split('=')[1]);
                    }

                    var sizeExists = filteredQueryString.Where(x => x.Contains("size")).FirstOrDefault();
                    if (sizeExists != null)
                    {
                        //Assign size value from Query String
                        sizeArray = sizeExists.Split('=')[1].Split(",");
                    }

                    var highlightExists = filteredQueryString.Where(x => x.Contains("highlight")).FirstOrDefault();
                    if (highlightExists != null)
                    {
                        // Splitting highlight multiple values
                        highlightArray = highlightExists.Split('=')[1].Split(',');
                    }
                    _logger.LogInformation("Products Controller filtering Query String execution Ended");

                }

                // Creating Client for HttpClient
                var httpClient = _httpClientFactory.CreateClient("products");
                _logger.LogInformation("Products Controller httpClient created");

               // Getting request from Products API
                var response = await httpClient.GetAsync("");


                if (response != null)
                {
                    _logger.LogInformation("Products Controller httpClient response created");

                    string responseBody = await response.Content.ReadAsStringAsync();
                    //Deserializing Products Data from Products API
                    jsonProductDetails = JsonSerializer.Deserialize<ProductsDetails>(responseBody);
                    if (filteredQueryString.Length == 0)
                    {
                        _logger.LogInformation("Products Controller Non Filter Json Result created");
                        //Returning Products data without filters
                        return new JsonResult(jsonProductDetails);
                    }
                    else if ((minPrice != 0 && maxPrice != 0 && sizeArray.Length == 0) || (minPrice == 0 && maxPrice == 0 && sizeArray.Length > 0))
                    {
                        //Code for Products data with filters

                         var filteredProducts = jsonProductDetails.products.Where(x => (
                                                                                        (minPrice != 0 && maxPrice != 0 && x.price >= minPrice && x.price <= maxPrice)
                                                                                         ||
                                                                                         (sizeArray.Length > 0 && x.sizes.Length > 0 && x.sizes.All(s => sizeArray.Contains(s))))
                                                                                         ).ToList();

                        List<Product> products = new List<Product>();
                        products = Converter.highLighter(filteredProducts, highlightArray);

                        ProductsDetails filteredData = new ProductsDetails();
                        filteredData.products.AddRange(products);
                        _logger.LogInformation("Products Controller Filter Json Result created");

                        //Returning Products data with filters
                        return new JsonResult(filteredData);
                    }
                    else if (minPrice != 0 && maxPrice != 0 && sizeArray.Length > 0)
                    {
                        //Code for Products data with filters

                        var filteredProducts = jsonProductDetails.products.Where(x => (
                                                                                        (minPrice != 0 && maxPrice != 0 && x.price >= minPrice && x.price <= maxPrice)
                                                                                         &&
                                                                                         (sizeArray.Length > 0 && x.sizes.Length > 0 &&  x.sizes.All(s=> sizeArray.Contains(s))))
                                                                                         ).ToList();
                        List<Product> products = new List<Product>();
                        products = Converter.highLighter(filteredProducts, highlightArray);

                        ProductsDetails filteredData = new ProductsDetails();
                        filteredData.products.AddRange(products);
                        _logger.LogInformation("Products Controller Filter Json Result created");

                        //Returning Products data with filters
                        return new JsonResult(filteredData);
                    }                                    
                }
             
                _logger.LogInformation("Products Controller Bad Request created");

            }
            catch (Exception ex)
            {
                _logger.LogError("Products Controller Error Created :" + ex.Message);
                return BadRequest();
            }
            return new JsonResult("Unable to return result");
        }
    }
}
