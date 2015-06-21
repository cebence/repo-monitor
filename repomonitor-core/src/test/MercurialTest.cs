using Moq;
using NUnit.Framework;
using RepoMonitor.Core;
using System;
using System.IO;

namespace RepoMonitor.Core.UnitTests {
  /// <summary>
  /// Class under test: <see cref="Mercurial"/>
  /// </summary>
  [TestFixture]
  public class MercurialTest {
    #region Property names
    private const String PROP_INCOMING = "Incoming";
    private const String PROP_NAME = "Name";
    private const String PROP_OUTGOING = "Outgoing";
    private const String PROP_STATUS = "Status";
    private const String PROP_URL = "Url";
    #endregion

    #region Utility methods

    /// <summary>
    /// Returns the full path to the `test-resources` folder (inside the
    /// test project) by searching upward from the given directory or
    /// <see langword="null"/> if it couldn't be found.
    /// </summary>
    /// <param name="startingPath">
    /// directory to start from (included in the search)
    /// </param>
    /// <returns>
    /// Path to the `test-resources` folder, if found, or <see langword="null"/>
    /// </returns>
    public static String FindResourcesPath(String startingPath) {
      if (Directory.Exists(startingPath)) {
        DirectoryInfo current = new DirectoryInfo(startingPath);
        while (current != null) {
          String testResources = Path.Combine(current.FullName, "test-resources");
          if (Directory.Exists(testResources)) {
            return testResources;
          }
          current = current.Parent;
        }
      }
      return null;
    }

    /// <summary>
    /// Returns the full path to the `test-resources` folder (inside the
    /// test project) by searching upward from the current working directory
    /// or <see langword="null"/> if it couldn't be found.
    /// </summary>
    /// <returns>
    /// Path to the `test-resources` folder, if found, or <see langword="null"/>
    /// </returns>
    public static String FindResourcesPath() {
      return FindResourcesPath(Directory.GetCurrentDirectory());
    }

    #endregion

    #region Test data
    private const String HG_VERSION =
        "Mercurial Distributed SCM (version 2.0.2)\n" +
        "(see http://mercurial.selenic.com for more information)\n" +
        "\n" +
        "Copyright (C) 2005-2011 Matt Mackall and others\n" +
        "This is free software; see the source for copying conditions. There is NO\n" +
        "warranty; not even for MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.";
    #endregion

    private Mercurial hgInstance;
    private ProcessExecutor processExecutorMock;

    /// <summary>
    /// Configures the <see cref="ProcessExecutor"/> mock.
    /// </summary>
    [TestFixtureSetUp]
    public void ConfigureProcessExecutorMock() {
      // Creates a ProcessExecutor mock that returns predefined execution output.
      var mock = new Mock<ProcessExecutor>();

      ProcessExecutor.Result versionResult = new ProcessExecutor.Result(0, HG_VERSION, String.Empty);
      mock.Setup(pe =>
          pe.Execute(Mercurial.CMD_VERSION, It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<string>()))
          .Returns(versionResult);

      processExecutorMock = mock.Object;
    }

    /// <summary>
    /// Sets up a Mercurial instance with ProcessExecutor mock that will return
    /// predefined execution results for testing output parsing logic.
    /// </summary>
    [SetUp]
    public void SetUp() {
      hgInstance = new Mercurial(processExecutorMock);
    }

    [TearDown]
    public void TearDown() {
      hgInstance = null;
    }

    /// <summary>
    /// Confirm a folder with the `.hg` sub-folder is recognized as a repository.
    /// </summary>
    [Test]
    public void RecognizeValidHgRepository() {
      String testRepo = Path.Combine(FindResourcesPath(), "test-repo");
      Boolean expected = true;

      Boolean actual = hgInstance.IsRepository(testRepo);

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Confirm a folder without the `.hg` sub-folder is not recognized as a repository.
    /// </summary>
    [Test]
    public void RecognizeInvalidHgRepository() {
      String testRepo = Path.Combine(FindResourcesPath(), "fake-repo");
      Boolean expected = false;

      Boolean actual = hgInstance.IsRepository(testRepo);

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Confirm a non-existing folder is not recognized as a repository.
    /// </summary>
    [Test]
    public void NonexistingFolderIsNotHgRepository() {
      String testRepo = Path.Combine(FindResourcesPath(), "123");
      Boolean expected = false;

      Boolean actual = hgInstance.IsRepository(testRepo);

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Confirm the version info is only the first line of Mercurial output
    /// to the `hg --version` command.
    /// </summary>
    [Test]
    public void GetVersionTextReturnsOnlyTheFirstLine() {
      String expected = "Mercurial Distributed SCM (version 2.0.2)";
      String actual = hgInstance.GetVersionText();

      Assert.AreEqual(expected, actual);
    }
  }
}
