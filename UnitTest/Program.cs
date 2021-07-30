using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace UnitTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MySqlConnection mySqlConn = new MySqlConnection("Server=localhost;Port=3306;Database=Game01; User=game;Password=1024;sslmode=none");
        }
    }
}
