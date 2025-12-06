//using System.Collections.Immutable;
//using System.Diagnostics;
//using System.Runtime.CompilerServices;
//using Newtonsoft.Json;

//[assembly: InternalsVisibleTo("DotNetTool.Builder.Test")]

//namespace RunJit.Cli.Models
//{
//    [DebuggerDisplay("{" + nameof(Name) + "}")]
//    [method: JsonConstructor]
//    public class CommandInfo(string value,
//                             string name,
//                             string normalizedName,
//                             string description,
//                             ArgumentInfo? argumentInfo,
//                             ImmutableList<OptionInfo> options,
//                             ImmutableList<CommandInfo> subCommands)
//        : InfoBase(value, name, normalizedName)
//    {
//        public ImmutableList<CommandInfo> SubCommands { get; } = subCommands;

//        public ImmutableList<OptionInfo> Options { get; } = options;

//        public ArgumentInfo? Argument { get; set; } = argumentInfo;

//        public string Description { get; } = description;
//    }
//}


