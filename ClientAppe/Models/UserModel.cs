using System.Text.Json.Serialization;

namespace ClientAppe.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RegistrationDate { get; set; }
        public string Address { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
        [JsonIgnore]
        public bool IsOwner => Role == "Owner";
    }
}