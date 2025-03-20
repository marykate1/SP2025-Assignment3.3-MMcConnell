using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP2025_Assignment3._3_MMcConnell.Data;
using SP2025_Assignment3._3_MMcConnell.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


namespace SP2025_Assignment3._3_MMcConnell.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Fetch the movie from the database
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            // Get Reddit comments for the movie
            var redditComments = await Services.Reddit.SearchRedditAsync(movie);

            // Calculate the overall sentiment or score from the comments.
            var overallSentiment = redditComments.Any()
                ? redditComments.Average(c => c.Score).ToString("F2")
                : "No comments";

            // Prepare the view model
            var movieDetailsVM = new MovieDetailsVM
            {
                Movie = movie,
                RedditComments = redditComments,
                OverallSentiment = overallSentiment
            };

            // Return the view with the populated view model
            return View(movieDetailsVM);
        }

        // GET: Movies/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Genre,Year,IMDBlink")] Movie movie, IFormFile MovieImage)
        {
            ViewData["MovieImageError"] = ""; // Reset the error message

            // Handle image upload and resizing
            if (MovieImage != null && MovieImage.Length > 0)
            {
                try
                {
                    using var memoryStream = new MemoryStream(); // Initialize memoryStream inside the try block
                    await MovieImage.CopyToAsync(memoryStream);

                    // Resize the image
                    using var originalImage = Image.FromStream(memoryStream);
                    int newHeight = 250;
                    int newWidth = (int)((double)originalImage.Width / originalImage.Height * newHeight);

                    using var resizedImage = new Bitmap(originalImage, newWidth, newHeight);
                    using var outputMemoryStream = new MemoryStream();
                    resizedImage.Save(outputMemoryStream, ImageFormat.Jpeg); // Save as JPEG
                    movie.MovieImage = outputMemoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    // Handle any exception that occurs during image resizing
                    ViewData["MovieImageError"] = "Error resizing image: " + ex.Message;
                    movie.MovieImage = null; // Optionally set the movie image to null if resizing fails
                }
            }
            else
            {
                movie.MovieImage = new byte[0]; // If no image is uploaded, set to empty byte array
            }
            //       _logger.LogInformation("Movie Data: Title: {Title}, Genre: {Genre}, Year: {Year}", movie.Title, movie.Genre, movie.Year);


//            Check if the model is valid
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                try
                {
                    await _context.SaveChangesAsync();
    }
                catch (Exception ex)
                {
        //            _logger.LogError("Error saving changes to database: " + ex.Message);
                    ViewData["DatabaseError"] = "Error saving movie to database. Please try again.";
                }

            return RedirectToAction(nameof(Index)); // Redirect to Index after successful creation
            }

            // Return the view if validation fails
            return View(movie);
        }


        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,Year,IMDBlink,MovieImage")] Movie movie, IFormFile MovieImage)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (MovieImage != null && MovieImage.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await MovieImage.CopyToAsync(memoryStream);
                            movie.MovieImage = memoryStream.ToArray(); // Update the movie image as byte array
                        }
                    }

                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
        public async Task<IActionResult> GetMovieImage(int id)
        {
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();

            }
            return File(movie.MovieImage, "image/jpg");


        }
    }
}
