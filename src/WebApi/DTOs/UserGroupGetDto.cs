using WebApi.Enums;

namespace WebApi.DTOs
{
    public class UserGroupGetDto
    {
        public int Id { get; set; }
        public UserGroupCode Code { get; set; }
        public string Description { get; set; }
    }
}
