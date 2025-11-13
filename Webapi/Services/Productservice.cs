using Webapi.Data;
using Webapi.Models;
using Microsoft.EntityFrameworkCore;

namespace Webapi.Services
{
    public class Productservice
    {
        public void Initialstock(Product product)
        {
            if (product.Stock < 0)
            {
                throw new ArgumentException("El stock inicial no puede ser negativo");
            }
        }
        public void Updatestock(Product product)
        {
            if (product.Stock < 0)
            {
                throw new ArgumentException("Stock insuficiente");
            }
        }

    }
}
