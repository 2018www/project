using BlogA.DAL;
using BlogA.Models;
using MongoDB.Driver;
using BlogA.ModelViews;
using MongoDB.Driver.Linq;

namespace BlogA.Services
{
    public class AuthorService
    {
        private IMongoCollection<Miscellaneous> _generalCollection;
        private IMongoCollection<Miscellaneous> _generalCollectionReader;
        private readonly int _authorId = 000;
        public AuthorService(DatabaseConnection dbConnection, ReaderDBConnection readerDBConnection)
        {
            _generalCollection = dbConnection.Misc;
            _generalCollectionReader = readerDBConnection.Misc;
        }

        //GET all the info 

        public async Task<General> GetAuthorInfo()
        {
            try
            {
                General general = await _generalCollection.AsQueryable()
                    .Where(a => a.AuthorId == _authorId)
                    .Select(x => new General
                    {
                        AuthorId = x.AuthorId,
                        PenName = x.PenName,
                        Intro = x.Introduction,
                        Announcement = x.Announcement,
                        Bio = x.Bio,
                        UpdateDateString = x.UpdateDateString,
                        IsSafe = x.Safety,
                        AllowComment = x.AllowComment,
                        CleanMode = x.CleanMode,
                        ForceClean = x.ForceClean,
                        CommentWordCount = x.CommentWordCount,
                        TagList = x.TagList,
                        EraList = x.EraList,
                        SexualityList = x.SexualityList,
                        Email = x.Email,
                        Pin = x.Pin
                    })
                    .FirstOrDefaultAsync();

                return general;
            }
            catch (Exception ex)
            {
                throw new Exception($"error on getting author information, in Author service: {ex}");
            }
        }


        //Update info - create one if none exists in database
        public async Task UpdateAuthorInfo(General generalInfo)
        {
            try
            {
                var filter = Builders<Miscellaneous>.Filter.Eq(m => m.AuthorId, _authorId);
                var update = Builders<Miscellaneous>.Update
                    .Set(m => m.PenName, generalInfo.PenName)
                    .Set(m => m.Introduction, generalInfo.Intro)
                    .Set(m => m.Announcement, generalInfo.Announcement)
                    .Set(m => m.Bio, generalInfo.Bio)
                    .Set(m => m.UpdateDateString, generalInfo.UpdateDateString)
                    .Set(m => m.Safety, generalInfo.IsSafe)
                    .Set(m=>m.AllowComment, generalInfo.AllowComment)
                    .Set(m => m.CleanMode, generalInfo.CleanMode)
                    .Set(m => m.ForceClean, generalInfo.ForceClean)
                    .Set(m => m.CommentWordCount, generalInfo.CommentWordCount)
                    .Set(m=>m.TagList,generalInfo.TagList)
                    .Set(m=>m.SexualityList,generalInfo.SexualityList)
                    .Set(m=>m.EraList, generalInfo.EraList)
                    .Set(m => m.Email, generalInfo.Email)
                    .Set(m => m.Pin, generalInfo.Pin);

                //do not need the email and pin in reader side becaue it is not sending that info anyway
                var updateReader = Builders<Miscellaneous>.Update
                    .Set(m => m.PenName, generalInfo.PenName)
                    .Set(m => m.Introduction, generalInfo.Intro)
                    .Set(m => m.Announcement, generalInfo.Announcement)
                    .Set(m => m.Bio, generalInfo.Bio)
                     .Set(m => m.SexualityList, generalInfo.SexualityList)
                    .Set(m => m.UpdateDateString, generalInfo.UpdateDateString)
                    .Set(m => m.Safety, generalInfo.IsSafe)
                    .Set(m => m.AllowComment, generalInfo.AllowComment)
                    .Set(m => m.CleanMode, generalInfo.CleanMode)
                    .Set(m => m.ForceClean, generalInfo.ForceClean)
                    .Set(m => m.CommentWordCount, generalInfo.CommentWordCount);

                var result = await _generalCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
                await _generalCollectionReader.UpdateOneAsync(filter, updateReader, new UpdateOptions { IsUpsert = true });
                if (!result.IsAcknowledged)
                {
                    throw new Exception($"error on updating author information, no connection, or no doc found, in update author service ");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"error on updating author information, in Author service: {ex}");
            }
        }

    }
}
