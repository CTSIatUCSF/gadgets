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
    }
}