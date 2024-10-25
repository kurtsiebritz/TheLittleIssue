using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;

var builder = WebApplication.CreateBuilder(args);

// Set the path to your service account key file using a relative path
string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
string keyFilePath = Path.Combine(baseDirectory, "appsettings", "the-little-issue-439607-87d078f1be56.json");
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", keyFilePath);

// Add Firestore
builder.Services.AddSingleton(FirestoreDb.Create("the-little-issue-439607"));

// Add Google Cloud Storage Client
builder.Services.AddSingleton(StorageClient.Create());

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
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
