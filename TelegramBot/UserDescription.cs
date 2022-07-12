using SQLite;
using SQLiteNetExtensions.Attributes;
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
        public Int64 UserId { get; set; }
        [OneToMany]
        public List<UserStatistic> UserStatistics { get; set; } = new();

        public UserDescription(string userName, Int64 userId)
        {
            UserName = userName;
            UserId = userId;
        }

        public UserDescription()
        {

        }
        public void AddStatistic(UserStatistic userStatistic)
        {
            UserStatistics.Add(userStatistic);
        }
    }
}
