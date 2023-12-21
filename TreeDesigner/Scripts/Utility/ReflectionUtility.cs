using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TreeDesigner
{
	public static partial class ReflectionUtility
	{
		static Dictionary<Type, List<Type>> s_CachedSelfAndBaseTypesMap = new Dictionary<Type, List<Type>>();
		static Dictionary<Type, FieldInfo[]> s_CachedTypeFieldInfoMap = new Dictionary<Type, FieldInfo[]>();
		static Dictionary<Type, PropertyInfo[]> s_CachedTypePropertyInfoMap = new Dictionary<Type, PropertyInfo[]>();

		public static IEnumerable<FieldInfo> GetAllFields(this object target, Func<FieldInfo, bool> predicate = null)
		{
			if (target == null)
			{
				Debug.LogError("The target object is null. Check for missing scripts.");
				yield break;
			}

			List<Type> types = GetSelfAndBaseTypes(target);
			for (int i = types.Count - 1; i >= 0; i--)
			{
				if (!s_CachedTypeFieldInfoMap.ContainsKey(types[i]))
					s_CachedTypeFieldInfoMap.Add(types[i], types[i].GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly));
				
				IEnumerable<FieldInfo> fieldInfos = s_CachedTypeFieldInfoMap[types[i]];
				if (predicate != null)
					fieldInfos = fieldInfos.Where(predicate);

				foreach (var fieldInfo in fieldInfos)
				{
					yield return fieldInfo;
				}
			}
		}
		public static IEnumerable<PropertyInfo> GetAllProperties(this object target, Func<PropertyInfo, bool> predicate = null)
		{
			if (target == null)
			{
				Debug.LogError("The target object is null. Check for missing scripts.");
				yield break;
			}

			List<Type> types = GetSelfAndBaseTypes(target);
			for (int i = types.Count - 1; i >= 0; i--)
			{
				if (!s_CachedTypePropertyInfoMap.ContainsKey(types[i]))
					s_CachedTypePropertyInfoMap.Add(types[i], types[i].GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly));

				IEnumerable<PropertyInfo> propertyInfos = s_CachedTypePropertyInfoMap[types[i]];
				if (predicate != null)
					propertyInfos = propertyInfos.Where(predicate);

				foreach (var propertyInfo in propertyInfos)
				{
					yield return propertyInfo;
				}
			}
		}
		public static IEnumerable<MethodInfo> GetAllMethods(this object target, Func<MethodInfo, bool> predicate = null)
		{
			if (target == null)
			{
				Debug.LogError("The target object is null. Check for missing scripts.");
				yield break;
			}

			List<Type> types = GetSelfAndBaseTypes(target);
			for (int i = types.Count - 1; i >= 0; i--)
			{
				IEnumerable<MethodInfo> methodInfos = types[i]
					.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
					.Where(predicate);

				foreach (var methodInfo in methodInfos)
				{
					yield return methodInfo;
				}
			}
		}
		public static IEnumerable<MethodInfo> GetAllMethods(this Type targetType, Func<MethodInfo, bool> predicate = null)
        {
			if (targetType == null)
			{
				Debug.LogError("The target object is null. Check for missing scripts.");
				yield break;
			}

			List<Type> types = GetSelfAndBaseTypes(targetType);
			for (int i = types.Count - 1; i >= 0; i--)
			{
				IEnumerable<MethodInfo> methodInfos = types[i]
					.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
					.Where(predicate);

				foreach (var methodInfo in methodInfos)
				{
					yield return methodInfo;
				}
			}
		}

		public static FieldInfo GetField(this object target, string fieldName)
		{
			return GetAllFields(target, f => f.Name.Equals(fieldName, StringComparison.Ordinal)).FirstOrDefault();
		}
		public static PropertyInfo GetProperty(this object target, string propertyName)
		{
			return GetAllProperties(target, p => p.Name.Equals(propertyName, StringComparison.Ordinal)).FirstOrDefault();
		}

		public static MethodInfo GetMethod(this Type targetType, string methodName)
        {
			return GetAllMethods(targetType, m => m.Name.Equals(methodName, StringComparison.Ordinal)).FirstOrDefault();
		}
		public static MethodInfo GetMethod(this object target, string methodName)
		{
			return GetAllMethods(target, m => m.Name.Equals(methodName, StringComparison.Ordinal)).FirstOrDefault();
		}

        static List<Type> GetSelfAndBaseTypes(object target)
		{
			Type targetType = target.GetType();
			return GetSelfAndBaseTypes(targetType);
		}
		static List<Type> GetSelfAndBaseTypes(Type targetType)
		{
			if (!s_CachedSelfAndBaseTypesMap.ContainsKey(targetType))
			{
				List<Type> types = new List<Type>() { targetType };
				while (types.Last().BaseType != null)
				{
					types.Add(types.Last().BaseType);
				}
				s_CachedSelfAndBaseTypesMap.Add(targetType, types);
			}
			return s_CachedSelfAndBaseTypesMap[targetType];
		}

		public static bool IsSubClassOfRawGeneric(this Type type, Type generic)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (generic == null) throw new ArgumentNullException(nameof(generic));

			while (type != null && type != typeof(object))
			{
				if (IsTheRawGenericType(type)) 
					return true;
				type = type.BaseType;
			}

			return false;

			bool IsTheRawGenericType(Type test)
			=> generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
		}
	}
}