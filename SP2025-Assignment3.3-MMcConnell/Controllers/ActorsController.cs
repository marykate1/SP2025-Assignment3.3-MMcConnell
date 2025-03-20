﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SP2025_Assignment3._3_MMcConnell.Data;
using SP2025_Assignment3._3_MMcConnell.Models;
using SP2025_Assignment3._3_MMcConnell.Services;

namespace SP2025_Assignment3._3_MMcConnell.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

       
        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var actor = await _context.Actors
                .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            var redditComments = await Services.Reddit.SearchRedditAsync(actor);

            var overallSentiment = redditComments.Any()
                ? redditComments.Average(c => c.Score).ToString("F2")
                : "No comments";

            var actorDetailsVM = new ActorDetailsVM
            {
                Actor = actor,
                RedditComments = redditComments,
                OverallSentiment = overallSentiment
            };

            return View(actorDetailsVM);
        }




        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,IMDBLink,ActorImage")] Actor actor, IFormFile ActorImage)
        {
            ViewData["ActorImageError"] = "";
            //  ModelState.Remove(nameof(actor.ActorImage));
            
            if (ModelState.IsValid)
            {
                if (ActorImage != null && ActorImage.Length > 0) 
                {
                    using var memoryStream = new MemoryStream();
                    await ActorImage.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;  // Reset position before reading the stream

                    try
                    {
                        using var orginialImage = Image.FromStream(memoryStream);
                        int newHeight = 250;
                        int newWidth = (int)((double)orginialImage.Width / orginialImage.Height * newHeight);

                        using var resizedImage = new Bitmap(orginialImage, newWidth, newHeight);
                        using var outputMemoryStream = new MemoryStream();
                        resizedImage.Save(outputMemoryStream, ImageFormat.Jpeg);

                        actor.ActorImage = outputMemoryStream.ToArray();
                    }
                    catch (Exception ex) {
                        ViewData["ActorImageError"] = "Invalid image uploaded. Data was not saved.";
                        return View();
                    }
                } else
                {
                    actor.ActorImage = new byte[0];
                }

                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }


            //if (ModelState.IsValid)
            //{
            //    if (ActorImage != null && ActorImage.Length > 0)
            //    {
            //        using (var memoryStream = new MemoryStream())
            //        {
            //            await ActorImage.CopyToAsync(memoryStream);
            //            actor.ActorImage = memoryStream.ToArray();
            //        }
            //    }

            //    _context.Add(actor);
            //    await _context.SaveChangesAsync();
            //    return RedirectToAction(nameof(Index));
            //}
            //return View(actor);
        
        public async Task<IActionResult> GetActorPhoto(int id)
        {
            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }
            return File(actor.ActorImage, "image/jpg");
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,IMDBLink,ActorImage")] Actor actor)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
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
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actors.Any(e => e.Id == id);
        }
        public async Task<IActionResult> GetActorImage(int id)
        {
            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();

            }
            return File(actor.ActorImage, "image/jpg");


        }
    }
}
