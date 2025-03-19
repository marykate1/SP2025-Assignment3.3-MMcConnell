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
            // Normalize and clean the text
            string normalizedText = commentText.ToLower().Trim();

            // Define positive and negative words
            string[] positiveWords = { "good", "great", "excellent", "amazing", "awesome", "fantastic", "love", "like", "enjoy",
                     "wonderful", "satisfying", "happy", "nice", "positive", "impressive", "brilliant", "perfect",
                     "delightful", "outstanding", "superb", "marvelous", "fabulous", "pleasant", "cheerful",
                     "recommend", "fun", "favorite", "cool", "must", "superior", "engaged", "awesome", "classic",
                     "iconic", "memorable", "fascinating", "absorbing", "remarkable", "impactful", "interesting",
                     "compelling", "masterpiece", "moving", "heartfelt", "intense", "riveting", "stellar",
                     "congratulations", "best", "signed up", "bullrun", "beautiful", "support", "positive impact",
                     "admirable", "loyalty", "fan", "historical", "generous" };

            string[] negativeWords = { "bad", "horrible", "terrible", "awful", "hate", "dislike", "poor", "worst", "sad",
                     "disappointing", "boring", "unpleasant", "frustrating", "annoying", "dreadful",
                     "mediocre", "lousy", "unhappy", "miserable", "inferior", "pathetic", "atrocious", "abysmal",
                     "overrated", "garbage", "waste", "dishonest", "false", "yawn", "misleading", "overhyped",
                     "forgettable", "dull", "predictable", "underwhelming", "tedious", "uninspired", "flawed",
                     "annoying", "exaggerated", "painful", "displeased", "cringy", "forgot", "redundant",
                     "backwards", "biased", "slave", "propaganda", "extreme", "misguided", "downvoted", "opposed",
                     "confused", "unrealistic", "narrowing", "contradictory", "disillusioned" };

            int positiveScore = 0;
            int negativeScore = 0;

            // Split comment into words and analyze
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

            // Determine sentiment based on scores
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
        }
      //  private static double CalculateSentimentScore(string commentText)
        //{
        //    string normalizedText = commentText.ToLower().Trim();

        //    string[] positiveWords = { "good", "great", "excellent", "amazing", "awesome", "fantastic", "love", "like", "enjoy",
        //                            "wonderful", "satisfying", "happy", "nice", "positive", "impressive", "brilliant", "perfect",
        //                            "delightful", "outstanding", "superb", "marvelous", "fabulous", "pleasant", "cheerful",
        //                            "recommend", "fun", "favorite", "cool", "must", "superior", "engaged", "iconic", "memorable",
        //                            "fascinating", "absorbing", "remarkable", "impactful", "interesting", "masterpiece", "moving",
        //                            "heartfelt", "intense", "riveting", "stellar" };

        //    string[] negativeWords = { "bad", "horrible", "terrible", "awful", "hate", "dislike", "poor", "worst", "sad",
        //                            "disappointing", "boring", "unpleasant", "frustrating", "annoying", "dreadful",
        //                            "mediocre", "lousy", "unhappy", "miserable", "inferior", "pathetic", "atrocious", "abysmal",
        //                            "overrated", "garbage", "waste", "dishonest", "false", "yawn", "misleading", "overhyped",
        //                            "forgettable", "dull", "predictable", "underwhelming", "tedious", "uninspired", "flawed",
        //                            "cringy", "exaggerated", "painful", "displeased" };

        //    string[] words = normalizedText.Split(new char[] { ' ', '.', ',', '!', '?', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

        //    int positiveScore = 0;
        //    int negativeScore = 0;

        //    foreach (string word in words)
        //    {
        //        if (positiveWords.Contains(word))
        //        {
        //            positiveScore++;
        //        }
        //        else if (negativeWords.Contains(word))
        //        {
        //            negativeScore++;
        //        }
        //    }

        //    // Calculate sentiment score
        //    int totalWords = words.Length;
        //    double sentimentScore = 0;

        //    if (totalWords > 0)
        //    {
        //        sentimentScore = (double)(positiveScore - negativeScore) / totalWords;
        //    }

        //    return Math.Max(-1, Math.Min(1, sentimentScore)); // Keep score between -1 and 1
        //}

        //// Categorize sentiment based on the score
        //public static string CategorizeSentiment(double sentiment)
        //{
        //    if (sentiment >= -1 && sentiment < -0.7)
        //    {
        //        return "Extremely Negative";
        //    }
        //    else if (sentiment >= -0.7 && sentiment < -0.2)
        //    {
        //        return "Very Negative";
        //    }
        //    else if (sentiment >= -0.2 && sentiment < 0)
        //    {
        //        return "Slightly Negative";
        //    }
        //    else if (sentiment >= 0 && sentiment < 0.2)
        //    {
        //        return "Slightly Positive";
        //    }
        //    else if (sentiment >= 0.2 && sentiment < 0.6)
        //    {
        //        return "Very Positive";
        //    }
        //    else if (sentiment >= 0.6 && sentiment <= 1)
        //    {
        //        return "Extremely Positive";
        //    }
        //    else
        //    {
        //        return "Invalid Sentiment Value";
        //    }
        //}

        //// Main method to analyze a comment
        //public static void AnalyzeComment(string comment)
        //{
        //    double sentimentScore = CalculateSentimentScore(comment);
        //    string sentimentCategory = CategorizeSentiment(sentimentScore);

        //    Console.WriteLine($"Comment: \"{comment}\"");
        //    Console.WriteLine($"Sentiment Score: {sentimentScore:F2}");
        //    Console.WriteLine($"Sentiment Category: {sentimentCategory}");
        //}



        private const int MaxInputLength = 200;  // Example length limit
    }
}
