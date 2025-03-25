using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BlogA.ModelViews
{
    public class ChapterView
    {
        public int BookId { get; set; }
        public string? BookTitle { get; set; } = null;

        public string? SectionName { get; set; } = null;
        public int SectionId { get; set; }

        //get the chapter collection last chapter's id and add 1
        public int ChapterId { get; set; }

       // display sequence in the list, reading sequence, can be adjusted
        public int Sequence { get; set; }

        public string Heading { get; set; } = string.Empty;

        public List<string>? Content { get; set; } = null;

        public List<string>? AuthorNote { get; set; } = null;
        public bool? AuthorNotePosition { get; set; } = null;

        //to display in chapter list (e.g. characters involve, plot description)
        public string? Sidenote { get; set; } = null;

        public List<int>? CharacterIdList { get; set; } = null;

        public string PostDateString { get; set; } = string.Empty;

        public int WordCount { get; set; }

        public bool Public { get; set; } = true;
    }
}
