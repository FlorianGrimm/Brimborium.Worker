using System.Runtime.CompilerServices;

namespace Brimborium.Worker.Frontend;

public static class FrontendLocation {
  public static string? GetWWWRoot() {
    var csproj = System.IO.Path.Combine(GetLocation(), "Brimborium.Worker.Frontend.csproj");
    if (File.Exists(csproj)) {
      var wwwRoot = System.IO.Path.Combine(GetLocation(), "wwwroot");
      return wwwRoot;
    } else {
      return default;
    }
  }

  private static string GetLocation([CallerFilePath] string callerFilePath = "") {
    return System.IO.Path.GetDirectoryName(callerFilePath)
      ?? throw new ArgumentException("is empty", nameof(callerFilePath));
  }
}
