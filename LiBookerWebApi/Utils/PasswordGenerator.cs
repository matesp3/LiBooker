using System.Security.Cryptography;

namespace LiBookerWebApi.Utils
{
    public static class PasswordGenerator
    {
        private static readonly char[] Lowercase = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly char[] Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] Digits = "0123456789".ToCharArray();
        private static readonly char[] Special = "!@#$%^&*()_-+=[]{}|;:,.<>?".ToCharArray();

        public static string Generate(int length = 10)
        {
            if (length < 6 || length > 12) length = 10; // range of password length

            var password = new char[length];
            var allChars = Lowercase.Concat(Uppercase).Concat(Digits).Concat(Special).ToArray();

            // we guarantee at least one character from each group
            password[0] = Lowercase[RandomNumberGenerator.GetInt32(Lowercase.Length)];
            password[1] = Uppercase[RandomNumberGenerator.GetInt32(Uppercase.Length)];
            password[2] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];
            password[3] = Special[RandomNumberGenerator.GetInt32(Special.Length)];

            // remaining space will be filled randomly
            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
            }

            // shuffling generated chars (Fisher-Yates shuffle) so the categories are not in the beggining always
            return new string([.. password.OrderBy(_ => RandomNumberGenerator.GetInt32(100))]);
        }
    }
}
