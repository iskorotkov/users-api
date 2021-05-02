namespace Auth.Hashing
{
    public class PasswordHasher
    {
        public HashedPassword Hash(string password)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);
            return new HashedPassword(hash, salt);
        }
    }
}
