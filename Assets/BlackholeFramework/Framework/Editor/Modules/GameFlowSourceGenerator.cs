using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blackhole.Framework.Composition.Editor
{
    internal static class GameFlowSourceGenerator
    {
        internal static void ValidateNamespace(string value)
        {
            using var provider = new Microsoft.CSharp.CSharpCodeProvider();
            if (string.IsNullOrWhiteSpace(value) || value.Split('.').Any(part => !provider.IsValidIdentifier(part)))
                throw new InvalidOperationException("Use a valid C# namespace for the generated GameFlow module.");
        }

        internal static string[] ValidateModuleIds(IEnumerable<string> moduleIds)
        {
            string[] ids = moduleIds.OrderBy(id => id, StringComparer.Ordinal).ToArray();
            var names = new HashSet<string>(StringComparer.Ordinal);
            var methods = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in ids)
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new InvalidOperationException("A root module has an empty module ID.");
                string name = ModuleName(id);
                if (!names.Add(name) || !methods.Add(SceneName(name)))
                    throw new InvalidOperationException($"Root module '{id}' produces a duplicate name or method. Give root modules unique names.");
            }
            return ids;
        }

        internal static string BuildGameFlow(IEnumerable<string> moduleIds)
        {
            return BuildInterface(moduleIds, "Blackhole.GameFlow.Contract", "IGameFlow", "NavigateTo");
        }

        internal static string BuildLoadScene(IEnumerable<string> moduleIds)
        {
            return BuildInterface(moduleIds, "Blackhole.GameFlow.Application", "ILoadScene", "Load");
        }

        internal static string BuildUsecase(IEnumerable<string> moduleIds)
        {
            string[] ids = ValidateModuleIds(moduleIds);
            var source = Header();
            source.AppendLine("using System;");
            source.AppendLine("using Blackhole.GameFlow.Contract;");
            source.AppendLine("using Blackhole.LoadingScreen.Contract;");
            source.AppendLine("using Blackhole.ModuleSystem.Contract;");
            source.AppendLine("using UnityEngine;");
            source.AppendLine();
            source.AppendLine("namespace Blackhole.GameFlow.Application");
            source.AppendLine("{");
            source.AppendLine("    public sealed class LoadGameUsecase : ILoadScene, IGameFlow");
            source.AppendLine("    {");
            foreach (string id in ids)
                source.AppendLine($"        private const string {ModuleName(id)}ModuleId = {Literal(id)};");
            source.AppendLine();
            source.AppendLine("        private readonly IModuleLoader _moduleLoader;");
            source.AppendLine("        private readonly ILoadingScreenService _loadingScreenService;");
            source.AppendLine();
            source.AppendLine("        public LoadGameUsecase(IModuleLoader moduleLoader, ILoadingScreenService loadingScreenService)");
            source.AppendLine("        {");
            source.AppendLine("            _moduleLoader = moduleLoader;");
            source.AppendLine("            _loadingScreenService = loadingScreenService;");
            source.AppendLine("        }");
            source.AppendLine();
            foreach (string id in ids)
            {
                string name = ModuleName(id);
                string scene = SceneName(name);
                source.AppendLine($"        public UniTask<Result<AsyncError>> NavigateTo{scene}Async(CancellationToken cancellationToken, RootModuleEntryOption entryOption = null)");
                source.AppendLine("        {");
                source.AppendLine($"            return LoadModuleAsync({name}ModuleId, cancellationToken, entryOption);");
                source.AppendLine("        }");
                source.AppendLine();
                source.AppendLine($"        public UniTask<Result<AsyncError>> Load{scene}Async(CancellationToken cancellationToken, RootModuleEntryOption entryOption = null)");
                source.AppendLine("        {");
                source.AppendLine($"            return NavigateTo{scene}Async(cancellationToken, entryOption);");
                source.AppendLine("        }");
                source.AppendLine();
            }
            source.AppendLine("        private async UniTask<Result<AsyncError>> LoadModuleAsync(string moduleId, CancellationToken cancellationToken, RootModuleEntryOption entryOption)");
            source.AppendLine("        {");
            source.AppendLine("            try");
            source.AppendLine("            {");
            source.AppendLine("                await _loadingScreenService.ExecuteAsync(() => entryOption == null");
            source.AppendLine("                    ? _moduleLoader.LoadRootModuleAsync(moduleId, cancellationToken)");
            source.AppendLine("                    : _moduleLoader.LoadRootModuleAsync(moduleId, entryOption, cancellationToken), LoadingScreenType.Black);");
            source.AppendLine("                return Result<AsyncError>.Success();");
            source.AppendLine("            }");
            source.AppendLine("            catch (OperationCanceledException)");
            source.AppendLine("            {");
            source.AppendLine("                return Result<AsyncError>.Failure(AsyncError.Canceled);");
            source.AppendLine("            }");
            source.AppendLine("            catch (Exception exception)");
            source.AppendLine("            {");
            source.AppendLine("                Debug.LogError($\"Failed to navigate to module '{moduleId}'.\\n{exception}\");");
            source.AppendLine("                return Result<AsyncError>.Failure(AsyncError.Unknown);");
            source.AppendLine("            }");
            source.AppendLine("        }");
            return Finish(source);
        }

        internal static string BuildInstaller()
        {
            return "// <auto-generated />\nusing Blackhole.GameFlow.Application;\n" +
                   "using Blackhole.SharedKernel.Composition;\n" +
                   "using UnityEngine;\n" +
                   "using VContainer;\n\n" +
                   "namespace Blackhole.GameFlow.Composition\n{\n" +
                   "    [CreateAssetMenu(fileName = \"GameFlowInstaller\", menuName = \"Blackhole/Modules/Installers/GameFlowInstaller\")]\n" +
                   "    public sealed class GameFlowInstaller : ModuleInstallerSO\n    {\n" +
                   "        public override void InstallModule(IContainerBuilder builder)\n        {\n" +
                   "            builder.Register<LoadGameUsecase>(Lifetime.Singleton).AsImplementedInterfaces();\n" +
                   "        }\n    }\n}\n";
        }

        private static string BuildInterface(IEnumerable<string> moduleIds, string targetNamespace, string name, string prefix)
        {
            var source = Header();
            if (targetNamespace != "Blackhole.GameFlow.Contract")
                source.AppendLine("using Blackhole.GameFlow.Contract;");
            source.AppendLine();
            source.AppendLine($"namespace {targetNamespace}");
            source.AppendLine("{");
            source.AppendLine($"    public interface {name}");
            source.AppendLine("    {");
            foreach (string id in ValidateModuleIds(moduleIds))
                source.AppendLine($"        UniTask<Result<AsyncError>> {prefix}{SceneName(ModuleName(id))}Async(CancellationToken cancellationToken, RootModuleEntryOption entryOption = null);");
            return Finish(source);
        }

        private static StringBuilder Header()
        {
            var source = new StringBuilder();
            source.AppendLine("// <auto-generated />");
            source.AppendLine("// Updated by RootModulesWindow.GenerateGameFlows.");
            source.AppendLine("using System.Threading;");
            source.AppendLine("using Blackhole.SharedKernel.Contract;");
            source.AppendLine("using Cysharp.Threading.Tasks;");
            return source;
        }

        private static string Finish(StringBuilder source)
        {
            source.AppendLine("    }");
            source.AppendLine("}");
            return source.ToString().Replace("\r\n", "\n");
        }

        private static string ModuleName(string moduleId)
        {
            string shortName = moduleId.Substring(moduleId.LastIndexOf('.') + 1);
            var name = new StringBuilder();
            bool capitalize = true;
            foreach (char character in shortName)
            {
                if (!char.IsLetterOrDigit(character) && character != '_')
                {
                    capitalize = true;
                    continue;
                }
                name.Append(capitalize ? char.ToUpperInvariant(character) : character);
                capitalize = false;
            }
            if (name.Length == 0)
                throw new InvalidOperationException($"Root module '{moduleId}' does not have a usable name.");
            if (char.IsDigit(name[0])) name.Insert(0, "Module");
            return name.ToString();
        }

        private static string SceneName(string name) => name.EndsWith("Scene", StringComparison.Ordinal) ? name : name + "Scene";

        private static string Literal(string value)
        {
            var literal = new StringBuilder("\"");
            foreach (char character in value)
            {
                if (character == '\\' || character == '"') literal.Append('\\').Append(character);
                else if (char.IsControl(character) || character == '\u2028' || character == '\u2029')
                    literal.Append("\\u").Append(((int)character).ToString("x4"));
                else literal.Append(character);
            }
            return literal.Append('"').ToString();
        }
    }
}
