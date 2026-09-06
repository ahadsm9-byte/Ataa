using Ataa.Data;
using Ataa.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ataa.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly AtaaDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProjectsController(AtaaDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }



        public async Task<IActionResult> Index()
         {
             var projects = await _context.Projects
                 .OrderByDescending(p => p.Id)
                 .ToListAsync();


             ViewBag.DonationsOpsCount = projects.Count;

             ViewBag.TotalDonationsAmount = projects.Sum(p => p.CurrentAmount);

 
             ViewBag.BeneficiariesCount = projects.Count * 100;

             return View(projects);
         }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        [HttpPost]
        public async Task<IActionResult> Donate(int projectId, string name, string phone, decimal amount)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Phone == phone);
            if (donor == null)
            {
                donor = new Donors { Name = name, Phone = phone };
                _context.Donors.Add(donor);
                await _context.SaveChangesAsync();
            }

            var donation = new Donations
            {
                DonorId = donor.DonorId,
                DonationDate = DateTime.Now, 
                TotalAmount = amount
            };
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            var donationItem = new DonationItems
            {
                DonationId = donation.DonationId,
                ProjectId = projectId,
                Amount = amount
            };
            _context.DonationItems.Add(donationItem);

            project.CurrentAmount += amount;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"شكراً لك يا {name}! تم استلام تبرعك بنجاح.";
            return RedirectToAction("donor_home");
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpGet]
        public IActionResult GetSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Json(new List<string>());

            var suggestions = _context.Projects
                .Where(p => p.Name.StartsWith(term))
                .Select(p => p.Name)
                .Distinct()
                .Take(10)
                .ToList();

            return Json(suggestions);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> donor_home(string searchString)
        {
            ViewBag.TotalDonationsAmount = await _context.Donations.SumAsync(d => d.TotalAmount);
            ViewBag.BeneficiariesCount = (await _context.Donors.CountAsync()) * 5;

            ViewBag.DonationsOpsCount = await _context.Projects.CountAsync(p => p.Status == "Published");

            var query = _context.Projects.Where(p => p.Status == "Published").AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString));
            }

            var projects = await query.OrderByDescending(p => p.Id).ToListAsync();
            return View(projects);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var project = await _context.Projects.FirstOrDefaultAsync(m => m.Id == id);
            if (project == null) return NotFound();
            return View(project);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> prodetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult Cart()
        {
            return View();
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult Payment()
        {
            return View();
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout(string name, string phone, string cartItemsJson)
        {
            var cartItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartItemsJson);
            if (cartItems == null || !cartItems.Any()) return BadRequest();

            var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Phone == phone);
            if (donor == null)
            {
                donor = new Donors { Name = name, Phone = phone };
                _context.Donors.Add(donor);
                await _context.SaveChangesAsync();
            }

            var totalSum = cartItems.Sum(i => i.Amount);
            var donation = new Donations
            {
                DonorId = donor.DonorId,
                DonationDate = DateTime.Now,
                TotalAmount = totalSum
            };
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                var donationItem = new DonationItems
                {
                    DonationId = donation.DonationId,
                    ProjectId = item.ProjectId,
                    Amount = item.Amount
                };
                _context.DonationItems.Add(donationItem);

                var project = await _context.Projects.FindAsync(item.ProjectId);
                if (project != null)
                {
                    project.CurrentAmount += item.Amount;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم تنفيذ جميع التبرعات بنجاح، كتب الله أجرك!";
            return Ok(); 
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult Create()
        {
            return View();
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Projects project)
        {
            if (project.TargetAmount > 0)
            {
                double perc = ((double)project.CurrentAmount / (double)project.TargetAmount) * 100;
                project.CompletionPercentage = (int)Math.Min(perc, 100);
            }
            else
            {
                project.CompletionPercentage = 0;
            }

            project.Date = DateTime.Now;
            if (project.ImageFile != null)
            {
                project.Image = await UploadImage(project.ImageFile);
            }

            _context.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();
            return View(project);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Projects project, string submitButton)
        {
            if (id != project.Id) return NotFound();

            if (!string.IsNullOrEmpty(submitButton))
            {
                project.Status = submitButton; 
            }

            if (project.Status == "Draft")
            {
                ModelState.Clear();
            }
            else
            {
                ModelState.Remove("Status");
                ModelState.Remove("ImageFile");
                ModelState.Remove("CompletionPercentage");
            }

            if (ModelState.IsValid || project.Status == "Draft")
            {
                try
                {
                    if (project.ImageFile != null)
                    {
                        project.Image = await UploadImage(project.ImageFile);
                    }

                    if (project.TargetAmount > 0)
                    {
                        double calculatedPerc = ((double)project.CurrentAmount / (double)project.TargetAmount) * 100;
                    }

                    _context.Update(project);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "تم تحديث بيانات المشروع بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectsExists(project.Id)) return NotFound();
                    else throw;
                }
            }

            return View(project);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private async Task<string> UploadImage(IFormFile file)
        {
            string folder = "images/projects/"; 
            string fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder, fileName);

            string directoryPath = Path.Combine(_webHostEnvironment.WebRootPath, folder);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var fileStream = new FileStream(serverFolder, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/" + folder + fileName;
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var Projects = await _context.Projects.FirstOrDefaultAsync(m => m.Id == id);
            if (Projects == null) return NotFound();
            return View(Projects);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var project = await _context.Projects.FindAsync(id);
                if (project != null)
                {
                    var relatedItems = _context.DonationItems.Where(d => d.ProjectId == id);
                    _context.DonationItems.RemoveRange(relatedItems);

                    _context.Projects.Remove(project);

                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "لا يمكن حذف المشروع لوجود بيانات مرتبطة به: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ProjectsExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}