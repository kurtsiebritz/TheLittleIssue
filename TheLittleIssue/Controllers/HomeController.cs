using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TheLittleIssue.Models;

namespace TheLittleIssue.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FirestoreDb _firestoreDb;

        public HomeController(ILogger<HomeController> logger, FirestoreDb firestoreDb)
        {
            _logger = logger;
            _firestoreDb = firestoreDb;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ReadArticle(string title)
        {
            ViewData["DocumentTitle"] = title;
            return View();
        }

        public async Task<IActionResult> Article()
        {
            var articles = new List<IssueModel>();
            var snapshot = await _firestoreDb.Collection("Issues").GetSnapshotAsync();

            Console.WriteLine("Snapshot empty? " + (snapshot.Documents.Count == 0));

            foreach (var document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    articles.Add(new IssueModel
                    {
                        Id = document.Id,
                        Title = document.GetValue<string>("title"),
                        CoverUrl = document.GetValue<string>("coverUrl"),
                        PdfUrl = document.GetValue<string>("pdfUrl")
                    });
                }
            }

            // Debugging output
            Console.WriteLine("Articles retrieved: " + articles.Count);
            return View(articles);
        }

        public IActionResult Resources()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult InteractiveActivities()
        {
            return View();
        }

        public IActionResult SocialWork()
        {
            return View();
        }
        public IActionResult Donations()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}