using System;

namespace Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public DateTime CreatedDate { get; set; }
        public int GroupId { get; set; }
        public virtual UserGroup Group { get; set; }
        public int StateId { get; set; }
        public virtual UserState State { get; set; }
    }
}
