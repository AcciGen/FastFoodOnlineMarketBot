using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FastFoodOnlineBot
{
    public class Categories
    {
        public string categoryName { get; set; }

        private static readonly string path = "C:\\AdminFolder\\Categories.json";

        public static void Create(Categories category)
        {
            try
            {
                List<Categories> categories = Serializer<Categories>.GetAll(path);
                if (categories.Any(c => c.categoryName == category.categoryName))
                {
                    return;
                }
                categories.Add(category);
                Serializer<Categories>.Save(categories, path);
            }
            catch { }
        }

        public static string Read()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                List<Categories> categories = Serializer<Categories>.GetAll(path);
                foreach (Categories ct in categories)
                {
                    stringBuilder.Append($"Category: {ct.categoryName}\n");
                }
                return stringBuilder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void Update(string oldName, string newName)
        {
            try
            {
                List<Categories> categories = Serializer<Categories>.GetAll(path);
                if (categories != null)
                {
                    int index = categories.FindIndex(name => name.categoryName == oldName);
                    
                    if (index != -1)
                    {
                        categories[index].categoryName = newName;
                        Serializer<Categories>.Save(categories, path);
                    }
                }
            }
            catch { }
        }

        public static void Delete(string delName)
        {
            try
            {
                List<Categories> categories = Serializer<Categories>.GetAll(path);
                var removableCategory = categories.Find(ct => ct.categoryName == delName);

                if (removableCategory != null)
                {
                    categories.Remove(removableCategory);
                    Serializer<Categories>.Save(categories, path);
                }
            }
            catch { }
        }
    }
}
