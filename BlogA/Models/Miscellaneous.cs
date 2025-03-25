using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;


namespace BlogA.Models
{
    public class Miscellaneous
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("author_id")]
        [BsonRepresentation(BsonType.Int32)]
        public int AuthorId { get; set; } = 000;

        [BsonElement("pen_name")]
        public string? PenName { get; set; } = null;

        [BsonElement("introduction")]
        public List<string>? Introduction { get; set; } = null;

        [BsonElement("announcement")]
        public List<string>? Announcement { get; set; } = null;

        [BsonElement("bio")]
        public List<string>? Bio { get; set; } = null;

        [BsonElement("edited_date")]
        public string? UpdateDateString { get; set; } = null;


        [BsonElement("safety")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool Safety { get; set; } = true;

        [BsonElement("can_comment")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool AllowComment { get; set; } = true;

        //will hide the meat content by default, unless the reader check the box 
        [BsonElement("clean")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool CleanMode { get; set; } = true;

        //disable meat content completely 
        [BsonElement("force_clean")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool ForceClean { get; set; } = false;


        [BsonElement("comment_word_count")]
        [BsonRepresentation(BsonType.Int32)]
        public int CommentWordCount { get; set; } = 500;


        [BsonElement("eras")]
        public List<string> EraList { get; set; } = new();


        [BsonElement("sexualities")]
        public List<string> SexualityList { get; set; } = new();

        [BsonElement("tags")]
        public List<string> TagList { get; set; } = new();


        [BsonElement("email")]
        public string Email { get; set; } = "abc@abc.com";


        [BsonElement("pin")]
        public string Pin { get; set; } = "abc";

    }
}
