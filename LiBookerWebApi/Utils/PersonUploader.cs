using LiBookerWebApi.Services;
using System.Text.RegularExpressions;

namespace LiBookerWebApi.Utils
{
    /// <summary>
    /// Uploads person data from database to create related user accounts for them.
    /// </summary>
    public class PersonUploader
    {
        public static async Task UploadPersonsFromExportFile(string filePath, IAuthService svc, ILogger<Program> logger)
        {
            int batchSize = 100;
            var usersToProcess = new List<UserAccountDto>();
            int currentCount = 0;
            var token = new CancellationTokenSource().Token;
            var users = await ParseExportFile(filePath);
            while (currentCount <= users.Count)
            {
                var batch = users.Skip(currentCount).Take(batchSize).ToList();
                if (batch.Count == 0)
                    break;
                var usersWithPasswords = await svc.CreateUserForPerson(batch, logger, token);
                currentCount += batchSize;
                await WriteUsersToFile(usersWithPasswords, filePath);
                logger.LogInformation("Created {UserCount} user accounts from export file.", batch.Count);
            }
        }

        private static async Task WriteUsersToFile(List<UserAccountDto> users, string pathForInputFile)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            // directory of input file
            string directory = Path.GetDirectoryName(pathForInputFile) ?? string.Empty;

            // new path for output file
            string outputPath = Path.Combine(directory, $"import_credentials_{timestamp}.txt");

            using (StreamWriter writer = new StreamWriter(outputPath, append: false))
            {
                await writer.WriteLineAsync("--- IMPORT CREDENTIALS REPORT ---");
                await writer.WriteLineAsync($"DATE: {timestamp}");
                await writer.WriteLineAsync("---------------------------------");
                await writer.WriteLineAsync("EMAIL | PASSWORD");
                await writer.WriteLineAsync("---------------------------------");

                foreach (var user in users)
                {
                    await writer.WriteLineAsync($"{user.Email} | {user.Password}");
                }
            }
        }

        private static async Task<List<UserAccountDto>> ParseExportFile(string filePath)
        {
            var users = new List<UserAccountDto>();

            var lines = await File.ReadAllLinesAsync(filePath);

            // skip header line
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // split by whitespace but respect quotes
                // this regex matches either sequences of non-whitespace characters or quoted strings
                var matches = Regex.Matches(line, @"[^\s""]+|""([^""]*)""");

                if (matches.Count >= 2)
                {
                    users.Add(new UserAccountDto
                    {
                        // first column is PersonId (cleaning quotes)
                        PersonId = int.Parse(matches[0].Value.Replace("\"", "")),

                        // second column is Email (cleaning quotes)
                        Email = matches[1].Value.Replace("\"", "")
                    });
                }
            }
            return users;
        }

        public class UserAccountDto
        {
            public int PersonId { get; set; }
            public string Email { get; set; } = string.Empty;

            public string? Password { get; set; } = null;
        }
    }
}
