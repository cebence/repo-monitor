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
    #region Instantiation test cases
    /// <summary>
    /// Confirm the newly created <see cref="RepositoryFinder"/> is
    /// correctly initialized, i.e. SCMs property equals passed SCMs.
    /// </summary>
    [Test]
    public void NewRepositoryFinderIsInitialized() {
      ICollection<SCM> scms = TestUtil.CreateArrayOfMockedSCMsOnTestRepos();
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
      Assert.Throws<ArgumentNullException>(() => new RepositoryFinder(null));
      Assert.Throws<ArgumentException>(() => new RepositoryFinder(
          Array.AsReadOnly(new SCM[0])));
    }
    #endregion

    #region FindRepositories() test cases
    /// <summary>
    /// Confirm an exception is thrown when passing invalid values to
    /// <see cref="RepositoryFinder.FindRepositories(String)"/> or
    /// <see cref="RepositoryFinder.FindRepositories(DirectoryInfo)"/>.
    /// </summary>
    [Test]
    public void InvalidParametersToFindRepositoriesThrowExeption() {
      String testResPath = TestUtil.TestResourcesPath;
      RepositoryFinder finder = new RepositoryFinder(
          TestUtil.CreateArrayOfMockedSCMsOnTestRepos());

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
      Mock<SCM>[] mocks = new Mock<SCM>[2];
      mocks[0] = TestUtil.CreateGitMockOnTestRepos();
      Mock<SCM> hgMock = mocks[1] = TestUtil.CreateHgMockOnTestRepos();

      RepositoryFinder finder = new RepositoryFinder(
          TestUtil.CreateArrayOfMockedSCMs(mocks));

      IDictionary<String, SCM> repos = finder.FindRepositories(testResPath);

      Assert.AreEqual(2, repos.Count);
      Assert.IsTrue(repos.ContainsKey(Path.Combine(testResPath, TestUtil.HG_REPO_1)));
      Assert.IsTrue(repos.ContainsKey(Path.Combine(testResPath, TestUtil.HG_REPO_2)));
      hgMock.Verify(m => m.IsRepository(It.IsAny<String>()));

      // TODO Add checks for Git: GIT_REPO_1 & GIT_REPO_2
    }

    /// <summary>
    /// Confirm the <see cref="RepositoryFinder"/> has found the repository.
    /// </summary>
    [Test]
    public void FindRepositoriesOnRepoDirReturnsOneItem() {
      String repoPath = Path.Combine(TestUtil.TestResourcesPath, TestUtil.HG_REPO_1);
      Mock<SCM>[] mocks = new Mock<SCM>[2];
      mocks[0] = TestUtil.CreateGitMockOnTestRepos();
      Mock<SCM> hgMock = mocks[1] = TestUtil.CreateHgMockOnTestRepos();

      RepositoryFinder finder = new RepositoryFinder(
          TestUtil.CreateArrayOfMockedSCMs(mocks));

      // Cast to Dictionary in order to use ContainsValue().
      Dictionary<String, SCM> repos = finder.FindRepositories(repoPath)
          as Dictionary<String, SCM>;

      Assert.AreEqual(1, repos.Count);
      Assert.IsTrue(repos.ContainsValue(hgMock.Object));
      hgMock.Verify(m => m.IsRepository(It.IsAny<String>()));

      // TODO Add checks for Git: GIT_REPO_1
    }
    #endregion
  }
}
