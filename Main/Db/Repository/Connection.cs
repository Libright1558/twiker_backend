namespace twiker_backend.Db.Repository
{
    public class DbConnectManager
    {
        private static string? DbConnectionString;

        public static string InitDbConnectionString()
        {
            DotNetEnv.Env.TraversePath().Load();
            DbConnectionString = DotNetEnv.Env.GetString("connection_string");
            return DbConnectionString;
        }
    }
}

