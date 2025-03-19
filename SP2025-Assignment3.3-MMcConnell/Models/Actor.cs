using System.ComponentModel.DataAnnotations;

namespace SP2025_Assignment3._3_MMcConnell.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender    { get; set; }
        public int Age  { get; set; }
        public string IMDBLink { get; set; }


        [DataType(DataType.Upload)]
        [Display(Name = "Actor Image")]
        public byte[]? ActorImage { get; set; }

        // reddit stuff is going to be under the actorDetailsVM
    }
}
