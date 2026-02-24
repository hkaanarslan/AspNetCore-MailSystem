using Project2EmailNight.Entities;

namespace Project2EmailNight.ViewModels
{
    public class ProfileViewModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public string? About { get; set; }
        public string? Phone { get; set; }
        public int TotalMessage { get; set; }
        public int InboxCount { get; set; }
        public int SendCount { get; set; }
        public int StarredCount { get; set; }

        public int UnreadCount { get; set; }
        public int DeletedCount { get; set; }
        public int Last7DaysCount { get; set; }
        public List<Message> LastMessages { get; set; } = new();
        public string? TopContact { get; set; }

        public DateTime? BirthDate { get; set; }
        public string City { get; set; }
        public DateTime? RegisterDate { get; set; }

        public int DraftCount { get; set; }
        public int TotalMessageCount { get; set; }
    }
}
