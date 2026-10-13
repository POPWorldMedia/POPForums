namespace PopForums.Test.Services;

public class ImageInspectorTests
{
	private static readonly ImageInspector Inspector = new();

	private static byte[] Png(uint width, uint height)
	{
		var bytes = new List<byte> { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13 };
		bytes.AddRange("IHDR"u8.ToArray());
		bytes.AddRange(BigEndian32(width));
		bytes.AddRange(BigEndian32(height));
		bytes.AddRange(new byte[] { 8, 6, 0, 0, 0, 0, 0, 0, 0 }); // rest of IHDR and a CRC
		return bytes.ToArray();
	}

	private static byte[] Gif(ushort width, ushort height, string version = "89a")
	{
		var bytes = new List<byte>();
		bytes.AddRange(System.Text.Encoding.ASCII.GetBytes("GIF" + version));
		bytes.AddRange(new[] { (byte)(width & 0xFF), (byte)(width >> 8), (byte)(height & 0xFF), (byte)(height >> 8) });
		bytes.AddRange(new byte[] { 0, 0, 0, 0x3B });
		return bytes.ToArray();
	}

	private static byte[] Jpeg(ushort width, ushort height, byte sofMarker = 0xC0, bool withExif = true)
	{
		var bytes = new List<byte> { 0xFF, 0xD8 };
		// APP0 (JFIF)
		bytes.AddRange(Segment(0xE0, "JFIF\0"u8.ToArray().Concat(new byte[] { 1, 1, 0, 0, 1, 0, 1, 0, 0 }).ToArray()));
		if (withExif)
		{
			// an APP1 segment with an embedded thumbnail frame header that must not be mistaken for the real one
			var exif = new List<byte>();
			exif.AddRange("Exif\0\0"u8.ToArray());
			exif.AddRange(new byte[] { 0xFF, 0xC0, 0x00, 0x11, 0x08, 0x00, 0x01, 0x00, 0x01, 0x03 });
			exif.AddRange(new byte[20]);
			bytes.AddRange(Segment(0xE1, exif.ToArray()));
		}
		// DQT
		bytes.AddRange(Segment(0xDB, new byte[65]));
		// SOF: precision, height, width, component count, then 3 components
		var sof = new List<byte> { 8 };
		sof.AddRange(BigEndian16(height));
		sof.AddRange(BigEndian16(width));
		sof.AddRange(new byte[] { 3, 1, 0x22, 0, 2, 0x11, 1, 3, 0x11, 1 });
		bytes.AddRange(Segment(sofMarker, sof.ToArray()));
		// DHT, SOS, a little entropy data and EOI
		bytes.AddRange(Segment(0xC4, new byte[20]));
		bytes.AddRange(Segment(0xDA, new byte[] { 3, 1, 0, 2, 0x11, 3, 0x11, 0, 0x3F, 0 }));
		bytes.AddRange(new byte[] { 0x12, 0x34, 0xFF, 0x00, 0x56, 0xFF, 0xD9 });
		return bytes.ToArray();
	}

	private static byte[] Segment(byte marker, byte[] payload)
	{
		var length = (ushort)(payload.Length + 2);
		return new byte[] { 0xFF, marker }.Concat(BigEndian16(length)).Concat(payload).ToArray();
	}

	private static byte[] BigEndian16(ushort value) => new[] { (byte)(value >> 8), (byte)(value & 0xFF) };

	private static byte[] BigEndian32(uint value) => new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };

	public class TryInspect
	{
		[Fact]
		public void ReadsPng()
		{
			var result = Inspector.TryInspect(Png(640, 480), out var info);

			Assert.True(result);
			Assert.Equal(new ImageInfo(ImageFormat.Png, 640, 480), info);
			Assert.Equal("image/png", info.ContentType);
		}

		[Theory]
		[InlineData("87a")]
		[InlineData("89a")]
		public void ReadsGif(string version)
		{
			var result = Inspector.TryInspect(Gif(300, 200, version), out var info);

			Assert.True(result);
			Assert.Equal(new ImageInfo(ImageFormat.Gif, 300, 200), info);
			Assert.Equal("image/gif", info.ContentType);
		}

		[Fact]
		public void ReadsJpegPastExifThumbnail()
		{
			var result = Inspector.TryInspect(Jpeg(1024, 768), out var info);

			Assert.True(result);
			Assert.Equal(new ImageInfo(ImageFormat.Jpeg, 1024, 768), info);
			Assert.Equal("image/jpeg", info.ContentType);
		}

		[Theory]
		[InlineData(0xC0)]
		[InlineData(0xC1)]
		[InlineData(0xC2)]
		[InlineData(0xC3)]
		[InlineData(0xC5)]
		[InlineData(0xC7)]
		[InlineData(0xC9)]
		[InlineData(0xCB)]
		[InlineData(0xCD)]
		[InlineData(0xCF)]
		public void ReadsEveryJpegFrameType(byte sofMarker)
		{
			var result = Inspector.TryInspect(Jpeg(50, 60, sofMarker, false), out var info);

			Assert.True(result);
			Assert.Equal(50, info.Width);
			Assert.Equal(60, info.Height);
		}

		[Fact]
		public void ReadsJpegWithFillBytesAndRestartMarkers()
		{
			var jpeg = Jpeg(20, 10, withExif: false).ToList();
			// fill bytes before the first marker after SOI, and a standalone RST marker
			jpeg.InsertRange(2, new byte[] { 0xFF, 0xFF, 0xFF, 0xD0 });

			var result = Inspector.TryInspect(jpeg.ToArray(), out var info);

			Assert.True(result);
			Assert.Equal(20, info.Width);
			Assert.Equal(10, info.Height);
		}

		[Fact]
		public void RejectsJpegWithScanBeforeFrame()
		{
			var bytes = new List<byte> { 0xFF, 0xD8 };
			bytes.AddRange(new byte[] { 0xFF, 0xDA, 0x00, 0x04, 0x00, 0x00 });

			Assert.False(Inspector.TryInspect(bytes.ToArray(), out var info));
			Assert.Null(info);
		}

		[Fact]
		public void RejectsJpegWithEndBeforeFrame()
		{
			Assert.False(Inspector.TryInspect(new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 }, out _));
		}

		[Fact]
		public void RejectsJpegWithSegmentLengthPastEnd()
		{
			Assert.False(Inspector.TryInspect(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x7F, 0xFF, 0x00 }, out _));
		}

		[Fact]
		public void RejectsJpegWithSegmentLengthUnderTwo()
		{
			Assert.False(Inspector.TryInspect(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x01, 0xFF, 0xC0 }, out _));
		}

		[Fact]
		public void RejectsJpegWithShortFrameHeader()
		{
			Assert.False(Inspector.TryInspect(new byte[] { 0xFF, 0xD8, 0xFF, 0xC0, 0x00, 0x05, 0x08, 0x00, 0x10 }, out _));
		}

		[Fact]
		public void RejectsJpegWithGarbageBetweenSegments()
		{
			var jpeg = Jpeg(20, 10, withExif: false).ToList();
			jpeg.Insert(2, 0x42);

			Assert.False(Inspector.TryInspect(jpeg.ToArray(), out _));
		}

		[Fact]
		public void RejectsZeroDimensions()
		{
			Assert.False(Inspector.TryInspect(Jpeg(0, 10), out _));
			Assert.False(Inspector.TryInspect(Jpeg(10, 0), out _));
			Assert.False(Inspector.TryInspect(Png(0, 10), out _));
			Assert.False(Inspector.TryInspect(Png(10, 0), out _));
			Assert.False(Inspector.TryInspect(Gif(0, 10), out _));
			Assert.False(Inspector.TryInspect(Gif(10, 0), out _));
		}

		[Fact]
		public void RejectsPngDimensionsBeyondInt()
		{
			Assert.False(Inspector.TryInspect(Png(0x80000000, 10), out _));
		}

		[Fact]
		public void RejectsPngWithoutIhdrFirst()
		{
			var png = Png(10, 10);
			png[12] = (byte)'X';

			Assert.False(Inspector.TryInspect(png, out _));
		}

		[Fact]
		public void RejectsUnsupportedFormats()
		{
			Assert.False(Inspector.TryInspect(System.Text.Encoding.ASCII.GetBytes("RIFF\x10\0\0\0WEBPVP8 "), out _));
			Assert.False(Inspector.TryInspect(new byte[] { 0x49, 0x49, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00, 0, 0, 0, 0 }, out _));
			Assert.False(Inspector.TryInspect(new byte[] { 0x42, 0x4D, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, out _));
			Assert.False(Inspector.TryInspect("<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>"u8, out _));
			Assert.False(Inspector.TryInspect("<html><script>alert(1)</script></html>"u8, out _));
		}

		[Fact]
		public void RejectsEmpty()
		{
			Assert.False(Inspector.TryInspect(Array.Empty<byte>(), out var info));
			Assert.Null(info);
		}

		[Fact]
		public void RejectsEveryTruncationOfValidHeaders()
		{
			var samples = new[] { Png(10, 10), Gif(10, 10), Jpeg(10, 10) };
			foreach (var sample in samples)
			{
				Inspector.TryInspect(sample, out var full);
				for (var length = 0; length < sample.Length; length++)
				{
					// a truncated file either fails or still reports the real header, never anything else
					if (Inspector.TryInspect(sample.AsSpan(0, length), out var info))
						Assert.Equal(full, info);
				}
			}
		}

		[Fact]
		public void NeverThrowsOnRandomInput()
		{
			var random = new Random(4242);
			var prefixes = new[] { new byte[] { 0xFF, 0xD8, 0xFF }, Png(1, 1)[..16], "GIF89a"u8.ToArray(), Array.Empty<byte>() };
			for (var i = 0; i < 20000; i++)
			{
				var prefix = prefixes[i % prefixes.Length];
				var bytes = new byte[prefix.Length + random.Next(0, 64)];
				random.NextBytes(bytes);
				prefix.CopyTo(bytes, 0);
				// skew toward markers so the JPEG walker sees lots of plausible segments
				for (var j = prefix.Length; j < bytes.Length; j++)
					if (random.Next(4) == 0)
						bytes[j] = 0xFF;

				var exception = Record.Exception(() => Inspector.TryInspect(bytes, out _));

				Assert.Null(exception);
			}
		}
	}

	public class Validate
	{
		[Fact]
		public void ValidWhenWithinLimits()
		{
			var result = Inspector.Validate(Jpeg(100, 50), 100, 50, 1, out var info);

			Assert.Equal(ImageValidationResult.Valid, result);
			Assert.Equal(ImageFormat.Jpeg, info.Format);
		}

		[Fact]
		public void TooLargeWhenWidthOver()
		{
			Assert.Equal(ImageValidationResult.TooLarge, Inspector.Validate(Png(101, 50), 100, 50, 1, out _));
		}

		[Fact]
		public void TooLargeWhenHeightOver()
		{
			Assert.Equal(ImageValidationResult.TooLarge, Inspector.Validate(Gif(100, 51), 100, 50, 1, out _));
		}

		[Fact]
		public void TooLargeWhenBytesOver()
		{
			var png = Png(10, 10).Concat(new byte[1024]).ToArray();

			Assert.Equal(ImageValidationResult.TooLarge, Inspector.Validate(png, 100, 100, 1, out _));
		}

		[Fact]
		public void InvalidWhenNotAnImage()
		{
			Assert.Equal(ImageValidationResult.Invalid, Inspector.Validate(new byte[100], 100, 100, 1, out var info));
			Assert.Null(info);
		}

		[Fact]
		public void InvalidWhenNull()
		{
			Assert.Equal(ImageValidationResult.Invalid, Inspector.Validate(null, 100, 100, 1, out _));
		}
	}
}
