using System.Globalization;
using System.Reflection;
using System.Text;

namespace Blazorme.Tests;

/// <summary>
/// Renders the public surface of an assembly as deterministic text.
///
/// Reads metadata only, via <see cref="MetadataLoadContext"/>, so it works on assemblies this
/// test project could never load for execution — <c>Blazorme.StreamSaver</c> and
/// <c>Blazorme.FFmpeg</c> target net5.0 only, and <c>Blazorme.TestHost</c> throws on net10.0.
/// </summary>
internal static class PublicApiDumper
{
    internal static string Dump(IEnumerable<string> assemblyNames) =>
        Dump(assemblyNames.Select(n => (n, TestAssemblies.LocateDll(n))));

    /// <summary>Dumps specific assembly files, so one multi-targeted library's frameworks can be
    /// compared against each other.</summary>
    internal static string Dump(IEnumerable<(string Name, string Path)> assemblies)
    {
        var resolver = new PathAssemblyResolver(
            TestAssemblies.ProbingDirectories()
                .Distinct()
                .SelectMany(d => Directory.GetFiles(d, "*.dll"))
                .GroupBy(Path.GetFileNameWithoutExtension)
                .Select(g => g.First()));

        using var context = new MetadataLoadContext(resolver);
        var sb = new StringBuilder();

        foreach (var (name, path) in assemblies)
        {
            var assembly = context.LoadFromAssemblyPath(path);
            sb.Append("assembly ").Append(name).AppendLine();

            foreach (var type in assembly.GetExportedTypes().OrderBy(t => t.FullName, StringComparer.Ordinal))
            {
                AppendType(sb, type);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static void AppendType(StringBuilder sb, Type type)
    {
        sb.Append("  type ").Append(Render(type)).Append(" : ").Append(Kind(type)).AppendLine();

        if (type.IsEnum)
        {
            // Numeric values are part of the contract: callers may have persisted them as ints.
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static)
                         .OrderBy(f => Convert.ToInt64(f.GetRawConstantValue(), CultureInfo.InvariantCulture)))
            {
                sb.Append("    ").Append(field.Name).Append(" = ")
                  .Append(Convert.ToString(field.GetRawConstantValue(), CultureInfo.InvariantCulture))
                  .AppendLine();
            }

            return;
        }

        foreach (var member in Members(type).OrderBy(RenderMember, StringComparer.Ordinal))
        {
            sb.Append("    ").Append(RenderMember(member)).AppendLine();
        }
    }

    private static IEnumerable<MemberInfo> Members(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var m in type.GetMembers(flags))
        {
            if (m is MethodInfo { IsSpecialName: true }) continue;   // property/event accessors
            if (m is Type) continue;                                  // nested types come from GetExportedTypes
            if (Visible(m)) yield return m;
        }
    }

    private static bool Visible(MemberInfo member) => member switch
    {
        FieldInfo f => f.IsPublic || f.IsFamily || f.IsFamilyOrAssembly,
        MethodBase m => m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly,
        PropertyInfo p => Visible(p.GetMethod ?? (MemberInfo)p.SetMethod!),
        EventInfo e => Visible(e.AddMethod!),
        _ => false,
    };

    private static string RenderMember(MemberInfo member) => member switch
    {
        FieldInfo f when f.IsLiteral =>
            $"const {Render(f.FieldType)} {f.Name} = {Literal(f.GetRawConstantValue())}",
        FieldInfo f =>
            $"field {(f.IsStatic ? "static " : "")}{Render(f.FieldType)} {f.Name}",
        ConstructorInfo c =>
            $"ctor .ctor({Parameters(c)})",
        MethodInfo m =>
            $"method {(m.IsStatic ? "static " : "")}{(m.IsVirtual && !m.IsFinal && !m.DeclaringType!.IsInterface ? "virtual " : "")}"
            + $"{Render(m.ReturnType)} {m.Name}{GenericParameters(m)}({Parameters(m)})",
        PropertyInfo p =>
            $"property {Render(p.PropertyType)} {p.Name} {{ {(p.GetMethod is not null && Visible(p.GetMethod) ? "get; " : "")}"
            + $"{(p.SetMethod is not null && Visible(p.SetMethod) ? "set; " : "")}}}",
        EventInfo e =>
            $"event {Render(e.EventHandlerType!)} {e.Name}",
        _ => member.ToString()!,
    };

    /// <summary>Arity matters: three overloads of AddComponent&lt;T&gt; differ only by their parameters.</summary>
    private static string GenericParameters(MethodBase method) =>
        method.IsGenericMethodDefinition
            ? "<" + string.Join(", ", method.GetGenericArguments().Select(a => a.Name)) + ">"
            : string.Empty;

    private static string Parameters(MethodBase method) =>
        string.Join(", ", method.GetParameters().Select(p =>
            $"{Render(p.ParameterType)} {p.Name}"
            + (p.HasDefaultValue ? $" = {Literal(p.RawDefaultValue)}" : string.Empty)));

    private static string Literal(object? value) => value switch
    {
        null => "null",
        string s => "\"" + s + "\"",
        bool b => b ? "true" : "false",
        _ => Convert.ToString(value, CultureInfo.InvariantCulture)!,
    };

    private static string Kind(Type type) =>
        type.IsEnum ? "enum " + Render(type.GetEnumUnderlyingType())
        : type.IsInterface ? "interface"
        : type.IsValueType ? "struct"
        : (type.IsAbstract && type.IsSealed) ? "static class"
        : $"{(type.IsAbstract ? "abstract " : "")}{(type.IsSealed ? "sealed " : "")}class";

    private static string Render(Type type)
    {
        if (type.IsArray) return Render(type.GetElementType()!) + "[]";
        if (type.IsByRef) return "ref " + Render(type.GetElementType()!);
        if (!type.IsGenericType) return type.FullName ?? type.Name;

        var name = (type.GetGenericTypeDefinition().FullName ?? type.Name);
        name = name[..name.IndexOf('`')];
        return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(Render))}>";
    }
}
