using MongoDB.Bson.Serialization.Attributes;

namespace NoSQL_Project.Models
{
    public class EmployeeName
    {
        [BsonElement("FirstName")]
        public string FirstName { get; set; } = "";

        [BsonElement("LastName")]
        public string LastName { get; set; } = "";

        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
