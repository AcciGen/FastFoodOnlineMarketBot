using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FastFoodOnlineBot
{
    public static class CRUD
    {
        public static string filePath = "C:\\Users\\Acer\\Desktop\\Info.json";

        public static void Create(Users chat)
        {
            List<Users> chats = GetAllChats();
            if (chats.Any(c => c.chatID == chat.chatID))
            {
                return;
            }
            chats.Add(chat);
            SaveChats(chats);
        }

        public static string Read(long chatId)
        {
            List<Users> chats = GetAllChats();
            var chat = chats.Find(c => c.chatID == chatId);

            return $"{chat!.chatID}:{chat.phoneNumber}";
        }

        public static void Update(long chatId, string newPhoneNumber)
        {
            try
            {
                List<Users> users = GetAllChats();

                if (users != null)
                {
                    int index = users.FindIndex(u => u.chatID == chatId);


                    if (index != -1)
                    {
                        users[index].phoneNumber = newPhoneNumber;

                        SaveChats(users);
                    }
                }
            }
            catch { }
        }

        public static void Delete(long chatId)
        {
            List<Users> chats = GetAllChats();
            var chatToRemove = chats.Find(c => c.chatID == chatId);

            if (chatToRemove != null)
            {
                chats.Remove(chatToRemove);
                SaveChats(chats);
            }
        }

        private static List<Users> GetAllChats()
        {
            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<Users>>(json) ?? new List<Users>();
            }
            else
            {
                return new List<Users>();
            }
        }
        public static int GetStatusCode(long chatId)
        {
            List<Users> users = GetAllChats();
            Users? chatToRemove = users.Find(c => c.chatID == chatId);

            return chatToRemove!.status;
        }

        public static void ChangeStatusCode(long chatId, int statusCode)
        {
            List<Users> users = GetAllChats();
            int index = users.FindIndex(u => u.chatID == chatId);
            if (index != -1)
            {
                users[index].status = statusCode;
            }
            SaveChats(users);
        }

        public static List<Users> GetAll()
        {
            return GetAllChats();
        }

        private static void SaveChats(List<Users> chats)
        {
            string json = JsonSerializer.Serialize(chats);
            System.IO.File.WriteAllText(filePath, json);
        }
    }

    public class Users
    {
        public long chatID { get; set; }

        public int status { get; set; }
        public string? phoneNumber { get; set; }
    }
}
