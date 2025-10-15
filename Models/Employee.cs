using System.ComponentModel.DataAnnotations;
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

        [BsonElement("IsDisabled")]
        public bool IsDisabled { get; set; } = false;

        [BsonElement("Name")]
        public Name Name { get; set; }

        [BsonElement("Role")]
        [BsonRepresentation(BsonType.String)]
        public RoleType Role { get; set; }

        [BsonElement("Password")]
        [BsonRepresentation(BsonType.String)]
        public string PasswordHashed { get; set; } = "";

        [BsonElement("Salt")]
        [BsonRepresentation(BsonType.String)]
        public string Salt { get; set; } = "";

        [BsonElement("contactInfo")]
        [JsonPropertyName("contactInfo")]
        public ContactInfo ContactInfo { get; set; } = new ContactInfo();

        [BsonElement("ReportedTickets")]
        [BsonIgnoreIfNull]
        public List<Ticket> ReportedTickets { get; set; } = new();


    }
}
