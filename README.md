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

```cs
using LLibrary;

var myLogger = new L();

myLogger.Log("INFO", "Some information");
myLogger.Log("ERROR", new Exception("BOOM!"));
```

You can use built-in methods for the classical DEBUG, INFO, WARN, ERROR, FATAL labels (but there's no logging level here, they're just labels):

```cs
myLogger.Info("Some information");
myLogger.Error(new Exception("BOOM!"));
```

A file named `yyyy-MM-dddd.log` will be created in a `logs` folder (located where the application is running), like `2014-12-16.log` containing:

```
2014-12-16 19:21:45 INFO  Some information.
2014-12-16 19:21:52 ERROR A System.Exception happened: BOOM!
```

## Configuration

The library works out-of-the-box without configuration, but you can configure a thing or two if you want:

```cs
var myLogger = new L(
    // True to use UTC time rather than local time.
    // Defaults to false.
    useUtcTime: true,

    // If other than null it sets to delete any file in the log folder that is older than the time set.
    // Defaults to null.
    deleteOldFiles: TimeSpan.FromDays(10),

    // Format string to use when calling DateTime.Format.
    // Defaults to "yyyy-MM-dd HH:mm:ss".
    dateTimeFormat: "dd MMM HH:mm:ss",

    // Directory where to create the log files.
    // Defaults to null, which creates a local "logs" directory.
    directory: @"C:\custom-directory\my-logs\",

    // Labels enabled to be logged by the library, an attempt to log with a label that is not enabled is ignored (no error is raised), null or empty enables all labels.
    // Defaults to null.
    enabledLabels: "INFO", "ERROR"
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
