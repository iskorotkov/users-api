namespace WebApi.DTOs
{
    public class UserPutDto
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int GroupId { get; set; }
        public int StateId { get; set; }
    }
}
