namespace Project2EmailNight.Entities
{
    public class Message
    {
        public int MessageId { get; set; }

        public string SenderEmail { get; set; }
        public string ReceiverEmail { get; set; }

        public string Subject { get; set; }
        public string MessageDetail { get; set; }

        public DateTime SendDate { get; set; }

        public bool IsRead { get; set; } = false;
        public bool IsStarred { get; set; } = false;
        public bool IsDraft { get; set; } = false;
        public bool IsDeleted { get; set; } = false;


        public string? Category { get; set; }
        public bool IsStatus { get; set; }

    }
}
