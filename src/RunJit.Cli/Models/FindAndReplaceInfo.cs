﻿using Argument.Check;

namespace RunJit.Cli.Models
{
    internal class FindAndReplaceInfo
    {
        public FindAndReplaceInfo(string find, string replace)
        {
            Throw.IfNullOrWhiteSpace(find);
            Throw.IfNullOrWhiteSpace(replace);

            Find = find;
            Replace = replace;
        }

        internal string Find { get; }

        internal string Replace { get; }
    }
}
