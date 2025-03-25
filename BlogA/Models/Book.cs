
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogA.Models
{
    public class Book
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        //get the last book_id  then add 1
        [BsonElement("book_id")]
        [BsonRepresentation(BsonType.Int32)]
        public int BookId { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("intro")]
        public List<string> Introduction { get; set; } = new();


        //each section may have different era, be default, the book's main era will be the first section's era
        [BsonElement("main_era")]
        public string Era {  get; set; } = string.Empty;

        //will be the main, or most described in the book, BL, GL, BG, NON-CP, MIX?
        [BsonElement("sexuality")]
        public string Sexuality { get; set; } = string.Empty;

        [BsonElement("tag")]
        public List<string> TagList { get; set; } = new();


        //a list of characters
        [BsonElement("character_List")]
        [BsonIgnoreIfNull]
        public List<CharacterInfo>? CharacterPool { get; set; } = null;

        //each section should tag the main character from CharacterList above
        [BsonElement("section_list")]
        public List<Section> SectionList { get; set; } = new();

        [BsonElement("finished")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool Finished { get; set; } = false;

        [BsonElement("public")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool IsPublic { get; set; } = true;

        [BsonElement("origin")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool IsOriginal { get; set; } = true;

        [BsonElement("has_meat")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool HasMeat { get; set; } = true;

        [BsonElement("can_comment")]
        [BsonRepresentation(BsonType.Boolean)]
        public bool AllowComment { get; set; } = true;


        [BsonElement("max_comment")]
        [BsonRepresentation(BsonType.Int32)]
        public int MaxCommentLength { get; set; } = 20;

        [BsonElement("newest_chapter")]
        [BsonRepresentation(BsonType.Int32)]
        [BsonIgnoreIfNull]
        public int? NewestChapterId { get; set; } = null;

    }
}

