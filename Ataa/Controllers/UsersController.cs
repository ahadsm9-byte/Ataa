using Ataa.Data;
using Ataa.Models;
using Ataa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Ataa.Controllers
{
    public class UsersController : Controller
    {
        private readonly AtaaDbContext _context;

        public UsersController(AtaaDbContext context)
        {
            _context = context;
        }



        public IActionResult home()
        {
            var totalDonations = _context.Donations.Any()
                                 ? _context.Donations.Sum(d => d.TotalAmount)
                                 : 0;

            var projectsCount = _context.Projects.Count();

            var volunteersCount = _context.Users.Count(u => u.Role == "volunteer");

            var donorsCount = _context.Donors.Count();

            var statsModel = new List<dynamic>
    {
        new { Title = "المشاريع", Value = projectsCount, Color = "#2d7a45" },
        new { Title = "المتطوعين", Value = volunteersCount, Color = "#1f5c34" },
        new { Title = "المتبرعين", Value = donorsCount, Color = "#0f3d22" },
        new { Title = "التبرعات", Value = (int)totalDonations, Color = "#4fa66a" }
    };

            return View(statsModel);
        }
        /// //////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult Login()
        {

            if (!HttpContext.Request.Cookies.ContainsKey("Name"))
                return View();
            else
            {
                string na = HttpContext.Request.Cookies["Name"].ToString();
                string ro = HttpContext.Request.Cookies["Role"].ToString();

                HttpContext.Session.SetString("Name", na);
                HttpContext.Session.SetString("Role", ro);


                string roleLower = ro.ToLower();
                if (roleLower == "admin")
                    return RedirectToAction("admin_home", "Users");
                else if (roleLower == "employee")
                    return RedirectToAction("Index", "Home");
                else if (roleLower == "volunteer")
                    return RedirectToAction("volunteer_home", "Users");
                else
                    return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost, ActionName("Login")]
        public async Task<IActionResult> Login(string na, string pa, string auto)
        {
            var ur = await _context.Users
                .FromSqlRaw("SELECT * FROM Users WHERE Email ='" + na + "' AND Password ='" + pa + "' ")
                .FirstOrDefaultAsync();

            if (ur != null)
            {
                int id = ur.Id;
                string na1 = ur.Name;
                string ro = ur.Role;

                HttpContext.Session.SetString("userid", Convert.ToString(id));
                HttpContext.Session.SetString("Name", na1);
                HttpContext.Session.SetString("Role", ro);
                HttpContext.Session.SetString("UserEmail", ur.Email);

                if (auto == "on")
                {
                    await HttpContext.Session.LoadAsync();
                    ViewData["role"] = HttpContext.Session.GetString("Role");
                    HttpContext.Response.Cookies.Append("Name", na1);
                    HttpContext.Response.Cookies.Append("Role", ro);
                }


                string roleLower = ro.ToLower();
                if (roleLower == "admin")
                    return RedirectToAction("admin_home", "Users");
                else if (roleLower == "employee")
                    return RedirectToAction("Index", "Home");
                else if (roleLower == "volunteer")
                {
                    var volunteer = await _context.Volunteers
                        .FirstOrDefaultAsync(v => v.Email == ur.Email);

                    if (volunteer != null)
                    {
                        HttpContext.Session.SetString("VolunteerId", volunteer.VolunteerID.ToString());
                    }
                    return RedirectToAction("Index", "Events");
                }

                else
                    return View();
            }
            else
            {
                ViewData["Message"] = "wrong user name password";
                return View();
            }
        }


        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            HttpContext.Response.Cookies.Delete("userid");
            HttpContext.Response.Cookies.Delete("Role");
            HttpContext.Response.Cookies.Delete("Name");

            return RedirectToAction("login", "Users");
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(
            string name, string phone, string email, string password, string gender,
            string[] availableDays, string shift, string location, string[] skills, string[] interests)
        {
            string connStr = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ahads\\OneDrive\\المستندات\\AtaaPro.mdf;Integrated Security=True;Connect Timeout=30;Encrypt=True";

            string daysText = availableDays != null ? string.Join(", ", availableDays) : "";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();

                string checkEmailSql = "SELECT COUNT(*) FROM Users WHERE Email = @email";

                SqlCommand checkCmd = new SqlCommand(checkEmailSql, conn);
                checkCmd.Parameters.AddWithValue("@email", email);

                int emailExists = (int)await checkCmd.ExecuteScalarAsync();

                if (emailExists > 0)
                {
                    ViewData["EmailError"] = "هذا البريد الإلكتروني مستخدم مسبقاً";
                    return View();
                }

                string sqlVol = @"
                INSERT INTO Volunteers (Name, Email, Password, Phone, Gender, Location, Shift, AvailableDays)
                VALUES (@n, @e, @p, @ph, @g, @l, @sh, @ad);
                SELECT SCOPE_IDENTITY();";

                SqlCommand cmdVol = new SqlCommand(sqlVol, conn);
                cmdVol.Parameters.AddWithValue("@n", name);
                cmdVol.Parameters.AddWithValue("@e", email);
                cmdVol.Parameters.AddWithValue("@p", password);
                cmdVol.Parameters.AddWithValue("@ph", phone);
                cmdVol.Parameters.AddWithValue("@g", gender ?? "");
                cmdVol.Parameters.AddWithValue("@l", location ?? "");
                cmdVol.Parameters.AddWithValue("@sh", shift ?? "");
                cmdVol.Parameters.AddWithValue("@ad", daysText);

                int volunteerID = Convert.ToInt32(await cmdVol.ExecuteScalarAsync());


                if (skills != null)
                {
                    foreach (var skillName in skills)
                    {
                        string sqlGetSkillId = "SELECT SkillID FROM Skills WHERE SkillName = @skillName";
                        SqlCommand cmdGetSkillId = new SqlCommand(sqlGetSkillId, conn);
                        cmdGetSkillId.Parameters.AddWithValue("@skillName", skillName);

                        object skillIdObj = await cmdGetSkillId.ExecuteScalarAsync();
                        if (skillIdObj != null)
                        {
                            int skillID = Convert.ToInt32(skillIdObj);

                            string sqlSkill = "INSERT INTO VolunteerSkills (VolunteerID, SkillID) VALUES (@vid, @skillID)";
                            SqlCommand cmdSkill = new SqlCommand(sqlSkill, conn);
                            cmdSkill.Parameters.AddWithValue("@vid", volunteerID);
                            cmdSkill.Parameters.AddWithValue("@skillID", skillID);

                            await cmdSkill.ExecuteNonQueryAsync();
                        }
                        
                    }
                }

             
                if (interests != null)
                {
                    foreach (var interestName in interests)
                    {
                        string sqlGetInterestId = "SELECT InterestID FROM Interests WHERE InterestName = @interestName";
                        SqlCommand cmdGetInterestId = new SqlCommand(sqlGetInterestId, conn);
                        cmdGetInterestId.Parameters.AddWithValue("@interestName", interestName);

                        object interestIdObj = await cmdGetInterestId.ExecuteScalarAsync();
                        if (interestIdObj != null)
                        {
                            int interestID = Convert.ToInt32(interestIdObj);

                            string sqlInterest = "INSERT INTO VolunteerInterests (VolunteerID, InterestID) VALUES (@vid, @interestID)";
                            SqlCommand cmdInterest = new SqlCommand(sqlInterest, conn);
                            cmdInterest.Parameters.AddWithValue("@vid", volunteerID);
                            cmdInterest.Parameters.AddWithValue("@interestID", interestID);

                            await cmdInterest.ExecuteNonQueryAsync();
                        }
                       
                    }
                }

                
                string sqlUser = "INSERT INTO Users (Role, Name, Email, Password) VALUES ('volunteer', @n, @e, @p)";
                SqlCommand cmdUser = new SqlCommand(sqlUser, conn);
                cmdUser.Parameters.AddWithValue("@n", name);
                cmdUser.Parameters.AddWithValue("@e", email);
                cmdUser.Parameters.AddWithValue("@p", password);
                await cmdUser.ExecuteNonQueryAsync();
            }

            return RedirectToAction("Login", "Users");
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public async Task<IActionResult> Profile()
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            int userId = int.Parse(userIdStr);

         
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.Email == user.Email);
            if (volunteer == null) return NotFound();

            var viewModel = new ProfileViewModel
            {
                UserProfile = volunteer,

         
                VolunteerHistory = await _context.EventRegistrations
                    .Include(r => r.Event)
                    .Where(r => r.VolunteerId == volunteer.VolunteerID)
                    .ToListAsync(),

                SelectedSkillIds = await _context.VolunteerSkills
                    .Where(vs => vs.VolunteerID == volunteer.VolunteerID)
                    .Select(vs => vs.SkillID).ToListAsync(),

                SelectedInterestIds = await _context.VolunteerInterests
                    .Where(vi => vi.VolunteerID == volunteer.VolunteerID)
                    .Select(vi => vi.InterestID).ToListAsync(),

                AllSkills = await _context.Skills.ToListAsync(),
                AllInterests = await _context.Interests.ToListAsync()
            };

            ViewBag.IssuedCertificates = await _context.Certificates
         .Where(c => _context.EventRegistrations
             .Where(r => r.VolunteerId == volunteer.VolunteerID)
             .Select(r => r.Id).Contains(c.RegistrationId))
         .Select(c => c.RegistrationId)
         .ToListAsync();

            return View(viewModel);
        }
        public async Task<IActionResult> DownloadCertificate(int registrationId)
        {
            var cert = await _context.Certificates
                .Include(c => c.Registration)
                    .ThenInclude(r => r.Event)
                .Include(c => c.Registration)
                    .ThenInclude(r => r.Volunteer)
                .FirstOrDefaultAsync(c => c.RegistrationId == registrationId);

            if (cert == null) return NotFound();

      
            return View("CertificateTemplate", cert);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult CertificateTemplate()
        {
            return View();
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBasicInfo(ProfileViewModel model)
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            int userId = int.Parse(userIdStr);
            var userAccount = await _context.Users.FindAsync(userId);
            var volunteerProfile = await _context.Volunteers.FirstOrDefaultAsync(v => v.Email == userAccount.Email);

            if (userAccount != null && volunteerProfile != null)
            {
                userAccount.Name = model.UserProfile.Name;
                userAccount.Password = model.UserProfile.Password;
                volunteerProfile.Name = model.UserProfile.Name;
                volunteerProfile.Phone = model.UserProfile.Phone;
                volunteerProfile.Gender = model.UserProfile.Gender;
                volunteerProfile.Password = model.UserProfile.Password;

                _context.Update(userAccount);
                _context.Update(volunteerProfile);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("Name", model.UserProfile.Name);
                TempData["Success"] = "تم تحديث البيانات الشخصية بنجاح";
            }
            return RedirectToAction("Profile");
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePreferences(int VolunteerID, int[] selectedSkills, int[] selectedInterests, string[] selectedDays, string Location, string Shift)
        {
            var volunteerProfile = await _context.Volunteers.FindAsync(VolunteerID);
            if (volunteerProfile != null)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        volunteerProfile.Location = Location;
                        volunteerProfile.Shift = Shift;
                        volunteerProfile.AvailableDays = selectedDays != null ? string.Join(", ", selectedDays) : "";
                        _context.Update(volunteerProfile);

                        await _context.Database.ExecuteSqlRawAsync("DELETE FROM VolunteerSkills WHERE VolunteerID = {0}", VolunteerID);
                        if (selectedSkills != null)
                            foreach (var sId in selectedSkills)
                                await _context.Database.ExecuteSqlRawAsync("INSERT INTO VolunteerSkills (VolunteerID, SkillID) VALUES ({0}, {1})", VolunteerID, sId);

                        await _context.Database.ExecuteSqlRawAsync("DELETE FROM VolunteerInterests WHERE VolunteerID = {0}", VolunteerID);
                        if (selectedInterests != null)
                            foreach (var iId in selectedInterests)
                                await _context.Database.ExecuteSqlRawAsync("INSERT INTO VolunteerInterests (VolunteerID, InterestID) VALUES ({0}, {1})", VolunteerID, iId);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        TempData["Success"] = "تم تحديث التفضيلات والمهارات بنجاح";
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "حدث خطأ أثناء الحفظ";
                    }
                }
            }
            return RedirectToAction("Profile");
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        [HttpPost]
        [Route("Users/UpdateRating")] 
        public async Task<JsonResult> UpdateRating(int id, int rating)
        {
            try
            {
                var registration = await _context.EventRegistrations.FindAsync(id);
                if (registration != null)
                {
                    registration.Rating = rating;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Id not found: " + id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> emProfile()
        {
            
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

         
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound();
            }

      
            return View(user);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> emProfile(int id, string Name, string Email, string Password)
        {
            
            var userInDb = await _context.Users.FindAsync(id);

            if (userInDb == null)
            {
                return NotFound();
            }

            try
            {
                
                userInDb.Name = Name;
                userInDb.Email = Email;
                userInDb.Password = Password;

                _context.Update(userInDb);
                await _context.SaveChangesAsync();

               
                HttpContext.Session.SetString("Name", userInDb.Name);
                HttpContext.Session.SetString("UserEmail", userInDb.Email);

                TempData["Success"] = "تم تحديث بيانات الملف الشخصي بنجاح";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء التحديث: " + ex.Message;
            }

            return View(userInDb);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult admin_home()
        {
                     
            if (HttpContext.Session.GetString("Role") != "admin")
            {
                return RedirectToAction("Login");
            }
           
            var stats = new[] {new {
 Title = "التبرعات",
  Value = (int)(_context.Donations.Any() ? _context.Donations.Sum(d => d.TotalAmount) : 0),  
                Color = "#2E7D32"},new {

 Title = "المشاريع",    
                    Value = _context.Projects.Count(),    
                    Color = "#4CAF50"},new {

 Title = "المتطوعين",  
                        Value = _context.Users.Count(u => u.Role == "volunteer"),    
                        Color = "#8BC34A"},new {

 Title = "المتبرعين",
  Value = _context.Donors.Count(),    Color = "#CDDC39"}};



            return View(stats);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            string role = user.Role;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(role == "employee" ? "Employees" : "Volunteers");
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult Donors()
        {
            var donorsList = _context.Donors.ToList();

            return View(donorsList);
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult Employees(string searchTerm)
        {
            var employees = _context.Users.Where(u => u.Role == "employee").AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                employees = employees.Where(e => e.Name.Contains(searchTerm) || e.Email.Contains(searchTerm));
            }

            return View(employees.ToList());
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////



        [HttpPost]
        public IActionResult AddEmployee(Users newUser)
        {
            bool emailExists = _context.Users.Any(u => u.Email.ToLower() == newUser.Email.ToLower());

            if (emailExists)
            {
                TempData["ErrorMessage"] = "عذراً، هذا البريد مسجل مسبقاً.";
                return RedirectToAction("Employees");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Users.Add(newUser);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "تم تسجيل الموظف الجديد بنجاح!";
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "حدث خطأ غير متوقع أثناء الحفظ.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "يرجى التأكد من صحة البيانات المدخلة.";
            }

            return RedirectToAction("Employees");
        }
        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////


        public IActionResult Volunteers()
        {
            var list = _context.Users.Where(u => u.Role == "volunteer").ToList();
            return View(list);
        }

        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////

     

        [HttpPost]
        public IActionResult AddVolunteer(Users newUser)
        {
            var emailExists = _context.Users
                .Any(u => u.Email == newUser.Email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "هذا البريد الإلكتروني مستخدم مسبقًا");
            }

            if (ModelState.IsValid)
            {
                newUser.Role = "volunteer";
                _context.Users.Add(newUser);
                _context.SaveChanges();
                return RedirectToAction("Volunteers");
            }

            var list = _context.Users
                .Where(u => u.Role == "volunteer")
                .ToList();

            ViewBag.OpenModal = true; 
            return View("Volunteers", list);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var users = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Role,Name,Email,Password")] Users users)
        {
            if (ModelState.IsValid)
            {
                _context.Add(users);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(users);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }
            return View(users);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Role,Name,Email,Password")] Users users)
        {
            if (id != users.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(users);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsersExists(users.Id))
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
            return View(users);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public IActionResult EditEmployee(Users updatedUser)
        {
            var userInDb = _context.Users.Find(updatedUser.Id);
            if (userInDb != null)
            {
                userInDb.Name = updatedUser.Name;
                userInDb.Email = updatedUser.Email;

                if (!string.IsNullOrEmpty(updatedUser.Password))
                {
                    userInDb.Password = updatedUser.Password;
                }

                _context.SaveChanges();
                return Ok();
            }
            return BadRequest();
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpGet]
        public IActionResult CheckEmailExists(string email)
        {
            if (string.IsNullOrEmpty(email)) return Json(false);

            bool exists = _context.Users.Any(u => u.Email.ToLower() == email.ToLower());

            return Json(exists);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var users = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}



    
