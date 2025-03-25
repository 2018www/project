using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;


namespace BlogA.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        //get the last comment id then add 1     
        [BsonElement("comment_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]
        public int CommentId { get; set; }

        //if this comment is a sub comment to another comment     
        [BsonElement("main_comment_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]
        public int MainCommentId { get; set; } = -1;

        [BsonElement("book_id")]
        [BsonRepresentation(BsonType.Int32)]
        public int BookId { get; set; }

        [BsonElement("section_id")]
        [BsonRepresentation(BsonType.Int32)]
        public int SectionId { get; set; }

        [BsonElement("chapter_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]
        [BsonIgnoreIfNull]
        public int? ChapterId { get; set; } = null;

        [BsonElement("content")]
        public List<string> Content { get; set; } = new();

        [BsonElement("post_date")]
        public string PostDateString { get; set; } = string.Empty;

        [BsonElement("profile_id")]
        [BsonRepresentation(BsonType.Int32)]
        public int ProfileId { get; set; }

        [BsonElement("username")]
        public string Username { get; set; } = "Reader";

        [BsonElement("viewed")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool IsViewed { get; set; } = false;

        [BsonElement("archived")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool Archived { get; set; } = false;

        [BsonElement("can_comment")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool CanBeCommented { get; set; } = true;

        //max sub comments allowed
        [BsonElement("max_subcomment")]
        [BsonRepresentation(BsonType.Int32)]
        public int MaxSubCommentLength { get; set; } = 10;

        [BsonElement("is_author")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool IsAuthor { get; set; } = false;

    }
}
