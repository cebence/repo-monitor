using System;

namespace RepoMonitor.Core {
  /// <summary>
  /// Executes a native process and collects console output (stdout,
  /// stderr, and exit code).
  /// </summary>
  public class ProcessExecutor {
    /// <summary>
    /// Process execution results (stdout, stderr, exit code) in one place.
    /// </summary>
    public class Result {
      public int ExitCode { get; private set; }
      public String StdOut { get; private set; }
      public String StdErr { get; private set; }
      public Boolean Success { get { return ExitCode == 0; } }

      public Result(int exitCode, String stdOut, String stdErr) {
        ExitCode = exitCode;
        StdOut = stdOut;
        StdErr = stdErr;
      }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ProcessExecutor"/>.
    /// </summary>
    public ProcessExecutor() {
    }

    /// <summary>
    /// Executes the given command in a separate process and collects its output
    /// (stdout, stderr, exit code) in a <see cref="Result"/>.
    /// </summary>
    /// <remarks>
    /// TODO Any notes on process execution?
    /// </remarks>
    /// <param name="command">
    /// Full command-line to execute, i.e. "path\to\some.exe arguments".
    /// If the executable is accessible via the <c>PATH</c> there's no need to
    /// specify full path to it.
    /// </param>
    /// <param name="currentDir">
    /// Current working directory for the process, e.g. repository root folder.
    /// If <see langword="null"/> is specified application's current working
    /// directory is used.
    /// </param>
    /// <param name="timeout">
    /// If process execution takes more time than specified kill it.
    /// </param>
    /// <param name="envVars">
    /// Additional environment variables for the process.
    /// </param>
    /// <returns>
    /// Process execution console output and exit code.
    /// </returns>
    public virtual Result Execute(String command, String currentDir,
        TimeSpan timeout, String envVars = "")
    {
      // TODO Implement ProcessStartInfo.
      return new Result(0, String.Empty, String.Empty);
    }

    /// <summary>
    /// Full path to active user's profile folder, i.e. <c>%UserProfile%</c>
    /// which is usually <c>C:\Users\{username}</c>.
    /// </summary>
    public static String UserHome
    {
      get {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      }
    }
  }
}
