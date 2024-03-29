﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
        string admin = "+998900246136";
        string userPhoneNumber;
        long chatId;

        string crud = "";
        string old = "";
        string allOrders = "";
        short count = 0;
        bool statusChange = false;

        int total = 0;
        string lastProduct = "";
        bool deletion = false;
        bool getPdf = false;
        bool getExcel = false;
        bool location = false;

        bool contact = false;
        bool receivedSms = false;
        
        bool adminPanel = false;
        bool userPanel = false;

        List<Users> users = Serializer<Users>.GetAll("C:\\AdminFolder\\Users.json");
        List<UserOrders> userOrders = Serializer<UserOrders>.GetAll("C:\\UserFolder\\UserOrders.json");
        List<Categories> categories = Serializer<Categories>.GetAll("C:\\AdminFolder\\Categories.json");
        List<Products> products = Serializer<Products>.GetAll("C:\\AdminFolder\\Products.json");
        List<PayTypes> payTypes = Serializer<PayTypes>.GetAll("C:\\AdminFolder\\PayTypes.json");

        string excelFilePath = "C:\\AdminFolder\\Orders.xlsx";
        string pdfFilePath = "C:\\UserFolder\\UserOrders.pdf";
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
                    crud = "";
                    total = 0;
                    deletion = false;
                    getPdf = false;
                    getExcel = false;
                    location = false;
                    contact = false;
                    receivedSms = false;
                    statusChange = false;
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
                //  new PhoneNumber(userPhoneNumber));
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
                    UserOrders.DeleteAll();

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
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Please enter phone number and new status of the user...\nExample: +998900246136 Delivered");

                        statusChange = true;
                        return;


                    case "All Orders":
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                        using (var package = new ExcelPackage(excelFilePath))
                        {
                            var sheet = package.Workbook.Worksheets.Add("Orders");

                            sheet.Cells["A1"].Value = "UserPhone";
                            sheet.Cells["B1"].Value = "Orders";
                            sheet.Cells["C1"].Value = "Order Status";

                            int row = 2;
                            foreach (var user in users)
                            {
                                sheet.Cells[row, 1].Value = user.phoneNumber;
                                sheet.Cells[row, 2].Value = user.orders;
                                sheet.Cells[row, 3].Value = user.orderStatus;
                                row++;
                            }

                            package.Save();
                        }
                        getExcel = true;

                        break;

                    case "All Users":
                        iTextSharp.text.Document pdf = new iTextSharp.text.Document();
                        users = Serializer<Users>.GetAll("C:\\AdminFolder\\Users.json");
                        PdfWriter writer = PdfWriter.GetInstance(pdf, new FileStream(pdfFilePath, FileMode.Create));
                        pdf.Open();
                        foreach (var user in users)
                        {
                            pdf.Add(new Paragraph($"Phone Number: {user.phoneNumber}\nOrder: {user.orders}\nOrder Status: {user.orderStatus}\n"));
                        }
                        pdf.Close();
                        getPdf = true;

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

                if (statusChange)
                {
                    Users.Update(message.Text!);
                    statusChange = false;

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Order Status of that user was changed successfully!");
                    return;
                }

                else if (getPdf)
                {
                    await using Stream stream = System.IO.File.OpenRead(pdfFilePath);

                    await botClient.SendDocumentAsync(
                        chatId: update.Message.Chat.Id,
                        document: InputFile.FromStream(stream: stream, fileName: $"Users.pdf"),
                        caption: "Your users");
                    stream.Dispose();

                    getPdf = false;
                    System.IO.File.Delete(pdfFilePath);
                    return;
                }

                else if (getExcel)
                {
                    await using Stream stream = System.IO.File.OpenRead(excelFilePath);

                    await botClient.SendDocumentAsync(
                        chatId: update.Message.Chat.Id,
                        document: InputFile.FromStream(stream: stream, fileName: $"Users.xlsx"),
                        caption: "Users Orders");
                    stream.Dispose();

                    getExcel = false;
                    System.IO.File.Delete(excelFilePath);
                    return;
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
                            text: $">>Orders<<\n{UserOrders.Read()}Total: {total}");

                        break;

                    case "Deliver":
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Please, send your location...");

                        break;

                    case "Remove Product":
                        deletion = true;

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Enter Product Type to delete...");
                            
                        break;

                    case "Clear Basket":
                        total = 0;
                        UserOrders.DeleteAll();

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Your basket is clear now!");

                        break;

                    case "Get Orders":
                        iTextSharp.text.Document pdf = new iTextSharp.text.Document();
                        userOrders = Serializer<UserOrders>.GetAll("C:\\UserFolder\\UserOrders.json");
                        PdfWriter writer = PdfWriter.GetInstance(pdf, new FileStream(pdfFilePath, FileMode.Create));
                        pdf.Open();
                        foreach (var order in userOrders)
                        {
                            pdf.Add(new Paragraph($"Product: {order.productName}\nType: {order.productType}\nAmount: {order.amount}x\nPrice: {order.price}\n"));
                        }
                        pdf.Add(new Paragraph($"Total: {total}"));
                        pdf.Close();

                        getPdf = true;
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Enter get to continue...");

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
                                    productName = product.categoryName,
                                    productType = product.productName,
                                    amount = message.Text,
                                    price = product.price
                                });

                                total += (product.price * int.Parse(message.Text));

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $"Great!");

                                return;
                            }
                        }

                    return;
                }

                if (getPdf && message.Text == "get")
                {
                    await using Stream stream = System.IO.File.OpenRead(pdfFilePath);

                    await botClient.SendDocumentAsync(
                        chatId: update.Message.Chat.Id,
                        document: InputFile.FromStream(stream: stream, fileName: $"Orders.pdf"),
                        caption: "Your orders");
                    stream.Dispose();

                    System.IO.File.Delete(pdfFilePath);

                    return;
                }

                await SearchCategory(botClient, update, cancellationToken, message.Text!);

                foreach (var product in products)
                {
                    if (message.Text ==  product.productName)
                    {
                        if (deletion)
                        {
                            foreach (var p in products)
                            {
                                if (message.Text == p.productName)
                                {
                                    total -= product.price * UserOrders.Delete(message.Text!);
                                }
                            }
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
                            text: $"Select the number...",
                            replyMarkup: categoryKeyboard);
                        
                        return;
                    }

                }

                if (message.Type == MessageType.Location)
                {
                    location = true;
                    var paymentKeyboard = new List<KeyboardButton[]>();
                    foreach (var pt in payTypes)
                    {
                        paymentKeyboard.Add([pt.type]);
                    }
                    paymentKeyboard.Add(["Basket"]);

                    ReplyKeyboardMarkup paymentKeyboardMarkup = new(paymentKeyboard) { ResizeKeyboard = true };

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        cancellationToken: cancellationToken,
                        text: "Great! Now choose the payment type...",
                        replyMarkup: paymentKeyboardMarkup);

                    return;
                }

                if (location)
                {
                    foreach (var payType in payTypes)
                    {
                        if (message.Text == payType.type)
                        {
                            if (message.Text == "Cash")
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Wait 10-15 minutes until our delivery boy come to you and he can get your money...\nSee You Soon!");

                                UserOrders.DeleteAll();
                                total = 0;
                                location = false;
                                allOrders = UserOrders.Read();

                                Users.Create(new Users()
                                {
                                    phoneNumber = userPhoneNumber,
                                    orders = allOrders,
                                    orderStatus = "Delivering"
                                });
                                return;
                            }

                            await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "Great!\nYour package is on the way...\nSee You Soon!");

                            UserOrders.DeleteAll();
                            total = 0;
                            location = false;

                            Users.Create(new Users()
                            {
                                phoneNumber = userPhoneNumber,
                                orders = allOrders,
                                orderStatus = "Delivering"
                            });
                            return;
                        }
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
                    var productsKeyboard = new List<KeyboardButton[]>();
                    foreach (var p in products)
                    {
                        if (p.categoryName == categories[c].categoryName)
                            productsKeyboard.Add([p.productName]);
                    }
                    productsKeyboard.Add(["Products"]);

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
