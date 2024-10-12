namespace twiker_backend.Db.Repository
{
    public class DbConnectManager
    {
        public static string? DbConnectionString;

        public static void InitDbConnectionString(bool IsDevMode)
        {
            DotNetEnv.Env.TraversePath().Load();
            if (IsDevMode)
            {
                DbConnectionString = DotNetEnv.Env.GetString("connection_mock");
            }
            else 
            {
                DbConnectionString = DotNetEnv.Env.GetString("connection_string");
            }
        }
    }
}

