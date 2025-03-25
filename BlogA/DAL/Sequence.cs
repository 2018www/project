
using MongoDB.Bson;
using MongoDB.Driver;


namespace BlogA.DAL
{

    public class Sequence
    {
        private readonly IMongoCollection<BsonDocument> _counterCollection;
        private readonly IMongoCollection<BsonDocument> _counterCollectionReader;


        private string book = "bookId";
        private string comment  = "commentId";
        private string chapter = "chapterId";
        private string topChSeqId = "top_chapter";

        public Sequence(DatabaseConnection dbconnection, ReaderDBConnection readerDBConnection)
        {
            _counterCollection = dbconnection.Counter;
            _counterCollectionReader = readerDBConnection.Counter;
        }

        //get all books sequence
        public async Task<List<int>> GetBookSequence()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", "book_seq");

            var existedSeq = await _counterCollection.Find(filter).FirstOrDefaultAsync();
            List<int> bookSeq = new();

            if (existedSeq != null && existedSeq.Contains("seq"))
            {
                var arrayRaw = existedSeq["seq"].AsBsonArray;
                bookSeq = arrayRaw.Select(x => x.AsInt32).ToList();
            }

            return bookSeq;

        }


        //update book sequence, receive List<int> newSeqList
        public async Task UpdateBookSequence(List<int> newSeq)
        {
            //check if there is duplicated bookId in the list
            var duplicatedId = newSeq.GroupBy(x => x).Where(s => s.Count() > 1).Select(i => i.Key).ToList();

            if (duplicatedId.Any())
            {
                throw new Exception($"Update book sequence failed. Duplicated id in new sequence list: {string.Join(',', duplicatedId)}");
            }

            var filter = Builders<BsonDocument>.Filter.Eq("_id", "book_seq");
            var update = Builders<BsonDocument>.Update.Set("seq", newSeq);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true
            };
            await _counterCollection.FindOneAndUpdateAsync(filter, update, options);

            //update client side 
            await _counterCollectionReader.FindOneAndUpdateAsync(filter, update, options);
        }


        //get one book's all chapters' sequence
        public async Task<Dictionary<int, List<int>>> GetOneBookChapterSequence(int bookId)
        {
            string bookChapSeqId = $"book_{bookId}_chapters";
            var filter = Builders<BsonDocument>.Filter.Eq("_id", bookChapSeqId);

            var existedSeq = await _counterCollection.Find(filter).FirstOrDefaultAsync();
            var result = new Dictionary<int, List<int>>();

            if (existedSeq != null)
            {
                var bsonDoc = existedSeq["seq"].AsBsonDocument;

                foreach (var secSeq in bsonDoc)
                {
                    int key = int.Parse(secSeq.Name);
                    var value = new List<int>();
                    foreach (var num in secSeq.Value.AsBsonArray)
                    {
                        value.Add(num.AsInt32);
                    }

                    result[key] = value;
                }
            }
            return result;
        }

        public async Task UpdateOneBookChapterSequence(int bookId, Dictionary<int, List<int>> newSecChSeq)
        {
            //check if there is duplicated sectionId in the list
            List<int> sectionIdList = newSecChSeq.Keys.ToList();
            var duplicatedId = sectionIdList.GroupBy(x => x).Where(s => s.Count() > 1).Select(i => i.Key).ToList();
            if (duplicatedId.Any())
            {
                throw new Exception($"Update book({bookId}) chapter sequence failed. Duplicated section id in new sequence list: {string.Join(',', duplicatedId)}");
            }

            //check if there is duplicated chapterId in the list
            List<int> chapterIdList = new();
            foreach (var secSeq in newSecChSeq)
            {
                chapterIdList.AddRange(secSeq.Value);
            }

            duplicatedId = chapterIdList.GroupBy(x => x).Where(s => s.Count() > 1).Select(i => i.Key).ToList();
            if (duplicatedId.Any())
            {
                throw new Exception($"Update book({bookId}) chapter sequence failed. Duplicated chapter id in new sequence list: {string.Join(',', duplicatedId)}");
            }


            string bookChapSeqId = $"book_{bookId}_chapters";
            var filter = Builders<BsonDocument>.Filter.Eq("_id", bookChapSeqId);

            var stringKeyDict = newSecChSeq.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value
            );

            var update = Builders<BsonDocument>.Update.Set("seq", stringKeyDict);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true
            };
            await _counterCollection.FindOneAndUpdateAsync(filter, update, options);

            //update client side 
            await _counterCollectionReader.FindOneAndUpdateAsync(filter, update, options);

        }

        public async Task DeleteBookChapterSeqByBookId(int bookId)
        {
            string bookChapSeqId = $"book_{bookId}_chapters";
            var filter = Builders<BsonDocument>.Filter.Eq("_id", bookChapSeqId);

            await _counterCollection.DeleteOneAsync(filter);

            //delete from client side 
            await _counterCollectionReader.DeleteOneAsync(filter);
        }


        //general get id function
        private async Task<int> GetNextId(string counterName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", counterName);
            var update = Builders<BsonDocument>.Update.Inc("seq", 1);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            //only versionB store the comments
            if(counterName==comment)
            {
                var resultCmt = await _counterCollectionReader.FindOneAndUpdateAsync(filter, update, options);
                return resultCmt["seq"].AsInt32;
            }

            var result = await _counterCollection.FindOneAndUpdateAsync(filter, update, options);
            //update client side
            await _counterCollectionReader.FindOneAndUpdateAsync(filter, update, options);

            return result["seq"].AsInt32;

        }

        //rollback id if insert book failed
        private async Task RollBackPreviousId(string counterName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", counterName);
            var update = Builders<BsonDocument>.Update.Inc("seq", -1);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = false
            };

            if (counterName == comment)
            {
                await _counterCollectionReader.FindOneAndUpdateAsync(filter, update, options);
                return;

            }

            await _counterCollection.FindOneAndUpdateAsync(filter, update, options);
            await _counterCollectionReader.FindOneAndUpdateAsync(filter, update, options);
        }

        //top 5 chapters sequence -> seq: [1,2,3,4,5]
        public async Task< List<int>> GetTopFiveChapterSequence()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", topChSeqId);
            var existedSeq = await _counterCollection.Find(filter).FirstOrDefaultAsync();
           List<int> result = new();

            if (existedSeq != null && existedSeq.Contains("seq"))
            {
                var arrayRaw = existedSeq["seq"].AsBsonArray;
                result = arrayRaw.Select(x => x.AsInt32).ToList();
            }

            return result;
        }

        public async Task UpdateTopFiveChapterSequence(List<int> newChSeq)
        {
            //check if there is duplicated chapter in the list
            var duplicatedId = newChSeq.GroupBy(x => x).Where(s => s.Count() > 1).Select(i => i.Key).ToList();

            if (duplicatedId.Any())
            {
                throw new Exception($"Update top 5 chapters sequence failed. Duplicated id in new sequence list: {string.Join(',', duplicatedId)}");
            }
            if (newChSeq.Count > 5)
            {
                throw new Exception($"Update top 5 chapters sequence failed. There are more than 5 chapters in the list.");
            }

            var filter = Builders<BsonDocument>.Filter.Eq("_id", topChSeqId);
            var update = Builders<BsonDocument>.Update.Set("seq", newChSeq);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                IsUpsert = true
            };
            await _counterCollection.FindOneAndUpdateAsync(filter, update, options);
            await _counterCollectionReader.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task<int> GetBookId() { return await GetNextId(book); }
        public async Task<int> GetCommentId() { return await GetNextId(comment); }
        public async Task<int> GetChapterId() { return await GetNextId(chapter); }

        public async Task CancelBookId() { await RollBackPreviousId(book); }
        public async Task CancelCommentId() { await RollBackPreviousId(comment); }
        public async Task CancelChapterId() { await RollBackPreviousId(chapter); }

    }
}
