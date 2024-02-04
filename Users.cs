using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodOnlineBot
{
    public class Users
    {
        public string phoneNumber { get; set; }
        public string orders { get; set; }
        public string orderStatus { get; set; }

        private static readonly string path = "C:\\AdminFolder\\Users.json";

        public static void Create(Users user)
        {
            try
            {
                List<Users> users = Serializer<Users>.GetAll(path);
                if (users.Any(u => u.phoneNumber == user.phoneNumber))
                {
                    return;
                }
                users.Add(user);
                Serializer<Users>.Save(users, path);
            }
            catch { }
        }

        public static string Read()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                List<Users> users = Serializer<Users>.GetAll(path);
                foreach (Users user in users)
                {
                    stringBuilder.Append($"Phone Number: {user.phoneNumber}\nOrders: {user.orders}\nOrders Status: {user.orderStatus}\n\n");
                }
                return stringBuilder.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
        public static void Update(string Status)
        {
            try
            {
                List<Users> users = Serializer<Users>.GetAll(path);
                if (users != null)
                {
                    string[] parts = Status.Split(' ');
                    int index = users.FindIndex(user => user.phoneNumber == parts[0]);

                    if (index != -1)
                    {

                        users[index].orderStatus = parts[1];
                        Serializer<Users>.Save(users, path);
                    }
                }
            }
            catch { }
        }
    }
}
