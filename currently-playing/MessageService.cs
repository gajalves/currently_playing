namespace currently_playing;
internal static class MessageService
{
    public static void LogMessage(String message)
    {
        Console.WriteLine($"[{DateTime.Now}] - {message}");
    }
}
