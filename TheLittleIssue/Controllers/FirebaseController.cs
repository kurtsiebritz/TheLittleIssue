using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using System.Threading.Tasks;

namespace TheLittleIssue.Controllers
{
    using Google.Cloud.Firestore;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    public class FirebaseController : Controller
    {
        private readonly FirestoreDb _firestoreDb;

        public FirebaseController(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<IActionResult> AddUser(string userId, string firstName, string dateOfBirth, string secondName, string email, string hashedPassword)
        {
            // Reference to the users collection
            CollectionReference usersCollection = _firestoreDb.Collection("users");

            // Create a document with a specific userId
            DocumentReference userDoc = usersCollection.Document(userId);

            // Data to store for the user
            Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "FirstName", firstName },
            { "SecondName", secondName },
            {"DateOfBirth", dateOfBirth },
            { "Email", email },
            { "HashedPassword", hashedPassword}
        };

            // Store the user data in Firestore
            await userDoc.SetAsync(userData);

            return Ok("User added successfully.");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetUser(string userId)
        {
            // Reference to the user's document
            DocumentReference userDoc = _firestoreDb.Collection("users").Document(userId);

            // Get the user's document snapshot
            DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();

            // Check if the document exists
            if (!snapshot.Exists)
            {
                return NotFound("User not found.");
            }

            // Retrieve user data from the document
            Dictionary<string, object> userData = snapshot.ToDictionary();

            // Extract user details (example for Name, Email, and Role)
            string name = userData.ContainsKey("Name") ? userData["Name"].ToString() : "No name found";
            string email = userData.ContainsKey("Email") ? userData["Email"].ToString() : "No email found";
            string role = userData.ContainsKey("Role") ? userData["Role"].ToString() : "No role found";

            // You can return the data to a view or in JSON format
            return Ok(new
            {
                Name = name,
                Email = email,
                Role = role
            });
        }
    }

}
