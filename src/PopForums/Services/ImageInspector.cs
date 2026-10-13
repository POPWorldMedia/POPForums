using System.Buffers.Binary;

namespace PopForums.Services;

public enum ImageFormat
{
	Jpeg,
	Png,
	Gif
}

// Invalid comes first so that a default or unset value never means an image passed
public enum ImageValidationResult
{
	Invalid,
	TooLarge,
	Valid
}

public record ImageInfo(ImageFormat Format, int Width, int Height)
{
	public string ContentType => Format switch
	{
		ImageFormat.Jpeg => "image/jpeg",
		ImageFormat.Png => "image/png",
		ImageFormat.Gif => "image/gif",
		_ => throw new ArgumentOutOfRangeException(nameof(Format))
	};
}

public interface IImageInspector
{
	/// <summary>
	/// Reads the format and dimensions of an image from its header bytes. Returns false for anything that isn't
	/// a well-formed JPEG, PNG or GIF header.
	/// </summary>
	bool TryInspect(ReadOnlySpan<byte> bytes, out ImageInfo info);
	/// <summary>
	/// Checks that the bytes are a supported image within the given dimensions and size.
	/// </summary>
	ImageValidationResult Validate(byte[] bytes, int maxWidth, int maxHeight, int maxKBytes, out ImageInfo info);
	/// <summary>
	/// A localized message explaining a failed validation, or null when the result is valid.
	/// </summary>
	string GetErrorMessage(ImageValidationResult result, int maxWidth, int maxHeight, int maxKBytes);
}

/// <summary>
/// Reads the format and dimensions of an uploaded image from its header bytes, without decoding any
/// pixel data. Resizing happens in the browser before upload, so this is the server's only gate on
/// what gets stored and served. It must never throw on malformed input.
/// </summary>
public class ImageInspector : IImageInspector
{
	private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
	private static readonly byte[] PngIhdr = "IHDR"u8.ToArray();
	private static readonly byte[] Gif87a = "GIF87a"u8.ToArray();
	private static readonly byte[] Gif89a = "GIF89a"u8.ToArray();

	public bool TryInspect(ReadOnlySpan<byte> bytes, out ImageInfo info)
	{
		return TryInspectJpeg(bytes, out info) || TryInspectPng(bytes, out info) || TryInspectGif(bytes, out info);
	}

	public ImageValidationResult Validate(byte[] bytes, int maxWidth, int maxHeight, int maxKBytes, out ImageInfo info)
	{
		info = null;
		if (bytes == null || !TryInspect(bytes, out info))
			return ImageValidationResult.Invalid;
		if (bytes.Length > maxKBytes * 1024L || info.Width > maxWidth || info.Height > maxHeight)
			return ImageValidationResult.TooLarge;
		return ImageValidationResult.Valid;
	}

	public string GetErrorMessage(ImageValidationResult result, int maxWidth, int maxHeight, int maxKBytes)
	{
		return result switch
		{
			ImageValidationResult.Invalid => Resources.ImageInvalid,
			ImageValidationResult.TooLarge => string.Format(Resources.ImageTooLarge, maxWidth, maxHeight, maxKBytes),
			_ => null
		};
	}

	private static bool TryInspectPng(ReadOnlySpan<byte> bytes, out ImageInfo info)
	{
		info = null;
		// signature (8), IHDR chunk length (4) and type (4), then width (4) and height (4)
		if (bytes.Length < 24 || !bytes.StartsWith(PngSignature))
			return false;
		if (BinaryPrimitives.ReadUInt32BigEndian(bytes[8..]) != 13 || !bytes.Slice(12, 4).SequenceEqual(PngIhdr))
			return false;
		var width = BinaryPrimitives.ReadUInt32BigEndian(bytes[16..]);
		var height = BinaryPrimitives.ReadUInt32BigEndian(bytes[20..]);
		if (width == 0 || height == 0 || width > int.MaxValue || height > int.MaxValue)
			return false;
		info = new ImageInfo(ImageFormat.Png, (int)width, (int)height);
		return true;
	}

	private static bool TryInspectGif(ReadOnlySpan<byte> bytes, out ImageInfo info)
	{
		info = null;
		// signature (6), then the logical screen width (2) and height (2), little-endian
		if (bytes.Length < 10 || (!bytes.StartsWith(Gif87a) && !bytes.StartsWith(Gif89a)))
			return false;
		var width = BinaryPrimitives.ReadUInt16LittleEndian(bytes[6..]);
		var height = BinaryPrimitives.ReadUInt16LittleEndian(bytes[8..]);
		if (width == 0 || height == 0)
			return false;
		info = new ImageInfo(ImageFormat.Gif, width, height);
		return true;
	}

	private static bool TryInspectJpeg(ReadOnlySpan<byte> bytes, out ImageInfo info)
	{
		info = null;
		if (bytes.Length < 4 || bytes[0] != 0xFF || bytes[1] != 0xD8 || bytes[2] != 0xFF)
			return false;
		var i = 2;
		while (i < bytes.Length)
		{
			if (bytes[i] != 0xFF)
				return false;
			// any number of 0xFF fill bytes may precede a marker
			while (i < bytes.Length && bytes[i] == 0xFF)
				i++;
			if (i >= bytes.Length)
				return false;
			var marker = bytes[i++];
			// TEM and RST markers stand alone with no length
			if (marker == 0x01 || marker is >= 0xD0 and <= 0xD7)
				continue;
			// reaching the scan data (SOS) or end of image (EOI) before a frame header means there are no dimensions to read
			if (marker is 0x00 or 0xD8 or 0xD9 or 0xDA)
				return false;
			if (i + 2 > bytes.Length)
				return false;
			// the segment length counts its own two bytes
			var length = BinaryPrimitives.ReadUInt16BigEndian(bytes[i..]);
			if (length < 2 || i + length > bytes.Length)
				return false;
			if (IsStartOfFrame(marker))
			{
				// length (2), precision (1), height (2), width (2), component count (1)
				if (length < 8)
					return false;
				var height = BinaryPrimitives.ReadUInt16BigEndian(bytes[(i + 3)..]);
				var width = BinaryPrimitives.ReadUInt16BigEndian(bytes[(i + 5)..]);
				if (width == 0 || height == 0)
					return false;
				info = new ImageInfo(ImageFormat.Jpeg, width, height);
				return true;
			}
			i += length;
		}
		return false;
	}

	// SOF0-SOF15, excluding DHT (C4), JPG (C8) and DAC (CC), which share the range
	private static bool IsStartOfFrame(byte marker) => marker is >= 0xC0 and <= 0xCF and not 0xC4 and not 0xC8 and not 0xCC;
}
