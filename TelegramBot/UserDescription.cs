using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class UserDescription
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string ChatId { get; set; }
        public string ChatName { get; set; }

        
        public double XP { get; set; } = 0;
        public double ToTalXP { get; set; } = 0;
        public int LVL { get; set; } = 1;
        public double TresHold { get; set; } = 100;

        public UserDescription(string userName, string serverName, string userId, string chatName)
        {
            UserName = userName;
            ChatId = serverName;
            UserId = userId;
            ChatName = chatName;
        }
        public UserDescription()
        {

        }

        public void XPGain(string Message)
        {
            if (Message.ToLower() != "/totalrating" && Message.ToLower() != "/xp" && Message.ToLower() != "/leaderboard")
            {
                XP += Message.Length;
                ToTalXP += Message.Length;
                if (XP >= TresHold)
                {
                    LVL++;
                    XP -= TresHold;
                    TresHold = TresHold * 1.5;
                }
            }
        }
    }
}
