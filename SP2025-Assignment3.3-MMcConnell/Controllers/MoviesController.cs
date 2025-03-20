using System;
using System.Collections.Generic;
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
using Microsoft.Data.SqlClient;
using System.Numerics;



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
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var redditComments = await Services.Reddit.SearchRedditAsync(movie);

            var overallSentiment = redditComments.Any()
                ? redditComments.Average(c => c.Score).ToString("F2")
                : "No comments";

            var movieDetailsVM = new MovieDetailsVM
            {
                Movie = movie,
                RedditComments = redditComments,
                OverallSentiment = overallSentiment
            };

            return View(movieDetailsVM);
        }

        //    // GET: Movies/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // from mia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Genre,Year,IMDBLink")] Movie movie, IFormFile MovieImage)
        {
            // removes the image since it is uploaded seperatly from bind
            ModelState.Remove(nameof(movie.MovieImage));

            // check to see if data is filled 
            if (ModelState.IsValid)
            {
                // checks if there is an image 
                if (MovieImage != null && MovieImage.Length > 0)
                {
                    //converts image into byte array
                    var memoryStream = new MemoryStream();
                    await MovieImage.CopyToAsync(memoryStream);
                    movie.MovieImage = memoryStream.ToArray();
                }
                else
                {
                    // if there is no image it just uploads 0
                    movie.MovieImage = new byte[0];
                }

                // adds actor to database
                _context.Add(movie);
                // saves changes
                await _context.SaveChangesAsync();
                // returns to home page 
                return RedirectToAction(nameof(Index));
            }
            // if save fails the form is reloaded 
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
                            movie.MovieImage = memoryStream.ToArray(); 
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
