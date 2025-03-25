using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BlogA.Models
{
    public class Chapter
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; } 

        [BsonElement("book_id")]
        [BsonRepresentation(BsonType.Int32)]
        public int BookId { get; set; }

        [BsonElement("section_id")]
        [BsonRepresentation(BsonType.Int32)]
        public int SectionId { get; set; }

        [BsonElement("chapter_id")]
        [BsonRepresentation(BsonType.Int32)]
        public int ChapterId { get; set; }


        [BsonElement("heading")]
        public string Heading { get; set; } = string.Empty;

        [BsonElement("content")]
        public List<string> Content { get; set; } = new();

        //display inside the actual chapter page
        [BsonElement("author_note")]
        [BsonIgnoreIfNull]
        public List<string>? AuthorNote { get; set; } = null;

        //determine if authorNote is top or bottom
        [BsonElement("author_note_place")]
        [BsonIgnoreIfNull]
        public bool? AuthorNotePosition { get; set; } = null;

        //to display in chapter list (e.g. characters involve, plot description)
        [BsonElement("sidenote")]
        [BsonIgnoreIfNull]
        public string? Sidenote { get; set; } = null;


        [BsonElement("character_involved")]
        [BsonIgnoreIfNull]
        public List<int>? CharacterIdList { get; set; } = null;


        [BsonElement("post_date")]
        public string PostDateString { get; set; } = string.Empty;

        [BsonElement("word_count")]
        [BsonRepresentation(BsonType.Int32)]
        public int WordCount { get; set; }

        [BsonElement("public")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool IsPublic { get; set; } = true;
    }
}
 