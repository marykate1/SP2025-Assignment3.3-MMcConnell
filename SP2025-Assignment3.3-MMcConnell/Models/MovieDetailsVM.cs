namespace SP2025_Assignment3._3_MMcConnell.Models
{
    public class MovieDetailsVM
    {
        public Movie Movie { get; set; }
        public List<RedditComment> RedditComments { get; set; } = new List<RedditComment>();
        public string OverallSentiment { get; set; }
    }
}
