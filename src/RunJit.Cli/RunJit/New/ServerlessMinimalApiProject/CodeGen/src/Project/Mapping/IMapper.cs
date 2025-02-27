using System.Collections.Immutable;

namespace $ProjectName$.Mapping
{
    internal interface IRequestMapper<TSource, TTarget> where TSource : class
                                                        where TTarget : class
    {
        TTarget MapTo(TSource source,
                      HttpContext httpContext);
    }

    internal interface IMapper<TSource, TTarget> where TSource : class
                                                 where TTarget : class
    {
        TTarget MapTo(TSource source);
        
        IImmutableList<TTarget> MapTo(IEnumerable<TSource> sources); 
    }
}
