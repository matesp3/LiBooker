namespace LiBooker.Shared.DTOs
{
    public class PublicationImageDto
    {
        public int ImageId { get; set; }
        public required byte[] ImageData { get; set; }

        // possible future extensions : ContentType, FileName, Size, etc.
    }
}