using System;
using System.ComponentModel;

namespace RepoMonitor.Core {
  /// <summary>
  /// Repository type so the correct SCM can be used.
  /// </summary>
  public enum RepositoryType {
    /// <summary>
    /// It is a Git repository.
    /// </summary>
    Git,

    /// <summary>
    /// It is a Mercurial (Hg) repository.
    /// </summary>
    Mercurial
  }

  /// <summary>
  /// Local repository status as compared to its default remote repository.
  /// </summary>
  public enum RepoStatus {
    /// <summary>
    /// Local repository is in sync with the remote.
    /// </summary>
    Synced,

    /// <summary>
    /// Local repository has incoming changes, i.e. it's behind the remote.
    /// </summary>
    Incoming,

    /// <summary>
    /// Local repository has outgoing changes, i.e. it's ahead the remote.
    /// </summary>
    Outgoing,

    /// <summary>
    /// Local repository has both incoming and outgoing changes.
    /// </summary>
    AllDirty
  }

  /// <summary>
  /// Represents a local Mercurial (hg) repository that can be queried for
  /// synchronization status against its default remote repository.
  /// </summary>
  public class Repository : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(String propertyName) {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    #region Bindable property "Url"
    private String _url;

    /// <summary>
    /// Gets or sets repository's URL (see <c>hg help urls</c>).
    /// </summary>
    public String Url {
      get { return _url; }
      set {
        _url = value;
        OnPropertyChanged("Url");
        OnPropertyChanged("Name");
      }
    }
    #endregion

    #region Bindable read-only property "Name"
    /// <summary>
    /// Gets repository's name.
    /// </summary>
    /// <remarks>
    /// Repository name is extracted from its <see cref="Url">URL</see>.
    /// </remarks>
    public String Name {
      get {
        String name = (Url != null && Url.Trim().Length > 0)
            ? Url.Substring(Url.LastIndexOf('/') + 1)
            : null;
        return name;
      }
    }
    #endregion

    #region Bindable property "Incoming"
    private int _incoming;

    /// <summary>
    /// Gets or sets repository's number of incoming changes.
    /// </summary>
    public int Incoming {
      get { return _incoming; }
      set {
        _incoming = value;
        OnPropertyChanged("Incoming");
        OnPropertyChanged("Status");
      }
    }
    #endregion

    #region Bindable property "Outgoing"
    private int _outgoing;

    /// <summary>
    /// Gets or sets repository's number of outgoing changes.
    /// </summary>
    public int Outgoing {
      get { return _outgoing; }
      set {
        _outgoing = value;
        OnPropertyChanged("Outgoing");
        OnPropertyChanged("Status");
      }
    }
    #endregion

    #region Bindable property "LastUpdate"
    private DateTime _lastUpdate;

    /// <summary>
    /// Gets or sets repository's last update timestamp.
    /// </summary>
    /// <remarks>
    /// This is the moment the last time an <c>hg summary --remote</c>
    /// command was invoked successfully.
    /// </remarks>
    public DateTime LastUpdate {
      get { return _lastUpdate; }
      set {
        _lastUpdate = value;
        OnPropertyChanged("LastUpdate");
      }
    }
    #endregion

    #region Bindable read-only property "Status"
    /// <summary>
    /// Gets repository's synchronization status.
    /// </summary>
    /// <see cref="RepoStatus"/>
    public RepoStatus Status {
      get {
        if (_incoming == 0 && _outgoing == 0) {
          return RepoStatus.Synced;
        }

        if (_incoming > 0 && _outgoing == 0) {
          return RepoStatus.Incoming;
        }

        if (_incoming == 0 && _outgoing > 0) {
          return RepoStatus.Outgoing;
        }

        return RepoStatus.AllDirty;
      }
    }
    #endregion

    /// <summary>
    /// Gets repository's type.
    /// </summary>
    public RepositoryType RepoType { get; private set; }

    /// <summary>
    /// Creates a new Mercurial repository with the specified URL.
    /// </summary>
    public Repository(String url) : this(url, RepositoryType.Mercurial) {
    }

    /// <summary>
    /// Creates a new repository with the specified URL and type.
    /// </summary>
    public Repository(String url, RepositoryType repoType) {
      _url = url;
      _incoming = 0;
      _outgoing = 0;
      _lastUpdate = DateTime.MinValue;
       RepoType = repoType;
    }
  }
}
