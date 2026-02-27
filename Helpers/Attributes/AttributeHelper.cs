namespace Mod.Helpers.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class AttributeHelper
{
    public static IEnumerable<(Type type, T attr)> GetTypesWithAttribute<T>() where T : Attribute =>
        AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Select(t => (type: t, attr: t.GetCustomAttribute<T>()))
            .Where(x => x.attr != null);

    public static IEnumerable<(FieldInfo field, T attr)> GetFieldsWithAttribute<T>() where T : Attribute =>
        AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                          BindingFlags.Instance | BindingFlags.Static))
            .Select(f => (field: f, attr: f.GetCustomAttribute<T>()))
            .Where(x => x.attr != null);
}