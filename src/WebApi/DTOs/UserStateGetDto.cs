using Models.Enums;

namespace WebApi.DTOs
{
    public class UserStateGetDto
    {
        public int Id { get; set; }
        public UserStateCode Code { get; set; }
        public string Description { get; set; }
    }
}
