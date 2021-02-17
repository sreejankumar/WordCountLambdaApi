namespace WordCount.Api.Core.Data.Messages
{
    public static class ExceptionMessages
    {
        public static string InvalidInput(string name)
        {
            return $"Property {name} cannot be empty or null";
        }

        public static string AllowedInputContentTypes(string contentType, string[] allowedList)
        {
            return
                $"Invalid Content Type({contentType}) received. Allowed values are {string.Join(",", allowedList)}";
        }
    }
}
