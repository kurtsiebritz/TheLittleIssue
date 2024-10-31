namespace TheLittleIssue.Models
{
    public class AdminDashboardViewModel
    {
        public List<PdfDocumentViewModel> PdfDocuments { get; set; }
    }

    public class PdfDocumentViewModel
    {
        public string FileName { get; set; }
        public string DownloadUrl { get; set; }
    }

}
