using System.Reflection;
using System.Runtime.InteropServices.Marshalling;

namespace SettingsGenerator
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	abstract public class BaseSettingsAttribute : Attribute
	{
		protected const BindingFlags _baseFilter = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
		private readonly BindingFlags _filter;

		private readonly string _fileName;
		private readonly bool _includeProperties;
		private readonly Type _attributeLink = typeof(SaveLoadAttribute);
		private readonly BaseErrorHandler _errorHandler;

		public BaseSettingsAttribute(string fileName, BaseErrorHandler? errorHandler = null, BindingFlags filter = _baseFilter, bool includeProperties = false)
		{
			_fileName = RealizeFilePath(fileName);
			_filter = filter;
			_includeProperties = includeProperties;
			_errorHandler = errorHandler ?? new();
			_errorHandler.LinkSettingsAttribute(this);
		}
		public BaseSettingsAttribute(string fileName, Type attributeLink, BaseErrorHandler? errorHandler = null, BindingFlags filter = _baseFilter, bool includeProperties = false) : this(fileName, errorHandler, filter, includeProperties)
		{
			if (attributeLink.IsAssignableFrom(typeof(SaveLoadAttribute))) { _attributeLink = attributeLink; }
		}

		public void SaveSettingsByAttribute(object caller)
		{
			MemberInfo[] members = caller.GetType().GetMembers(_filter);

			if (members.Length == 0) { _errorHandler.HandleNoMembersFound(caller); return; }

			Dictionary<string, object?> settings = new();

			foreach (MemberInfo member in members)
			{
				if (GetLinkedAttribute(member) != null)
				{
					string memberName = member.Name;

					if (member.MemberType == MemberTypes.Property && _includeProperties)
					{
						settings.Add(memberName, caller.GetType().GetProperty(member.Name, _filter)!.GetValue(caller));
					}
					else if (member.MemberType == MemberTypes.Field)
					{
						settings.Add(memberName, caller.GetType().GetField(member.Name, _filter)!.GetValue(caller));
					}
				}
			}

			if (settings.Count == 0) { _errorHandler.HandleNoMarkedMembersFound(caller); return; }

			using FileStream file = File.Open($"{_fileName}", FileMode.Create);

			WriteFile(file, settings);
		}
		public void LoadSettingsByAttribute(object caller)
		{
			MemberInfo[] members = caller.GetType().GetMembers(_filter);

			if (members.Length == 0) { _errorHandler.HandleNoMembersFound(caller); return; }

			FileStream file;			
			
			try
			{
				file = File.OpenRead($"{_fileName}");
			}
			catch (FileNotFoundException)
			{
				var vFile = _errorHandler.HandleNoFileFound(caller);
				if (vFile == null) { return; }
				file = vFile;
			}

			ReadFile(file, out Dictionary<string, object?>? args);
			file.Dispose();

			if (args == null || args.Count == 0) { _errorHandler.HandleEmptyLoadFile(caller); return; }

			foreach (var arg in args)
			{
				MemberInfo member;
				try
				{
					member = members.First(memb => memb.Name == arg.Key);
				}
				catch
				{
					if (_errorHandler.HandleNoTargetMemberFound(caller, arg) == NoTargetMemberHandle.Continue) { continue; } else { break; }
				}

				if (GetLinkedAttribute(member) != null)
				{

					if (member.MemberType == MemberTypes.Property && _includeProperties)
					{
						if (LoadProperty(caller, member, arg)) { continue; } else { break; }
					}
					else if (member.MemberType == MemberTypes.Field)
					{
						if (LoadField(caller, member, arg)) { continue; } else { break; }
					}
				}
			}
		}

		private PayloadHandle HandleLoadError(object caller, ref object? newValue, Type oldValueType)
		{
			if (newValue == null)
			{
				if (!IsNullable(oldValueType))
				{
					return _errorHandler.HandleNonNullableNull(caller, ref newValue);
				}
			}
			else if (!newValue.GetType().IsAssignableTo(oldValueType) && newValue != null)
			{
				return _errorHandler.HandleTypeMissmatch(caller, ref newValue);
			}

			return PayloadHandle.ExplicitSet;
		}
		private bool LoadProperty(object caller, MemberInfo member, KeyValuePair<string, object?> arg)
		{
			PropertyInfo prop = caller.GetType().GetProperty(member.Name, _filter)!;

			Type propertyType = prop.PropertyType;
			object? propertyValue = arg.Value;

			var payloadHandle = HandleLoadError(caller, ref propertyValue, propertyType);
			if (payloadHandle != PayloadHandle.ExplicitSet) { return payloadHandle == PayloadHandle.Skip; }

			prop.SetValue(caller, propertyValue);

			return true;
		}
		private bool LoadField(object caller, MemberInfo member, KeyValuePair<string, object?> arg)
		{
			FieldInfo field = caller.GetType().GetField(member.Name, _filter)!;

			Type fieldType = field.FieldType;
			var fieldValue = arg.Value;

			var payloadHandle = HandleLoadError(caller, ref fieldValue, fieldType);
			if (payloadHandle != PayloadHandle.ExplicitSet) { return payloadHandle == PayloadHandle.Skip; }

			field.SetValue(caller, fieldValue);

			return true;
		}
		private static bool IsNullable(Type type) => Nullable.GetUnderlyingType(type) != null;
		private Attribute? GetLinkedAttribute(MemberInfo member)
		{
			try
			{
				return member.GetCustomAttributes().First(x => x.GetType().IsAssignableFrom(_attributeLink));
			}
			catch { return null; }
		}

		virtual protected string RealizeFilePath(string path) => path;

		abstract protected void ReadFile(FileStream file, out Dictionary<string, object?>? args);
		abstract protected void WriteFile(FileStream file, Dictionary<string, object?> args);
	}
}