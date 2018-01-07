# Aws Tools

- Plain old class objects (POCO's) for DynamoDB.
- Compatible with AWS Lambda
  - Recent versions of AWS packages have been defective in AWS Lambda
  - AWS packages are locked in at 3.3.1.1 until a critical feature is released in a new version
- Compatible with MVC
  - MVC has inherent issues with await/async requiring every usage of await to use ConfigureAwait(false) or the application will deadlock
- Assistance with batching
- Improve visibility on errors for bulk operations
- Make it easier to retry failed messages

# Nuget Deploy

## Increment version in Visual Studio

    Right-Click AwsTools -> Properties -> Package -> Package Version

## Build

    \projects\AwsTools\AwsTools>dotnet build --configuration release

## Deploy

    \projects\AwsTools\AwsTools\bin\release>nuget push AwsTools.1.0.9.nupkg -src https://nuget.org/

*`pack` isn't required, because a nuget package is created on build*
