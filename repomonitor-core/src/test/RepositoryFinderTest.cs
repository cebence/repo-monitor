using Moq;
using NUnit.Framework;
using RepoMonitor.Core;
using System;
using System.IO;
using System.Collections.Generic;

namespace RepoMonitor.Core.UnitTests {
  /// <summary>
  /// Class under test: <see cref="RepositoryFinder"/>
  /// </summary>
  [TestFixture]
  public class RepositoryFinderTest {
    #region Test data
    private const String HG_REPO_1 = "test-repo";
    private const String HG_REPO_2 = "test-repo-clone";
    private const String GIT_REPO_1 = "git-repo";
    private const String GIT_REPO_2 = "git-repo-clone";
    #endregion

    #region SCM mocks
    private Mock<SCM> hgMock;
    private Mock<SCM> gitMock;

    /// <summary>
    /// Configures the specified mock to respond to the "hg --version" command
    /// and returns the mock.
    /// </summary>
    [TestFixtureSetUp]
    public void MockSCMs() {
      hgMock = new Mock<SCM>();
      hgMock.Setup(scm => scm.IsRepository(It.Is<String>(
          s => s.EndsWith(HG_REPO_1) || s.EndsWith(HG_REPO_2)))).Returns(true);

      gitMock = new Mock<SCM>();
      gitMock.Setup(scm => scm.IsRepository(It.Is<String>(
          s => s.EndsWith(GIT_REPO_1) || s.EndsWith(GIT_REPO_2)))).Returns(true);
    }

    private SCM[] CreateArrayOfMockedSCMs() {
      SCM[] result = new SCM[2];
      result[0] = hgMock.Object;
      result[1] = gitMock.Object;

      return result;
    }
    #endregion

    #region Instantiation test cases

    /// <summary>
    /// Confirm the newly created <see cref="RepositoryFinder"/> is
    /// correctly initialized, i.e. SCMs property equals passed SCMs.
    /// </summary>
    [Test]
    public void NewRepositoryFinderIsInitialized() {
      SCM[] scms = CreateArrayOfMockedSCMs();
      RepositoryFinder finder = new RepositoryFinder(scms);

      Assert.IsNotNull(finder);
      Assert.AreEqual(scms, finder.SCMs);
    }

    /// <summary>
    /// Confirm an exception is thrown when trying to create
    /// <see cref="RepositoryFinder"/> without SCMs.
    /// </summary>
    [Test]
    public void InitializingRepositoryFinderWithNoSCMsThrowsExeption() {
      Assert.Throws<ArgumentException>(() => new RepositoryFinder(null));
      Assert.Throws<ArgumentException>(() => new RepositoryFinder(new SCM[0]));
    }

    #endregion

    /// <summary>
    /// Confirm an exception is thrown when passing invalid values to
    /// <see cref="RepositoryFinder.FindRepositories(String)"/> or
    /// <see cref="RepositoryFinder.FindRepositories(DirectoryInfo)"/>.
    /// </summary>
    [Test]
    public void InvalidParametersToFindRepositoriesThrowExeption() {
      String testResPath = TestUtil.TestResourcesPath;
      RepositoryFinder finder = new RepositoryFinder(CreateArrayOfMockedSCMs());

      Assert.Throws<ArgumentNullException>(() =>
          finder.FindRepositories((String) null));
      Assert.Throws<ArgumentException>(() =>
          finder.FindRepositories(""));
      Assert.Throws<DirectoryNotFoundException>(() =>
          finder.FindRepositories(Path.Combine(testResPath, "unknown")));

      Assert.Throws<ArgumentNullException>(() =>
          finder.FindRepositories((DirectoryInfo) null));
    }

    /// <summary>
    /// Confirm the <see cref="RepositoryFinder"/> has found the test repositories.
    /// </summary>
    [Test]
    public void FindRepositoriesOnTestReposReturnsCorrectList() {
      String testResPath = TestUtil.TestResourcesPath;
      RepositoryFinder finder = new RepositoryFinder(CreateArrayOfMockedSCMs());

      IDictionary<String, SCM> repos = finder.FindRepositories(testResPath);

      Assert.AreEqual(2, repos.Count);
      Assert.IsTrue(repos.ContainsKey(Path.Combine(testResPath, HG_REPO_1)));
      Assert.IsTrue(repos.ContainsKey(Path.Combine(testResPath, HG_REPO_2)));
      hgMock.Verify(m => m.IsRepository(It.IsAny<String>()));

      // TODO Add checks for Git: GIT_REPO_1 & GIT_REPO_2
    }
  }
}
