# $ProjectName$
### Overview
The `$ProjectName$` and `$ProjectName$.Contracts` packages are designed to provide robust error handling capabilities for ASP.NET Core applications.

`$ProjectName$`: This package offers middleware and services that handle errors, translate error messages based on the client's "Accepted-Languages" header, and ensure consistent error responses following the [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) standard. It simplifies the development of multilingual applications by supporting structured and localized error messages. [Read more](./$ProjectName$/README.md)

`$ProjectName$.Contracts`: This companion package defines the core data types and base classes for standardizing error handling across your application. It includes classes like ProblemDetails, ValidationProblemDetails, and several exception classes for handling specific types of errors, all adhering to the [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) specification. [Read more](./$ProjectName$.Contracts/README.md)

To have an overview of the possible Http-Codes, please take a look at [http.cat](https://http.cat/) or if you are a dog friend [http.dog](https://http.dog/).

For License information tale a look into our [license file](./$ProjectName$/LICENSE.md)