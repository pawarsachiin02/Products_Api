using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Products_Api.Controllers;
using Products_Api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Products_Api.Test
{
    public class ProductsControllerTest
    {
        private static readonly IHttpClientFactory _httpClientFactory;
        private static readonly ILogger<ProductsController> _logger;
        [Fact]
        public async Task  ProductsController_Products_ValidResult() 
        {            
            //Arrange
            ProductsController productsController = new ProductsController(_httpClientFactory, _logger);
            string filter = "";
            string json =  System.IO.File.ReadAllText(@"E:\Sachin\DotnetSmartWorking\Products_Api\Products_Api.Test\Data\products.json");

            var jsonData = JsonSerializer.Deserialize<ProductsDetails>(json);           
            var expectedResult = new JsonResult(jsonData);
            
            var actualResult = await productsController.products(filter);
            //Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
