using MongoDB.Bson.Serialization.Attributes;


namespace BlogA.Models
{
    public class Section
    {
        public int Id { get; set; } = 1;
        public string Name { get; set; } = "default";

        public int DisplaySequence { get; set; }

        public string Era { get; set; } = string.Empty;

        [BsonIgnoreIfNull]
        public List<int>? MainCharacterId { get; set; } = null;

        [BsonIgnoreIfNull]
        public string? Description { get; set; } = null;

        public bool IsPublic { get; set; } = true;
    }
}
