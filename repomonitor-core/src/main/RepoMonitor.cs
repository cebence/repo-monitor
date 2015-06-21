using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RepoMonitor.Core {
  /// <summary>
  /// Repository monitor manages a list of repositories it monitors.
  /// </summary>
  public class RepoMonitor {
    #region Bindable property "Repositories"
    private ObservableCollection<Repository> _monitoredRepos = new ObservableCollection<Repository>();

    /// <summary>
    /// List of monitored repositories.
    /// </summary>
    public IList<Repository> Repositories {
      get {
        return _monitoredRepos;
      }
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of <see cref="RepoMonitor"/>.
    /// </summary>
    public RepoMonitor() {
    }
  }
}
