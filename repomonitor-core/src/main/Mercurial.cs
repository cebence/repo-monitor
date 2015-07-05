using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace RepoMonitor.Core {
  /// <summary>
  /// SCM implementation for the Mercurial.
  /// </summary>
  public class Mercurial : SCM {
    #region Mercurial executable and command-line arguments
    public const String HG_EXE = "hg.exe";
    public const String CMD_VERSION = "--version";
    public const String CMD_SUMMARY_REMOTE = "summary --remote";
    #endregion

    private ProcessExecutor procExecutor;

    /// <summary>
    /// Initializes a new instance of <see cref="Mercurial"/>.
    /// </summary>
    /// <param name="processExecutor">
    /// A <see cref="ProcessExecutor"/> to use to invoke Mercurial commands.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// When <paramref name="processExecutor"/> is <see langword="null"/>.
    /// </exception>
    public Mercurial(ProcessExecutor processExecutor) {
      if (processExecutor == null) {
        throw new ArgumentNullException("ProcessExecutor is null.");
      }
      this.procExecutor = processExecutor;
    }

    /// <summary>
    /// This SCM tool supports <see cref="RepositoryType.Mercurial"/>.
    /// </summary>
    public RepositoryType RepoType { get { return RepositoryType.Mercurial; } }

    /// <summary>
    /// Returns <see langword="true"/> if Mercurial is available on the
    /// system (i.e. is it installed), <see langword="false"/> otherwise.
    /// </summary>
    /// <remarks>
    /// Quick and dirty check - Mercurial is available if <c>hg.exe</c> is
    /// in the <c>PATH</c> and responds to <c>hg --version</c> command.
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> if Mercurial is available on the
    /// system, <see langword="false"/> otherwise.
    /// </returns>
    public Boolean IsAvailable() {
      try {
        String version = GetVersionText();
        return version != null;
      }
      catch (FileNotFoundException) {
        return false;
      }
    }

    /// <summary>
    /// Returns Mercurial's version text, i.e. entire response of the
    /// <c>hg --version</c> command.
    /// </summary>
    public String GetVersionText() {
      // Approx. execution time: 125 ms.
      ProcessExecutor.Result result = procExecutor.Execute(HG_EXE, CMD_VERSION,
          ProcessExecutor.UserHome, null, TimeSpan.FromSeconds(5));
      if (result.ExitCode == 0) {
        String version = result.StdOut.Split("\n".ToCharArray())[0];
        return version;
      }
      return null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the specified path is a Mercurial
    /// repository, <see langword="false"/> otherwise.
    /// </summary>
    /// <remarks>
    /// Quick and dirty check - asume any folder containing the <c>.hg</c>
    /// sub-folder is a Mercurial repository.
    /// </remarks>
    /// <param name="path">
    /// Full path to a potential Mercurial repository.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified path is a Mercurial
    /// repository, <see langword="false"/> otherwise.
    /// </returns>
    public Boolean IsRepository(String path) {
      return Directory.Exists(Path.Combine(path, ".hg"));
    }

    /// <summary>
    /// Returns entire response to the <c>hg summary --remote</c> command.
    /// </summary>
    /// <param name="repositoryPath">
    /// Full path to a Mercurial repository.
    /// </param>
    /// <summary>
    /// Returns entire response to the <c>hg summary --remote</c> command.
    /// </summary>
    public String GetSummaryRemoteText(String repositoryPath) {
      ProcessExecutor.Result result = procExecutor.Execute(
          HG_EXE, CMD_SUMMARY_REMOTE,
          repositoryPath, null, TimeSpan.FromSeconds(5));
      if (result.ExitCode == 0) {
        return result.StdOut;
      }
      return null;
    }

    private static readonly Regex REX_REMOTE_LINE = new Regex(
        "^remote: (.+)$", RegexOptions.Multiline);
    private static readonly Regex REX_REMOTE_LINE_INCOMING = new Regex(
        "1 or more incoming");
    private static readonly Regex REX_REMOTE_LINE_OUTGOING = new Regex(
        @"(\d+) outgoing");

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
    public void ParseSummaryRemoteText(String summary, out int incoming, out int outgoing) {
      // Assume the repository is "synced".
      incoming = 0;
      outgoing = 0;

      // Locate the line starting with "remote: ", extract the rest of the line.
      Match match = REX_REMOTE_LINE.Match(summary);
      if (!match.Success) {
        throw new ArgumentException("'remote:' line not found.");
      }
      String value = match.Groups[1].Value;

      // "synced" repository has 0 changes (vars already initialized).
      if ("(synced)".Equals(value)) {
        return;
      }

      Boolean hasIncoming = false;
      Boolean hasOutgoing = false;

      if (REX_REMOTE_LINE_INCOMING.Match(value).Success) {
        incoming = 1;
        hasIncoming = true;
      }

      match = REX_REMOTE_LINE_OUTGOING.Match(value);
      if (match.Success) {
        outgoing = Convert.ToInt32(match.Groups[1].Value);
        hasOutgoing = true;
      }

      if (!hasIncoming && !hasOutgoing) {
        throw new ArgumentException("'remote:' line is invalid.");
      }
    }

  }
}
