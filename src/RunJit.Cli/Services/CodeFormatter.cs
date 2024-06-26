﻿using Extensions.Pack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli
{
    public static class AddCodeFormatterExtension
    {
        public static void AddCodeFormatter(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<CodeFormatter>();
        }
    }
    
    internal class CodeFormatter
    {
        internal string FormatCode(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var formattedNode = syntaxTree.GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
            // ToDo: quickfix
            formattedNode = formattedNode.Replace(",,", ",")
                                         .Replace(",)", ")")
                                         .Replace(", )", ")")
                                         .Replace(",  )", ")");
            return formattedNode;
        }
    }
}
