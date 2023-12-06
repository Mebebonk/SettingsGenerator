using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingsGenerator
{
	public class BaseErrorHandler
	{
		private BaseSettingsAttribute? _settings;
		protected BaseSettingsAttribute Settings { get => _settings!; }

		public BaseErrorHandler() { }

		public bool LinkSettingsAttribute(BaseSettingsAttribute settingsAttribute)
		{
			if (_settings == null)
			{
				_settings = settingsAttribute;				
				return true;
			}
			else
			{
				return false;
			}
		}

		virtual public NoTargetMemberHandle HandleNoTargetMemberFound(object caller, KeyValuePair<string, object?> arg) => throw new Exception("No member found in target class");
		virtual public PayloadHandle HandleNonNullableNull(object caller, ref object? payload) => throw new Exception("Null found for non-nullable type");
		virtual public PayloadHandle HandleTypeMissmatch(object caller, ref object payload) => throw new Exception("Type missmatch");
		virtual public FileStream? HandleNoFileFound(object caller) => throw new Exception("No save file found");

		virtual public void HandleEmptyLoadFile(object caller) => throw new Exception("No fields read from file");
		virtual public void HandleNoMembersFound(object caller) => throw new Exception("No members found");
		virtual public void HandleNoMarkedMembersFound(object caller) => throw new Exception("No marked members found");
	}

	public enum PayloadHandle
	{
		Skip, ExplicitSet, Break
	}
	public enum NoTargetMemberHandle
	{
		Continue,
		Break
	}
}

