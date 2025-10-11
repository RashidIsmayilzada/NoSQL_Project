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

        [BsonElement("EmployeeId")]
        public int EmployeeId { get; set; }

        [BsonElement("Name")]
        public EmployeeName Name { get; set; }

        [BsonElement("Role")]
        [BsonRepresentation(BsonType.String)]
        public RoleType Role { get; set; }

        [BsonElement("contactInfo")]
        public EmployeeContactInfo ContactInfo { get; set; }

        [BsonElement("ReportedTickets")]
        [BsonIgnoreIfNull]
        public List<Ticket> ReportedTickets { get; set; } = new();

        public Employee()
        {
            ReportedTickets = new List<Ticket>();
        }

    }



}
