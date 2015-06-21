using System;

namespace RepoMonitor.Core {
  /// <summary>
  /// Common SCM functionalities.
  /// </summary>
  public interface SCM {
    /// <summary>
    /// Returns <see langword="true"/> if the SCM tool is available on the
    /// system (i.e. is it installed), <see langword="false"/> otherwise.
    /// </summary>
    Boolean IsAvailable();

    /// <summary>
    /// Returns SCM tool's version text.
    /// </summary>
    /// <remarks>
    /// This is usually the CLI header that includes tool name, version number,
    /// copyright information, etc.
    /// </remarks>
    String GetVersionText();

    /// <summary>
    /// Returns <see langword="true"/> if the specified path is a supported
    /// repository, <see langword="false"/> otherwise.
    /// </summary>
    Boolean IsRepository(String path);
  }
}
