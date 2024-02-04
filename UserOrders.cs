using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodOnlineBot
{
    public class UserOrders
    {
        public string productName { get; set; }
        public int amount { get; set; }

        private static readonly string path = "C:\\UserFolder\\UserOrders.json";

        public static void Create(UserOrders userOrder)
        {
            try
            {
                List<UserOrders> userOrders = Serializer<UserOrders>.GetAll(path);
                if (userOrders.Any(uo => uo.productName == userOrder.productName))
                {
                    return;
                }
                userOrders.Add(userOrder);
                Serializer<UserOrders>.Save(userOrders, path);
            }
            catch { }
        }

        public static string Read()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                List<UserOrders> userOrders = Serializer<UserOrders>.GetAll(path);
                foreach (UserOrders uo in userOrders)
                {
                    stringBuilder.Append($"{uo.productName} {uo.amount}x\n");
                }
                return stringBuilder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void Delete(string delProduct)
        {
            try
            {
                List<UserOrders> userOrders = Serializer<UserOrders>.GetAll(path);
                var removableUserOrder = userOrders.Find(uo => uo.productName == delProduct);

                if (removableUserOrder != null)
                {
                    userOrders.Remove(removableUserOrder);
                    Serializer<UserOrders>.Save(userOrders, path);
                }
            }
            catch { }
        }

        public static void DeleteAll()
        {
            try
            {
                List<UserOrders> userOrders = Serializer<UserOrders>.GetAll(path);
                userOrders.Clear();
                Serializer<UserOrders>.Save(userOrders, path);
            }
            catch { }
        }
    }
}
