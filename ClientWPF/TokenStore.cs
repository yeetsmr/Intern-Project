namespace InternProject.WPF 
{

    public static class TokenStore
    {
        public static string Token { get; set; } = string.Empty;
    }


    public class LoginResponse
    {
        public string Token { get; set; }
        public string Message { get; set; }
    }
}