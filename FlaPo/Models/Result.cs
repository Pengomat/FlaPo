using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlaPo.Models
{
    public class Result
    {
        public string ResultDescription { get; set; }
        public List<Product> Products { get; set; }
    }
}