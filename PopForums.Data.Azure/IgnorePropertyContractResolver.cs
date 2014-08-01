using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PopForums.Data.Azure
{
	// based on a solution from SO user drzaus: http://stackoverflow.com/questions/6851899/is-there-a-way-to-make-javascriptserializer-ignore-properties-of-a-certain-gener/14510098#14510098
	public class IgnorePropertyContractResolver : DefaultContractResolver
	{
		protected readonly Dictionary<Type, HashSet<string>> Ignores;

		public IgnorePropertyContractResolver()
		{
			Ignores = new Dictionary<Type, HashSet<string>>();
		}

		public void Ignore(Type type, params string[] propertyName)
		{
			if (!Ignores.ContainsKey(type)) Ignores[type] = new HashSet<string>();
			foreach (var prop in propertyName)
				Ignores[type].Add(prop);
		}

		public bool IsIgnored(Type type, string propertyName)
		{
			if (!Ignores.ContainsKey(type)) return false;
			return Ignores[type].Count == 0 || Ignores[type].Contains(propertyName);
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			if (IsIgnored(property.DeclaringType, property.PropertyName))
				property.ShouldSerialize = instance => false;
			return property;
		}
	}
}