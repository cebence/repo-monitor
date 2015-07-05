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
    /// <param name="path">
    /// Full path to a potential repository.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified path is a repository,
    /// <see langword="false"/> otherwise.
    /// </returns>
    Boolean IsRepository(String path);

    /// <summary>
    /// Returns entire response to the <c>hg summary --remote</c> command.
    /// </summary>
    /// <param name="repositoryPath">
    /// Full path to a Mercurial repository.
    /// </param>
    /// <summary>
    /// Returns entire response to the <c>hg summary --remote</c> command.
    /// </summary>
    String GetSummaryRemoteText(String repositoryPath);

    /// <summary>
    /// Parses Mercurial's <c>summary --remote</c> response into the number
    /// of incoming and the number of outgoing changes.
    /// </summary>
    /// <param name="summary">
    /// The summary response text to parse.
    /// </param>
    /// <param name="incoming">
    /// When this method returns, contains the number of incoming changes
    /// parsed from summary text. This parameter is passed uninitialized.
    /// </param>
    /// <param name="outgoing">
    /// When this method returns, contains the number of outgoing changes
    /// parsed from summary text. This parameter is passed uninitialized.
    /// </param>
    /// <exception cref="ArgumentException">
    /// When <paramref name="summary"/> is not in the correct format, i.e.
    /// the <c>remote:</c> line is missing or invalid.
    /// </exception>
    void ParseSummaryRemoteText(String summary, out int incoming, out int outgoing);
  }
}
