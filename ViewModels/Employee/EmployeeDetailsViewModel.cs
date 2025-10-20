using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NoSQL_Project.Models;        
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.ViewModels.Employee
{
    public class EmployeeDetailsViewModel
    {
        public string Id { get; set; } = "";
        public bool IsDisabled { get; set; } = false;
        public Name Name { get; set; } = new Name();
        public RoleType Role { get; set; }
        public ContactInfo ContactInfo { get; set; } = new ContactInfo();
        public List<Ticket> ReportedTickets { get; set; } = new List<Ticket>();
    }
}
