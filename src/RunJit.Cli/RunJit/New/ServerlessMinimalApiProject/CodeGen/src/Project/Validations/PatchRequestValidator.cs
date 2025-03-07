using System.Collections.Immutable;
using Extensions.Pack;
using $ProjectName$.JsonSerializing;
using Microsoft.AspNetCore.Mvc;
using Siemens.AspNet.ErrorHandling.Contracts;
using System.Net;
using System.Text.Json.Nodes;

namespace $ProjectName$.Validations
{
    public abstract class PatchRequestValidator<TRequest>(IJsonDiffer jsonDiffer) where TRequest : class
    {
        public Task ValidateAsync(JsonObject jsonObject,
                                  TRequest request,
                                  Guid id)
        {
            var errors = GetValidationErrorsInternal(jsonObject, request).ToList();
            if (errors.Any())
            {
                var errorInfos = errors.GroupBy(item => item.PropertyName)
                                       .ToDictionary(item => item.Key, item => item.Select(i => i.Error).ToArray());

                throw new ValidationProblemDetailsException(HttpStatusCode.BadRequest,
                                                            $"Your {typeof(TRequest).Name} was invalid",
                                                            $"Your {typeof(TRequest).Name} for the object id: {id} was invalid. Please check the error details. You might pass invalid data, or try to patch properties which not exists or not allow to patch",
                                                            errorInfos);
            }

            return Task.CompletedTask;
        }

        protected abstract IEnumerable<(string PropertyName, string Error)> GetValidationErrors(TRequest request);

        private IEnumerable<(string PropertyName, string Error)> GetValidationErrorsInternal(JsonObject jsonObject, TRequest request)
        {
            // 1. First we evaluate the json structures
            var differences = jsonDiffer.FindDifferences(jsonObject.ToString(), request.ToJson());
            var notExistingProperties = differences.Where(d => d.MismatchType == MismatchType.MissingInSecond).ToImmutableList();

            foreach (var notExistingProperty in notExistingProperties)
            {
                yield return (notExistingProperty.MemberPath, $"Does not exists on the {typeof(TRequest).Name}. Patchable properties are: {typeof(TRequest).GetProperties().Select(p => p.Name).ToFlattenString(";")}");
            }

            // 2. Specific errors
            var specificErrors = GetValidationErrors(request);

            foreach (var specificError in specificErrors)
            {
                yield return specificError;
            }
        }
    }
}
