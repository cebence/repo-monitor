using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace RepoMonitor.Core {
  /// <summary>
  /// Executes a native process and collects console output
  /// (stdout, stderr, and exit code).
  /// </summary>
  public class ProcessExecutor {
    /// <summary>
    /// Process execution results (stdout, stderr, exit code) in one place.
    /// </summary>
    public class Result {
      /// <summary>
      /// Information on the process that was executed.
      /// </summary>
      public ProcessStartInfo StartInfo { get; private set; }

      /// <summary>
      /// Exit code returned by the executable - zero (<c>0</c>)
      /// usually means successful command execution.
      /// </summary>
      public int ExitCode { get; private set; }

      /// <summary>
      /// Standard process output, i.e. <c>stdout</c>.
      /// </summary>
      public String StdOut { get; private set; }

      /// <summary>
      /// Exceptional (error) process output, i.e. <c>stderr</c>.
      /// </summary>
      public String StdErr { get; private set; }

      public Result(ProcessStartInfo startInfo, int exitCode, String stdOut, String stdErr) {
        StartInfo = startInfo;
        ExitCode = exitCode;
        StdOut = stdOut;
        StdErr = stdErr;
      }
    }

    private const int FILE_NOT_FOUND = -2147467259; // 0x80004005
    private const String EXE_NOT_FOUND = "Executable not found.";
    private const String EXE_DID_NOT_FINISH_IN_TIME = "Executable did not finish in {0}.";

    /// <summary>
    /// Initial buffer size in lines. Default value is <c>50</c>.
    /// </summary>
    /// <remarks>
    /// If you are running an executable that prints out a lot
    /// of text, e.g. usually in "verbose" mode, setting
    /// <see cref="InitialBufferSize"/> to a larger number will
    /// improve performance.
    /// </remarks>
    public int InitialBufferSize { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="ProcessExecutor"/>.
    /// </summary>
    public ProcessExecutor() {
      InitialBufferSize = 50;
    }

    /// <summary>
    /// Executes the specified command (executable and arguments)
    /// in a separate process and collects its output
    /// (stdout, stderr, exit code) in a <see cref="Result"/>.
    /// </summary>
    /// <param name="executable">
    /// File name to execute. It is recommended to specify the
    /// full path, i.e. <c>c:\path\to\some.exe</c>, but if the
    /// executable is accessible via the <c>PATH</c> there is
    /// no need to specify full path to it.
    /// </param>
    /// <param name="arguments">
    /// Command-line arguments to pass to the <paramref name="executable"/>.
    /// </param>
    /// <param name="currentDir">
    /// Current working directory for the process, e.g. repository root folder.
    /// If <see langword="null"/> is specified application's current working
    /// directory is used.
    /// </param>
    /// <param name="envVars">
    /// Additional environment variables for the process.
    /// </param>
    /// <param name="timeout">
    /// If process execution takes more time than specified kill it.
    /// </param>
    /// <returns>
    /// Process' console outputs and exit code.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// If the specified <paramref name="executable"/> could not
    /// be found, either the absolute path is invalid, the
    /// relative path is invalid or the executable is not in
    /// the <c>PATH</c>.
    /// </exception>
    /// <exception cref="TimeoutException">
    /// If process execution took more time than the specified
    /// <paramref name="timeout"/>.
    /// </exception>
    public virtual Result Execute(String executable,
        String arguments, String currentDir,
        StringDictionary envVars, TimeSpan timeout)
    {
      int ms = (int) timeout.TotalMilliseconds;
      return Execute(executable, arguments, currentDir, envVars, ms);
    }

    /// <summary>
    /// Executes the specified command (executable and arguments)
    /// in a separate process and collects its output
    /// (stdout, stderr, exit code) in a <see cref="Result"/>.
    /// </summary>
    /// <remarks>
    /// Just a short-hand in case you don't need a
    /// <see cref="ProcessExecutor"/> instance.
    /// </remarks>
    /// <param name="executable">
    /// File name to execute. It is recommended to specify the
    /// full path, i.e. <c>c:\path\to\some.exe</c>, but if the
    /// executable is accessible via the <c>PATH</c> there is
    /// no need to specify full path to it.
    /// </param>
    /// <param name="arguments">
    /// Command-line arguments to pass to the <paramref name="executable"/>.
    /// </param>
    /// <param name="currentDir">
    /// Current working directory for the process, e.g. repository root folder.
    /// If <see langword="null"/> is specified application's current working
    /// directory is used.
    /// </param>
    /// <param name="envVars">
    /// Additional environment variables for the process.
    /// </param>
    /// <param name="timeout">
    /// If process execution takes more time than specified kill it.
    /// </param>
    /// <returns>
    /// Process' console outputs and exit code.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// If the specified <paramref name="executable"/> could not
    /// be found, either the absolute path is invalid, the
    /// relative path is invalid or the executable is not in
    /// the <c>PATH</c>.
    /// </exception>
    /// <exception cref="TimeoutException">
    /// If process execution took more time than the specified
    /// <paramref name="timeout"/>.
    /// </exception>
    public static Result Exec(String executable,
        String arguments, String currentDir,
        StringDictionary envVars, TimeSpan timeout)
    {
      return new ProcessExecutor().Execute(executable, arguments,
          currentDir, envVars, timeout);
    }

    /// <summary>
    /// Executes the specified command and waits indefinitely
    /// for the process to finish.
    /// </summary>
    /// <remarks>
    /// It is recomended to set a limit on how long a process
    /// can block the current thread - see <see cref="Execute(String,String,String,StringDictionary,TimeSpan)"/>.
    /// </remarks>
    /// <param name="executable">
    /// File name to execute. It is recommended to specify the
    /// full path, i.e. <c>c:\path\to\some.exe</c>, but if the
    /// executable is accessible via the <c>PATH</c> there is
    /// no need to specify full path to it.
    /// </param>
    /// <param name="arguments">
    /// Command-line arguments to pass to the <paramref name="executable"/>.
    /// </param>
    /// <param name="currentDir">
    /// Current working directory for the process, e.g. repository root folder.
    /// If <see langword="null"/> is specified application's current working
    /// directory is used.
    /// </param>
    /// <param name="envVars">
    /// Additional environment variables for the process.
    /// </param>
    /// <returns>
    /// Process' console outputs and exit code.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// If the specified <paramref name="executable"/> could not
    /// be found, either the absolute path is invalid, the
    /// relative path is invalid or the executable is not in
    /// the <c>PATH</c>.
    /// </exception>
    /// <exception cref="TimeoutException">
    /// If process execution took more time than the specified
    /// <paramref name="timeout"/>.
    /// </exception>
    public virtual Result Execute(String executable,
        String arguments, String currentDir,
        StringDictionary envVars)
    {
      return Execute(executable, arguments, currentDir, envVars, 0);
    }

    private Result Execute(String executable,
        String arguments, String currentDir,
        StringDictionary envVars, int timeout)
    {
      if (Path.IsPathRooted(executable) && !File.Exists(executable)) {
        throw new FileNotFoundException(EXE_NOT_FOUND, executable);
      }

      if (String.IsNullOrEmpty(currentDir)) {
        currentDir = Environment.CurrentDirectory;
      }

      List<String> bufferOut = new List<String>(InitialBufferSize);
      List<String> bufferErr = new List<String>(InitialBufferSize);
      Result result = null;

      Process process = CreateProcess(executable, arguments,
          currentDir, envVars, bufferOut, bufferErr);
      using (process) {
        try {
          process.Start();
        }
        catch (Win32Exception wex) {
          if (wex.ErrorCode == FILE_NOT_FOUND) {
            throw new FileNotFoundException(EXE_NOT_FOUND, executable, wex);
          }
          throw /*wex*/;
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (timeout == 0) {
          process.WaitForExit();
          result = new Result(process.StartInfo,
              process.ExitCode,
              String.Join(Environment.NewLine, bufferOut),
              String.Join(Environment.NewLine, bufferErr));
        }
        else {
          Boolean finished = process.WaitForExit(timeout);
          if (finished) {
            result = new Result(process.StartInfo,
                process.ExitCode,
                String.Join(Environment.NewLine, bufferOut),
                String.Join(Environment.NewLine, bufferErr));
          }
          else {
            // XXX Console.WriteLine("About to kill {0}", process.Id);
            process.Kill();
            String message = String.Format(EXE_DID_NOT_FINISH_IN_TIME,
                TimeSpan.FromMilliseconds(timeout));
            throw new TimeoutException(message);
          }
        }
      }

      return result;
    }

    private Process CreateProcess(String executable, String args,
        String currentDir, StringDictionary envVars,
        List<String> bufferOut, List<String> bufferErr)
    {
      Process p = new Process();
      ConfigureProcessStartInfo(p.StartInfo,
          executable, args, currentDir, envVars);

      // Collect all text into line buffers.
      p.OutputDataReceived += new DataReceivedEventHandler((sender, e) => {
        if (!String.IsNullOrEmpty(e.Data)) {
          bufferOut.Add(e.Data);
        }
      });
      p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => {
        if (!String.IsNullOrEmpty(e.Data)) {
          bufferErr.Add(e.Data);
        }
      });

      return p;
    }

    private void ConfigureProcessStartInfo(ProcessStartInfo psi,
        String executable, String arguments,
        String currentDir, StringDictionary envVars)
    {
      psi.FileName = executable;
      psi.Arguments = arguments;
      psi.WorkingDirectory = currentDir;

      if (envVars != null) {
        // Can't assign the property so copy any items.
        foreach (DictionaryEntry envVar in envVars) {
          psi.EnvironmentVariables.Add(
              envVar.Key as String, envVar.Value as String);
        }
      }

      psi.UseShellExecute = false;
      psi.CreateNoWindow = true;
      psi.RedirectStandardOutput = true;
      psi.RedirectStandardError = true;
    }

    /// <summary>
    /// Full path to active user's profile folder, i.e. <c>%UserProfile%</c>
    /// which is usually <c>C:\Users\{username}</c>.
    /// </summary>
    public static String UserHome {
      get {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      }
    }
  }
}
