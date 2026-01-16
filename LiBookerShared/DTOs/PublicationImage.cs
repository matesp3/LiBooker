namespace LiBooker.Shared.DTOs
{
    public class PublicationImage
    {
        public int ImageId { get; set; }
        public required byte[] RawImage { get; set; }

        // possible future extensions : ContentType, FileName, Size, etc.
    }
}