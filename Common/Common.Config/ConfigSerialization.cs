using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Common.Configuration
{
	using Reflection;

	partial class Config
	{
		class ConfigContractResolver: DefaultContractResolver
		{
			// serialize only fields (including private and readonly, except static)
			// don't serialize properties
			protected override List<MemberInfo> GetSerializableMembers(Type objectType) =>
				objectType.fields().Where(field => !field.IsStatic).Cast<MemberInfo>().ToList();

			// we can deserialize all members and serialize members without LoadOnly attribute
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var property = base.CreateProperty(member, memberSerialization);
				property.Writable = property.Readable = true;

				return property;
			}
		}

		static JsonSerializerSettings _initSerializer(Type _)
		{
			JsonSerializerSettings settings = new()
			{
				Formatting = Formatting.Indented,
				ContractResolver = new ConfigContractResolver(),
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				Converters = { new StringEnumConverter() }
			};

			return settings;
		}

		JsonSerializerSettings srzSettings;

		string serialize() => JsonConvert.SerializeObject(this, srzSettings ??= _initSerializer(GetType()));

		static Config deserialize(string text, Type configType) => JsonConvert.DeserializeObject(text, configType, _initSerializer(configType)) as Config;
	}
}