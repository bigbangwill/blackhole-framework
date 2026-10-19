using System;
using System.Text.RegularExpressions;

namespace Blackhole.Editor
{
    internal static class RootModuleScopeSource
    {
        internal static string EnsureEntryPointRegistration(string source, string entryPointTypeName)
        {
            string entryPointName = entryPointTypeName.Substring(entryPointTypeName.LastIndexOf('.') + 1);
            const string triviaPattern = "@\"(?:[^\"]|\"\")*\"|\"(?:\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*'|//[^\\r\\n]*|/\\*[\\s\\S]*?\\*/";
            string code = Regex.Replace(source, triviaPattern, match => Regex.Replace(match.Value, @"[^\r\n]", " "));
            string entryPointPattern = "(?:" + Regex.Escape(entryPointTypeName) + "|" + Regex.Escape(entryPointName) + ")";
            string registrationPattern = @"\bRegisterEntryPoint\s*<\s*(?:global::)?" + entryPointPattern + @"\s*>\s*\(";
            if (Regex.IsMatch(code, registrationPattern))
                return source;

            MatchCollection methods = Regex.Matches(code, @"(?m)^([ \t]*)protected\s+override\s+void\s+Configure\s*\(\s*(?:global::)?(?:VContainer\.)?IContainerBuilder\s+(\w+)\s*\)\s*\{");
            if (methods.Count != 1)
                throw new InvalidOperationException("The existing LifetimeScope must have one Configure(IContainerBuilder) method to register the root entry point automatically.");

            Match method = methods[0];
            string newline = source.Contains("\r\n") ? "\r\n" : "\n";
            string indent = method.Groups[1].Value + "    ";
            string registration = $"{method.Groups[2].Value}.RegisterEntryPoint<global::{entryPointTypeName}>(global::VContainer.Lifetime.Singleton);";
            source = source.Insert(method.Index + method.Length, newline + indent + registration);
            if (!Regex.IsMatch(code, @"(?m)^\s*using\s+(?:global::)?VContainer\.Unity\s*;"))
                source = "using VContainer.Unity;" + newline + source;

            return source;
        }
    }
}
