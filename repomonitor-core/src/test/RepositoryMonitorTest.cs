using NUnit.Framework;
using RepoMonitor.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace RepoMonitor.Core.UnitTests {
  /// <summary>
  /// Class under test: <see cref="RepositoryMonitor"/>
  /// </summary>
  [TestFixture]
  public class RepositoryMonitorTest {
    #region Test data
    private const String REPO_URL = "local/filesystem/path";
    #endregion

    #region Property names
    //private const String PROP_URL = "Url";
    #endregion

    /// <summary>
    /// Confirm the newly created <see cref="RepositoryMonitor"/> is
    /// correctly initialized:
    /// - it has a list of monitored repositories,
    /// - the list is empty.
    /// </summary>
    [Test]
    public void NewRepositoryMonitorIsEmpty() {
      ICollection<SCM> scms = TestUtil.CreateArrayOfMockedSCMsOnTestRepos();
      RepositoryMonitor repoMonitor = new RepositoryMonitor(scms);

      Assert.IsNotNull(repoMonitor.Repositories);
      Assert.IsEmpty(repoMonitor.Repositories);
    }

    /// <summary>
    /// Confirm the `PropertyChanged` event is generated when a repository
    /// is added to the list.
    /// Also confirm the repository is the only one, and the correct one.
    /// </summary>
    [Test]
    public void AddingRepoGeneratesOnChangeEvent() {
      ICollection<SCM> scms = TestUtil.CreateArrayOfMockedSCMsOnTestRepos();
      RepositoryMonitor repoMonitor = new RepositoryMonitor(scms);

      Boolean listChanged = false;
      INotifyCollectionChanged list = repoMonitor.Repositories as INotifyCollectionChanged;
      list.CollectionChanged += (obj, args) => { listChanged = true; };

      repoMonitor.Repositories.Add(new Repository(REPO_URL));

      Assert.IsNotNull(repoMonitor.Repositories);
      Assert.AreEqual(1, repoMonitor.Repositories.Count);
      Assert.AreEqual(REPO_URL, repoMonitor.Repositories[0].Url);
      Assert.IsTrue(listChanged);
    }

    /// <summary>
    /// Confirm the `PropertyChanged` event is generated when a repository
    /// is removed from the list.
    /// Also confirm the list is empty after removal.
    /// </summary>
    [Test]
    public void RemovingRepoGeneratesOnChangeEvent() {
      ICollection<SCM> scms = TestUtil.CreateArrayOfMockedSCMsOnTestRepos();
      RepositoryMonitor repoMonitor = new RepositoryMonitor(scms);
      repoMonitor.Repositories.Add(new Repository(REPO_URL));

      Boolean listChanged = false;
      INotifyCollectionChanged list = repoMonitor.Repositories as INotifyCollectionChanged;
      list.CollectionChanged += (obj, args) => { listChanged = true; };

      repoMonitor.Repositories.Remove(repoMonitor.Repositories[0]);

      Assert.IsEmpty(repoMonitor.Repositories);
      Assert.IsTrue(listChanged);
    }
  }
}
