﻿namespace RunJit.Cli.Models
{
    public class TypeToAlias
    {
        internal TypeToAlias(Type type, string alias)
        {
            Type = type;
            Alias = alias;
        }

        internal Type Type { get; }

        internal string Alias { get; }
    }
}
