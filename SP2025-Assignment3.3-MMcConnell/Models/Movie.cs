using System.ComponentModel.DataAnnotations;

namespace SP2025_Assignment3._3_MMcConnell.Models
{
    public class Movie
    {
        public int Id { get; set; }
            public string Title { get; set; }

           
            public string Genre { get; set; }
            public int Year { get; set; }

            public string IMDBlink { get; set; }

            [DataType(DataType.Upload)]
            [Display(Name = "Movie Image")]
            public byte[]? MovieImage { get; set; }

        // reddit stuff is going to be under the movieDetailsVM
    }
}
