using FlaPo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Helpers;
using System.Web.Http;

namespace FlaPo.Controllers
{
    public class ProductController : ApiController
    {
        public const double FIXPRICE = 17.99;
        List<Product> products;

        #region API - Endpoints
        /// <summary>
        /// Analyzes the given URL for most expensive and cheapest product(s)
        /// </summary>
        /// <returns>Most expensive and cheapest Product</returns>
        [Route("~/api/products/minMaxPricePerLitre/{url?}")]
        public List<Product> GetProductsWithMinMaxPricePerLitre(string url)
        {
            //Check if given data is already read
            if (products == null) {
                DeserializeJsonContent(url);
            }

            List<Product> resultProductsMaxUnitPrice = new List<Product>();
            List<Product> resultProductsMinUnitPrice = new List<Product>();

            if (products?.Count != 0)
            {
                //Declare a first product to start and compare with
                Product startProduct = products.First();
                Article startArticleMax = startProduct.Articles.First();

                Article startArticleMin = startProduct.Articles.First();

                ///Iterate through articles in products
                foreach (Product p in products)
                {
                    foreach (Article a in p.Articles)
                    {
                        ///Case for articles with higher price as the current most expensive one
                        if (GetPricePerUnitByText(a.PricePerUnitText) > GetPricePerUnitByText(startArticleMax.PricePerUnitText))
                        {

                            //Rebuild product to get rid of other articles
                            Product resultProduct = new Product
                            {
                                ID = p.ID,
                                BrandName = p.BrandName,
                                DescriptionText = p.DescriptionText,
                                Name = p.Name,
                                Articles = new List<Article>()

                            };

                            resultProduct.Articles.Add(a);

                            resultProductsMaxUnitPrice.Clear();
                            resultProductsMaxUnitPrice.Add(resultProduct);

                            //Set new article to compare current most expensive Article
                            startArticleMax = resultProductsMaxUnitPrice.First().Articles.First();
                        }
                        ///Case for articles with the same price as the current most expensive one
                        else if (GetPricePerUnitByText(a.PricePerUnitText) == GetPricePerUnitByText(startArticleMax.PricePerUnitText))
                        {
                            //Rebuild product to get rid of other articles
                            Product resultProduct = new Product
                            {
                                ID = p.ID,
                                BrandName = p.BrandName,
                                DescriptionText = p.DescriptionText,
                                Name = p.Name,
                                Articles = new List<Article>()

                            };
                            resultProduct.Articles.Add(a);

                            resultProductsMaxUnitPrice.Add(resultProduct);

                            //Set new article to compare current most expensive Article
                            startArticleMax = resultProductsMaxUnitPrice.First().Articles.First();
                        }

                        ///Case for articles with lower price as the current most expensive one
                        if (GetPricePerUnitByText(a.PricePerUnitText) < GetPricePerUnitByText(startArticleMin.PricePerUnitText))
                        {
                            //Rebuild product to get rid of other articles
                            Product resultProduct = new Product
                            {
                                ID = p.ID,
                                BrandName = p.BrandName,
                                DescriptionText = p.DescriptionText,
                                Name = p.Name,
                                Articles = new List<Article>()

                            };
                            resultProduct.Articles.Add(a);

                            resultProductsMinUnitPrice.Clear();
                            resultProductsMinUnitPrice.Add(resultProduct);

                            //new item to compare with
                            startArticleMin = resultProductsMinUnitPrice.First().Articles.First();
                        }

                        ///Case for articles with same price as the current cheapest one
                        else if (GetPricePerUnitByText(a.PricePerUnitText) == GetPricePerUnitByText(startArticleMin.PricePerUnitText))
                        {
                            Product resultProduct = new Product
                            {
                                ID = p.ID,
                                BrandName = p.BrandName,
                                DescriptionText = p.DescriptionText,
                                Name = p.Name,
                                Articles = new List<Article>()

                            };
                            resultProduct.Articles.Add(a);

                            resultProductsMinUnitPrice.Add(resultProduct);

                            startArticleMin = resultProductsMinUnitPrice.First().Articles.First();
                        }
                    }
                }

               
            }

            //Return cheapest and most expensive product (with article) as one list
            return resultProductsMaxUnitPrice.Union(resultProductsMinUnitPrice).ToList();
        }

        /// <summary>
        /// Gives the Consumer the ability to request a product with a specific price.
        /// </summary>
        /// <returns>
        /// Returns the product with give specific price.
        /// </returns>
        [Route("~/api/products/fixedPrice/{url?}")]
        public List<Product> GetProductByPrice(string url)
        {
            //Check if given data is already read
            if (products == null)
            {
                DeserializeJsonContent(url);
            }

            List<Product> resultProducts = new List<Product>();
            List<Article> resultArticles = new List<Article>();

            if (products?.Count != 0)
            {
                //Iterate through articles in products
                foreach (Product p in products)
                {
                    foreach (Article a in p.Articles)
                    {
                        if (a.Price == FIXPRICE)
                        {
                            Product resultProduct = new Product
                            {
                                ID = p.ID,
                                BrandName = p.BrandName,
                                DescriptionText = p.DescriptionText,
                                Name = p.Name,
                                Articles = new List<Article>()

                            };
                            resultProduct.Articles.Add(a);
                            resultProducts.Add(resultProduct);
                        }
                    }
                }
            }
            
            //Return ordered result by first Article Price Per unit in Product (Only equal costing Articles are left inside Products, so comparing first one is sufficient)
            return resultProducts.OrderBy(p => GetPricePerUnitByText(p.Articles[0].PricePerUnitText)).ToList();
        }

        /// <summary>
        /// Gives the Consumer the ability to request the product with the most bottles.
        /// </summary>
        /// <returns>
        /// Returns the product with the most bottles.
        /// </returns>
        [Route("~/api/products/mostBottles/{url?}")]
        public List<Product> GetProductsWithMostBottles(string url)
        {
            //Check if given data is already read
            if (products == null)
            {
                DeserializeJsonContent(url);
            }

            List<Product> resultProducts = new List<Product>();

            if (products?.Count != 0)
            {
                Product startProduct = products.First();
                Article startArticle = startProduct.Articles.First();

                foreach (Product p in products)
                {
                    foreach (Article a in p.Articles)
                    {
                        //In case the article we are looking at comes with a larger amount of bottles replace clear the result list and add the current article to the list
                        if (GetBottlesByShortdescription(a.ShortDescription) > GetBottlesByShortdescription(startArticle.ShortDescription))
                        {
                            Product resultProduct = new Product
                            {
                                ID = p.ID,
                                BrandName = p.BrandName,
                                DescriptionText = p.DescriptionText,
                                Name = p.Name,
                                Articles = new List<Article>()

                            };
                            resultProduct.Articles.Add(a);

                            //Since one product has a larger number of bottles, the resultProducts needs to be emptied and filled ith the new Product
                            resultProducts.Clear();
                            resultProducts.Add(resultProduct);

                        }
                        //In case the article we are looking at comes with the same amount of bottles as the articles we added before add the article to the result list.
                        else if (GetBottlesByShortdescription(a.ShortDescription) == GetBottlesByShortdescription(startArticle.ShortDescription))
                        {
                            Product resultProduct = new Product
                            {
                                ID = p.ID,
                                BrandName = p.BrandName,
                                DescriptionText = p.DescriptionText,
                                Name = p.Name,
                                Articles = new List<Article>()

                            };
                            resultProduct.Articles.Add(a);
                            //Since the products have the same number of bottles, the resultProducts does not need to be emptied
                            resultProducts.Add(resultProduct);

                        }
                        startArticle = resultProducts.First().Articles.First();
                    }
                }

            }

            return resultProducts;
        }

        /// <summary>
        /// Contains all routes from above.
        /// </summary>
        /// <returns>
        /// Returns a maximum of four Products that match the following criteria:
        /// Most expensive and cheapest beer per litre
        /// The beer that cost exactly €17.99
        /// The Product that comes in the most bottles
        /// </returns>
        [Route("~/api/products/faq/{url?}")]
        public List<Result> GetProductsFAQ(string url)
        {
            DeserializeJsonContent(url);

            List<Result> results = new List<Result>();

            if (products?.Count != 0)
            {
                //Use Result-Datatype to have a description in the JSON result
                Result resultMinMax = new Result
                {
                    ResultDescription = "Most expensive and cheapest Product(s)",
                    Products = GetProductsWithMinMaxPricePerLitre(url)
                };

                Result resultExactPrice = new Result
                {
                    ResultDescription = "Product(s) with exact price of " + FIXPRICE,
                    Products = GetProductByPrice(url)
                };

                Result resultMostBottles = new Result
                {
                    ResultDescription = "Product(s) with most bottles",
                    Products = GetProductsWithMostBottles(url)
                };

                //Putting lists together as one result.
                results = new List<Result>
                {
                    resultMinMax,
                    resultExactPrice,
                    resultMostBottles
                };

            }

            return results;
        }
        #endregion

        #region Helperfunctions
        /// <summary>
        /// Fuction to deserialize the given JSON in the URL
        /// </summary>
        /// <param name="url"></param>
        private void DeserializeJsonContent(String url) {

            try
            {
                string json = new WebClient() { Encoding = Encoding.UTF8 }.DownloadString(url);

                //Deserialize given JSON data to Product Model
                products = JsonConvert.DeserializeObject<List<Product>>(json);
            }
            catch (Exception ex) {
                System.Console.WriteLine(ex.Message);
                
                throw ex;
            }
               
        }

        /// <summary>
        /// Helperfunction to extract number of Bottles out of Shortdescription
        /// </summary>
        /// <param name="shortdescription">
        /// Product Shortdescription in the format "20 x 0,5L (Glas)"
        /// </param>
        /// <returns>
        /// The number of bottles as Integer
        /// </returns>
        private int GetBottlesByShortdescription(string shortdescription) {

            //Split the shortdescription string at "x" and take the first part without spaces
            Int32.TryParse(shortdescription.Split('x')[0].Trim(), out int result);
            return result;
        }

        /// <summary>
        /// Helperfunction to extract price per unit out of pricePerUnitText
        /// </summary>
        /// <param name="pricePerUnitText">
        /// Product pricePerUnitText in the format "(2,10 €/Liter)"
        /// </param>
        /// <returns>
        /// The price per unit as Integer
        /// </returns>
        private double GetPricePerUnitByText(string pricePerUnitText)
        {
            //Parse the price per unit text start at second chatacter and take the part before "€" 
            Double.TryParse(pricePerUnitText.Substring(1).Split('€')[0].Trim(), out Double result);
            return result;
        }
        #endregion

    }
}
