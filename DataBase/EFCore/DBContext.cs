

using DataBase.EFCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace DataBase
{
    public class MySqlDBContext : DbContext
    {
        static MySqlDBContext _instance;
        public static MySqlDBContext Instance { get
            {
                if (_instance == null)
                    _instance = new MySqlDBContext();
                return _instance;
            } }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(@"Server=localhost;Port=3306;Database=Game01; User=game;Password=1024;sslmode=none;");
            Console.WriteLine(12);
        }

        public DbSet<CommonDb> DBPlayer { get; set; }
    }
}
