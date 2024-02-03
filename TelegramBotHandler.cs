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
        string userPhoneNumber;
        string crud = "";
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
                    userPhoneNumber = "";
                    contact = false;
                    receivedSms = false;
                    adminPanel = false;
                    userPanel = false;
                }

                var replyKeyboard = new ReplyKeyboardMarkup(
                      new List<KeyboardButton[]>()
                       {
                        new KeyboardButton[]
                        {
                            KeyboardButton.WithRequestContact("Phone Number📱"),
                        }
                       })
                {
                    ResizeKeyboard = true
                };

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Welcome to Online Fast Food Market Bot!🌮\nPlease share your contact number to continue your order...",
                    replyMarkup: replyKeyboard);

                return;
            }

            else if (message.Type == MessageType.Contact && contact != true)
            {
                contact = true;
                userPhoneNumber = message.Contact!.PhoneNumber;

                //var smsOptions = new CreateMessageOptions(
                //  new PhoneNumber(user));
                //smsOptions.From = new PhoneNumber("+12512201568");
                //smsOptions.Body = "777";

                //var sms = MessageResource.Create(smsOptions);
                //Console.WriteLine(sms.Body);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Please enter the code which was sent to your number...");

                return;
            }

            else if (!contact)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Please send your phone number!");

                return;
            }

            else if (contact && message.Text == "777")
            {
                receivedSms = true;

                if (userPhoneNumber == admin)
                {
                    adminPanel = true;

                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[] { "Category", "Product", "PayType", "OrderStatus" },
                            [ "Users OrderStatus", "All Orders", "All Users" ]
                    })
                    {
                        ResizeKeyboard = true
                    };

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        cancellationToken: cancellationToken,
                        text: "Hi, admin!\nWhat do you want to do here?",
                        replyMarkup: replyKeyboardMarkup);
                }

                else
                {
                    userPanel = true;

                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[] { "Products", "Basket" },
                            [ "All Orders", "Deliver" ]
                    })
                    {
                        ResizeKeyboard = true
                    };

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        cancellationToken: cancellationToken,
                        text: "Congratulations!\nYou can start your order now...",
                        replyMarkup: replyKeyboardMarkup);
                }

                return;
            }

            else if (contact && adminPanel)
            {
                switch (crud)
                {
                    case "CA":
                        Categories.Create(new Categories()
                        {
                            categoryName = message.Text!
                        });

                        crud = "C";
                        await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "New Category created successfully!");
                        break;

                    case "PA":
                        Categories.Create(new Categories()
                        {
                            categoryName = message.Text!
                        });

                        crud = "P";
                        await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "New Product created successfully!");
                        break;

                    case "PtA":
                        Categories.Create(new Categories()
                        {
                            categoryName = message.Text!
                        });

                        crud = "Pt";
                        await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "New PayType created successfully!");
                        break;

                    case "OA":
                        Categories.Create(new Categories()
                        {
                            categoryName = message.Text!
                        });

                        crud = "O";
                        await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "New OrderStatus created successfully!");
                        break;
                }

                switch (message.Text)
                {
                    case "Panel":
                        ReplyKeyboardMarkup panelKeyboard = new(new[]
                        {
                            new KeyboardButton[] { "Category", "Product", "PayType", "OrderStatus" },
                                [ "Users OrderStatus", "All Orders", "All Users" ]
                        })
                        {
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            cancellationToken: cancellationToken,
                            text: "<-Back Anew✨🎰",
                            replyMarkup: panelKeyboard);

                        break;

                    case "Category":
                        crud = "C";

                        ReplyKeyboardMarkup categoryKeyboard = new(new[]
                        {
                            new KeyboardButton[] { "Add", "Read" },
                                [ "Update", "Delete", "Panel" ],
                        })
                        {
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            cancellationToken: cancellationToken,
                            text: "Sure",
                            replyMarkup: categoryKeyboard);

                        break;

                    case "Product":
                        crud = "P";

                        ReplyKeyboardMarkup productKeyboard = new(new[]
                        {
                            new KeyboardButton[] { "Add", "Read" },
                                [ "Update", "Delete", "Panel" ],
                        })
                        {
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            cancellationToken: cancellationToken,
                            text: "Sure",
                            replyMarkup: productKeyboard);

                        break;

                    case "PayType":
                        crud = "Pt";

                        ReplyKeyboardMarkup paytypeKeyboard = new(new[]
                        {
                            new KeyboardButton[] { "Add", "Read" },
                                [ "Update", "Delete", "Panel" ],
                        })
                        {
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            cancellationToken: cancellationToken,
                            text: "Sure",
                            replyMarkup: paytypeKeyboard);

                        break;

                    case "OrderStatus":
                        crud = "O";

                        ReplyKeyboardMarkup orderStatusKeyboard = new(new[]
                        {
                            new KeyboardButton[] { "Add", "Read" },
                                [ "Update", "Delete", "Panel" ],
                        })
                        {
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            cancellationToken: cancellationToken,
                            text: "Sure",
                            replyMarkup: orderStatusKeyboard);

                        break;

                    case "Users OrderStatus":
                    case "All Orders":
                    case "All Users":

                        break;

                    case "Add":
                        switch (crud)
                        {
                            case "C":
                                crud = "CA";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter new Category...");
                                
                                break;

                            case "P":
                                crud = "PA";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter new Product...");

                                break;

                            case "Pt":
                                crud = "PtA";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter new PayType...");

                                break;

                            case "O":
                                crud = "OA";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter new OrderStatus...");

                                break;
                        }

                        break;

                    case "Read":
                        switch (crud)
                        {
                            case "C":
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $">>Categories List<<\n{Categories.Read()}");

                                break;

                            case "P":
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $">>Products List<<\n{.Read()}");

                                break;

                            case "Pt":
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $">>PayTypes List<<\n{.Read()}");

                                break;

                            case "O":
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $"OrderStatuses List<<\n{.Read()}");

                                break;
                        }

                        break;

                    case "Update":
                        switch (crud)
                        {
                            case "C":

                                break;
                        }

                        break;

                    case "Delete":
                        switch (crud)
                        {
                            case "C":

                                break;
                        }

                        break;
                }

                return;
            }

            else if (contact && userPanel)
            {
                switch (message.Text)
                {
                    case "Product":
                        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                            {
                                new KeyboardButton[] { "Sandwich", "Taco", "Hot Dog", "Fries" },
                                    ["Cheeseburger", "Pizza", "Chicken", "Ice Cream"],
                                    [ "Coke", "Juice", "Coffee", "<-Back" ]
                            })
                        {
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            cancellationToken: cancellationToken,
                            text: "Choose the products you want to buy...",
                            replyMarkup: replyKeyboardMarkup);

                        break;
                }

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
