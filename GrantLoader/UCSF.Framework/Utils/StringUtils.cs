namespace UCSF.Framework.Utils
{
    public static class StringUtils
    {
         public static string SafeTrim(this string value)
         {
             if(!string.IsNullOrEmpty(value))
             {
                 return value.Trim();
             }
             return value;
         }

        public static string DropQuotes(this string value)
         {
             if (!string.IsNullOrEmpty(value) && value.Length >= 2 && value.StartsWith("\"") && value.EndsWith("\""))
             {
                 return value.Substring(1, value.Length - 2);
             }
             return value;
         }
    }
}