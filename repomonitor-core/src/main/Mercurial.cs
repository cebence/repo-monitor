using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace RepoMonitor.Core {
  /// <summary>
  /// SCM implementation for the Mercurial.
  /// </summary>
  public class Mercurial : SCM {
    #region Mercurial executable and command-line arguments
    public const String HG_EXE = "hg.exe";
    public const String CMD_VERSION = "--version";
    #endregion

    private ProcessExecutor procExecutor;

    /// <summary>
    /// Initializes a new instance of <see cref="Mercurial"/>.
    /// </summary>
    public Mercurial(ProcessExecutor processExecutor) {
      this.procExecutor = processExecutor;
    }

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
      catch (FileNotFoundException e) {
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
    /// <returns>
    /// <see langword="true"/> if the specified path is a Mercurial
    /// repository, <see langword="false"/> otherwise.
    /// </returns>
    public Boolean IsRepository(String path) {
      return Directory.Exists(Path.Combine(path, ".hg"));
    }
  }
}
