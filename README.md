# Aws Tools

- Plain old class objects (POCO's) for DynamoDB.
- Assistance with batching
- Improve visibility on errors for bulk operations
- Make it easier to retry failed messages

This package is intended to be used within AWS Lambda. I have selected what I have found to be the best release for AWS Lambda compatibility version 3.3.1.1.

# Nuget deploy

## Increment version in Visual Studio


    Right-Click AwsTools -> Properties -> Package -> Package Version


## Build

    C:\Users\peon\Desktop\projects\AwsTools\AwsTools>dotnet build --configuration release

## Deploy

    C:\Users\peon\Desktop\projects\AwsTools\AwsTools\bin\release>C:\Users\peon\Desktop\nuget.exe push AwsTools.1.0.2.nupkg -src https://nuget.org/

*`pack` isn't required, because a nuget package is created on build*
