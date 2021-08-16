using static System.FormattableString;


namespace SupportFunctions.Utilities
{
    internal static class WebFunctions
    {
        public static string AsImg(string base64EncodedImage) => 
            Invariant($"<img alt=\"Time series chart\" src=\"data:image/png;base64,{base64EncodedImage}\"/>");
    }
}
