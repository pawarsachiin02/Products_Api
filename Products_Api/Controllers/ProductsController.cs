using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Products_Api.Model;


namespace Products_Api.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet("")]
        public async Task<IActionResult> products(string? filter)
        {
            _logger.LogInformation("Products Controller execution Started");
            try
            {
                //Initialize variables
                var minPrice = 0;
                var maxPrice = 0;
                string size = "";
                string highlight = "";
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
                        size = sizeExists.Split('=')[1];
                    }

                    var highlightExists = filteredQueryString.Where(x => x.Contains("highlight")).FirstOrDefault();
                    if (highlightExists != null)
                    {
                        //Assign highlight value from Query String
                        highlight = highlightExists.Split('=')[1];

                        // Splitting highlight multiple values
                        highlightArray = highlight.Split(',');
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
                    else
                    {
                       //Code for Products data with filters

                        var filteredSizeData = jsonProductDetails.products.Where(x => (
                                                                                        (minPrice != 0 && maxPrice != 0 && x.price >= minPrice && x.price <= maxPrice)
                                                                                         || 
                                                                                         (size != "" && x.sizes.Contains(size)))
                                                                                         ).ToList();
                       // looping for multiple highlight items
                        for (int i = 0; i < highlightArray.Length; i++)
                        {
                            for (int j = 0; j < filteredSizeData.Count(); j++)
                            {
                                //Checking for highlight color exists or not
                                if (filteredSizeData[j].description.Contains(highlightArray[i].ToString()))
                                {
                                    //Assigning <em></em> according to highlight color
                                    filteredSizeData[j].description = filteredSizeData[j].description.Replace("green", "<em>green</em>").Replace("blue", "<em>blue</em>").Replace("red", "<em>red</em>");
                                }
                            }
                        }
                        ProductsDetails filteredData = new ProductsDetails();
                        filteredData.products.AddRange(filteredSizeData);
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
