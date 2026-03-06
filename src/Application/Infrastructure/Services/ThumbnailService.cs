using Application.Common.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace Application.Infrastructure.Services;

public class ThumbnailService : IThumbnailService
{
    public string ImageToThumbnailBase64(byte[] image)
    {
        if (image == null || image.Length == 0)
        {
            throw new ArgumentException("Image data cannot be null or empty.", nameof(image));
        }

        try
        {
            using var inputStream = new MemoryStream(image);
            using var img = Image.Load(inputStream);

            // Resize to 250x250 thumbnail
            img.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(250, 250),
                Mode = ResizeMode.Max
            }));

            using var outputStream = new MemoryStream();
            img.Save(outputStream, new PngEncoder());

            return Convert.ToBase64String(outputStream.ToArray());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create thumbnail.", ex);
        }
    }
}
