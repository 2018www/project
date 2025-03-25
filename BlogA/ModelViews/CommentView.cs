

namespace BlogA.ModelViews
{
    public class CommentView
    {
        public int CommentId { get; set; }

        //if this comment is a sub comment to another comment     
        public int MainCommentId { get; set; } = -1;

        public int BookId { get; set; }

        public string? BookTitle { get; set; } = null;

        public int SectionId { get; set; }

        public string? SectionName { get; set; } = null;

        public int? ChapterId { get; set; } = null;

        public string? ChapterHeading { get; set; } = null;

        public List<string> Content { get; set; } = new();

        public string PostDateString { get; set; } = string.Empty;

        public int ProfileId { get; set; }

        public string Username { get; set; } = "Reader";

        public bool IsViewed { get; set; } = false;

        public bool Archived { get; set; } = false;

        public bool CanBeCommented { get; set; } = true;

        public int MaxSubCommentLength { get; set; } = 10;
        public bool IsAuthor { get; set; } = false;
    }
}
