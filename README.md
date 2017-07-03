<p align="center">
    <a href="#l">
        <img alt="logo" src="Assets/logo-200x200.png">
    </a>
</p>

# L

[![][build-img]][build]
[![][nuget-img]][nuget]

Logging as simple as it can be.

Most of the time I don't need a sophisticated logger and I got tired of [configuring the same thing for log4net] over
and over again.

[build]:                                  https://ci.appveyor.com/project/TallesL/net-L
[build-img]:                              https://ci.appveyor.com/api/projects/status/github/tallesl/net-L?svg=true
[nuget]:                                  https://www.nuget.org/packages/L
[nuget-img]:                              https://badge.fury.io/nu/L.svg
[configuring the same thing for log4net]: https://logging.apache.org/log4net/release/manual/configuration.html

## Usage

Just instantiate and use it:

```cs
using LLibrary;

var myLogger = new L();

myLogger.Log("INFO", "Some information");
myLogger.Log("ERROR", new Exception("BOOM!"));
```

You can use built-in methods for the classical DEBUG, INFO, WARN, ERROR, FATAL labels (but there's no idea of a 
logging level here, they're just labels):

```cs
myLogger.LogInfo("Some information");
myLogger.LogError(new Exception("BOOM!"));
```

A file named `yyyy-MM-dddd.log` will be created in a `logs` folder (relative from where the application is running). 

The code above could yield, for instance, a file named `2014-12-16.log` with the content of:

```
2014-12-16 19:21:45 INFO  Some information.
2014-12-16 19:21:52 ERROR A System.Exception happened: BOOM!
```

## Static

There's a handy static instance for you to use in case you don't want to instantiate and hold a reference yourself:

```cs
L.InitializeStatic();

L.Static.Log("INFO", "Some information");
L.Static.Log("ERROR", new Exception("BOOM!"));
```

## Configuration

The library works out-of-the-box, no configuration needed, but you can configure a thing or two if you want:

```cs
var myLogger = new L(
    new LConfiguration
    {
        // Format string to use when calling DateTime.Format.
        // Defaults to "yyyy-MM-dd HH:mm:ss".
        DateTimeFormat = "dd MMM HH:mm:ss",

        // If other than null it sets to delete any file in the log folder that is older than the time set.
        // Defaults to null.
        DeleteOldFiles = TimeSpan.FromDays(10),

        // Directory where to create the log files.
        // Defautls to a local "logs" directory.
        Directory = @"C:\custom-directory\my-logs\",

        // Labels enabled to be logged by the library.
        // An attempt to log with a label that is not enabled is simply ignored, no error is raised.
        // Leave it empty or null to enable any label, which is the default.
        EnabledLabels = new[] { "INFO", "ERROR", },

        // True to use UTC time rather than local time.
        // Defaults to false.
        UseUtcTime = true,
    }
);
```

## But I want...

To restrict the file size?
Log to a database?
What about a fancy web interface?

If that is the case, this library is not for you.
Consider using other library such as [log4net], [NLog], [ELMAH] or [Logary].

[log4net]: http://logging.apache.org/log4net
[NLog]:    http://nlog-project.org
[ELMAH]:   https://code.google.com/p/elmah
[Logary]:  http://logary.github.io
