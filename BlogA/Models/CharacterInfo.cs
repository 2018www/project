
namespace BlogA.Models
{
    public class CharacterInfo
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string FullName_LF => $"{LastName}{FirstName}";
        public string FullName_FL => $"{FirstName} {LastName}";

        public int IdInList { get; set; }


        public bool LastName_Front { get; set; } = true;

        public List<string>? Description { get; set; } = null;



    }
}
