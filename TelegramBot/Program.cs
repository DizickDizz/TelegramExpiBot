using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace TelegramBot
{
    class Program
    {
        private static string token { get; set; } = "5477392401:AAEjX9nJhok_sPIWJqHrzlQ8cxQLvt6DAaI";
        private static TelegramBotClient client;
        private static ReceiverOptions? receiverOptions;
        private static List<UserDescription> Users = new List<UserDescription>();
        private static string DBpaht = $"{Environment.CurrentDirectory}/data.db";
        private static List<String> Gifs = new List<String>();

        static async Task Main(string[] args)
        {
            Init();
            Load();
            Fill(Gifs);
            using var cts = new CancellationTokenSource();
            client = new TelegramBotClient(token);
            client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
                );
            var me = await client.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
            Save();
        }

        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;
            var UserName = message.From.Username;
            var UserId = message.From.Id;
            
            var chatName = message.Chat.Title;
            var flag = false;

            var NeededUser = Users.Find(x => x.UserId == UserId);
            if (NeededUser != null)
            {
                foreach (var user in NeededUser.UserStatistics)
                {
                    if (chatId == user.ChatId)
                    {
                        if (messageText != "/xp" && messageText != "/leaderboard") user.XPGain(messageText);
                        Console.WriteLine($"Received a '{NeededUser.UserStatistics.Last().XP}' message in chat {chatId}.");
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    NeededUser.UserStatistics.Add(new UserStatistic(chatId, chatName));
                    if (messageText != "/xp" && messageText != "/leaderboard") NeededUser.UserStatistics.Last().XPGain(messageText);
                    Console.WriteLine($"Received a '{NeededUser.UserStatistics.Last().XP}' message in chat {chatId}.");
                }
                flag = false;
            }
            else
            {
                var userDescription = new UserDescription(UserName.ToString(), UserId);
                userDescription.AddStatistic(new UserStatistic(chatId, chatName));
                if (messageText != "/xp" && messageText != "/leaderboard") userDescription.UserStatistics.Last().XPGain(messageText);
                Users.Add(userDescription);
                Console.WriteLine($"Received a '{userDescription.UserStatistics.Last().XP}' message in chat {chatId}.");
            }

            //XP
            if (messageText.ToLower() == "/xp")
            {
                NeededUser = Users.Find(x => x.UserId == UserId);
                var NeededStatistic = NeededUser.UserStatistics.Find(x => x.ChatId == chatId);

                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"LVL: {NeededStatistic.LVL} \n XP: {NeededStatistic.XP} \n До следующего уровня осталось {100 - NeededStatistic.XPToNextLVL}% ",
                    cancellationToken: cancellationToken);
            }

            //SendSticker
            if (messageText.ToLower().Contains("/crim"))
            {
                var rnd = new Random();
                var gifNum = rnd.Next(0, 4);
                Message sentMessage = await botClient.SendStickerAsync(
                chatId: chatId,
                sticker: Gifs[gifNum],
                cancellationToken: cancellationToken);
            }

            //LeaderBoard
            if (messageText.ToLower() == "/leaderboard")
            {
                var sb = new StringBuilder();
                var NeededStatistic = new List<UserStatisticWithUserName>();
                foreach (var user in Users)
                {
                    foreach (var userStatistic in user.UserStatistics)
                    {
                        if (userStatistic.ChatId == chatId)
                            NeededStatistic.Add(new UserStatisticWithUserName(user.UserName,chatId,userStatistic.ChatName,userStatistic.XP, userStatistic.LVL));
                    }
                }
                var SortedNeededStatistic = NeededStatistic.OrderByDescending(x => x.XP).Take(3);
        

                foreach (var statistic in SortedNeededStatistic)
                {
                    sb.AppendLine($"{statistic.UserName}, LVL: {statistic.LVL}, XP: {statistic.XP}");
                }
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text:$"🏆{sb.ToString()}",
                    cancellationToken: cancellationToken);
            }

            //TotalRation
            //if (messageText.ToLower() == "/totalrating")
            //{
            //    var sb = new StringBuilder();
            //    var SortedUsers = Users.FindAll(x => x.UserName == UserName.ToString() && x.ChatName != null).OrderByDescending(x => x.ToTalXP);
            //    foreach (var user in SortedUsers)
            //    {
            //        sb.AppendLine($"{user.ChatName}, LVL: {user.LVL}, TotalXP: {user.ToTalXP}");
            //    }

            //    Message sentMessage = await botClient.SendTextMessageAsync(
            //        chatId: chatId,
            //        text: sb.ToString(),
            //        cancellationToken: cancellationToken);
            //}
            Save();
        }

        static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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

        //StickersIdList initializing
        public static void Fill(List<string> list)
        {
            list.Add("CAACAgIAAxkBAAOvYsqY8cK5uhzpazVwjd6TFSCWi3cAAvweAAKku1FKhi5gdPOXJaMpBA");
            list.Add("CAACAgIAAxkBAAO0YsqZ88lLQX5SFqP9iV6q596FsAEAArcbAAKVoFBKcwbMF0dqr38pBA");
            list.Add("CAACAgIAAxkBAAO1YsqaG0l-VVTFz-XNRIW9VGqugkwAAu4cAALnSUhKIOImbwlJ0PcpBA");
            list.Add("CAACAgIAAxkBAAO2YsqagE2FDgOE84dLz59NvICcjRIAAs8eAAJhQFBKRxGYhXudEMEpBA");
        }
        #region DataBase
        public static void Init()
        {
            var db = new SQLiteConnection(DBpaht);
            
            db.CreateTable<UserDescription>();
            db.CreateTable<UserStatistic>();
        }
        public static void Save()
        {
            var db = new SQLiteConnection(DBpaht);

            foreach (var user in Users)
            {
                if (user.Id == 0)
                    db.Insert(user);
                else db.Update(user);
                foreach (var statistic in user.UserStatistics)
                {
                    if (statistic.Id == 0)
                        db.Insert(statistic);
                    else db.Update(statistic);
                }
                db.UpdateWithChildren(user);
            }
        }
        public static void Load()
        {
            var db = new SQLiteConnection(DBpaht);

            Users = db.GetAllWithChildren<UserDescription>();
            foreach (var user in Users)
            {
                foreach (var statistic in user.UserStatistics)
                {
                    statistic.LVLUPCheck();
                }
            }
            

        }

        #endregion
    }
}