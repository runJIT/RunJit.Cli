using Extensions.Pack;

namespace RunJit.Cli.Models.Validation
{
    internal sealed class GenericValidationResult<T>(T source,
                                              string errors) : ObjectValidationResult(source!, errors)
    {
        public T Object => Source.Cast<T>();
    }
}
