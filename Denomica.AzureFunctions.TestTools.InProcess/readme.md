# Denomica.AzureFunctions.TestTools.InProcess

This library is designed to facilitate Azure Functions application testing. The focus in the initial versions is
on unit testing Durable Functions, including orchestration, activity and entity functions.

## Getting Started

After you have added this library as a reference to your test project, have a look at the [project wiki](https://github.com/Denomica/Denomica.AzureFunctions.TestTools/wiki) for more details on how to use this library.

- [Getting started](https://github.com/Denomica/Denomica.AzureFunctions.TestTools/wiki/Getting-Started)
- [Code samples](https://github.com/Denomica/Denomica.AzureFunctions.TestTools/wiki/In-Process-Samples)
- [Durable Functions Constraints](https://github.com/Denomica/Denomica.AzureFunctions.TestTools/wiki/Durable-Functions-Constraints)

## Version Highlights

### v1.0.0.alpha.5

- Added support for mocking `IDurableOrchestrationContext.CurrentUtcDateTime` property.

### v1.0.0.alpha.4

- Added an overloaded method that was missing from the previous version.

### v1.0.0.alpha.3

- Added a shorthand method for calling an orchestration function with input.

### v1.0.0-alpha.2

- Changed target framework to .NET Standard 2.1 to support both .NET6 and .NET8.
- Added limited support for activity functions that take an `IDurableActivityContext` parameter.

### v1.0.0-alpha.1

The initial prerelease of the library. This version has support for unit testing the following.

- Orchestration functions
- Activity functions with function input specifies as the function parameter
- Partial support for durable entities using the function syntax