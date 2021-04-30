using System.Collections.Generic;
using WebApi.Enums;

namespace WebApi.Entities
{
    public class UserState
    {
        public int Id { get; set; }
        public UserStateCode Code { get; set; }
        public string Description { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
