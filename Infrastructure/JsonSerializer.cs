using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Infrastructure
{
	public static class JsonSerializer
	{
		static readonly Newtonsoft.Json.JsonSerializer Serializer;

		static JsonSerializer()
		{
			Serializer = new Newtonsoft.Json.JsonSerializer
			{
				TypeNameHandling = TypeNameHandling.None,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				
			};
		}

		public static byte[] Serialize(object graph)
		{
			using (var stream = new MemoryStream())
			{
				Serialize(stream, graph);
				return stream.ToArray();
			}
		}

		public static void Serialize(Stream output, object graph)
		{
			JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(output, new UTF8Encoding(false)));
			Serializer.Serialize(jsonWriter, graph);
			jsonWriter.Flush();
		}

		public static T Deserialize<T>(byte[] serilized)
		{
			return (T) Deserialize(serilized, typeof (T));
		}

		public static object Deserialize(byte[] serilized, Type objectType)
		{
			using (var stream = new MemoryStream(serilized))
			{
				return Deserialize(stream, objectType);
			}
		}

		public static object Deserialize(Stream input, Type objectType)
		{
			using (var jsonReader = new JsonTextReader(new StreamReader(input, Encoding.UTF8)))
			{
				return Serializer.Deserialize(jsonReader, objectType);
			}
		}
	}
}
