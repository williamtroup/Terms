namespace Terms.Tools.Extensions
{
    public static class StringExtensions
    {
        public static bool ToBoolean(this string value)
        {
            return value == "1";
        }
    }
}