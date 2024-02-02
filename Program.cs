namespace FastFoodOnlineBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string token = "6722876525:AAGHQwoySwAtXzHb92skwNjvFKU7G7Leps4";
            TelegramBotHandler handler = new TelegramBotHandler(token);

            try
            {
                await handler.BotHandle();
            }
            catch (Exception)
            {
                throw new Exception("Access denied!");
            }
        }
    }
}
