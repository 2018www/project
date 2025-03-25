using BlogA.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BlogA.DAL
{
    public class DatabaseConnection
    {

        private readonly DatabaseConfig _dbConfig;
        private readonly IMongoDatabase _database;
      
        public DatabaseConnection(IOptions<DatabaseConfig> options)
        {
            _dbConfig = options.Value;
            var client = new MongoClient(_dbConfig.Url);
            _database = client.GetDatabase(_dbConfig.DB);

        }



        public IMongoCollection<Miscellaneous> Misc => _database.GetCollection<Miscellaneous>(_dbConfig.GeneralCollectionName);

        public IMongoCollection<Book> Book => _database.GetCollection<Book>(_dbConfig.BookCollectionName);

        public IMongoCollection<Comment> Comment => _database.GetCollection<Comment>(_dbConfig.CommentCollectionName);

        public IMongoCollection<Chapter> Chapter => _database.GetCollection<Chapter>(_dbConfig.ChapterCollectionName);

        public IMongoCollection<BsonDocument> Counter => _database.GetCollection<BsonDocument>(_dbConfig.CounterCollectionName);


        public string DbAWord=> _dbConfig.DBAWord;
        public string DbANum=>_dbConfig.DBANum;
        public string DbBWord => _dbConfig.DBBWord;
        public string DbBNum => _dbConfig.DBBNum;
    }
}
