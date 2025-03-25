using BlogA.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BlogA.DAL
{
    public class ReaderDBConnection
    {
        private readonly DatabaseConfig _dbConfig;
        private readonly IMongoDatabase _database;

        public ReaderDBConnection(IOptions<DatabaseConfig> options)
        {
            _dbConfig = options.Value;
            var client = new MongoClient(_dbConfig.ReaderUrl);
            _database = client.GetDatabase(_dbConfig.ReaderDB);

        }

        public IMongoCollection<Miscellaneous> Misc => _database.GetCollection<Miscellaneous>(_dbConfig.GeneralCollectionName);

        public IMongoCollection<Book> Book => _database.GetCollection<Book>(_dbConfig.BookCollectionName);

        public IMongoCollection<Comment> Comment => _database.GetCollection<Comment>(_dbConfig.CommentCollectionName);

        public IMongoCollection<Chapter> Chapter => _database.GetCollection<Chapter>(_dbConfig.ChapterCollectionName);

        public IMongoCollection<BsonDocument> Counter => _database.GetCollection<BsonDocument>(_dbConfig.CounterCollectionName);
    }
}
