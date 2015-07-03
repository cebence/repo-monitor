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
    /// <summary>
    /// Default timeout for tests is 5 seconds.
    /// </summary>
    const int DEFAULT_TIMEOUT = 5;

    /// <summary>
    /// Configures the <see cref="ProcessExecutor"/> mock.
    /// </summary>
    [TestFixtureSetUp]
    public void ConfigureProcessExecutorMock() {
      if (TestUtil.EchoerPath == null) {
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
      ProcessExecutor.Result result = pe.Execute(TestUtil.EchoerPath,
          String.Format("-exit {0}", expected),
          null, null, TimeSpan.FromSeconds(DEFAULT_TIMEOUT));

      int actual = result.ExitCode;

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Confirm the "no-timeout" version works the same.
    /// </summary>
    [Test]
    public void ExecuteNoTimeoutCollectsExitCode() {
      const int expected = 21;

      ProcessExecutor pe = new ProcessExecutor();
      ProcessExecutor.Result result = pe.Execute(TestUtil.EchoerPath,
          String.Format("-exit {0}", expected), null, null);

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
      ProcessExecutor.Result result = pe.Execute(TestUtil.EchoerPath,
          String.Format("-out \"{0}\"", expected),
          null, null, TimeSpan.FromSeconds(DEFAULT_TIMEOUT));

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
      ProcessExecutor.Result result = pe.Execute(TestUtil.EchoerPath,
          String.Format("-err \"{0}\"", expected),
          null, null, TimeSpan.FromSeconds(DEFAULT_TIMEOUT));

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
      ProcessExecutor.Result result = pe.Execute(TestUtil.EchoerPath,
          String.Format("-env {0}", expectedName),
          null, expectedVars, TimeSpan.FromSeconds(DEFAULT_TIMEOUT));

      String actual = result.StdOut;

      Assert.AreEqual(expectedValue, actual);
    }

    /// <summary>
    /// Confirm exception is thrown when process exceeds timeout limit.
    /// </summary>
    [Test, ExpectedException(typeof(TimeoutException))]
    public void TooLongProcessingThrowsException() {
      new ProcessExecutor().Execute(TestUtil.EchoerPath, "-wait 2",
          null, null, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Confirm exception is thrown in case of invalid absolute path.
    /// </summary>
    [Test, ExpectedException(typeof(FileNotFoundException))]
    public void InvalidAbsolutePathThrowsException() {
      // Create an invalid path by appending "-not" to a valid one.
      String invalidPath = TestUtil.EchoerPath + "-not";

      new ProcessExecutor().Execute(invalidPath, "-out Ouch!",
          null, null, TimeSpan.FromSeconds(DEFAULT_TIMEOUT));
    }

    /// <summary>
    /// Confirm exception is thrown in case of invalid relative path.
    /// </summary>
    [Test, ExpectedException(typeof(FileNotFoundException))]
    public void InvalidRelativePathThrowsException() {
      // This executable should not exist in the current folder or the PATH.
      String invalidPath = String.Format("{0}.exe", Path.GetRandomFileName());

      new ProcessExecutor().Execute(invalidPath, "-out Ouch!",
          null, null, TimeSpan.FromSeconds(DEFAULT_TIMEOUT));
    }

    /// <summary>
    /// Confirm the static "Exec()" calls the instance method.
    /// </summary>
    [Test, ExpectedException(typeof(FileNotFoundException))]
    public void StaticExecAlsoWorks() {
      ProcessExecutor.Exec(TestUtil.EchoerPath + "-not", "-out Ouch!",
          null, null, TimeSpan.FromSeconds(DEFAULT_TIMEOUT));
    }
  }
}
