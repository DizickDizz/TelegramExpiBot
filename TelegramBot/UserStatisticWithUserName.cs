using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class UserStatisticWithUserName
    {
        public readonly string UserName;
        public readonly Int64 ChatId;
        public readonly string ChatName;
        public readonly double XP;
        public readonly int LVL;
        public UserStatisticWithUserName(string userName, long chatId, string chatName, double xP, int lVL)
        {
            UserName = userName;
            ChatId = chatId;
            ChatName = chatName;
            XP = xP;
            LVL = lVL;
        }
    }
}
