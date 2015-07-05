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
    private const String HG_SUMMARY_SYNCED =
        "parent: 0:6678dbbc2b3d tip\n" +
        " 1st change.\n" +
        "branch: default\n" +
        "commit: (clean)\n" +
        "update: (current)\n" +
        "remote: (synced)";
    private const String HG_SUMMARY_INCOMING =
        "parent: 1:c3a767fedb7d tip\n" +
        " Incoming.\n" +
        "branch: default\n" +
        "commit: (clean)\n" +
        "update: (current)\n" +
        "remote: 1 or more incoming";
    private const String HG_SUMMARY_OUTGOING =
        "parent: 1:c3a767fedb7d tip\n" +
        " 2nd outgoing.\n" +
        "branch: default\n" +
        "commit: (clean)\n" +
        "update: (current)\n" +
        "remote: 2 outgoing";
    private const String HG_SUMMARY_ALLDIRTY =
        "parent: 2:71e86589c1bb tip\n" +
        " All dirty.\n" +
        "branch: default\n" +
        "commit: (clean)\n" +
        "update: (current)\n" +
        "remote: 1 or more incoming, 2 outgoing";
    #endregion

    #region Instantiation test cases
    /// <summary>
    /// Confirm the newly created <see cref="Mercurial"/> is initialized.
    /// </summary>
    [Test]
    public void NewMercurialIsInitialized() {
      Mercurial hg = new Mercurial(new Mock<ProcessExecutor>().Object);

      Assert.IsNotNull(hg);
    }

    /// <summary>
    /// Confirm an exception is thrown when trying to create
    /// <see cref="Mercurial"/> without ProcessExecutor.
    /// </summary>
    [Test]
    public void InitializingMercurialWithNoExecutorThrowsExeption() {
      Assert.Throws<ArgumentNullException>(() => new Mercurial(null));
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
      peMock.Verify(pe => pe.Execute(
          Mercurial.HG_EXE,
          Mercurial.CMD_VERSION,
          It.IsAny<String>(),
          It.IsAny<StringDictionary>(),
          It.IsAny<TimeSpan>()
      ));
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

    #region GetSummaryRemoteText() test cases
    /// <summary>
    /// Confirm the `hg summary --remote` command was issued and the output
    /// is collected.
    /// </summary>
    [Test]
    public void GetSummaryRemoteTextReturnsSynced() {
      String expected = HG_SUMMARY_SYNCED;
      String repoPath = @"path\to\repo";

      var peMock = new Mock<ProcessExecutor>();
      ConfigureMock_CMD_SUMMARY_REMOTE(peMock, HG_SUMMARY_SYNCED);

      Mercurial hg = new Mercurial(peMock.Object);
      String actual = hg.GetSummaryRemoteText(repoPath);

      Assert.AreEqual(expected, actual);
      peMock.Verify(pe => pe.Execute(
          Mercurial.HG_EXE,
          Mercurial.CMD_SUMMARY_REMOTE,
          repoPath,
          It.IsAny<StringDictionary>(),
          It.IsAny<TimeSpan>()
      ));
    }
    #endregion

    #region ParseSummaryRemoteText() test cases
    /// <summary>
    /// Confirm the synced summary text is parsed into "no changes", i.e.
    /// both incoming and outgoing are zero.
    /// </summary>
    [Test]
    public void SyncedSummaryResultsInNoChanges() {
      const int expectedIncoming = 0;
      const int expectedOutgoing = 0;
      int actualIncoming;
      int actualdOutgoing;

      Mercurial hg = new Mercurial(new Mock<ProcessExecutor>().Object);
      hg.ParseSummaryRemoteText(HG_SUMMARY_SYNCED,
          out actualIncoming, out actualdOutgoing);

      Assert.AreEqual(expectedIncoming, actualIncoming);
      Assert.AreEqual(expectedOutgoing, actualdOutgoing);
    }

    /// <summary>
    /// Confirm the incoming-only summary text is parsed into "1/0", i.e.
    /// incoming is 1 and outgoing is 0.
    /// </summary>
    [Test]
    public void IncomingOnlySummaryResultsInIncomingChangesOnly() {
      const int expectedIncoming = 1;
      const int expectedOutgoing = 0;
      int actualIncoming;
      int actualdOutgoing;

      Mercurial hg = new Mercurial(new Mock<ProcessExecutor>().Object);
      hg.ParseSummaryRemoteText(HG_SUMMARY_INCOMING,
          out actualIncoming, out actualdOutgoing);

      Assert.AreEqual(expectedIncoming, actualIncoming);
      Assert.AreEqual(expectedOutgoing, actualdOutgoing);
    }

    /// <summary>
    /// Confirm the outgoing-only summary text is parsed into "0/2", i.e.
    /// incoming is 0 and outgoing is 2.
    /// </summary>
    [Test]
    public void OutgoingOnlySummaryResultsInOutgoingChangesOnly() {
      const int expectedIncoming = 0;
      const int expectedOutgoing = 2;
      int actualIncoming;
      int actualdOutgoing;

      Mercurial hg = new Mercurial(new Mock<ProcessExecutor>().Object);
      hg.ParseSummaryRemoteText(HG_SUMMARY_OUTGOING,
          out actualIncoming, out actualdOutgoing);

      Assert.AreEqual(expectedIncoming, actualIncoming);
      Assert.AreEqual(expectedOutgoing, actualdOutgoing);
    }

    /// <summary>
    /// Confirm the all-dirty summary text is parsed into "1/2", i.e.
    /// incoming is 1 and outgoing is 2.
    /// </summary>
    [Test]
    public void AllDirtySummaryResultsInCorrectChanges() {
      const int expectedIncoming = 1;
      const int expectedOutgoing = 2;
      int actualIncoming;
      int actualdOutgoing;

      Mercurial hg = new Mercurial(new Mock<ProcessExecutor>().Object);
      hg.ParseSummaryRemoteText(HG_SUMMARY_ALLDIRTY,
          out actualIncoming, out actualdOutgoing);

      Assert.AreEqual(expectedIncoming, actualIncoming);
      Assert.AreEqual(expectedOutgoing, actualdOutgoing);
    }

    /// <summary>
    /// Confirm the parsing of invalid summary text fails with an exception.
    /// </summary>
    [Test]
    public void ParsingInvalidSummaryThrowsException() {
      int i, o;
      Mercurial hg = new Mercurial(new Mock<ProcessExecutor>().Object);

      Assert.Throws<ArgumentException>(() =>
          hg.ParseSummaryRemoteText("", out i, out o));
      Assert.Throws<ArgumentException>(() =>
          hg.ParseSummaryRemoteText("remote: ", out i, out o));
      Assert.Throws<ArgumentException>(() =>
          hg.ParseSummaryRemoteText("remote: qwerty", out i, out o));
    }
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

    /// <summary>
    /// Configures the specified mock to respond to the "hg summary --remote"
    /// command with the specified response.
    /// </summary>
    Mock<ProcessExecutor> ConfigureMock_CMD_SUMMARY_REMOTE(Mock<ProcessExecutor> mock, String response) {
      ProcessExecutor.Result summaryResult = new ProcessExecutor.Result(null,
          0, response, String.Empty);
      mock.Setup(pe => pe.Execute(
          Mercurial.HG_EXE,
          Mercurial.CMD_SUMMARY_REMOTE,
          It.IsAny<String>(),
          It.IsAny<StringDictionary>(),
          It.IsAny<TimeSpan>())
      ).Returns(summaryResult);

      return mock;
    }
    #endregion
  }
}
