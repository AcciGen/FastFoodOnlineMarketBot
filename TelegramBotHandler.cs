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
        string old = "";
        short count = 0;

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

            #region //Phone Asker
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
                    text: "Please enter the code which was sent📨 to your number...");

                return;
            }

            else if (!contact)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Please send your phone number!");

                return;
            }
            #endregion

            #region //Recognize Person
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
            #endregion

            #region //Admin Panel
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
                        string[] parts = message.Text!.Split(' ');

                        Products.Create(new Products()
                        {
                            productName = parts[0],
                            price = int.Parse(parts[1])
                        });

                        crud = "P";
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "New Product created successfully!");

                        break;

                    case "PTA":
                        PayTypes.Create(new PayTypes()
                        {
                            type = message.Text!
                        });

                        crud = "PT";
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "New PayType created successfully!");

                        break;

                    case "OSA":
                        OrderStatuses.Create(new OrderStatuses()
                        {
                            status = message.Text!
                        });

                        crud = "OS";
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "New OrderStatus created successfully!");

                        break;


                    case "CU":
                        if (++count == 1)
                        {
                            old = message.Text!;
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Now enter new name...");

                            break;
                        }
                        count = 0;
                        crud = "C";

                        Categories.Update(old, message.Text!);

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Updated successfully!");

                        break;

                    case "PU":
                        if (++count == 1)
                        {
                            old = message.Text!;
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Now enter new name and price...");

                            break;
                        }
                        count = 0;
                        crud = "P";

                        Products.Update(old, message.Text!);

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Updated successfully!");

                        break;

                    case "PTU":
                        if (++count == 1)
                        {
                            old = message.Text!;
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Now enter new type...");

                            break;
                        }
                        count = 0;
                        crud = "PT";

                        PayTypes.Update(old, message.Text!);

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Updated successfully!");

                        break;

                    case "OSU":
                        if (++count == 1)
                        {
                            old = message.Text!;
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Now enter new status...");

                            break;
                        }
                        count = 0;
                        crud = "OS";

                        OrderStatuses.Update(old, message.Text!);

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Updated successfully!");

                        break;

                    case "CD":
                        crud = "C";

                        Categories.Delete(message.Text!);

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Deleted successfully!");

                        break;

                    case "PD":
                        crud = "P";

                        Products.Delete(message.Text!);

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Deleted successfully!");

                        break;

                    case "PTD":
                        crud = "PT";

                        PayTypes.Delete(message.Text!);

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Deleted successfully!");

                        break;

                    case "OSD":
                        crud = "OS";

                        OrderStatuses.Delete(message.Text!);

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Deleted successfully!");

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
                        crud = "PT";

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
                        crud = "OS";

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
                        using (var package = new ExcelPackage(@"c:\temp\myWorkbook.xlsx"))
                        {
                            var sheet = package.Workbook.Worksheets.Add("My Sheet");
                            sheet.Cells["A1"].Value = "Hello World!";

                            // Save to file
                            package.Save();
                        }

                        break;
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
                                    text: "Enter new Product with price...\nExample: Fries 14000");

                                break;

                            case "PT":
                                crud = "PTA";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter new PayType...");

                                break;

                            case "OS":
                                crud = "OSA";

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
                                    text: $">>Products List<<\n{Products.Read()}");

                                break;

                            case "PT":
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $">>PayTypes List<<\n{PayTypes.Read()}");

                                break;

                            case "OS":
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $">>OrderStatuses List<<\n{OrderStatuses.Read()}");

                                break;
                        }

                        break;

                    case "Update":
                        switch (crud)
                        {
                            case "C":
                                crud = "CU";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter the old Category name...");

                                break;

                            case "P":
                                crud = "PU";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter the old Product name...");

                                break;

                            case "PT":
                                crud = "PTU";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter the old PayType...");

                                break;

                            case "OS":
                                crud = "OSU";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter the old OrderStatus...");

                                break;
                        }

                        break;

                    case "Delete":
                        switch (crud)
                        {
                            case "C":
                                crud = "CD";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter Category name to delete...");

                                break;

                            case "P":
                                crud = "PD";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter Product name to delete...");

                                break;

                            case "PT":
                                crud = "PTD";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter PayType to delete...");

                                break;

                            case "OS":
                                crud = "OSD";

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Enter OrderStatus to delete...");

                                break;
                        }

                        break;
                }

                return;
            }
#endregion

            #region //User Panel
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
            #endregion
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
