using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
//using System.Text;

// https://wadl.java.net/
// http://raml.org/
// http://www.sitepoint.com/raml-restful-api-modeling-language/
// http://www.sitepoint.com/writing-api-documentation-slate/
// http://swagger.io/

// #note: We recommend that you add the backslash# https://msdn.microsoft.com/en-us/library/dd576348.aspx
// >>> http://stackoverflow.com/questions/5321959/remove-trailing-backslash-from-msbuild-batching-directory-property
// http://blogs.msdn.com/b/dsvc/archive/2012/02/29/output-from-exec-task-resulting-in-build-failure.aspx
// StdOutEncoding @ https://msdn.microsoft.com/en-us/library/x8zx72cd.aspx

// http://buksbaum.us/2011/08/20/gentle-introduction-to-mefpart-one/

// https://www.packtpub.com/application-development/microsoft-windows-communication-foundation-40-cookbook-developing-soa-applic
// https://www.packtpub.com/application-development/net-framework-45-expert-programming-cookbook

namespace RepoMonitor.Core {
  /// <summary>
  /// Executes a native process and collects console output (stdout,
  /// stderr, and exit code).
  /// </summary>
  public class ProcessExecutor {
    static void Main(String[] args) {
      String currentDir = Environment.CurrentDirectory;
      String exe = Path.GetFullPath(Path.Combine(new String[] { currentDir, "..", "..", "..", "echoer.exe" }));
      //String exeArgs = "-wait 6 -out \"Too long\"";
      String exeArgs = "-out \"Something here\" -err \"And there\"";
      //String exeArgs = "-out \"Mercurial Distributed SCM (version 2.0.2)\" -out \"(see http://mercurial.selenic.com for more information)\"";
      TimeSpan timeout = TimeSpan.FromSeconds(5);
      StringDictionary envVars = new StringDictionary();

      ProcessExecutor pe = new ProcessExecutor(exe, exeArgs, currentDir);
      pe.EnvVars = envVars;

// TODO exceptions @ https://msdn.microsoft.com/en-us/library/0w4h05yb%28v=vs.110%29.aspx
      try {
        pe.Execute(timeout);

        Console.WriteLine();
        Console.WriteLine("OUT = {0}", pe.StdOut);
        Console.WriteLine("ERR = {0}", pe.StdErr);
        Console.WriteLine("EXIT CODE = {0}", pe.ExitCode);
      }
      catch (Exception e) {
        Console.WriteLine();
        Console.WriteLine(e);
      }
    }






    private ProcessStartInfo _info;
    private List<String> _listOut = new List<String>();
    //XXX private StringBuilder _out = new StringBuilder();
    private List<String> _listErr = new List<String>();
    //XXX private StringBuilder _err = new StringBuilder();

    /// <summary>
    /// TODO ExitCode
    /// </summary>
    public int ExitCode { get; private set; }

    /// <summary>
    /// TODO StdOut
    /// </summary>
    public String StdOut {
      get {
        return String.Join(Environment.NewLine, _listOut);
        //XXX return _out.ToString();
      }
    }

    /// <summary>
    /// TODO StdErr
    /// </summary>
    public String StdErr {
      get {
        return String.Join(Environment.NewLine, _listErr);
        //XXX return _err.ToString();
      }
    }

    /// <summary>
    /// TODO EnvVars
    /// </summary>
    public StringDictionary EnvVars { get; set; }

    public ProcessExecutor(String executable, String arguments, String currentDir) {
      _info = new ProcessStartInfo(executable);
      _info.Arguments = arguments;
      _info.WorkingDirectory = currentDir;

      _info.UseShellExecute = false;
      _info.CreateNoWindow = true; // ?!
      //_info.WindowStyle = ProcessWindowStyle.Hidden;

      _info.RedirectStandardOutput = true;
      _info.RedirectStandardError = true;
    }

// TODO https://msdn.microsoft.com/en-us/library/system.diagnostics.process.outputdatareceived%28v=vs.110%29.aspx
//      https://msdn.microsoft.com/en-us/library/system.diagnostics.process.errordatareceived%28v=vs.110%29.aspx

    // TODO <exception "FileNotFoundException"/>
    // TODO <exception "TimeoutException"/>
    public void Execute(TimeSpan timeout) {
      int millis = (int) timeout.TotalMilliseconds;
      Execute(millis);
    }

    // TODO <exception "FileNotFoundException"/>
    // TODO <exception "TimeoutException"/>
    public void Execute(int timeout = 0) {
      if (!File.Exists(_info.FileName)) {
        throw new FileNotFoundException("Executable not found.", _info.FileName);
      }

      if (EnvVars != null) {
        // Can't assign the property so copy any items from one to another.
        foreach (DictionaryEntry envVar in EnvVars) {
          _info.EnvironmentVariables.Add(envVar.Key as String, envVar.Value as String);
        }
      }

      Console.WriteLine("About to execute:");
      Console.WriteLine("  FileName = {0}", _info.FileName);
      Console.WriteLine("  Arguments = {0}", _info.Arguments);
      Console.WriteLine("  WorkingDir = {0}", _info.WorkingDirectory);
      Console.WriteLine("  EnvVars = {0}",  _info.EnvironmentVariables);
      Console.WriteLine("  Timeout = {0}", timeout);

      using (Process process = new Process()) {
        process.StartInfo = _info;

        // Collect all the text from the process' standard output stream.
        process.OutputDataReceived += new DataReceivedEventHandler((sender, e) => {
          if (!String.IsNullOrEmpty(e.Data)) {
            _listOut.Add(e.Data);
            //XXX _out.Append(e.Data + Environment.NewLine);
          }
        });

        // Collect all the text from the process' standard error stream.
        process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => {
          if (!String.IsNullOrEmpty(e.Data)) {
            _listErr.Add(e.Data);
            //XXX _err.Append(e.Data + Environment.NewLine);
          }
        });

        process.Start();
        Console.WriteLine("Process ID = {0}", process.Id);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (timeout == 0) {
          process.WaitForExit();
          ExitCode = process.ExitCode;
        }
        else {
          Boolean finished = process.WaitForExit(timeout);
          if (finished) {
            ExitCode = process.ExitCode;

            // TODO public event EventHandler Finished;
            // TODO raise Finished(new EventArgs(this));
          }
          else {
            Console.WriteLine("About to kill {0}", process.Id); //Process.GetProcessById(process.Id) && .Responding && .HasExited
            process.Kill(); // XXX Do we need to wait? process.WaitForExit();
            String message = String.Format("Executable did not finish in {0}.",
                TimeSpan.FromMilliseconds(timeout));
            throw new TimeoutException(message);
          }
        }
      }
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
