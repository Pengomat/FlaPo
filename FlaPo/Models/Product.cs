using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlaPo.Models
{
    public class Product
    {
        public int ID { get; set; }
        public string BrandName { get; set; }
        public string Name { get; set; }
        public string DescriptionText { get; set; }
        public List<Article> Articles { get; set; }
    }
}