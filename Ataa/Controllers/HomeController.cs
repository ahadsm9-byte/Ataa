using Ataa.Data;
using Ataa.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;



namespace Ataa.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AtaaDbContext _db;

        public HomeController(ILogger<HomeController> logger, AtaaDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
   

[HttpGet]
            public IActionResult GetTasks()
            {
                var name = HttpContext.Session.GetString("Name") ?? "";

                var tasks = _db.Tasks
                    .Where(x => x.UserName == name)
                    .Select(x => new
                    {
                        day = x.Day,
                        text = x.Text
                    })
                    .ToList();

                return Json(tasks);
            }
            [HttpPost]
            public IActionResult SaveTask([FromBody] TaskModel model)
            {
                var name = HttpContext.Session.GetString("Name") ?? "";

                var task = _db.Tasks
                    .FirstOrDefault(x => x.UserName == name && x.Day == model.Day);

                if (task == null)
                {
                    _db.Tasks.Add(new UserTask
                    {
                        UserName = name,
                        Day = model.Day,
                        Text = model.Text
                    });
                }
                else
                {
                    task.Text = model.Text;
                }

                _db.SaveChanges();
                return Ok();
            }
            public IActionResult About()
            {
                ViewData["Message"] = "نبذة عن جمعية البيئة بالمدينه المنوره";
                return View();
            }
        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public IActionResult ExecutiveManager()
            {
                ViewBag.TopLevel = new List<dynamic> {
        new { Name = "أ. أحمد المغيدي", Role = "المدير التنفيذي", Email = "ahmed.a@madinah-eco.org.sa", Color = "#2d4d22" },
        new { Name = "أ. بدرية الحربي", Role = "مدير إداري", Email = "badriah.a@madinah-eco.org.sa", Color = "#5a8c4f" }
    };

                ViewBag.StaffLevel = new List<dynamic> {
        new { Name = "أ. فاطمة عبدالله", Role = "أخصائي بيئي", Email = "fatima.a@madinah-eco.org.sa" },
        new { Name = "د. خمائل خلف الله", Role = "المستشار البيئي", Email = "khamail.g@madinah-eco.org.sa" },
        new { Name = "أ. فهد سعود الحربي", Role = "مدير المشاريع", Email = "fahad.a@madinah-eco.org.sa" }
    };

                return View();
            }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public IActionResult ChairmanSpeech()
            {
                ViewBag.ChairmanName = "أ. طه محمد هاشم الغزي";
                return View();
            }
            //////////////////////////////////
            public IActionResult BoardMembers()
            {
                return View();
            }
            /////////////////////////////////
            public IActionResult GeneralAssembly()
            {
                return View();
            }
            ///////////////////////////////
            public IActionResult OrganizationalChart()
            {
                return View();
            }
            /////////////////////////////
            public IActionResult Certificates()
            {
                return View();
            }
            ////////////////////////////
            public IActionResult Documents()
            {
                ViewBag.Title = "الوثائق والأنظمة - جمعية البيئة";
                return View();
            }
            ////////////////////////////
            public IActionResult Awards()
            {
                ViewBag.Title = "جوائز التميز - جمعية البيئة";
                return View();
            }
            //////////////////////////
            public IActionResult Partners()
            {
                ViewBag.Title = "شركاؤنا - جمعية البيئة";
                return View();
            }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public IActionResult Regulations()
            {
                return View();
            }
            //////////////////////////
            public IActionResult AnnualReports()
            {
                return View();
            }
            /////////////////////////
            public IActionResult Guides()
            {
                return View();
            }
            ////////////////////////
            public IActionResult GovernanceRecords()
            {
                return View();
            }
        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public IActionResult MediaGallery()
            {
                return View();
            }
            //////////////////////
            public IActionResult VideoAlbum()
            {
                return View();
            }
            /////////////////////
            public IActionResult OurImpact()
            {
                ViewData["Title"] = "قالوا عنا | جمعية البيئة بالمدينة المنورة";
                return View();
            }

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
