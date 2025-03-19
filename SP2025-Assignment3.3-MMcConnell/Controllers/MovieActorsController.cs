using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SP2025_Assignment3._3_MMcConnell.Data;
using SP2025_Assignment3._3_MMcConnell.Models;

namespace SP2025_Assignment3._3_MMcConnell.Controllers
{
    public class MovieActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieActorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MovieActors
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.MovieActor
                .Include(a => a.actor)
                .Include(a => a.movie);
            return View(await applicationDbContext.ToListAsync());
        }


        // GET: MovieActors/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var movieActor = await _context.MovieActor
        //        .Include(m => m.actor)
        //        .Include(m => m.movie)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (movieActor == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(movieActor);
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActor = await _context.MovieActor
                .Include(ma => ma.actor)
                .Include(ma => ma.movie)
                .FirstOrDefaultAsync(ma => ma.Id == id);

            if (movieActor == null)
            {
                return NotFound();
            }

            var viewModel = new MovieActorsDetails
            {
                Id = movieActor.Id,
                ActorName = movieActor.actor.Name,
                MovieTitle = movieActor.movie.Title
            };

            return View(viewModel); 
        }



        // GET: MovieActors/Create
        //public IActionResult Create()
        //{
        //    ViewData["ActorID"] = new SelectList(_context.Actors, "Id", "Id");
        //    ViewData["MovieID"] = new SelectList(_context.Movies, "Id", "Id");
        //    return View();
        //}
        public IActionResult Create()
        {
            ViewData["ActorID"] = new SelectList(_context.Actors, "Id", "Name");
            ViewData["MovieID"] = new SelectList(_context.Movies, "Id", "Title");
            return View();
        }

        // POST: MovieActors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,ActorID,MovieID")] MovieActor movieActor)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(movieActor);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["ActorID"] = new SelectList(_context.Actors, "Id", "Id", movieActor.ActorID);
        //    ViewData["MovieID"] = new SelectList(_context.Movies, "Id", "Id", movieActor.MovieID);
        //    return View(movieActor);
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("ActorID,MovieID")] MovieActor movieActor)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(movieActor);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(movieActor);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ActorID,MovieID")] MovieActor movieActor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movieActor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ActorID"] = new SelectList(_context.Actors, "Id", "Name", movieActor.ActorID);
            ViewData["MovieID"] = new SelectList(_context.Movies, "Id", "Title", movieActor.MovieID);
            return View(movieActor);
        }


        // GET: MovieActors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActor = await _context.MovieActor.FindAsync(id);
            if (movieActor == null)
            {
                return NotFound();
            }
            ViewData["ActorID"] = new SelectList(_context.Actors, "Id", "Id", movieActor.ActorID);
            ViewData["MovieID"] = new SelectList(_context.Movies, "Id", "Id", movieActor.MovieID);
            return View(movieActor);
        }

        // POST: MovieActors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ActorID,MovieID")] MovieActor movieActor)
        {
            if (id != movieActor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movieActor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieActorExists(movieActor.Id))
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
            ViewData["ActorID"] = new SelectList(_context.Actors, "Id", "Id", movieActor.ActorID);
            ViewData["MovieID"] = new SelectList(_context.Movies, "Id", "Id", movieActor.MovieID);
            return View(movieActor);
        }

        // GET: MovieActors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActor = await _context.MovieActor
                .Include(m => m.actor)
                .Include(m => m.movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movieActor == null)
            {
                return NotFound();
            }

            return View(movieActor);
        }

        // POST: MovieActors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movieActor = await _context.MovieActor.FindAsync(id);
            if (movieActor != null)
            {
                _context.MovieActor.Remove(movieActor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieActorExists(int id)
        {
            return _context.MovieActor.Any(e => e.Id == id);
        }
    }
}
