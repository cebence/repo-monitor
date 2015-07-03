using Moq;
using NUnit.Framework;
using RepoMonitor.Core;
using System;
using System.Collections.Specialized;
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

    #region Test data
    private const String HG_VERSION =
        "Mercurial Distributed SCM (version 2.0.2)\n" +
        "(see http://mercurial.selenic.com for more information)\n" +
        "\n" +
        "Copyright (C) 2005-2011 Matt Mackall and others\n" +
        "This is free software; see the source for copying conditions. There is NO\n" +
        "warranty; not even for MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.";
    #endregion

    #region Instantiation test cases

    /// <summary>
    /// Confirm the specified ProcessExecutor instance is used.
    /// </summary>
    [Test]
    public void MercurialInstanceUsesSpecifiedExecutor() {
      var peMock = new Mock<ProcessExecutor>();
      ConfigureMock_CMD_VERSION(peMock);

      Mercurial hg = new Mercurial(peMock.Object);
      hg.GetVersionText();

      peMock.Verify(pe => pe.Execute(
          Mercurial.HG_EXE,
          Mercurial.CMD_VERSION,
          It.IsAny<String>(),
          It.IsAny<StringDictionary>(),
          It.IsAny<TimeSpan>()
      ));
    }

    #endregion

    #region IsRepository(String) test cases

    /// <summary>
    /// Confirm a folder with the `.hg` sub-folder is recognized as a repository.
    /// </summary>
    [Test]
    public void RecognizeValidHgRepository() {
      String testRepo = Path.Combine(TestUtil.TestResourcesPath, "test-repo");
      Boolean expected = true;

      Mercurial hg = new Mercurial(new Mock<ProcessExecutor>().Object);
      Boolean actual = hg.IsRepository(testRepo);

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Confirm a folder without the `.hg` sub-folder is not recognized as a repository.
    /// </summary>
    [Test]
    public void RecognizeInvalidHgRepository() {
      String testRepo = Path.Combine(TestUtil.TestResourcesPath, "fake-repo");
      Boolean expected = false;

      Mercurial hg = new Mercurial(new Mock<ProcessExecutor>().Object);
      Boolean actual = hg.IsRepository(testRepo);

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Confirm a non-existing folder is not recognized as a repository.
    /// </summary>
    [Test]
    public void NonexistingFolderIsNotHgRepository() {
      String testRepo = Path.Combine(TestUtil.TestResourcesPath, "123");
      Boolean expected = false;

      Mercurial hg = new Mercurial(new Mock<ProcessExecutor>().Object);
      Boolean actual = hg.IsRepository(testRepo);

      Assert.AreEqual(expected, actual);
    }

    #endregion

    #region GetVersionText() test cases

    /// <summary>
    /// Confirm the version info is only the first line of Mercurial output
    /// to the `hg --version` command.
    /// </summary>
    [Test]
    public void GetVersionTextReturnsOnlyTheFirstLine() {
      String expected = "Mercurial Distributed SCM (version 2.0.2)";

      var peMock = new Mock<ProcessExecutor>();
      ConfigureMock_CMD_VERSION(peMock);

      Mercurial hg = new Mercurial(peMock.Object);
      String actual = hg.GetVersionText();

      Assert.AreEqual(expected, actual);
    }

    #endregion

    #region IsAvailable() test cases

    /// <summary>
    /// Confirm the successful execution of the `hg --version` command
    /// is recognized as "Mercurial is installed".
    /// </summary>
    [Test]
    public void IsAvailableReturnsTrue() {
      Boolean expected = true;

      var peMock = new Mock<ProcessExecutor>();
      ConfigureMock_CMD_VERSION(peMock);

      Mercurial hg = new Mercurial(peMock.Object);
      Boolean actual = hg.IsAvailable();

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Confirm the FileNotFoundException to the `hg --version` command
    /// is recognized as "Mercurial installation not found".
    /// </summary>
    [Test]
    public void IsAvailableReturnsFalse() {
      Boolean expected = false;

      var peMock = new Mock<ProcessExecutor>();
      ConfigureMock_FILE_NOT_FOUND(peMock);

      Mercurial hg = new Mercurial(peMock.Object);
      Boolean actual = hg.IsAvailable();

      Assert.AreEqual(expected, actual);
    }

    #endregion

    #region method() test cases
    #endregion
    
    #region method() test cases
    #endregion

    #region method() test cases
    #endregion

    #region method() test cases
    #endregion

    #region method() test cases
    #endregion

    #region Mocking methods

    /// <summary>
    /// Configures the specified mock to respond to the "hg --version" command
    /// and returns the mock.
    /// </summary>
    Mock<ProcessExecutor> ConfigureMock_CMD_VERSION(Mock<ProcessExecutor> mock) {
      ProcessExecutor.Result versionResult = new ProcessExecutor.Result(null,
          0, HG_VERSION, String.Empty);
      mock.Setup(pe => pe.Execute(
          Mercurial.HG_EXE,
          Mercurial.CMD_VERSION,
          It.IsAny<String>(),
          It.IsAny<StringDictionary>(),
          It.IsAny<TimeSpan>())
      ).Returns(versionResult);

      return mock;
    }

    /// <summary>
    /// Configures the specified mock to always respond with the
    /// FileNotFoundException exception.
    /// </summary>
    Mock<ProcessExecutor> ConfigureMock_FILE_NOT_FOUND(Mock<ProcessExecutor> mock) {
      mock.Setup(pe => pe.Execute(
          It.IsAny<String>(),
          It.IsAny<String>(),
          It.IsAny<String>(),
          It.IsAny<StringDictionary>(),
          It.IsAny<TimeSpan>())
      ).Throws<FileNotFoundException>();

      return mock;
    }

    #endregion
  }
}
