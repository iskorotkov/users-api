namespace WebApi.DTOs
{
    public class UserPutDto
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public int GroupId { get; set; }
    }
}
