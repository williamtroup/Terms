namespace Terms.Tools.Extensions
{
    public static class BooleanExtensions
    {
        public static string ToNumericString(this bool value)
        {
            return value ? "1" : "0";
        }
    }
}