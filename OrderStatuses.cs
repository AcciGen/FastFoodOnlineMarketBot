using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodOnlineBot
{
    public class OrderStatuses
    {
        public string status { get; set; }

        private static readonly string path = "C:\\AdminFolder\\OrderStatuses.json";

        public static void Create(OrderStatuses orderStatus)
        {
            try
            {
                List<OrderStatuses> orderStatuses = Serializer<OrderStatuses>.GetAll(path);
                if (orderStatuses.Any(os => os.status == orderStatus.status))
                {
                    return;
                }
                orderStatuses.Add(orderStatus);
                Serializer<OrderStatuses>.Save(orderStatuses, path);
            }
            catch { }
        }

        public static string Read()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                List<OrderStatuses> orderStatuses = Serializer<OrderStatuses>.GetAll(path);
                foreach (OrderStatuses os in orderStatuses)
                {
                    stringBuilder.Append($"Status: {os.status}\n");
                }
                return stringBuilder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void Update(string oldStatus, string newStatus)
        {
            try
            {
                List<OrderStatuses> orderStatuses = Serializer<OrderStatuses>.GetAll(path);
                if (orderStatuses != null)
                {
                    int index = orderStatuses.FindIndex(os => os.status == oldStatus);

                    if (index != -1)
                    {
                        orderStatuses[index].status = newStatus;
                        Serializer<OrderStatuses>.Save(orderStatuses, path);
                    }
                }
            }
            catch { }
        }

        public static void Delete(string delStatus)
        {
            try
            {
                List<OrderStatuses> orderStatuses = Serializer<OrderStatuses>.GetAll(path);
                var removableOrderStatus = orderStatuses.Find(os => os.status == delStatus);

                if (removableOrderStatus != null)
                {
                    orderStatuses.Remove(removableOrderStatus);
                    Serializer<OrderStatuses>.Save(orderStatuses, path);
                }
            }
            catch { }
        }
    }
}
