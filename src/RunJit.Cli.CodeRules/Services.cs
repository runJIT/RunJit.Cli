using System.Collections.Immutable;
using Extensions.Pack;
using Solution.Parser.CSharp;

namespace RunJit.Cli.CodeRules
{
    [TestCategory("Coding-Rules")]
    [TestCategory("Services")]
    [TestClass]
    public class Services : MsTestBase
    {
        private static ImmutableList<(Class Class, Class? Registration, CSharpSyntaxTree syntaxTree)> _services = ImmutableList<(Class Class, Class? Registration, CSharpSyntaxTree syntaxTree)>.Empty;

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            _services = FindServices().ToImmutableList();

            IEnumerable<(Class Class, Class? Registration, CSharpSyntaxTree syntaxTree)> FindServices()
            {
                foreach (var cSharpSyntaxTree in ProductiveCodeSyntaxTreesToAnaylze)
                {
                    var services = cSharpSyntaxTree.Classes.Where(@class => @class.Modifiers.Any(m => m is Modifier.Static or Modifier.Abstract).IsFalse() &&
                                                                            @class.Methods.Any()).ToList();

                    foreach (var service in services)
                    {
                        var registrationClass = cSharpSyntaxTree.Classes.FirstOrDefault(@class => @class.Name.Contains($"{service.Name}Extension"));

                        yield return (service, registrationClass, cSharpSyntaxTree);
                    }
                }
            }
        }

        [TestMethod]
        public void Each_Service_Should_Have_A_Registration_Extension_In_The_Same_File()
        {
            var missingServiceRegistration = (from service in _services
                                              where service.Registration is null
                                              select new
                                                     {
                                                         Error = $@"
Your service:       {service.Class.Name} does not have a service registration declared in same file. 
                    If it is in same file please check the nam
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
FullQualifiedName:  {service.Class.FullQualifiedName}.{service.Class.Name}'
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Sample:             internal static class Add{service.Class.Name}Extension
                    {{
                        internal static void Add{service.Class.Name}(this IServiceCollection services)
                        {{
                            services.AddSingletonIfNotExists<{service.Class.Name}>();
                        }}
                    }}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
"
                                                     }).ToImmutableList();

            Assert.IsTrue(missingServiceRegistration.IsEmpty(),
                          $"Total errors: {missingServiceRegistration.Count}. No service registration was found for your declared service:{Environment.NewLine}{missingServiceRegistration.Select(e => e.Error).Flatten(Environment.NewLine)}");
        }

        [TestMethod]
        public void Each_Service_Have_To_Register_Its_Dependencies_Too()
        {
            var exceptions = ImmutableList.Create("ILoggerFactory", "IFileService", "IDirectoryService",
                                                  "IHttpClientFactory");

            var missingDependencyRegistrations = GetMissingDependencyRegistrations().ToList();

            Assert.IsTrue(missingDependencyRegistrations.IsEmpty(),
                          $"Missing dependency registraions detected (Total: {missingDependencyRegistrations.Count}:{Environment.NewLine}{missingDependencyRegistrations.Flatten(Environment.NewLine)}");

            IEnumerable<string> GetMissingDependencyRegistrations()
            {
                var servicesWithRegistrations = _services.Where(s => s.Registration is not null).ToImmutableList();

                foreach (var service in servicesWithRegistrations)
                {
                    var dependencies = service.Class.Constructors.SelectMany(c => c.Parameters).DistinctBy(item => item.Name).ToList();
                    var simpleDependencies = dependencies.Where(d => d.Type.DoesNotContain("<")).ToList();

                    var missingRegistrationsOf = simpleDependencies.Where(dependency =>
                                                                          {
                                                                              var registrationName = servicesWithRegistrations.FirstOrDefault(reg => (reg.Registration?.Name).EqualsTo($"Add{dependency.Type}"));

                                                                              var neutralName = registrationName.Registration.IsNull() ? dependency.Type.TrimStart('I').ToLowerInvariant() : registrationName.Registration?.Name.ToLowerInvariant() ?? string.Empty;

                                                                              var missingDependencyRegistration = service.Registration!.Methods.All(method =>
                                                                                                                                                    {
                                                                                                                                                        var result = method.LineStatements.Any(statement =>
                                                                                                                                                                                               {
                                                                                                                                                                                                   if (statement.Contains("//"))
                                                                                                                                                                                                   {
                                                                                                                                                                                                       return false;
                                                                                                                                                                                                   }

                                                                                                                                                                                                   var statementToLower = statement.ToLowerInvariant();

                                                                                                                                                                                                   var result = statementToLower.Contains($"add{neutralName}") ||
                                                                                                                                                                                                                statementToLower.Contains($"add{neutralName.Replace("settings", string.Empty)}") ||
                                                                                                                                                                                                                statementToLower.Contains($"add{dependency.Name.ToLowerInvariant()}") ||
                                                                                                                                                                                                                statementToLower.Contains($"add{dependency.Name.ToLowerInvariant()}") ||
                                                                                                                                                                                                                statementToLower.Contains($"addhostedservice<{dependency.Type.ToLowerInvariant()}") ||
                                                                                                                                                                                                                statementToLower.Contains($"addsingletonifnotexists<{dependency.Type.ToLowerInvariant()}");

                                                                                                                                                                                                   return result;
                                                                                                                                                                                               }).IsFalse();

                                                                                                                                                        return result;
                                                                                                                                                    });

                                                                              return missingDependencyRegistration;
                                                                          }).ToImmutableList();

                    var filteredMissing = missingRegistrationsOf.Where(missing => exceptions.All(e => missing.Type.NotEqualsTo(e))).ToImmutableList();

                    if (filteredMissing.Any())
                    {
                        yield return $@"
Your service registration: {service.Registration!.Name} does not contains all registrations for the service dependencies
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Depdencies: {service.Class.Constructors.MaxBy(c => c.Parameters.Count)?.Parameters.Select(p => $"{p.Type}").Flatten(" ,")}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Current:    {service.Registration.Methods.FirstOrDefault()!.MethodValue}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Sample:     internal static class Add{service.Class.Name}Extension
            {{
                internal static void Add{service.Class.Name}(this IServiceCollection services)
                {{
                    {simpleDependencies.Select(dependency => $"services.Add{dependency.Type.TrimStart('I').FirstCharToUpper()}();").Flatten(Environment.NewLine)}

                    services.AddSingletonIfNotExists<{service.Class.Name}>();
                }}
            }} 
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Ctor:       {service.Class.Name}({service.Class.Constructors.MaxBy(c => c.Parameters.Count)?.Parameters.Select(p => $"{p.Type} {p.Name}").Flatten(" ,")})
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
";
                    }
                }
            }
        }

        [TestMethod]
        public void Each_Service_Registration_Have_To_Be_Called()
        {
            var missingRegistrationCalls = FindMissingRegistrationCalls().ToList();

            Assert.IsTrue(missingRegistrationCalls.IsEmpty(),
                          $"We found not called service registrations extension. Please call your registrations otherwise your application will not work.{Environment.NewLine}{missingRegistrationCalls.Flatten(Environment.NewLine)}");

            IEnumerable<string> FindMissingRegistrationCalls()
            {
                var serviceWithRegistrations = from service in _services
                                               where service.Registration is not null
                                               select service;

                var statements = ProductiveCodeSyntaxTreesToAnaylze.SelectMany(c => c.Classes).SelectMany(c => c.Methods).SelectMany(m => m.Statements).ToList();

                foreach (var serviceWithRegistration in serviceWithRegistrations)
                {
                    foreach (var registrationMethod in serviceWithRegistration.Registration.Methods)
                    {
                        var missing = !statements.Any(statement => statement.Contains($".{registrationMethod.Name}"));

                        if (missing)
                        {
                            yield return $@"
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Registration: {registrationMethod.MethodValue}
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
Sample:       services.{registrationMethod.Name}();
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
";
                        }
                    }
                }
            }
        }

        [Ignore("Next test :)")]
        [TestMethod]
        public void Each_Service_Should_Have_Only_1_Constructor()
        {
        }
    }
}
