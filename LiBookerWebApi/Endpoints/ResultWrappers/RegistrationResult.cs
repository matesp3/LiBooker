namespace LiBookerWebApi.Endpoints.ResultWrappers
{
    public class RegistrationResult
    {
        public bool IsSuccessful { get; set; } = true;

        public string? FailureReason { get; set; }

        public static RegistrationResult Success => new();

        public static RegistrationResult EmailAlreadyUsed()
        {
            return new()
            {
                IsSuccessful = false,
                FailureReason = "Email already used"
            };
        }

        public static RegistrationResult Failure(string reason)
        {
            return new()
            {
                IsSuccessful = false,
                FailureReason = reason
            };
        }
    }
}
