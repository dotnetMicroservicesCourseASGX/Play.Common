namespace Play.Common.Settings
{
    public class MongoDbSettings
    {
        public string Host { get; init; }

        public int Port { get; init; }

        private string connectionString;
        public string ConnectionString
        {
            get { return string.IsNullOrWhiteSpace(connectionString) ? $"mongodb://{Host}:{Port}" : connectionString; }
            init { connectionString = value; }
        }

    }
}