namespace Application.Common.Interfaces;

public interface IThumbnailService
{
    string ImageToThumbnailBase64(byte[] image);
}
