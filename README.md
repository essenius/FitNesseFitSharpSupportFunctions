# SupportFunctions ![workflow badge](../../actions/workflows/support-functions-ci.yml/badge.svg)


Fixture support functions such as Echo, Times Series, Dictionary Library objects for FitNesse/FitSharp.

## Introduction 
This repo contains support functions such as Echo, Times Series, Dictionary Library objects, including demos. For more details see the [Wiki](../../wiki).

## Installation
The steps to install are very similar to that of installing the [FibonacciDemo](../../../FitNesseFitSharpFibonacciDemo).

Differences are:
* Download the repo code as a zip file and extract the contents of the folder `FitNesseFitSharpSupportFunctions-master`. 
* Go to the solution folder: `cd /D %LOCALAPPDATA%\FitNesse\SupportFunctions`
* If youy have .NET SDK installed:
    * Build solution: `dotnet build --configuration release SupportFunctions.sln`
    * Go to fixture folder: `cd SupportFunctions`
    * Publish, including selecting the right runtime: `dotnet publish --output bin\Deploy\net8.0 --framework net8.0 --configuration release --runtime win-x64 SupportFunctions.csproj`
* If you don't have .NET SDK installed: download `SupportFunctions.zip` from the latest [release](../../releases) and extract it into the `SupportFunctions\SupportFunctions` folder
* Go to the assemby folder `bin\Deploy\net8.0` and start FitNesse.
* Run the suite: Open a browser and enter the URL http://localhost:8080/FitSharpDemos.SupportFunctionsSuite?suite

## Contribute
Enter an [issue](../../issues) or provide a [pull request](../../pulls). 
