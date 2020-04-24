Mistware.Files - miscellaneous utilities for handling files
========================================

Mistware is an identity chosen to evoke the concept of [Vapourware](https://en.wikipedia.org/wiki/Vaporware).

These are utilities to simplify the task of storing, uploading and downloading files in web applications. In particular: matching classes to handle local an Azure file storage, logging on blob append storage and large file upload / download. 

Local file storage or Azure file storage can be selected by configuration, which facilites migration between Azure Web App hosting and normal web server hosting.

The large file upload / download is based on a Microsoft article [Upload files in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads) and [sample](https://github.com/dotnet/AspNetCore.Docs/tree/master/aspnetcore/mvc/models/file-uploads/samples/). Code from the Streamed Single File Upload Physical example was adapted and incorporated into this library, in order to hide the complexity from any web app (with the aim of making the web app easier to maintain). 



Documentation
--------

Each class has Intellisense documentation. There is also a MistwareFiles.doc file, with additional usage guidance.


Usage
--------

To add the nuget package to a .Net Core application:

```
dotnet add package Mistware.Files 
```



Testing
---------------------
Mistware.Files has a basic test suite in the test folder (MistwareFilesTest.csproj)

There is also an example web application in webapp, based on the Microsoft Upload sample (see above).


