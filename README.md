# DotNet Files

[![Test](https://github.com/popcron/dotnet-files/actions/workflows/test.yml/badge.svg)](https://github.com/popcron/dotnet-files/actions/workflows/test.yml)

High level API for working with C# project and solution files.

### Projects

```cs
ReadOnlySpan<byte> xmlBytes = File.ReadAllBytes("MyProject.csproj");
using Project project = Project.Parse(xmlBytes);
using List<ProjectReference> references = new();
project.GetProjectReferences(references);
project.ClearProjectReferences();
project.AddProjectReference("../../MyOtherProject/MyOtherProject.csproj");
```

### Solutions

```cs
ReadOnlySpan<byte> slnBytes = File.ReadAllBytes("MySolution.slnx");
using Solution solution = Solution.Parse(slnBytes);
using List<SolutionProject> projects = new();
solution.GetProjects(projects);
solution.ClearProjects();
solution.AddProject("../../MyOtherProject/MyOtherProject.csproj");
```

### Contributing and direction

This library was made to make it easier to write scripts that
change many `.csproj` in bulk.

Contributions that fit this are welcome.