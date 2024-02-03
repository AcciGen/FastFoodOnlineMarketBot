using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodOnlineBot
{
    public class Products
    {
        public string productName { get; set; }
        public int price { get; set; }

        private static readonly string path = "C:\\AdminFolder\\Products.json";

        public static void Create(Products product)
        {
            try
            {
                List<Products> products = Serializer<Products>.GetAll(path);
                if (products.Any(p => p.productName == product.productName))
                {
                    return;
                }
                products.Add(product);
                Serializer<Products>.Save(products, path);
            }
            catch { }
        }

        public static string Read()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                List<Products> products = Serializer<Products>.GetAll(path);
                foreach (Products p in products)
                {
                    stringBuilder.Append($"Product: {p.productName}\nPrice: {p.price}\n");
                }
                return stringBuilder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void Update(string oldName, string newProduct)
        {
            try
            {
                List<Products> products = Serializer<Products>.GetAll(path);
                if (products != null)
                {
                    int index = products.FindIndex(name => name.productName == oldName);

                    if (index != -1)
                    {
                        string[] parts = newProduct.Split(' ');

                        products[index].productName = parts[0];
                        products[index].price = int.Parse(parts[1]);
                        Serializer<Products>.Save(products, path);
                    }
                }
            }
            catch { }
        }

        public static void Delete(string delName)
        {
            try
            {
                List<Products> products = Serializer<Products>.GetAll(path);
                var removableProduct = products.Find(p => p.productName == delName);

                if (removableProduct != null)
                {
                    products.Remove(removableProduct);
                    Serializer<Products>.Save(products, path);
                }
            }
            catch { }
        }
    }
}
