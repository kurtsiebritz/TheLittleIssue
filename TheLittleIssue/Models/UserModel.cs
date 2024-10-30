namespace TheLittleIssue.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public List<UserHistoryModel> ReadingHistory { get; set; } = new List<UserHistoryModel>();
    }
}
