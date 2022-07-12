using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class UserStatistic
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public Int64 ChatId { get; set; }
        public string ChatName { get; set; }
        public double XP { get; set; } = 0;
        [Ignore]
        public double XPToNextLVL { get; set; } = 0; 
        [Ignore]
        public int LVL { get; set; } = 1;
        [ForeignKey(typeof(UserDescription))]
        public int UserId { get; set; }
        public UserStatistic(Int64 chatId, string chatName)
        {
            ChatId = chatId;
            ChatName = chatName;
        }
        public UserStatistic()
        {

        }
        public void XPGain(string message)
        {
            XP += message.Length;
            LVLUPCheck();
        }
        public void LVLUPCheck()
        {
            if (XP >= LevelFormula(Convert.ToDouble(LVL)))
            {
                LVL++;
                LVLUPCheck();
            }
            XPToNextLVL = Math.Round(XP * 100 / LevelFormula(Convert.ToDouble(LVL)));
        }

        public double LevelFormula(double level)
        {
            if (level <= 100)
            {
                if (level >= 2)
                {
                    return (5000 / 3 * (4 * Math.Pow(level, 3) - 3 * Math.Pow(level, 2) - level) + 1.25 * Math.Pow(1.8, (level - 60)))/250;
                }
                else if (level <= 0 || level == 1) return 1;
            }
            else if (level >= 101)
            {
                return (26931190829 + 100000000000 * (level - 100))/250;
            }
            return 0;
        }
    }
}
