namespace CMGWpf.Utilities
{
    public static class StringUtils
    {
        private static readonly FastRandom random = MathUtilities.StartFastRandom(null);
        public static string GenerateRandomString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] buffer = new char[length];
            for (int i = 0; i < length; i++)
                buffer[i] = chars[random.Next(chars.Length)];
            return new string(buffer);
        }
    }
}
