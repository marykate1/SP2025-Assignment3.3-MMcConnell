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

        private static string DetermineSentiment(string commentText)
        {
            
            string normalizedText = commentText.ToLower().Trim();

         
            string[] positiveWords = { "good", "great", "excellent", "amazing", "awesome", "fantastic", "love", "like", "enjoy",
                     "wonderful", "satisfying", "happy", "nice", "positive", "impressive", "brilliant", "perfect",
                     "delightful", "outstanding", "superb", "marvelous", "fabulous", "pleasant", "cheerful",
                     "recommend", "fun", "favorite", "cool", "must", "superior", "engaged", "awesome", "classic",
                     "iconic", "memorable", "fascinating", "absorbing", "remarkable", "impactful", "interesting",
                     "compelling", "masterpiece", "moving", "heartfelt", "intense", "riveting", "stellar",
                     "congratulations", "best", "signed up", "bullrun", "beautiful", "support", "positive impact",
                     "admirable", "loyalty", "fan", "historical", "generous", "yes","love", "loved", "remake","with","and", "wow" };

            string[] negativeWords = { "bad", "horrible", "terrible", "awful", "hate", "dislike", "poor", "worst", "sad",
                     "disappointing", "boring", "unpleasant", "frustrating", "annoying", "dreadful",
                     "mediocre", "lousy", "unhappy", "miserable", "inferior", "pathetic", "atrocious", "abysmal",
                     "overrated", "garbage", "waste", "dishonest", "false", "yawn", "misleading", "overhyped",
                     "forgettable", "dull", "predictable", "underwhelming", "tedious", "uninspired", "flawed",
                     "annoying", "exaggerated", "painful", "displeased", "cringy", "forgot", "redundant",
                     "backwards", "biased", "slave", "propaganda", "extreme", "misguided", "downvoted", "opposed",
                     "confused", "unrealistic", "narrowing", "contradictory", "disillusioned", "no", "don't", "not","didn't" };

            int positiveScore = 0;
            int negativeScore = 0;

            string[] words = normalizedText.Split(new char[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                if (positiveWords.Contains(word))
                {
                    positiveScore++;
                }
                else if (negativeWords.Contains(word))
                {
                    negativeScore++;
                }
            }

            if (positiveScore > negativeScore)
            {
                return "positive";
            }
            else if (negativeScore > positiveScore)
            {
                return "negative";
            }
            else
            {
                return "neutral";
            }

            double OverallSentiment = (positiveScore - negativeScore) / 25.0;

            if (positiveScore > negativeScore)
            {
                return $"positive (Score: {OverallSentiment:F2})";
            }
            else if (negativeScore > positiveScore)
            {
                return $"negative (Score: {OverallSentiment:F2})";
            }
            else
            {
                return $"neutral (Score: {OverallSentiment:F2})";
            }
        }

        private const int MaxInputLength = 200; 
    }
}
