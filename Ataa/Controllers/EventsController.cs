using Ataa.Models;
using Ataa.Data;
using Ataa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ataa.Controllers


{
    public class EventsController : Controller
    {
        private readonly AtaaDbContext _context;

        public EventsController(AtaaDbContext context)
        {
            _context = context;
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public async Task<IActionResult> Index(string searchString, string category)
        {
            var volunteerIdStr = HttpContext.Session.GetString("VolunteerId");

            var query = _context.Events.Where(e => e.Status == "Published" || e.Status == "open").AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(e => e.Name.Contains(searchString) ||
                                         e.Location.Contains(searchString) ||
                                         e.Category.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(e => e.Category == category);
            }

            var allEvents = await query.OrderByDescending(e => e.Id).ToListAsync();
            var vm = new VolunteerEventsViewModel();

            if (!string.IsNullOrEmpty(volunteerIdStr) && int.TryParse(volunteerIdStr, out int vId))
            {
                var vSkillIds = await _context.VolunteerSkills.Where(vs => vs.VolunteerID == vId).Select(vs => vs.SkillID).ToListAsync();
                var vInterestIds = await _context.VolunteerInterests.Where(vi => vi.InterestID != null && vi.VolunteerID == vId).Select(vi => vi.InterestID).ToListAsync();

                var matchedIds = await _context.EventSkills.Where(es => vSkillIds.Contains(es.SkillID)).Select(es => es.EventID)
                    .Union(_context.EventInterests.Where(ei => vInterestIds.Contains(ei.InterestID)).Select(ei => ei.EventID))
                    .ToListAsync();

                vm.PreferredEvents = allEvents.Where(e => matchedIds.Contains(e.Id)).ToList();
                vm.OtherEvents = allEvents.Except(vm.PreferredEvents).ToList();
            }
            else
            {
                vm.PreferredEvents = new List<Events>();
                vm.OtherEvents = allEvents;
            }

            return View(vm);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        [HttpGet]
        public IActionResult GetSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Json(new List<string>());

            var suggestions = _context.Events
                .Where(e => e.Name.StartsWith(term))
                .Select(e => e.Name)
                .Distinct()
                .Take(10)
                .ToList();

            return Json(suggestions);
        }



        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


      
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null) return NotFound();

            var vIdStr = HttpContext.Session.GetString("VolunteerId");

            if (!string.IsNullOrEmpty(vIdStr))
            {
                int vId = int.Parse(vIdStr);
                var registration = await _context.EventRegistrations
                    .FirstOrDefaultAsync(r => r.EventId == id && r.VolunteerId == vId);

                ViewBag.IsRegistered = registration != null;
                ViewBag.RegId = registration?.Id;
            }
            else
            {
                ViewBag.IsRegistered = false;
            }

            return View(@event);
        }



        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public async Task<IActionResult> Register(int eventId)
        {
            var vIdStr = HttpContext.Session.GetString("VolunteerId");

            if (string.IsNullOrEmpty(vIdStr))
            {
                return RedirectToAction("Login", "Users");
            }

            int volunteerId = int.Parse(vIdStr);

            var alreadyRegistered = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == eventId && r.VolunteerId == volunteerId);

            if (alreadyRegistered)
            {
                return RedirectToAction("Details", new { id = eventId });
            }

            var registration = new EventRegistrations
            {
                EventId = eventId,
                VolunteerId = volunteerId,
                AttendanceStatus = "لم يحضر",
                Status = "مؤكد",
                RegistrationDate = DateTime.Now
            };

            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = eventId });
        }



        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public async Task<IActionResult> CancelRegistration(int regId)
        {
            var registration = await _context.EventRegistrations.FindAsync(regId);
            if (registration == null) return NotFound();

            int eventId = registration.EventId;

            _context.EventRegistrations.Remove(registration);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = eventId });
        }


        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public async Task<IActionResult> ViewTicket(int id)
        {
            var registration = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.Id == id); //Id أو RegistrationID؟

            if (registration == null) return NotFound();
 
            var ev = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == registration.EventId);

          
            var volunteer = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == registration.VolunteerId);

            if (ev == null || volunteer == null) return NotFound();

            ViewBag.EventName = ev.Name;
            ViewBag.VolunteerName = volunteer.Name;
            ViewBag.EventDate = ev.Date;
            ViewBag.EventLocation = ev.Location;

            return View(registration);
        }


        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult Scanner()
        {
            return View();
        }
        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpGet]
        public async Task<IActionResult> GetVolunteerData(int regId, int eventId)
        {
            var registration = await _context.EventRegistrations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.VolunteerId == regId && r.EventId == eventId);

            if (registration == null)
            {
                registration = await _context.EventRegistrations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == regId && r.EventId == eventId);
            }

            if (registration == null)
                return NotFound(new { message = "هذا المتطوع غير مسجل في هذه الفعالية" });

            var volunteer = await _context.Volunteers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VolunteerID == registration.VolunteerId);

            if (volunteer == null)
                return NotFound(new { message = "بيانات المتطوع الشخصية غير موجودة" });

            return Json(new
            {
                name = volunteer.Name,
                regId = registration.Id,
                attendanceStatus = registration.AttendanceStatus
            });
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public async Task<IActionResult> ConfirmAttendance([FromBody] AttendanceConfirmModel model)
        {
            var registration = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.Id == model.RegId);

            if (registration == null)
                return BadRequest(new { message = "تسجيل غير موجود" });

            if (registration.AttendanceStatus == "تم الحضور")
                return BadRequest(new { message = "تم تسجيل الحضور مسبقاً" });

            registration.AttendanceStatus = "تم الحضور";

            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تسجيل الحضور بنجاح" });
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public class AttendanceConfirmModel
        {
            public int RegId { get; set; }
        }



        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public IActionResult EmployeeEventsIndex()
        {
            var events = _context.Events.OrderByDescending(e => e.Date).ToList();
            return View(events);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> allEvent()
        {
            var completedReports = await _context.Events
        .Where(e => e.Status == "Closed" && !string.IsNullOrEmpty(e.GoalsReport))
        .ToListAsync();

            return View(completedReports);
        }

        public async Task<IActionResult> SuccessStory(int id)
        {
         
            var ev = await _context.Events
                .Include(e => e.EventRegistrations)
                    .ThenInclude(r => r.Volunteer) 
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound();

            return View(ev);
        }


        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        public async Task<IActionResult> emIndex(string searchString, string category)
        {

            var eventsQuery = from e in _context.Events
                              where e.Status != "Closed"  
                              select e;

            if (!string.IsNullOrEmpty(searchString))
            {
                eventsQuery = eventsQuery.Where(s => s.Name.Contains(searchString)
                                                || s.Location.Contains(searchString)
                                                || s.Category.Contains(searchString));

                 
                ViewData["CurrentFilter"] = searchString;
            }

          
            if (!string.IsNullOrEmpty(category))
            {
                eventsQuery = eventsQuery.Where(x => x.Category == category);
            }

            
            eventsQuery = eventsQuery.OrderByDescending(e => e.Id);

             
            return View(await eventsQuery.ToListAsync());
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public async Task<IActionResult> emDetails(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var events = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (events == null)
            {
                return NotFound();
            }

            return View(events);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var events = await _context.Events.FindAsync(id);
            if (events == null) return NotFound();

            return View(events);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Events events, IFormFile? ImageFile)
        {
            if (id != events.Id) return Json(new { success = false, message = "ID mismatch" });

            ModelState.Remove("Image");
            ModelState.Remove("ImageFile"); 

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }
                        events.Image = "/images/" + fileName;
                    }
                    else
                    {
                        
                        _context.Entry(events).Property(x => x.Image).IsModified = false;
                    }


                    _context.Update(events);
                    await _context.SaveChangesAsync();
                    if (events.Status == "Published")
                    {
                        await SendUpdateNotifications(events.Id, events.Name);
                    }
                   
                    return Ok();
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(string.Join(", ", errors));
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task SendUpdateNotifications(int eventId, string eventName)
        {
            var volunteerIds = await _context.EventRegistrations
                .Where(r => r.EventId == eventId)
                .Select(r => r.VolunteerId) 
                .ToListAsync();

            if (volunteerIds.Any())
            {
                foreach (var vId in volunteerIds)
                {
                    var notification = new Notifications
                    {
                        VolunteerId = vId, 
                        Message = $"تحديث: تم تعديل بيانات الفعالية ({eventName}). يرجى مراجعة التفاصيل.",
                        CreatedDate = DateTime.Now, 
                        IsRead = false 
                    };
                    _context.Notifications.Add(notification);
                }
                await _context.SaveChangesAsync();
            }
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var events = await _context.Events.FirstOrDefaultAsync(m => m.Id == id);
            if (events == null) return NotFound();

            return View(events);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM EventRegistrations WHERE EventId = {0}", id);

                await _context.Database.ExecuteSqlRawAsync("DELETE FROM EventSkills WHERE EventID = {0}", id);

                await _context.Database.ExecuteSqlRawAsync("DELETE FROM EventInterests WHERE EventID = {0}", id);

                var @event = await _context.Events.FindAsync(id);
                if (@event != null)
                {
                    _context.Events.Remove(@event);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(emIndex));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "لا يزال هناك جدول مرتبط: " + ex.Message;
                return RedirectToAction(nameof(emIndex));
            }
        }


        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Events events, IFormFile? ImageFile)
        {
            ModelState.Remove("Image");

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    events.Image = "/images/" + fileName;
                }

                events.Status = "open";
                _context.Add(events);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(emIndex));
            }

            return View(events);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult CreateEvent()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreateEvent(Events ev, string[] skills, string[] interests, IFormFile imageFile)
        {
            ModelState.Remove("Status");
            ModelState.Remove("Image");
            ModelState.Remove("OrganizingEntity");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                ViewBag.ErrorMessage = "يرجى ملء الحقول المطلوبة: " + string.Join(", ", errors);
                return View(ev);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        ev.Image = "/images/" + fileName;
                    }

                    ev.Status = "open";
                    if (string.IsNullOrEmpty(ev.OrganizingEntity)) ev.OrganizingEntity = "الجهة المنظمة";

                    _context.Events.Add(ev);
                    await _context.SaveChangesAsync();

                    if (skills != null)
                    {
                        foreach (var sName in skills)
                        {
                            var skill = await _context.Skills.FirstOrDefaultAsync(s => s.SkillName == sName.Trim());
                            if (skill != null)
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    "INSERT INTO EventSkills (EventID, SkillID) VALUES ({0}, {1})",
                                    ev.Id, skill.SkillID);
                            }
                        }
                    }

                    if (interests != null)
                    {
                        foreach (var iName in interests)
                        {
                            var interest = await _context.Interests.FirstOrDefaultAsync(i => i.InterestName == iName.Trim());
                            if (interest != null)
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    "INSERT INTO EventInterests (EventID, InterestID) VALUES ({0}, {1})",
                                    ev.Id, interest.InterestID);
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    return RedirectToAction("emIndex");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ViewBag.ErrorMessage = "خطأ في قاعدة البيانات: " + (ex.InnerException?.Message ?? ex.Message);
                    return View(ev);
                }
            }
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        [HttpPost]
        public async Task<IActionResult> CloseEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                @event.Status = "Closed"; 
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(emIndex));
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        [HttpPost]
        public async Task<IActionResult> SaveReport(int id, string ExecutionMechanism, string GoalsReport, List<IFormFile> ExtraImages)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            ev.ExecutionMechanism = ExecutionMechanism;
            ev.GoalsReport = GoalsReport;
            ev.Status = "Closed";

            if (ExtraImages != null && ExtraImages.Count > 0)
            {
                List<string> imagePaths = new List<string>();
                foreach (var file in ExtraImages)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    imagePaths.Add("/images/" + fileName);
                }
                ev.ExtraImages = string.Join(",", imagePaths); 
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(FinishedEvents));
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult AddReport()
        {
            return View();
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public async Task<IActionResult> CloseAndReport(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            return View(ev);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        [HttpPost]
        public async Task<IActionResult> SubmitFinalReport(int id, string GoalsReport, string ExecutionMechanism, List<IFormFile> NewImages)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            ev.GoalsReport = GoalsReport;
            ev.ExecutionMechanism = ExecutionMechanism;
            ev.Status = "Closed"; 

            if (NewImages != null && NewImages.Count > 0)
            {
                List<string> paths = new List<string>();
                foreach (var file in NewImages)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    paths.Add("/images/" + fileName);
                }
                ev.ExtraImages = string.Join(",", paths); 
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("FinishedEvents"); 
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public async Task<IActionResult> FinishedEvents()
        {
            var finished = await _context.Events.Where(e => e.Status == "Closed").ToListAsync();
            return View(finished);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult SendNotification()
        {
            var events = _context.Events.OrderByDescending(e => e.Date).ToList();
            return View(events);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public IActionResult GetVolunteersForEvent(int eventId)
        {
            var registrations = _context.EventRegistrations
                .Include(r => r.Volunteer)
                .Where(r => r.EventId == eventId)
                .ToList();

            if (!registrations.Any())
                return Content("<p class='text-center text-muted'>لا يوجد متطوعون مسجلون حالياً.</p>");

            string html = "";
            foreach (var reg in registrations)
            {
                if (reg.Volunteer != null)
                {
                    html += $"<div class='vol-item shadow-sm p-2 mb-2 rounded'><i class='fas fa-user-circle me-2 text-success'></i> {reg.Volunteer.Name}</div>";
                }
            }
            return Content(html);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public async Task<IActionResult> SendToAll(string message)
        {
            if (string.IsNullOrEmpty(message)) return RedirectToAction("SendNotification");

            var allVolunteers = _context.Volunteers.ToList();
            foreach (var v in allVolunteers)
            {
                _context.Notifications.Add(new Notifications 
                {
                    VolunteerId = v.VolunteerID,
                    Message = message,
                    CreatedDate = DateTime.Now,
                    IsRead = false
                });
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إرسال الإشعار لجميع المتطوعين بنجاح";
            return RedirectToAction("SendNotification");
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public async Task<IActionResult> SendToEventVolunteers(int eventId, string message)
        {
            if (string.IsNullOrEmpty(message)) return RedirectToAction("SendNotification");

            var eventVolunteerIds = _context.EventRegistrations
                .Where(r => r.EventId == eventId)
                .Select(r => r.VolunteerId)
                .ToList();

            foreach (var vId in eventVolunteerIds)
            {
                _context.Notifications.Add(new Notifications
                {
                    VolunteerId = vId,
                    Message = message,
                    CreatedDate = DateTime.Now,
                    IsRead = false
                });
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم إرسال الإشعار لمتطوعي الفعالية بنجاح";
            return RedirectToAction("SendNotification");
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> GenerateCertificate(int registrationId)
        {
            var registration = await _context.EventRegistrations
                .Include(r => r.Event)
                .Include(r => r.Volunteer)
                .FirstOrDefaultAsync(r => r.Id == registrationId);

            if (registration == null)
            {
                return NotFound();
            }

    
            var model = new CertificateViewModel
            {
                VolunteerName = registration.Volunteer.Name,
                EventName = registration.Event.Name,
                Hours = registration.Event.Hours,
                IssueDate = DateTime.Now
            };

            return View(model);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult ViewVolunteers(int id)
        {
            var registrations = _context.EventRegistrations
                .Include(r => r.Volunteer)
                .Where(r => r.EventId == id)
                .ToList();

            var currentEvent = _context.Events.Find(id);

            ViewData["Title"] = "متطوعي " + (currentEvent?.Name ?? "الفعالية");

            ViewBag.EventName = currentEvent?.Name;
            ViewBag.EventId = id;

            ViewBag.IssuedCertificates = _context.Certificates
                .Where(c => c.RegistrationId != 0)
                .Select(c => c.RegistrationId)
                .ToList();

            return View(registrations);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public IActionResult IssueCertificates(int eventId, string supervisorName, string AddSignature)
        {
            if (string.IsNullOrEmpty(AddSignature))
            {
                TempData["Error"] = "يرجى إضافة التوقيع أولاً";
                return RedirectToAction("ViewVolunteers", new { id = eventId });
            }

            var attendees = _context.EventRegistrations
                .Where(r => r.EventId == eventId)
                .ToList();

            if (!attendees.Any())
            {
                TempData["Error"] = "لا يوجد متطوعين لهذه الفعالية";
                return RedirectToAction("ViewVolunteers", new { id = eventId });
            }

            foreach (var reg in attendees)
            {
                bool exists = _context.Certificates
                    .Any(c => c.RegistrationId == reg.Id);

                if (!exists)
                {
                    var cert = new Certificates
                    {
                        RegistrationId = reg.Id,
                        SupervisorName = supervisorName,
                        AddSignature = AddSignature,
                        IssueDate = DateTime.Now
                    };

                    _context.Certificates.Add(cert);
                }
            }

            _context.SaveChanges();

            TempData["Success"] = "تم إصدار الشهادات بنجاح";

            return RedirectToAction("ViewVolunteers", new { id = eventId });
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> Rating()
        {
            var events = await _context.Events.ToListAsync();

            var registrations = await _context.EventRegistrations
                .Include(r => r.Volunteer)
                .Include(r => r.Event)
                .ToListAsync();

            ViewBag.AllRegs = registrations;

            return View(events);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public async Task<IActionResult> ViewEventRatings(int id)
        {
            var registrations = await _context.EventRegistrations
                .Include(r => r.Volunteer)
                .Include(r => r.Event)
                .Where(r => r.EventId == id) 
                .ToListAsync();

            var selectedEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            ViewBag.EventName = selectedEvent?.Name;

            return View(registrations);
        }

    }
}











