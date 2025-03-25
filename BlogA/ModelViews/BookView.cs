

namespace BlogA.ModelViews
{
    public class BookView
    {

        public int BookId { get; set; }

        //display sequence at reader side
        public int DisplaySequence { get; set; }

        public string Title { get; set; } = string.Empty;

        public List<string> Introduction { get; set; } = new();


        //each section may have different era, be default, the book's main era will be the first section's era
        public string Era {  get; set; } = string.Empty;

        public string Sexuality { get; set; } = string.Empty;

        public List<string> TagList { get; set; } = new();

        public List<CharacterInfoView>? CharacterPool { get; set; } = null;

        //each section should tag the main character from CharacterList above
        public List<SectionView> SectionList { get; set; } = new();

        public bool Finished { get; set; } = false;
        public bool Public { get; set; } = true;
        public bool Original { get; set; } = true;

        public bool Meat { get; set; } = true;
        public bool AllowComment { get; set; } = true;

        public int MaxCommentLength { get; set; } = 20;
        public int? NewestChapterId { get; set; } = null;

        public int? NewestChapterBelongSecId { get; set; } = null;
        public string? NewestChapterHeading { get; set; } = null;

    }
}

