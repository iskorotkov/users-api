using System;

namespace WebApi.DTOs
{
    public class UserGetDto
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public DateTime CreatedDate { get; set; }
        public UserGroupGetDto Group { get; set; }
        public UserStateGetDto State { get; set; }
    }
}
