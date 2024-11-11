
namespace Chofu.RestApi {

	using System;
	using LitJson;
	using System.Linq;

	public static class Responce {

		private static void RegisterImporters() {

			JsonMapper.RegisterImporter<string, short>((string value) => { return short.Parse(value); });
			JsonMapper.RegisterImporter<string, int>((string value) => { return int.Parse(value); });
			JsonMapper.RegisterImporter<string, long>((string value) => { return long.Parse(value); });

			JsonMapper.RegisterImporter<short, string>((short value) => { return value.ToString(); });
			JsonMapper.RegisterImporter<int, string>((int value) => { return value.ToString(); });
			JsonMapper.RegisterImporter<long, string>((long value) => { return value.ToString(); });

			JsonMapper.RegisterImporter<short, int>((short value) => { return (int)value; });
			JsonMapper.RegisterImporter<int, long>((int value) => { return (long)value; });

			JsonMapper.RegisterImporter<int, bool>((int value) => { return value > 0; });
			JsonMapper.RegisterImporter<string, bool>((string value) => { return int.Parse(value) > 0; });
			JsonMapper.RegisterImporter<string, int[]>((string value) => {
				char[] separators = new char[] { ' ', ',', '[', ']' };
				return (from s in value.Replace(separators, "|").Split('|') where !string.IsNullOrEmpty(s) select int.Parse(s)).ToArray();
			});

		}

		public static bool ReadErrors(string message, out ClearResponce errors) {
			RegisterImporters();
			var msg = JsonMapper.ToObject<ClearResponce>(message);
			if (string.IsNullOrEmpty(msg.messages) || string.IsNullOrWhiteSpace(msg.messages)) {
				errors = msg;
				return false;
			} else {
				errors = msg;
				return true;
			}
		}

		public static Responce<T> Read<T>(string message) {
			RegisterImporters();
			try {
				return new Responce<T>() { status = true, data = JsonMapper.ToObject<T>(message) };
			} catch {
				return new Responce<T>() { status = false, data = default };
			}
		}

	}

	public struct ClearResponce {
		public string messages;
		public string[] errors;
	}

	public struct Responce<T> {
		public bool status;
		public T data;
		public static Responce<T> Read(string message) => Responce.Read<T>(message);
	}

	[Serializable]
	public struct EmptyData {
		public string[] values;
	}

}