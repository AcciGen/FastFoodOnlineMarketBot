using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodOnlineBot
{
    public class PayTypes
    {
        public string type { get; set; }

        private static readonly string path = "C:\\AdminFolder\\PayTypes.json";

        public static void Create(PayTypes payType)
        {
            try
            {
                List<PayTypes> payTypes = Serializer<PayTypes>.GetAll(path);
                if (payTypes.Any(pt => pt.type == payType.type))
                {
                    return;
                }
                payTypes.Add(payType);
                Serializer<PayTypes>.Save(payTypes, path);
            }
            catch { }
        }

        public static string Read()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                List<PayTypes> payTypes = Serializer<PayTypes>.GetAll(path);
                foreach (PayTypes pt in payTypes)
                {
                    stringBuilder.Append($"Type: {pt.type}\n");
                }
                return stringBuilder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void Update(string oldType, string newType)
        {
            try
            {
                List<PayTypes> payTypes = Serializer<PayTypes>.GetAll(path);
                if (payTypes != null)
                {
                    int index = payTypes.FindIndex(pt => pt.type == oldType);

                    if (index != -1)
                    {
                        payTypes[index].type = newType;
                        Serializer<PayTypes>.Save(payTypes, path);
                    }
                }
            }
            catch { }
        }

        public static void Delete(string delType)
        {
            try
            {
                List<PayTypes> payTypes = Serializer<PayTypes>.GetAll(path);
                var removablePayType = payTypes.Find(pt => pt.type == delType);

                if (removablePayType != null)
                {
                    payTypes.Remove(removablePayType);
                    Serializer<PayTypes>.Save(payTypes, path);
                }
            }
            catch { }
        }
    }
}
