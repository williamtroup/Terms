namespace Terms.Tools.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToSeperatedString(this object value, string seperator = ", ")
        {
            return string.Join(seperator, (string[]) value);
        }
    }
}