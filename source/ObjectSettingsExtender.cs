using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SettingsGenerator
{
	public static class ObjectSettingsExtender
	{
		public static void SaveSettings(this IGenerateSettings caller, bool inherit = false)
		{
			foreach (var attribute in GetSettingsAttributes(caller, inherit))
			{
				attribute.SaveSettingsByAttribute(caller);
			}
		}

		public static void LoadSettings(this IGenerateSettings caller, bool inherit = false)
		{
			foreach (var attribute in GetSettingsAttributes(caller, inherit))
			{
				attribute.LoadSettingsByAttribute(caller);
			}
		}

		private static BaseSettingsAttribute[] GetSettingsAttributes(IGenerateSettings caller, bool inherit)
		{
			List<BaseSettingsAttribute> holder = new();
			var attributes = caller.GetType().GetCustomAttributes(typeof(BaseSettingsAttribute), inherit) ?? throw new Exception("No SettingsOwnerAttribute was found");

			foreach (var attribute in attributes)
			{
				holder.Add((attribute as BaseSettingsAttribute)!);
			}

			return holder.ToArray();
		}
	}

	public interface IGenerateSettings { }
}
