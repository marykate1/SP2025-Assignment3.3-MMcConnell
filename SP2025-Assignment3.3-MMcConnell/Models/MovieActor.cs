using System.ComponentModel.DataAnnotations.Schema;

namespace SP2025_Assignment3._3_MMcConnell.Models
{
    public class MovieActor
    {
        public int Id { get; set; }

        [ForeignKey("Actor")]
        public int? ActorID { get; set; }
        public Actor? actor { get; set; }

        [ForeignKey("Movie")]
        public int? MovieID { get; set; }
        public Movie? movie { get; set; }
    }
}
