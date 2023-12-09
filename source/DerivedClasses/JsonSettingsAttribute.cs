using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SettingsGenerator
{
	public class JsonSettingsAttribute : BaseSettingsAttribute
	{		
		public JsonSettingsAttribute(string fileName, bool includeProperties = false) : base(fileName, new SimpleErrorHandler(), _baseFilter, includeProperties) { }

		public JsonSettingsAttribute(string fileName, Type attributeLink, bool includeProperties = false) : base(fileName, attributeLink, new SimpleErrorHandler(), _baseFilter, includeProperties) { }

		protected override void ReadFile(FileStream file, out Dictionary<string, object?>? args)
		{
			JsonDocument json = JsonDocument.Parse(file);

			var loopMap = json.RootElement.Deserialize<Dictionary<string, KeyValuePair<string, object>>>();
			if (loopMap == null) { args = null; return; }
			Dictionary<string, object?> keyValuePairs = new();

			foreach (var a in loopMap)
			{
				JsonElement jsonElement = json.RootElement.GetProperty(a.Key);
				Type type = Type.GetType(a.Value.Key)!;

				var actualValue = jsonElement.GetProperty("Value").Deserialize(type);

				keyValuePairs.Add(a.Key, actualValue);
			}

			args = keyValuePairs;
		}
		protected override void WriteFile(FileStream file, Dictionary<string, object?> args)
		{
			Dictionary<string, KeyValuePair<string, object>> typeInjector = new();

			foreach (var item in args)
			{
				typeInjector.Add(item.Key, new(item.Value == null ? "null" : item.Value.GetType().ToString(), item.Value ?? "null"));
			}

			string json = JsonSerializer.Serialize(typeInjector);
			{
				using StreamWriter fileStream = new(file);

				fileStream.Write(json);
			}
		}
	}
}
