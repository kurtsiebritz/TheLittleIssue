using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

public class DocumentController : Controller
{
    private readonly FirestoreDb _firestoreDb;
    private readonly StorageClient _storageClient;
    private const string BucketName = "little-issue-issues";

    public DocumentController(FirestoreDb firestoreDb, StorageClient storageClient)
    {
        _firestoreDb = firestoreDb;
        _storageClient = storageClient;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pdfFile"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> UploadPdf(IFormFile pdfFile)
    {
        if (pdfFile == null || pdfFile.Length == 0)
        {
            return BadRequest("No PDF file selected.");
        }

        // Define the path where the PDF will be stored in Google Cloud Storage
        string fileName = Path.GetFileName(pdfFile.FileName);
        string filePath = $"pdfs/{fileName}";

        // Upload the PDF file to Google Cloud Storage
        using (var stream = pdfFile.OpenReadStream())
        {
            await _storageClient.UploadObjectAsync(BucketName, filePath, "application/pdf", stream);
        }

        // Get the public download URL
        string downloadUrl = $"https://storage.googleapis.com/{BucketName}/{filePath}";

        // Store the metadata in Firestore
        CollectionReference documents = _firestoreDb.Collection("documents");
        DocumentReference newDocument = documents.Document();
        await newDocument.SetAsync(new
        {
            FileName = fileName,
            DownloadUrl = downloadUrl,
            UploadedAt = Timestamp.FromDateTime(System.DateTime.UtcNow)
        });

        return Ok("PDF uploaded successfully and metadata saved.");
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetPdfDocuments()
    {
        // Get the list of documents from Firestore
        CollectionReference documents = _firestoreDb.Collection("documents");
        QuerySnapshot snapshot = await documents.GetSnapshotAsync();

        // Create a list of PDF metadata to display
        var pdfList = snapshot.Documents.Select(doc => new
        {
            FileName = doc.GetValue<string>("FileName"),
            DownloadUrl = doc.GetValue<string>("DownloadUrl")
        }).ToList();

        return View(pdfList); // Pass the PDF metadata to the view
    }
}
