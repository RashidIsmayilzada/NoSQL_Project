using MongoDB.Bson.Serialization.Attributes;

namespace NoSQL_Project.Models
{
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
