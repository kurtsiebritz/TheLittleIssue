using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TheLittleIssue.Models;

namespace TheLittleIssue.Controllers
{
    public class AdminController : Controller
    {
        private readonly FirestoreDb _firestoreDb;

        public AdminController(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            CollectionReference documents = _firestoreDb.Collection("documents");
            QuerySnapshot snapshot = await documents.GetSnapshotAsync();

            var pdfList = snapshot.Documents.Select(doc => new PdfDocumentViewModel
            {
                FileName = doc.GetValue<string>("FileName"),
                DownloadUrl = doc.GetValue<string>("DownloadUrl")
            }).ToList();

            var viewModel = new AdminDashboardViewModel
            {
                PdfDocuments = pdfList
            };

            return View(viewModel);
        }

        // Register action moved from UserController with validation
        [HttpPost]
        public async Task<IActionResult> Register(string firstName, string secondName, string dateOfBirth, string email, string password, string confirmPassword, string role)
        {
            // Check if the email already exists
            CollectionReference usersCollection = _firestoreDb.Collection("users");
            QuerySnapshot existingUserSnapshot = await usersCollection.WhereEqualTo("Email", email).GetSnapshotAsync();
            if (existingUserSnapshot.Count > 0)
            {
                ModelState.AddModelError("email", "Email already exists.");
            }

            // Validate password requirements
            if (!IsValidPassword(password))
            {
                ModelState.AddModelError("password", "Password must be at least 6 characters long and contain at least one digit.");
            }

            // Validate password match
            if (password != confirmPassword)
            {
                ModelState.AddModelError("confirmPassword", "Passwords do not match.");
            }

            // Validate role
            if (string.IsNullOrEmpty(role) || !(role == "Admin" || role == "Employee" || role == "User"))
            {
                ModelState.AddModelError("role", "Invalid role selected.");
            }

            // If any validation errors, return to the admin dashboard with errors
            if (!ModelState.IsValid)
            {
                TempData["ShowRegisterTab"] = true;
                var pdfList = (await _firestoreDb.Collection("documents").GetSnapshotAsync())
                    .Documents.Select(doc => new PdfDocumentViewModel
                    {
                        FileName = doc.GetValue<string>("FileName"),
                        DownloadUrl = doc.GetValue<string>("DownloadUrl")
                    }).ToList();

                var viewModel = new AdminDashboardViewModel
                {
                    PdfDocuments = pdfList
                };

                return View("AdminDashboard", viewModel);
            }

            DocumentReference userDoc = usersCollection.Document();
            string hashedPassword = HashPassword(password);

            Dictionary<string, object> userData = new Dictionary<string, object>
            {
                { "FirstName", firstName },
                { "SecondName", secondName },
                { "DateOfBirth", dateOfBirth },
                { "Email", email },
                { "HashedPassword", hashedPassword },
                { "Role", role }
            };

            await userDoc.SetAsync(userData);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim("FirstName", firstName),
                new Claim("SecondName", secondName),
                new Claim(ClaimTypes.Role, role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // Helper method for password hashing
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Helper method for password validation
        private bool IsValidPassword(string password)
        {
            return password.Length >= 6 && Regex.IsMatch(password, @"\d");
        }
    }
}
