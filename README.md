# FitNesseFitSharpSupportFunctions
Fixture support functions such as Echo, Times Series, Dictionary Library objects


# Introduction 
This repo contains support functions such as Echo, Times Series, Dictionary Library objects, including demos.

# Installation
The steps to install are very similar to that of installing the [FibonacciDemo](../../../FitNesseFitSharpFibonacciDemo).

Differences are:
* Download the repo code as a zip file and extract the contents of the folder ```FitNesseFitSharpSupportFunctions```. 
* Build command becomes: `dotnet build -c release %LOCALAPPDATA%\FitNesse\SupportFunctions\SupportFunctions.sln`
* Publish, to get the necessary dependencies (primarily LiveCharts), and take the runtime you need: 
    ```
    cd %LOCALAPPDATA%\FitNesse\SupportFunctions\SupportFunctions
    dotnet publish SupportFunctions.csproj --output bin\Deploy\net5.0 --framework net5.0 --configuration release --runtime win-x64
    ```
* Before starting FitNesse, go to folder: `cd /D %LOCALAPPDATA%\FitNesse\SupportFunctions\SupportFunctions\bin\Deploy\net5.0`
* Run the suite: Open a browser and enter the URL http://localhost:8080/FitSharpDemos.SupportFunctionsSuite?suite

# Contribute
Enter an [issue](../../issues) or provide a [pull request](../../pulls). 
