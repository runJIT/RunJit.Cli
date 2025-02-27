namespace $ProjectName$.Validations
{
    public interface IRequestValidator<in T> where T : class
    {
        Task ValidateAsync(T request);
    }
}
