FROM --platform=linux/amd64 public.ecr.aws/lambda/dotnet:8 AS base

WORKDIR /var/task
COPY "bin/Release/lambda-publish"  .

CMD ["dotnet","$ProjectName$.dll"]
ENTRYPOINT ["dotnet", "$ProjectName$.dll"]