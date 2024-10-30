using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Google.Cloud.Firestore.V1;
using TheLittleIssue.Models;

namespace TheLittleIssue.Controllers
{
    public class UserController : Controller
    {
        private readonly FirestoreDb _firestoreDb;

        // Constructor injecting FirestoreDb
        public UserController(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        // Login action
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            CollectionReference usersCollection = _firestoreDb.Collection("users");
            QuerySnapshot userSnapshot = await usersCollection.WhereEqualTo("Email", email).GetSnapshotAsync();

            if (userSnapshot.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View();
            }

            DocumentSnapshot userDoc = userSnapshot.Documents[0];
            string storedHashedPassword = userDoc.GetValue<string>("HashedPassword");
            string hashedInputPassword = HashPassword(password);

            if (storedHashedPassword != hashedInputPassword)
            {
                ModelState.AddModelError(string.Empty, "Invalid password.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim("FirstName", userDoc.GetValue<string>("FirstName")),
                new Claim("SecondName", userDoc.GetValue<string>("SecondName"))
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // Register action with validation
        [HttpPost]
        public async Task<IActionResult> Register(string firstName, string secondName, string dateOfBirth, string email, string password, string confirmPassword)
        {
            // Check if the email already exists
            CollectionReference usersCollection = _firestoreDb.Collection("users");
            QuerySnapshot existingUserSnapshot = await usersCollection.WhereEqualTo("Email", email).GetSnapshotAsync();
            if (existingUserSnapshot.Count > 0)
            {
                ModelState.AddModelError("email", "Email already exists.");
                ViewData["EmailError"] = "Email already exists."; // Set error message for Razor view
            }

            // Validate password requirements
            if (!IsValidPassword(password))
            {
                ModelState.AddModelError("password", "Password must be at least 6 characters long and contain at least one digit.");
                ViewData["PasswordError"] = "Password must be at least 6 characters long and contain at least one digit."; // Set error message for Razor view
            }

            // Validate password match
            if (password != confirmPassword)
            {
                ModelState.AddModelError("confirmPassword", "Passwords do not match.");
                ViewData["ConfirmPasswordError"] = "Passwords do not match."; // Set error message for Razor view
            }

            // If any validation errors, return to the register view with error messages
            if (!ModelState.IsValid)
            {
                return View();
            }

            DocumentReference userDoc = usersCollection.Document();
            string hashedPassword = HashPassword(password);

            Dictionary<string, object> userData = new Dictionary<string, object>
            {
                { "FirstName", firstName },
                { "SecondName", secondName },
                { "DateOfBirth", dateOfBirth },
                { "Email", email },
                { "HashedPassword", hashedPassword }
            };

            await userDoc.SetAsync(userData);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim("FirstName", firstName),
                new Claim("SecondName", secondName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User");
        }


        public async Task<IActionResult> ContinueAsGuest()
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "Guest"),
            new Claim("FirstName", "Guest"),
            new Claim("SecondName", "")
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home"); // Redirect to the home page or a guest landing page
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

        public async Task<string> GetUserIdByEmailAsync(string email)
        {
            try
            {
                // Reference to the users collection
                CollectionReference usersRef = _firestoreDb.Collection("users"); // Ensure 'users' is the correct collection name

                // Create a query to find the user by email
                Query query = usersRef.WhereEqualTo("Email", email); // Adjust 'email' to your field name in Firestore

                // Execute the query
                QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

                if (querySnapshot.Documents.Count > 0)
                {
                    // Get the first matching document
                    DocumentSnapshot documentSnapshot = querySnapshot.Documents[0];
                    string userId = documentSnapshot.Id; // The document ID is typically the userId

                    return userId;
                }
                else
                {
                    Console.WriteLine($"No user found with email: {email}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user ID: {ex.Message}");
                return null;
            }
        }

        [HttpGet("User/SaveReadingHistory/{articleTitle}/{pageNumber}")]
        public async Task<IActionResult> SaveReadingHistory(string articleTitle, int pageNumber)
        {
            if (string.IsNullOrEmpty(articleTitle) || pageNumber <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid reading history data." });
            }

            if (User.Identity.Name.Equals("Guest"))
            {
                // Return early if the user is a guest
                return Json(new { success = false, isGuest = true, message = "Please log in to save your reading history." });
            }

            var userEmail = User.Identity.Name;
            string userId = await GetUserIdByEmailAsync(User.Identity.Name);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not found." });
            }

            var userDocRef = _firestoreDb.Collection("users").Document(userId);

            try
            {
                // Create the new history entry
                var readingHistoryEntry = new Dictionary<string, object>
                {
                    { "Title", articleTitle },
                    { "Page", pageNumber },
                    { "Timestamp", DateTime.UtcNow }
                };

                // Get existing history
                var snapshot = await userDocRef.GetSnapshotAsync();
                List<Dictionary<string, object>> existingHistory = new List<Dictionary<string, object>>();

                if (snapshot.Exists && snapshot.ContainsField("ReadingHistory"))
                {
                    existingHistory = snapshot.GetValue<List<Dictionary<string, object>>>("ReadingHistory");

                    // Check if there's an existing entry for this title
                    var existingEntryIndex = existingHistory.FindIndex(h => h["Title"].ToString() == articleTitle);

                    if (existingEntryIndex >= 0)
                    {
                        // Update the existing entry
                        existingHistory[existingEntryIndex] = readingHistoryEntry;
                    }
                    else
                    {
                        // Add new entry if title does not exist
                        existingHistory.Add(readingHistoryEntry);
                    }
                }
                else
                {
                    // Initialize the history list with the first entry
                    existingHistory.Add(readingHistoryEntry);
                }

                // Save the updated history back to Firestore
                await userDocRef.UpdateAsync("ReadingHistory", existingHistory);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetReadingHistory()
        {
            if (User.Identity.Name.Equals("Guest"))
            {
                return Json(new { success = false, isGuest = true, message = "Please log in to view your reading history." });
            }

            var userEmail = User.Identity.Name;
            string userId = await GetUserIdByEmailAsync(User.Identity.Name);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not found." });
            }

            var userDocRef = _firestoreDb.Collection("users").Document(userId);

            try
            {
                var snapshot = await userDocRef.GetSnapshotAsync();
                if (snapshot.Exists && snapshot.ContainsField("ReadingHistory"))
                {
                    var readingHistory = snapshot.GetValue<List<Dictionary<string, object>>>("ReadingHistory");

                    // Sort the entries by Timestamp in ascending order (latest last)
                    var sortedHistory = readingHistory
                        .OrderBy(entry => entry.ContainsKey("Timestamp") && entry["Timestamp"] is DateTime timestamp ? timestamp : DateTime.MinValue)
                        .ToList();

                    // Reverse the order so the latest one is at the top
                    sortedHistory.Reverse();

                    return Json(new { success = true, data = sortedHistory });
                }

                return Json(new { success = true, data = new List<object>() }); // Empty history if none exists
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }






    }
}
