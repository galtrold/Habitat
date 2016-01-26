using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using PT.Framework.ViewModelResolver.Presentation;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;

namespace PT.Framework.ViewModelResolver.Infrastructure
{
  public class MappedViewModelProcessor
  {
    public void Process(PipelineArgs args)
    {
      var folder = Path.Combine(HttpContext.Current.Server.MapPath("~/"), "bin");
      var files = FilterExcludedAssemblies(Directory.GetFiles(folder, "*.dll"));
      try
      {
        foreach (var file in files)
        {
          ConfigureAssembly(file);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    private IEnumerable<string> FilterExcludedAssemblies(string[] assemblyFileNames)
    {
      var excludeAssemblyService = new ExcludeAssemblyService();
      return
        assemblyFileNames.Where(assemblyFileName => !excludeAssemblyService.Check(assemblyFileName.ToLowerInvariant()));
    }

    private void ConfigureAssembly(string path)
    {
      Assembly assembly;
      try
      {
        assembly = Assembly.Load(File.ReadAllBytes(path));
      }
      catch (Exception ex)
      {
        Log.Warn(ex.Message, ex, this);
        return;
      }

      if (assembly == null)
        return;

      InstantiateConfigurableViewModels(assembly);
    }

    private void InstantiateConfigurableViewModels(Assembly assembly)
    {
      Type[] types;
      try
      {
        types = assembly.GetTypes();  // Using this in a foreach caused the app to stop loading dll's. Hence this structure. 
      }
      catch (ReflectionTypeLoadException ex)
      {
        Log.Warn("[MappedViewModelProcessor] " + ex.Message, ex, this);
        return;
      }

      foreach (var type in types)
      {
        try
        {
          if (!type.IsClass || type.IsNotPublic)
            continue;

          if (type.BaseType != null && !string.IsNullOrWhiteSpace(type.BaseType.Name) &&
              typeof (MappedViewModelConfiguration<>).Name == type.BaseType.Name)
          {
            Activator.CreateInstance(type);
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex.Message, ex, this);
        }
      }
    }
  }
}