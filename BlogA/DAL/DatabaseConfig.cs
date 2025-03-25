namespace BlogA.DAL
{
    public class DatabaseConfig
    {
        public string Url { get; set; } = string.Empty;
        public string DB { get; set; } = string.Empty;

        public string ReaderUrl { get; set; } = string.Empty;
        public string ReaderDB { get; set; } = string.Empty;

        public string BookCollectionName { get; set; } = string.Empty;
        public string CommentCollectionName { get; set; } = string.Empty;
        public string ChapterCollectionName { get; set; } = string.Empty;
        public string CharacterCollectionName { get; set; } = string.Empty;
        public string GeneralCollectionName { get; set; } = string.Empty;

        public string CounterCollectionName { get; set; } = string.Empty;
        public string DBAWord { get; set; } = string.Empty;
        public string DBANum { get; set; } = string.Empty;
        public string DBBWord { get; set; } = string.Empty;
        public string DBBNum { get; set; } = string.Empty;

    }
}
