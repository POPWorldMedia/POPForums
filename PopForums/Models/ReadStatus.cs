using System;

namespace PopForums.Models
{
	[Flags]
	public enum ReadStatus
	{
		NoNewPosts = 1,
		NewPosts = 2,
		Closed = 4,
		Open = 8,
		Pinned = 16,
		NotPinned = 32
	}
}