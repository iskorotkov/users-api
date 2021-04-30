using System.Collections.Generic;
using WebApi.Enums;

namespace WebApi.Entities
{
    public class UserGroup
    {
        public int Id { get; set; }
        public UserGroupCode Code { get; set; }
        public string Description { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
