using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Models
{
    [BsonIgnoreExtraElements]
    public class Ticket
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public int TicketId { get; set; }

        [Required]
        public string Title { get; set; } = "";

        [BsonRepresentation(BsonType.String)]
        public TicketType Type { get; set; }

        [BsonRepresentation(BsonType.String)]
        public TicketPriority Priority { get; set; }

        public string Deadline { get; set; } = "";

        public string Description { get; set; } = "";

        [BsonRepresentation(BsonType.String)]
        public TicketStatus Status { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? ReportedBy { get; set; }

        [BsonElement("HandeledBy")]
        public List<HandlingInfo> HandledBy { get; set; } = new();
    }

    public class HandlingInfo
    {
        [BsonElement("employeeID")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? EmployeeId { get; set; }

        [BsonElement("date")]
        public string Date { get; set; } = "";

        [BsonIgnore]
        public Employee Employee { get; set; } = new();
    }
}
