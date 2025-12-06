using System.Collections.Immutable;
using Extensions.Pack;
using Solution.Parser.CSharp;

namespace RunJit.Cli.Extensions
{
    internal static class SyntaxTreeExtensions
    {
        internal static (DeclarationBase? Declaration, Type Type) FindDataType(this ImmutableList<CSharpSyntaxTree> syntaxTrees,
                                                                               Type reflectionType)
        {
            var fullqualifiedName = reflectionType.FullName?.Replace("+", "."); // Nested classes have + in reflection full name as separator !

            if (fullqualifiedName.IsNull())
            {
                return (null, reflectionType);
            }

            // Generic types have to be simplified against generic types of solution parser
            fullqualifiedName = fullqualifiedName.Split("`").First();

            // 2.1 Check find class first
            var @class = syntaxTrees.SelectMany(tree => tree.Classes).FirstOrDefault(c => c.FullQualifiedName.EqualsTo(fullqualifiedName));

            if (@class.IsNotNull())
            {
                return (@class, reflectionType);
            }

            // 2.2. Check records
            var record = syntaxTrees.SelectMany(tree => tree.Records).FirstOrDefault(c => c.FullQualifiedName.EqualsTo(fullqualifiedName));

            if (record.IsNotNull())
            {
                return (record, reflectionType);
            }

            // 2.3. Check enum
            var @enum = syntaxTrees.SelectMany(tree => tree.Enums).FirstOrDefault(c => c.FullQualifiedName.EqualsTo(fullqualifiedName));

            if (@enum.IsNotNull())
            {
                return (@enum, reflectionType);
            }

            // 2.3. Check interface
            var @interface = syntaxTrees.SelectMany(tree => tree.Interfaces).FirstOrDefault(c => c.FullQualifiedName.EqualsTo(fullqualifiedName));

            if (@interface.IsNotNull())
            {
                return (@interface, reflectionType);
            }

            // 2.4. Check interface
            var stuct = syntaxTrees.SelectMany(tree => tree.Structs).FirstOrDefault(c => c.FullQualifiedName.EqualsTo(fullqualifiedName));

            if (stuct.IsNotNull())
            {
                return (stuct, reflectionType);
            }

            return (null, reflectionType);
        }
    }
}
