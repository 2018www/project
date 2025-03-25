
namespace BlogA.ModelViews
{
    public class CharacterInfoView
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string FullName_LF => $"{LastName}{FirstName}";
        public string FullName_FL => $"{FirstName} {LastName}";

        //id in the character pool, unique to the book that it belongs to
        public int IdInList { get; set; }

        //is the character only has a firstname, without lastname, then does not matter if it is trure/false because only one item will be displayed
        public bool LastName_Front { get; set; } = true;

        //short description for this character, can be transfered to character profile when created a new character instance
        public List<string>? Description { get; set; } = null;




    }
}
