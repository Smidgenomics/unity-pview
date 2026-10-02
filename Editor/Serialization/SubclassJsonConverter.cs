// smidgens @ github

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	/// <summary>
	/// Deserialization converter that instantiates a default instance for missing type
	/// </summary>
	internal sealed class SubclassJsonConverter<T> : JsonConverter<T> where T : class
	{
		public SubclassJsonConverter(string typeProperty = "$type", Type defaultType = null)
		{
			_typeProperty = typeProperty;
			_defaultType = defaultType;
		}

		private readonly string _typeProperty;
		private readonly Type _defaultType;

		public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
		{
			throw new NotImplementedException("Cannot write");
		}

		public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			JObject jo = null;
			try
			{
				jo = JObject.Load(reader);
			}
			catch
			{
				return null;
			}

			var typeName = (string)jo.GetValue(_typeProperty);

			var type = serializer.SerializationBinder.BindToType("", typeName ?? "") ?? _defaultType;

			if (type == null || type.IsAbstract || !typeof(T).IsAssignableFrom(type))
			{
				return null;
			}
			var instance = (T)Activator.CreateInstance(type);
			serializer.Populate(jo.CreateReader(), instance);
			return instance;
		}

	}
}



