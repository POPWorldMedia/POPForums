using System.Web;

namespace PopForums.Extensions
{
	public static class HttpPostedFileBases
	{
		public static byte[] GetBytes(this HttpPostedFileBase file)
		{
			var length = (int) file.InputStream.Length;
			var bytes = new byte[length];
			file.InputStream.Read(bytes, 0, length);
			return bytes;
		}
	}
}
