# Repository for my Database Course project.
## Sophie -- Simple API for conference management.

Info:

*C#, .Net Core, PostgreSQL*

[Description (in polish)](Description.md).

[Project name choice reason](https://www.facebook.com/ZOSIA.KSI).

## How to launch?

You can use ruby to launch with one command:

`./sophie.rb`

or you can run 

```
dotnet restore Sophie
dotnet run -p Sophie/Sophie.csproj
```

in this directory.

## How to test?

* ruby input tests:
    `./sophie.rb t[est] [test_scenarios/some.scenario]`

* C# xunit unit tests in directory Sophie.Tests

## ER Diagrams

ER diagrams are located in `entity_relation_diagrams/` directory.