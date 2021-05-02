namespace Auth.Hashing
{
    public class HashedPassword
    {
        public HashedPassword(string hash, string salt)
        {
            Hash = hash;
            Salt = salt;
        }

        public string Hash { get; }
        public string Salt { get; }
    }
}
