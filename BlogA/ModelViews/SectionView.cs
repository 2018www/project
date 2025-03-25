

namespace BlogA.ModelViews
{
    public class SectionView
    {
        //get the last section then add 1
        public int Id { get; set; } = 1;
        public string Name { get; set; } = "default";

        public int DisplaySequence { get; set; }

        public int ChapterCount { get; set; } = 0;

        //if it is the first/default section for the book, the era will be the book's main era
        public string Era { get; set; } = string.Empty;

        public List<int>? MainCharacterId { get; set; } = null;

        public string? Description { get; set; } = null;

        public bool Public { get; set; } = true;
    }
}
