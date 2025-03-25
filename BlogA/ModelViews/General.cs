

namespace BlogA.ModelViews
{
    public class General
    {
        // for author
        public int AuthorId { get; set; }
        public string? PenName { get; set; } =null;
        
        public List<string>? Intro { get; set; } = null;
        public List<string>? Announcement { get; set; } = null;
        public List<string>? Bio { get; set; } = null;

        public string? UpdateDateString { get; set; } = null;

        //if false,will shut down the reader side project
        public bool IsSafe { get; set; } = true;

        public bool AllowComment { get; set; } = true;

        //will hide the meat content by default, unless the reader check the box 
        public bool CleanMode { get; set; } = true;

        //disable meat content completely 
        public bool ForceClean { get; set; } = false;

        public int CommentWordCount { get; set; } = 500;

        public List<string> EraList { get; set; } = new();
        public List<string> SexualityList { get; set; } = new();
        public List<string> TagList { get; set; } = new();

        public string Email { get; set; } = "abc@abc.com";

        public string Pin { get; set; } = "abc";

  

  
    }
}
