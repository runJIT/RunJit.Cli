using System.Collections.Immutable;
using $ProjectName$.Api.$DomainNamePlural$.V1._Shared_;

namespace $ProjectName$.Api.$DomainNamePlural$.V1.GetAll
{
    public sealed record GetAllProjectsResponse(IImmutableList<$DomainModel$> $DomainNamePlural$);
}
