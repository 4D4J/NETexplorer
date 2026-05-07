using System.Text;
using System.Text.RegularExpressions;
using dnlib.DotNet;

namespace UnityCrackTool.Core;

public static class SignatureFormatter
{
    private static readonly Regex ArityRegex = new(@"`\d+", RegexOptions.Compiled);

    public static string Format(MethodDef m)
    {
        var sb = new StringBuilder();

        sb.Append((m.Attributes & MethodAttributes.MemberAccessMask) switch
        {
            MethodAttributes.Public      => "public ",
            MethodAttributes.Family      => "protected ",
            MethodAttributes.Private     => "private ",
            MethodAttributes.Assembly    => "internal ",
            MethodAttributes.FamORAssem  => "protected internal ",
            MethodAttributes.FamANDAssem => "private protected ",
            _                            => ""
        });

        if (m.IsStatic)        sb.Append("static ");
        if (m.IsAbstract)      sb.Append("abstract ");
        else if (m.IsVirtual && !m.IsNewSlot) sb.Append("override ");
        else if (m.IsVirtual)  sb.Append("virtual ");

        sb.Append(FormatTypeSig(m.ReturnType));
        sb.Append(' ');
        sb.Append(m.Name.String);

        if (m.HasGenericParameters)
        {
            sb.Append('<');
            sb.Append(string.Join(", ", m.GenericParameters.Select(g => g.Name.String)));
            sb.Append('>');
        }

        sb.Append('(');
        var plist = m.Parameters.Where(p => p.IsNormalMethodParameter).ToList();
        for (int i = 0; i < plist.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(FormatTypeSig(plist[i].Type));
            if (!string.IsNullOrEmpty(plist[i].Name))
                sb.Append(' ').Append(plist[i].Name);
        }
        sb.Append(')');

        return sb.ToString();
    }

    private static string FormatTypeSig(TypeSig? ts)
    {
        if (ts is null) return "void";
        return ArityRegex.Replace(ts.TypeName, "");
    }
}
