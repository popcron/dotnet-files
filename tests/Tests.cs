using Collections.Generic;
using System;
using Unmanaged;

namespace DotNetFiles.Tests;

public class Tests
{
    [Test]
    public void PackageAndProjectReferences()
    {
        string projectText = Samples.Get("Samples/Project.Tests.csproj.txt");
        using ByteReader byteReader = ByteReader.CreateFromUTF8(projectText);
        using Project project = byteReader.ReadObject<Project>();

        using List<ProjectReference> projectReferences = new();
        project.GetProjectReferences(projectReferences);
        Assert.That(projectReferences.Count, Is.EqualTo(1));
        Assert.That(projectReferences[0].Include.ToString(), Is.EqualTo("..\\source\\Project.csproj"));

        using List<PackageReference> packageReferences = new();
        project.GetPackageReferences(packageReferences);
        Assert.That(packageReferences.Count, Is.EqualTo(5));
        Assert.That(packageReferences[0].Include.ToString(), Is.EqualTo("coverlet.collector"));
        Assert.That(packageReferences[0].Version.ToString(), Is.EqualTo("6.0.4"));
        Assert.That(packageReferences[1].Include.ToString(), Is.EqualTo("Microsoft.NET.Test.Sdk"));
        Assert.That(packageReferences[1].Version.ToString(), Is.EqualTo("17.14.0"));
        Assert.That(packageReferences[2].Include.ToString(), Is.EqualTo("NUnit"));
        Assert.That(packageReferences[2].Version.ToString(), Is.EqualTo("4.3.2"));
        Assert.That(packageReferences[3].Include.ToString(), Is.EqualTo("NUnit.Analyzers"));
        Assert.That(packageReferences[3].Version.ToString(), Is.EqualTo("4.7.0"));
        Assert.That(packageReferences[4].Include.ToString(), Is.EqualTo("NUnit3TestAdapter"));
        Assert.That(packageReferences[4].Version.ToString(), Is.EqualTo("5.0.0"));

        using List<EmbeddedResource> embeddedResources = new();
        project.GetEmbeddedResources(embeddedResources);
        Assert.That(embeddedResources.Count, Is.EqualTo(2));
        Assert.That(embeddedResources[0].Include.ToString(), Is.EqualTo("Samples\\Project.Tests.csproj.txt"));
        Assert.That(embeddedResources[1].Include.ToString(), Is.EqualTo("Samples\\Project.csproj.txt"));

        using List<Analyzer> analyzers = new();
        project.GetAnalyzers(analyzers);
        Assert.That(analyzers.Count, Is.EqualTo(1));
        Assert.That(analyzers[0].Include.ToString(), Is.EqualTo("..\\..\\unmanaged\\generator\\bin\\$(Configuration)\\netstandard2.0\\Unmanaged.Generator.dll"));

        using List<Content> content = new();
        project.GetContent(content);
        Assert.That(content.Count, Is.EqualTo(2));
        Assert.That(content[0].Include.ToString(), Is.EqualTo("bin/**/*"));
        Assert.That(content[0].Pack, Is.True);
        Assert.That(content[0].PackagePath.ToString(), Is.EqualTo("lib"));
        Assert.That(content[0].Visible, Is.False);
        Assert.That(content[1].Include.ToString(), Is.EqualTo("buildTransitive/**/*"));
        Assert.That(content[1].Pack, Is.True);
        Assert.That(content[1].PackagePath.ToString(), Is.EqualTo("buildTransitive"));
    }

    [Test]
    public void ReadingPrivateAssets()
    {
        string projectText = Samples.Get("Samples/Unmanaged.Tests.Old.csproj.txt");
        using ByteReader byteReader = ByteReader.CreateFromUTF8(projectText);
        using Project project = byteReader.ReadObject<Project>();

        project.ClearTargetFrameworks();
    }

    [Test]
    public void ModifyToTargetNet9()
    {
        string projectText = Samples.Get("Samples/Unmanaged.Tests.New.csproj.txt");
        using ByteReader byteReader = ByteReader.CreateFromUTF8(projectText);
        using Project project = byteReader.ReadObject<Project>();
        project.ClearTargetFrameworks();
        project.AddTargetFramework(TargetFramework.Net9);
        project.ClearAnalyzers();
        Console.WriteLine(project.ToString());
    }

    [Test]
    public void AddingAndRemovingEmbeddedReferences()
    {
        string projectText = Samples.Get("Samples/Project.Tests.csproj.txt");
        using ByteReader byteReader = ByteReader.CreateFromUTF8(projectText);
        using Project project = byteReader.ReadObject<Project>();

        using List<EmbeddedResource> embeddedResources = new();
        project.GetEmbeddedResources(embeddedResources);
        int count = embeddedResources.Count;
        project.AddEmbeddedResource("NewResource1.txt");
        project.AddEmbeddedResource("NewResource2.txt");

        embeddedResources.Clear();
        project.GetEmbeddedResources(embeddedResources);
        Assert.That(embeddedResources.Count, Is.EqualTo(count + 2));

        project.ClearEmbeddedResources();

        embeddedResources.Clear();
        project.GetEmbeddedResources(embeddedResources);
        Assert.That(embeddedResources.Count, Is.EqualTo(0));

        project.AddEmbeddedResource("Apple.txt");

        embeddedResources.Clear();
        project.GetEmbeddedResources(embeddedResources);
        Assert.That(embeddedResources.Count, Is.EqualTo(1));

        Console.WriteLine(project.ToString());
    }

    [Test]
    public void ReadModernProject()
    {
        string projectText = Samples.Get("Samples/Project.csproj.txt");
        using ByteReader byteReader = ByteReader.CreateFromUTF8(projectText);
        using Project project = byteReader.ReadObject<Project>();
        string debugCondition = "'$(Configuration)|$(Platform)'=='Debug|AnyCPU'";
        string releaseCondition = "'$(Configuration)|$(Platform)'=='Release|AnyCPU'";

        Assert.That(project.Sdk, Is.EqualTo(Sdk.MicrosoftNETSdk));
        Assert.That(project.LangVersion, Is.Null);
        Assert.That(project.TargetFrameworks.Length, Is.EqualTo(3));
        Assert.That(project.TargetFrameworks[0], Is.EqualTo(TargetFramework.Net8));
        Assert.That(project.TargetFrameworks[1], Is.EqualTo(TargetFramework.Net9));
        Assert.That(project.TargetFrameworks[2], Is.EqualTo(TargetFramework.Net10));
        Assert.That(project.Nullable(), Is.EqualTo(Nullable.Enable));
        Assert.That(project.AllowUnsafeBlocks(), Is.False);
        Assert.That(project.GenerateDocumentationFile(), Is.True);
        Assert.That(project.ImplicitUsings(), Is.False);
        Assert.That(project.IsAotCompatible(debugCondition), Is.True);
        Assert.That(project.IsTrimmable(debugCondition), Is.True);
        Assert.That(project.Optimize(debugCondition), Is.False);
        Assert.That(project.Optimize(releaseCondition), Is.Null);
        Assert.That(project.IsPackable(), Is.Null);
        Assert.That(project.TreatWarningsAsErrors(debugCondition), Is.True);
    }

    [Test]
    public void ReadModernTestProject()
    {
        string projectText = Samples.Get("Samples/Project.Tests.csproj.txt");
        using ByteReader byteReader = ByteReader.CreateFromUTF8(projectText);
        using Project project = byteReader.ReadObject<Project>();

        Assert.That(project.Sdk, Is.EqualTo(Sdk.MicrosoftNETSdk));
        Assert.That(project.TargetFrameworks.Length, Is.EqualTo(1));
        Assert.That(project.TargetFrameworks[0], Is.EqualTo(TargetFramework.Net10));
        Assert.That(project.LangVersion, Is.EqualTo(LangVersion.Latest));
        Assert.That(project.Nullable(), Is.EqualTo(Nullable.Enable));
        Assert.That(project.AllowUnsafeBlocks(), Is.Null);
        Assert.That(project.GenerateDocumentationFile(), Is.Null);
        Assert.That(project.ImplicitUsings(), Is.False);
        Assert.That(project.IsAotCompatible(), Is.Null);
        Assert.That(project.IsTrimmable(), Is.Null);
        Assert.That(project.IsPackable(), Is.False);
        Assert.That(project.TreatWarningsAsErrors(), Is.Null);
    }

    [Test]
    public void ListProjectsInSolution()
    {
        string solutionText = Samples.Get("Samples/Solution.slnx.txt");
        using ByteReader byteReader = ByteReader.CreateFromUTF8(solutionText);
        using Solution solution = byteReader.ReadObject<Solution>();
        using List<SolutionProject> projects = new();
        solution.GetProjects(projects);

        Assert.That(projects.Count, Is.EqualTo(5));
        Assert.That(projects[0].Path.ToString(), Is.EqualTo("../../clipboard/source/Clipboard.csproj"));
        Assert.That(projects[1].Path.ToString(), Is.EqualTo("../../collections/source/Collections.csproj"));
        Assert.That(projects[2].Path.ToString(), Is.EqualTo("../../unmanaged/core/Unmanaged.Core.csproj"));
        Assert.That(projects[3].Path.ToString(), Is.EqualTo("../../xml/source/XML.csproj"));
        Assert.That(projects[4].Path.ToString(), Is.EqualTo("Project.csproj"));

        solution.ClearProjects();

        projects.Clear();
        solution.GetProjects(projects);
        Assert.That(projects.Count, Is.EqualTo(0));

        solution.AddProject("NewProject1.csproj");

        Assert.That(solution.ContainsProject("NewProject1.csproj"), Is.True);

        projects.Clear();
        solution.GetProjects(projects);
        Assert.That(projects.Count, Is.EqualTo(1));
        Assert.That(projects[0].Path.ToString(), Is.EqualTo("NewProject1.csproj"));
    }
}
