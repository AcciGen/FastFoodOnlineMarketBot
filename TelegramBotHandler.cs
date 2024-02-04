using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Twilio.TwiML.Messaging;
using Twilio.Types;

namespace FastFoodOnlineBot
{
    public class TelegramBotHandler
    {
        public string Token { get; set; }
        string accountSid = "AC7bcc36021b3503cdd0f2e0cd579a3904";
        string authToken = "afb47832338a2e4306c8612861de1917";
        string admin = "+99890024613";
        string userPhoneNumber;
        long chatId;

        string crud = "";
        string old = "";
        short count = 0;

        int total = 0;
        string lastProduct = "";
        bool deletion = false;

        bool contact = false;
        bool receivedSms = false;
        
        bool adminPanel = false;
        bool userPanel = false;

        List<Categories> categories;
        List<Products> products;

        string categoriesPath = "C:\\AdminFolder\\Categories.json";
        string productsPath = "C:\\AdminFolder\\Products.json";
        string payTypesPath = "C:\\AdminFolder\\PayTypes.json";
        string excelFilePath = "C:\\AdminFolder\\Orders.xlsx";
        string ordersPath = "C:\\UserFolder\\Orders.json";

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

            chatId = message.Chat.Id;

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
                    crud = "";
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
                            [ "Get Orders" ]
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
                            categoryName = parts[0],
                            productName = parts[1],
                            price = int.Parse(parts[2])
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

                        await CRUDPanel(botClient, update, cancellationToken);

                        break;

                    case "Product":
                        crud = "P";

                        await CRUDPanel(botClient, update, cancellationToken);

                        break;

                    case "PayType":
                        crud = "PT";

                        await CRUDPanel(botClient, update, cancellationToken);

                        break;

                    case "OrderStatus":
                        crud = "OS";

                        await CRUDPanel(botClient, update, cancellationToken);

                        break;

                    case "Users OrderStatus":
                        

                        break;

                    case "All Orders":
                        //using (var package = new ExcelPackage(excelFilePath))
                        //{
                        //    var sheet = package.Workbook.Worksheets.Add("Orders");

                        //    sheet.Cells["A1"].Value = "UserId";
                        //    sheet.Cells["B1"].Value = "Product";
                        //    sheet.Cells["C1"].Value = "Price";

                        //    List<OrderStatuses> orders = Serializer<OrderStatuses>.GetAll(ordersPath);

                        //    int row = 2; // starting from row 2 to skip headers
                        //    foreach (var order in orders)
                        //    {
                        //        sheet.Cells[row, 1].Value = order.UserId;
                        //        sheet.Cells[row, 2].Value = order.Product;
                        //        sheet.Cells[row, 3].Value = order.Product;
                        //        row++;
                        //    }

                        //    package.Save();
                        //}

                        break;

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
                                    text: "Enter new Product...\nExample: Burgers Hamburger 35000");

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
                    case "Main Menu":
                        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                        {
                            new KeyboardButton[] { "Products", "Basket" },
                                [ "Get Orders" ]
                        })
                        {
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            cancellationToken: cancellationToken,
                            text: "Magic Main Menu🔮🪄",
                            replyMarkup: replyKeyboardMarkup);

                        break;

                    case "Products":
                        categories = Serializer<Categories>.GetAll(categoriesPath);

                        var categoriesKeyboard = new List<KeyboardButton[]>();
                        if (categories.Count % 2 == 0)
                        {
                            for (int i = 0; i < categories.Count; i += 2)
                            {
                                categoriesKeyboard.Add([categories[i].categoryName, categories[i + 1].categoryName,]);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < categories.Count; i += 2)
                            {
                                if (i + 1 == categories.Count)
                                {
                                    categoriesKeyboard.Add([categories[i].categoryName]);
                                    break;
                                }
                                categoriesKeyboard.Add([categories[i].categoryName, categories[i + 1].categoryName,]);
                            }
                        }
                        categoriesKeyboard.Add(["Main Menu"]);

                        ReplyKeyboardMarkup categoriesKeyboardMarkup = new(categoriesKeyboard) { ResizeKeyboard = true };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            cancellationToken: cancellationToken,
                            text: "Choose Category...",
                            replyMarkup: categoriesKeyboardMarkup);
                        break;

                    case "Basket":
                        ReplyKeyboardMarkup basketKeyboardMarkup = new(new[]
                        {
                            new KeyboardButton[] { "My Orders", "Deliver" },
                                [ "Remove Product", "Clear Basket" ],
                                [ "Main Menu" ]
                        })
                        {
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            cancellationToken: cancellationToken,
                            text: "Here you go!",
                            replyMarkup: basketKeyboardMarkup);

                        break;

                    case "My Orders":
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $">>Orders<<\n{Categories.Read()}Total: {total}");

                        break;

                    case "Deliver":
                        List<PayTypes> payTypes = Serializer<PayTypes>.GetAll(payTypesPath);

                        var paymentKeyboard = new List<KeyboardButton[]>();
                        foreach (var pt in payTypes)
                        {
                            paymentKeyboard.Add([pt.type]);
                        }

                        ReplyKeyboardMarkup paymentKeyboardMarkup = new(paymentKeyboard) { ResizeKeyboard = true };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            cancellationToken: cancellationToken,
                            text: "Choose Payment Type...",
                            replyMarkup: paymentKeyboardMarkup);

                        break;

                    case "Remove Product":
                        deletion = true;

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Enter Product name to delete...");
                            
                        break;

                    case "Clear Basket":
                        UserOrders.DeleteAll();

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Your basket is clear now!");

                        break;

                    case "Get Orders":

                        break;

                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                    case "8":
                    case "9":
                        foreach (var product in products)
                        {
                            if (lastProduct == product.productName)
                            {
                                UserOrders.Create(new UserOrders()
                                {
                                    productName = product.productName,
                                    amount = message.Text
                                });

                                total += product.price * int.Parse(message.Text);
                                return;
                            }
                        }
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"Great!");

                        break;
                }

                await SearchCategory(botClient, update, cancellationToken, message.Text!);

                foreach (var product in products)
                {
                    if (message.Text ==  product.productName)
                    {
                        if (deletion)
                        {
                            UserOrders.Delete(message.Text!);
                            deletion = false;

                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Removed from your basket successfully!");
                            
                            return;
                        }

                        lastProduct = product.productName;

                        ReplyKeyboardMarkup categoryKeyboard = new(new[]
                        {
                            new KeyboardButton[] { "1", "2", "3", "4", "5" },
                                [ "6", "7", "8", "9", $"{product.categoryName}" ],
                        })
                        {
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            cancellationToken: cancellationToken,
                            text: $"Select the number of {product.productName}",
                            replyMarkup: categoryKeyboard);
                        
                        return;
                    }

                }

                return;
            }
            #endregion
        }

        public async Task CRUDPanel(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            ReplyKeyboardMarkup categoryKeyboard = new(new[]
            {
                new KeyboardButton[] { "Add", "Read" },
                    [ "Update", "Delete" ],
                    [ "Panel" ]
            })
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                cancellationToken: cancellationToken,
                text: "Sure",
                replyMarkup: categoryKeyboard);
        }

        public async Task SearchCategory(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, string category)
        {
            for (int c = 0; c < categories.Count; c++)
            {
                if (category == categories[c].categoryName)
                {
                    products = Serializer<Products>.GetAll(productsPath);

                    var productsKeyboard = new List<KeyboardButton[]>();
                    foreach (var p in products)
                    {
                        if (p.categoryName == categories[c].categoryName)
                            productsKeyboard.Add([p.productName]);
                    }

                    ReplyKeyboardMarkup productsKeyboardMarkup = new(productsKeyboard) { ResizeKeyboard = true };

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        cancellationToken: cancellationToken,
                        text: "Sure",
                        replyMarkup: productsKeyboardMarkup);

                    return;
                }
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
