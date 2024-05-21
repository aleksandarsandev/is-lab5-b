using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShop.Domain.DTO
{
    public class MovieDTO
    {
        public string MovieName { get; set; }
        public string MovieDescription { get; set; }
        public string MovieImage { get; set; }
        public double Rating { get; set; }
    }
}
