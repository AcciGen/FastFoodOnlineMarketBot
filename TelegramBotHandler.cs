using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace FastFoodOnlineBot
{
    public class TelegramBotHandler
    {
        public string Token { get; set; }
        string accountSid = "AC7bcc36021b3503cdd0f2e0cd579a3904";
        string authToken = "afb47832338a2e4306c8612861de1917";
        string admin = "+998900246136";
        string user;
        bool contact = false;
        bool receivedSms = false;
        bool adminPanel = false;
        bool userPanel = false;

        public TelegramBotHandler(string token)
        {
            Token = token;
            TwilioClient.Init(accountSid, authToken);
        }

        public async Task BotHandle()
        {
            var botClient = new TelegramBotClient(Token);

            using CancellationTokenSource cts = new();

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"I'm here @{me.Username}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadLine();

            cts.Cancel();

        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;

            long chatId = message.Chat.Id;

            if (message.Text == "/start")
            {
                if (contact == true)
                {
                    user = "";
                    contact = false;
                    receivedSms = false;
                    adminPanel = false;
                    userPanel = false;
                    return;
                }

                var replyKeyboard = new ReplyKeyboardMarkup(
                      new List<KeyboardButton[]>()
                       {
                        new KeyboardButton[]
                        {
                            KeyboardButton.WithRequestContact("Send Phone Number"),
                        }
                       })
                {
                    ResizeKeyboard = true
                };

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Welcome to Online Fast Food Market Bot!🌮\nPlease share your contact number!",
                    replyMarkup: replyKeyboard);

                return;
            }

            else if (message.Type == MessageType.Contact && contact != true)
            {
                contact = true;
                user = message.Contact!.PhoneNumber;

                //var smsOptions = new CreateMessageOptions(
                //  new PhoneNumber(user));
                //smsOptions.From = new PhoneNumber("+12512201568");
                //smsOptions.Body = "777";

                //var sms = MessageResource.Create(smsOptions);
                //Console.WriteLine(sms.Body);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Please enter the code which was sent to you...");

                return;
            }

            else if (contact && message.Text == "777")
            {
                receivedSms = true;

                if (user == admin)
                {
                    adminPanel = true;

                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[] { "Category", "Product", "Pay Type" },
                        ["Order Status", "Change Order Status", "All Orders", "All Users"],
                    })
                    {
                        ResizeKeyboard = true
                    };

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        cancellationToken: cancellationToken,
                        text: "Hi, admin!\nWhat you want to change here?",
                        replyMarkup: replyKeyboardMarkup);
                }

                else
                {
                    userPanel = true;

                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[] { "Products", "Basket 3x" },
                        [ "All Orders", "Deliver" ],
                    })
                    {
                        ResizeKeyboard = true
                    };

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        cancellationToken: cancellationToken,
                        text: "Congratulations!\nYou can start your order from now...",
                        replyMarkup: replyKeyboardMarkup);
                }

                return;
            }

            else if (message.Text == "Products")
            {
                var inlineKeyboard = new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton[]>()
                    {
                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData("Sandwich", "Sandwich"),
                            InlineKeyboardButton.WithCallbackData("Taco", "Taco"),
                            InlineKeyboardButton.WithCallbackData("Hot Dog", "Hot Dog"),
                            InlineKeyboardButton.WithCallbackData("Fries", "Fries"),
                        },

                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData("Cheeseburger", "Cheeseburger"),
                            InlineKeyboardButton.WithCallbackData("Pizza", "Pizza"),
                            InlineKeyboardButton.WithCallbackData("Chicken", "Chicken"),
                            InlineKeyboardButton.WithCallbackData("Ice Cream", "Ice Cream"),
                        },

                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData("Coke", "Coke"),
                            InlineKeyboardButton.WithCallbackData("Juice", "Juice"),
                            InlineKeyboardButton.WithCallbackData("Coffee", "Coffee"),
                            InlineKeyboardButton.WithCallbackData("Tea", "Tea"),
                        }
                    });

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    cancellationToken: cancellationToken,
                    text: "Choose the products you want...",
                    replyMarkup: inlineKeyboard);

                return;
            }



        }

        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
