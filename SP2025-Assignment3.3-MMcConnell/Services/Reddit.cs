using System.Text;
using System.Net.Http;
using System.Text.Json;
using SP2025_Assignment3._3_MMcConnell.Models;
using System.Web;
using Microsoft.Data.SqlClient;

namespace SP2025_Assignment3._3_MMcConnell.Services
{
    public class Reddit
    {
        private string _connectionString = "Server=tcp:assignment3marykate3.database.windows.net,1433;Initial Catalog=assignment3marykate3;Persist Security Info=False;User ID=SQLadmin;Password=SQLpassword!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        //// Fetch Reddit comments related to a Movie
        //public static async Task<List<RedditComment>> SearchRedditAsync(Movie movie)
        //{
        //    return await SearchRedditAsync(movie.Title);
        //}

        //// Fetch Reddit comments related to an Actor
        //public static async Task<List<RedditComment>> SearchRedditAsync(Actor actor)
        //{
        //    return await SearchRedditAsync(actor.Name);
        //}
        public static async Task<List<RedditComment>> SearchRedditAsync(dynamic entity)
        {
            string searchQuery = entity is Movie movie ? movie.Title : (entity as Actor)?.Name;
            return await SearchRedditAsync(searchQuery);
        }

        // A shared method for searching Reddit comments based on a query (either movie or actor)
        private static async Task<List<RedditComment>> SearchRedditAsync(string searchQuery)
        {
            List<RedditComment> returnList = new List<RedditComment>();

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                    string url = "https://api.pullpush.io/reddit/search/comment/?size=25&q=" + HttpUtility.UrlEncode(searchQuery);

                    // Fetch the Reddit data
                    string json = await client.GetStringAsync(url);

                    JsonDocument doc = JsonDocument.Parse(json);

                    // Check if "data" array exists
                    if (doc.RootElement.TryGetProperty("data", out JsonElement dataArray))
                    {
                        foreach (JsonElement comment in dataArray.EnumerateArray())
                        {
                            // Check if "body" and "score" exist in the comment
                            if (comment.TryGetProperty("body", out JsonElement bodyElement) && comment.TryGetProperty("score", out JsonElement scoreElement))
                            {
                                string commentText = bodyElement.GetString();
                                double score = scoreElement.GetDouble();

                                if (!string.IsNullOrEmpty(commentText))
                                {
                                    // Truncate the comment if it's too long
                                    commentText = TruncateToMaxLength(commentText, MaxInputLength);

                                    // Determine sentiment (this can be a more advanced approach or external API)
                                    string sentiment = DetermineSentiment(commentText);

                                    // Add the RedditComment to the list
                                    returnList.Add(new RedditComment
                                    {
                                        Comment = commentText,
                                        Sentiment = sentiment,
                                        Score = score
                                    });
                                }
                            }
                        }
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    // Handle any HTTP request exceptions here
                    Console.WriteLine($"Error fetching data from Reddit API: {httpEx.Message}");
                }
                catch (JsonException jsonEx)
                {
                    // Handle any issues with parsing JSON
                    Console.WriteLine($"Error parsing JSON: {jsonEx.Message}");
                }
                catch (Exception ex)
                {
                    // Catch any other unexpected exceptions
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                }
            }

            return returnList;
        }

        // Helper method to truncate comment text to a maximum length
        private static string TruncateToMaxLength(string input, int maxLength)
        {
            if (input.Length > maxLength)
            {
                return input.Substring(0, maxLength);
            }
            return input;
        }

        // A simple sentiment analysis (can be improved with more sophisticated methods)
        private static string DetermineSentiment(string commentText)
        {
            if (commentText.Contains("good") || commentText.Contains("great"))
            {
                return "positive";
            }
            else if (commentText.Contains("bad") || commentText.Contains("horrible"))
            {
                return "negative";
            }
            else
            {
                return "neutral";
            }
        }

        private const int MaxInputLength = 200;  // Example length limit
    }
}
