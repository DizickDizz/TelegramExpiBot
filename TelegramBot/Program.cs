using SQLite;
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

        static async Task Main(string[] args)
        {
            Init();
            Load();

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
            var User = message.From.Username;
            var UserId = message.From;
            
            var chatName = message.Chat.Title;


            var NeededUser = Users.Find(x => x.UserId == UserId.ToString() && x.ChatId == chatId.ToString());
            if (NeededUser == null)
            {
                Users.Add(new UserDescription(User.ToString(), chatId.ToString(), UserId.ToString(), chatName?.ToString()));
                Users.Last().XPGain(messageText);
                Console.WriteLine($"Received a '{Users.Last().XP}' message in chat {chatId}.");
            }
            else
            {
                NeededUser.XPGain(messageText);
                Console.WriteLine($"Received a '{NeededUser.XP}' message in chat {chatId}.");
            }

            //XP
            if (messageText.ToLower() == "/xp")
            {
                NeededUser = Users.Find(x => x.UserId == UserId.ToString() && x.ChatId == chatId.ToString());
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"LVL: {NeededUser.LVL} \n XP: {NeededUser.XP} \n До следующего уровня осталось {NeededUser.TresHold-NeededUser.XP} xp",
                    cancellationToken: cancellationToken);
            }

            //SendSticker
            var Gifs = new List<String>();
            Fill(Gifs);
            var rnd = new Random();
            var gifNum = rnd.Next(0, 4);
            if (messageText.ToLower().Contains("/crim"))
            {
                Message sentMessage = await botClient.SendStickerAsync(
                chatId: chatId,
                sticker: Gifs[gifNum],
                cancellationToken: cancellationToken);
            }

            //LeaderBoard
            if (messageText.ToLower() == "/leaderboard")
            {
                var sb = new StringBuilder(); 
                var SortedUsers = Users.FindAll(x => x.ChatId == chatId.ToString()).OrderByDescending(x => x.ToTalXP).Take(3);
                foreach (var user in SortedUsers)
                {
                    sb.AppendLine($"{user.UserName}, LVL: {user.LVL}, TotalXP: {user.ToTalXP}");
                }
                
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text:sb.ToString(),
                    cancellationToken: cancellationToken);
            }

            //TotalRation
            if (messageText.ToLower() == "/totalrating")
            {
                var sb = new StringBuilder();
                var SortedUsers = Users.FindAll(x => x.UserName == User.ToString() && x.ChatName != null).OrderByDescending(x => x.ToTalXP);
                foreach (var user in SortedUsers)
                {
                    sb.AppendLine($"{user.ChatName}, LVL: {user.LVL}, TotalXP: {user.ToTalXP}");
                }

                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: sb.ToString(),
                    cancellationToken: cancellationToken);
            }
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
        }
        public static void Save()
        {
            var db = new SQLiteConnection(DBpaht);

            foreach (var user in Users)
            {
                if (user.Id == 0)
                    db.Insert(user);
                else db.Update(user);
            }
        }
        public static void Load()
        {
            var db = new SQLiteConnection(DBpaht);
            Users = db.Table<UserDescription>().ToList();
        }

        #endregion
    }
}