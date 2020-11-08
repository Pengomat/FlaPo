using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlaPo.Models
{
    public class Article
    {
        public int ID { get; set; }
        public string ShortDescription { get; set; }
        public double Price { get; set; }
        public String Unit { get; set; }
        public String PricePerUnitText { get; set; }
        public String Image { get; set; }
       }
}