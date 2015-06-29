using Moq;
using NUnit.Framework;
using RepoMonitor.Core;
using System;
using System.Collections.Specialized;
using System.IO;

namespace RepoMonitor.Core.UnitTests {
  /// <summary>
  /// Class under test: <see cref="ProcessExecutor"/>
  /// </summary>
  [TestFixture]
  public class ProcessExecutorTest {
    #region Utility methods

    /// <summary>
    /// Returns the full path to solution's "packages" folder (where NuGet
    /// dependencies are kept) by searching upward from the given directory or
    /// <see langword="null"/> if it couldn't be found.
    /// </summary>
    /// <param name="startingPath">
    /// Directory to start from (included in the search).
    /// If <see langword="null"/> current directory is used.
    /// </param>
    /// <returns>
    /// Full path to the "packages" folder, if found, or <see langword="null"/>
    /// </returns>
    public static String FindPackagesFolder(String startingPath = null) {
      if (startingPath == null) {
          startingPath = Directory.GetCurrentDirectory();
      }
      if (Directory.Exists(startingPath)) {
        DirectoryInfo current = new DirectoryInfo(startingPath);
        while (current != null) {
          String path = Path.Combine(current.FullName, "packages");
          if (Directory.Exists(path)) {
            return path;
          }
          current = current.Parent;
        }
      }
      return null;
    }

    /// <summary>
    /// Returns the full path to the "echoer.exe" file (inside solution's
    /// NuGet packages) by searching upward from the given directory or
    /// <see langword="null"/> if it couldn't be found.
    /// </summary>
    /// <param name="startingPath">
    /// Directory to start from (included in the search).
    /// If <see langword="null"/> current directory is used.
    /// </param>
    /// <returns>
    /// Full path to the "echoer.exe", if found, or <see langword="null"/>
    /// </returns>
    public static String FindEchoerTool(String startingPath = null) {
      String packages = FindPackagesFolder(startingPath);
      if (packages != null) {
        String[] dirs = Directory.GetDirectories(packages, "echoer.*");
        if (dirs.Length > 0) {
          String echoer = Path.Combine(dirs[0], "lib", "net45", "echoer.exe");
          if (File.Exists(echoer)) {
            return echoer;
          }
        }
      }
      return null;
    }

    #endregion

    private String echoerPath;

    /// <summary>
    /// Configures the <see cref="ProcessExecutor"/> mock.
    /// </summary>
    [TestFixtureSetUp]
    public void ConfigureProcessExecutorMock() {
      echoerPath = FindEchoerTool();
      if (echoerPath == null) {
        throw new Exception("Could not find Echoer tool.");
      }
    }

    /// <summary>
    /// Confirm all the parameters are correctly passed and collected.
    /// </summary>
    [Test]
    public void ProcessInfoIsCorrect() {
      const String expectedExe = "cmd.exe";
      const String expectedArgs = "/c echo 1";
      const String expectedFolder = @"C:\";
      const String expectedName = "RABBIT";
      const String expectedValue = "ROGER";

      StringDictionary expectedVars = new StringDictionary();
      expectedVars.Add(expectedName, expectedValue);

      ProcessExecutor pe = new ProcessExecutor();
      ProcessExecutor.Result result = pe.Execute(expectedExe,
          expectedArgs, expectedFolder, expectedVars, TimeSpan.FromSeconds(1));

      Assert.AreEqual(expectedExe, result.StartInfo.FileName);
      Assert.AreEqual(expectedArgs, result.StartInfo.Arguments);
      Assert.AreEqual(expectedFolder, result.StartInfo.WorkingDirectory);

      StringDictionary envvars = result.StartInfo.EnvironmentVariables;
      Assert.IsTrue(envvars.ContainsKey(expectedName));
      Assert.AreEqual(expectedValue, envvars[expectedName]);
    }

    /// <summary>
    /// Confirm the executable's exit code was collected.
    /// </summary>
    [Test]
    public void ExitCodeIsCollected() {
      const int expected = 21;

      ProcessExecutor pe = new ProcessExecutor();
      ProcessExecutor.Result result = pe.Execute(echoerPath,
          String.Format("-exit {0}", expected),
          null, null, TimeSpan.FromSeconds(1));

      int actual = result.ExitCode;

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Confirm the executable's stdout output was collected.
    /// </summary>
    [Test]
    public void StdOutIsCollected() {
      const String expected = "1, 2, 3, testing";

      ProcessExecutor pe = new ProcessExecutor();
      ProcessExecutor.Result result = pe.Execute(echoerPath,
          String.Format("-out \"{0}\"", expected),
          null, null, TimeSpan.FromSeconds(1));

      String actual = result.StdOut;

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Confirm the executable's stderr output was collected.
    /// </summary>
    [Test]
    public void StdErrIsCollected() {
      const String expected = "F1! F1!";

      ProcessExecutor pe = new ProcessExecutor();
      ProcessExecutor.Result result = pe.Execute(echoerPath,
          String.Format("-err \"{0}\"", expected),
          null, null, TimeSpan.FromSeconds(1));

      String actual = result.StdErr;

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Confirm the executable sees the custom envvar.
    /// </summary>
    [Test]
    public void EnvvarIsPassed() {
      const String expectedName = "RABBIT";
      const String expectedValue = "ROGER";

      StringDictionary expectedVars = new StringDictionary();
      expectedVars.Add(expectedName, expectedValue);

      ProcessExecutor pe = new ProcessExecutor();
      ProcessExecutor.Result result = pe.Execute(echoerPath,
          String.Format("-env {0}", expectedName),
          null, expectedVars, TimeSpan.FromSeconds(1));

      String actual = result.StdOut;

      Assert.AreEqual(expectedValue, actual);
    }

    /// <summary>
    /// Confirm exception is thrown when process exceeds timeout limit.
    /// </summary>
    [Test, ExpectedException("System.TimeoutException")]
    public void TooLongProcessingThrowsException() {
      new ProcessExecutor().Execute(echoerPath, "-wait 2",
          null, null, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Confirm exception is thrown in case of invalid absolute path.
    /// </summary>
    [Test, ExpectedException("System.IO.FileNotFoundException")]
    public void InvalidAbsolutePathThrowsException() {
      // Create an invalid path by appending "-not" to a valid one.
      String invalidPath = echoerPath + "-not";

      new ProcessExecutor().Execute(invalidPath, "-out Ouch!",
          null, null, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Confirm exception is thrown in case of invalid relative path.
    /// </summary>
    [Test, ExpectedException("System.IO.FileNotFoundException")]
    public void InvalidRelativePathThrowsException() {
      // This executable should not exist in the current folder or the PATH.
      String invalidPath = String.Format("{0}.exe", Path.GetRandomFileName());

      new ProcessExecutor().Execute(invalidPath, "-out Ouch!",
          null, null, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Confirm the static "Exec()" calls the instance method.
    /// </summary>
    [Test, ExpectedException("System.IO.FileNotFoundException")]
    public void StaticExecAlsoWorks() {
      ProcessExecutor.Exec(echoerPath + "-not", "-out Ouch!",
          null, null, TimeSpan.FromSeconds(1));
    }
  }
}
