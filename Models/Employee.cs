using System.Net.Sockets;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Models
{
    [BsonIgnoreExtraElements]
    public class Employee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Name")]
        public Name Name { get; set; }

        [BsonElement("Role")]
        [BsonRepresentation(BsonType.String)]
        public RoleType Role { get; set; }

        [BsonElement("contactInfo")]
        public ContactInfo ContactInfo { get; set; }

        [BsonIgnore]
        public List<Ticket> ReportedTickets { get; set; } = new();

        public Employee()
        {
            ReportedTickets = new List<Ticket>();
        }
        
    }

    public class Name
    {
        [BsonElement("FirstName")]
        public string FirstName { get; set; } = "";

        [BsonElement("LastName")]
        public string LastName { get; set; } = "";
    }

    public class ContactInfo
    {
        [BsonElement("Email")]
        public string Email { get; set; } = "";

        [BsonElement("Phone")]
        public string Phone { get; set; } = "";

        [BsonElement("Location")]
        public string Location { get; set; } = "";
    }

}
