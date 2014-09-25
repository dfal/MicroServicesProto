using System.Text;
using Infrastructure;

namespace WebApi
{
	static class ToBytesExtensions
	{
		public static byte[] ToBytes(this string s)
		{
			return Encoding.UTF8.GetBytes(s);
		}

		public static byte[] ToBytes(this string s, params object[] parameters)
		{
			return Encoding.UTF8.GetBytes(string.Format(s, parameters));
		}

		public static byte[] ToJsonBytes(this object obj)
		{
			return JsonSerializer.Serialize(obj);
		}
	}
}