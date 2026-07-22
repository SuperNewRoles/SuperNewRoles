using System.IO;
using FluentAssertions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class AnnouncementImageCacheTests
{
    [Fact]
    public void TryConvertWebPToPng_ConvertsWebPBytesToPng()
    {
        using var webpStream = new MemoryStream();
        using (var source = new Image<Rgba32>(2, 1))
        {
            source[0, 0] = new Rgba32(255, 0, 0, 255);
            source[1, 0] = new Rgba32(0, 255, 0, 128);
            source.Save(webpStream, new WebpEncoder { FileFormat = WebpFileFormatType.Lossless });
        }

        byte[] webpBytes = webpStream.ToArray();
        AnnouncementImageCache.IsWebP(webpBytes).Should().BeTrue();

        bool converted = AnnouncementImageCache.TryConvertWebPToPng(webpBytes, out var pngBytes);

        converted.Should().BeTrue();
        pngBytes.Should().NotBeNullOrEmpty();
        pngBytes.Should().StartWith(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        using var decoded = Image.Load<Rgba32>(pngBytes);
        decoded.Width.Should().Be(2);
        decoded.Height.Should().Be(1);
        decoded[0, 0].Should().Be(new Rgba32(255, 0, 0, 255));
        decoded[1, 0].Should().Be(new Rgba32(0, 255, 0, 128));
    }

    [Fact]
    public void IsWebP_RejectsOtherRiffContent()
    {
        byte[] waveHeader = { (byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0, (byte)'W', (byte)'A', (byte)'V', (byte)'E' };

        AnnouncementImageCache.IsWebP(waveHeader).Should().BeFalse();
    }
}
