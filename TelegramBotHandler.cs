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

namespace FastFoodOnlineBot
{
    public class TelegramBotHandler
    {
        public string Token { get; set; }
        public bool Subscription = false;
        public TelegramBotHandler(string token)
        {
            Token = token;
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

            string replaceMessage = message.Text!.Replace("www.", "dd");

            if (message.Text == "/start")
            {
                ChatMember membership = await botClient.GetChatMemberAsync("@Ieltszoneuzswluchat", userId: message.Chat.Id);

                if (membership != null && membership.Status != ChatMemberStatus.Member && membership.Status != ChatMemberStatus.Administrator && membership.Status != ChatMemberStatus.Creator)
                {
                    await ForceUserToSubscribe();
                }

                else
                {
                    Subscription = true;
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Welcome to InstaSaverBot!🎯\nSend me your Instagram link...🔗⛓️",
                        cancellationToken: cancellationToken);
                }
            }

            else if (message.Text.StartsWith("https://www.instagram.com"))
            {
                if (Subscription == true)
                {
                    try
                    {
                        await botClient.SendVideoAsync(
                           chatId: message.Chat.Id,
                           video: $"{replaceMessage}",
                           supportsStreaming: true,
                           cancellationToken: cancellationToken);
                    }
                    catch (Exception) { }

                    try
                    {
                        await botClient.SendPhotoAsync(
                               chatId: message.Chat.Id,
                               photo: $"{replaceMessage}",
                               cancellationToken: cancellationToken);
                    }
                    catch (Exception) { }
                }
                return;
            }


            async Task ForceUserToSubscribe()
            {
                InlineKeyboardMarkup inlineKeyboard = new(new[]
                      {
                        new []
                        {
                            InlineKeyboardButton.WithUrl(text: "Channel", url: "https://t.me/Ieltszoneuzswluchat"),
                        },
                      }
                );

                await botClient.SendTextMessageAsync(
                chatId: message!.Chat.Id,
                text: "Welcome to InstaSaverBot!🎯\nBefore of all!\nPlease subscribe to the following channel and press /start to start again...",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
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
