using Siemens.AspNet.ErrorHandling.Contracts;
using System.Net;

namespace $ProjectName$.Validations
{
    public abstract class RequestValidator<TRequest> where TRequest : class
    {
        public Task ValidateAsync(TRequest request)
        {
            var errors = GetValidationErrors(request).ToList();
            if (errors.Any())
            {
                var errorInfos = errors.GroupBy(item => item.PropertyName)
                                       .ToDictionary(item => item.Key, item => item.Select(i => i.Error).ToArray());

                throw new ValidationProblemDetailsException(HttpStatusCode.BadRequest,
                                                            $"Your {typeof(TRequest).Name} was invalid",
                                                            $"Your {typeof(TRequest).Name} was invalid. Please check error details for detailed information.",
                                                            errorInfos);
            }

            return Task.CompletedTask;
        }

        protected abstract IEnumerable<(string PropertyName, string Error)> GetValidationErrors(TRequest request);
    }
}
