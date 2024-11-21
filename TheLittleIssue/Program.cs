using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.Cookies;
using Google.Cloud.SecretManager.V1;

var builder = WebApplication.CreateBuilder(args);

string projectId = "the-little-issue-439607";
string secretId = "the-little-issue-439607-87d078f1be56";
string versionId = "latest";

// Access the secret
SecretManagerServiceClient client = SecretManagerServiceClient.Create();
AccessSecretVersionResponse response = client.AccessSecretVersion(
    new SecretVersionName(projectId, secretId, versionId));

string keyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings", "service-account-key.json");
Directory.CreateDirectory(Path.GetDirectoryName(keyFilePath)); // Ensure the directory exists
File.WriteAllBytes(keyFilePath, response.Payload.Data.ToByteArray());

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyFilePath);

// Set the path to your service account key file using a relative path
//string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
//string keyFilePath = Path.Combine(baseDirectory, "appsettings", "the-little-issue-439607-87d078f1be56.json");
//Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyFilePath);

// Add Firestore
builder.Services.AddSingleton(FirestoreDb.Create("the-little-issue-439607"));

// Add Google Cloud Storage Client
builder.Services.AddSingleton(StorageClient.Create());

// Configure Authentication Services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login"; // Specify the login path
        options.AccessDeniedPath = "/Home/AccessDenied"; // Specify the access denied path
    });

// Add services to the container
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Use Authentication Middleware
app.UseAuthentication(); // Ensure this is added before UseAuthorization
app.UseAuthorization();

// Change the default route to point to the Login action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}"); // Set to User/Login

app.Run();
