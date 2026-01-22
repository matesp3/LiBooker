namespace LiBookerWasmApp.Utils
{
    public class Converter
    {
        /// <summary>
        /// Retrieves image data URL from byte array.
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public static string GetImageDataUrl(byte[]? imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return string.Empty;

            var base64 = Convert.ToBase64String(imageData);
            return $"data:image/jpeg;base64,{base64}";
        }
    }
}
