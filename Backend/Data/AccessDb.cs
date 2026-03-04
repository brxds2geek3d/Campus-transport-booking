namespace Backend.Data
{
    public class AccessDb
    {
        public string ConnectionString { get; }

        public AccessDb(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
