using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SettingsGenerator
{
	internal class JsonSettingsAttribute : BaseSettingsAttribute
	{
		public JsonSettingsAttribute(string fileName, BaseErrorHandler? errorHandler = null, BindingFlags filter = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, bool includeProperties = false) : base(fileName, errorHandler, filter, includeProperties) { }

		public JsonSettingsAttribute(string fileName, Type attributeLink, BaseErrorHandler? errorHandler = null, BindingFlags filter = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, bool includeProperties = false) : base(fileName, attributeLink, errorHandler, filter, includeProperties) { }

		protected override void ReadFile(FileStream file, out Dictionary<string, object?>? args)
		{
			JsonDocument json = JsonDocument.Parse(file);

			var loopMap = json.RootElement.Deserialize<Dictionary<string, KeyValuePair<string, object>>>();
			if (loopMap == null) { args = null; return; }
			Dictionary<string, object?> keyValuePairs = new();

			foreach (var a in loopMap)
			{
				keyValuePairs.Add(a.Key, a.Value.Value);
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
