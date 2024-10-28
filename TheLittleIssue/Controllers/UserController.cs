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
    }
}
