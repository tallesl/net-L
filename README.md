<p align="center">
    <a href="#l">
        <img alt="logo" src="Assets/logo-200x200.png">
    </a>
</p>

# L

[![][build-img]][build]
[![][nuget-img]][nuget]

A configless .NET file logger.

Most of the time I don't need a sophisticated logger and I got tired of [configuring the same thing for log4net] over
and over again.

[build]:                                  https://ci.appveyor.com/project/TallesL/net-L
[build-img]:                              https://ci.appveyor.com/api/projects/status/github/tallesl/net-L?svg=true
[nuget]:                                  https://www.nuget.org/packages/L
[nuget-img]:                              https://badge.fury.io/nu/L.svg
[configuring the same thing for log4net]: https://logging.apache.org/log4net/release/manual/configuration.html

## Usage

Register your desired formats:

```cs
using LLibrary;

L.Register("INFO")
L.Register("FATAL", "An exception happened. Exception: {0}, message: {1}.")
```

Then use it:

```cs
L.Register("INFO");
L.Register("ERROR", "A {0} happened: {1}");
```

A file named `yyyy-MM-dddd.log` will be created in a `logs` folder (relative from where the application is running). 

The code above could yield, for instance, a file named `2014-12-16.log` with the content of:

```
2014-12-16 19:21:45 INFO  Some information.
2014-12-16 19:21:52 ERROR A System.Exception happened: BOOM!
```

## Disabling

You can unregister a single format with `L.Unregister(name)` or all of them at once with `L.UnregisterAll()`.
The library raises no error when an attempt to log with an unregistered format is made (it simply ignores it).

## Cleaning up

You can set the library to clean itself by calling `L.CleanItself()`.
It checks every 8 hours for files older than 10 days and then deletes it.

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