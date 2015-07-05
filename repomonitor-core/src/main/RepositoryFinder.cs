using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace RepoMonitor.Core {
  /// <summary>
  /// Searches the file system for supported repositories.
  /// </summary>
  public class RepositoryFinder {
    /// <summary>
    /// Returns a collection of <see cref="SCM"/>s used.
    /// </summary>
    public ICollection<SCM> SCMs { get; private set; }

    /// <summary>
    /// Initializes a new instance of <see cref="RepositoryFinder"/>.
    /// </summary>
    /// <param name="scms">
    /// A non-empty collection of <see cref="SCM"/>s used to recognize a repository.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// When <paramref name="scms"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// When <paramref name="scms"/> is an empty array.
    /// </exception>
    public RepositoryFinder(ICollection<SCM> scms) {
      if (scms == null) {
        throw new ArgumentNullException("A collection of SCMs is null.");
      }
      if (scms.Count == 0) {
        throw new ArgumentException("A collection of SCMs is empty.");
      }
      SCMs = scms;
    }

    /// <summary>
    /// Searches the specified folder and its sub-folders for repositories.
    /// Returns a dictionary where repository path is the key, and the
    /// <see cref="SCM"/> that detected it is the value.
    /// </summary>
    /// <remarks>
    /// When a repository is detected its sub-folders are not searched - it is
    /// highly unlikely to have one repository embedded in another.
    /// </remarks>
    /// <param name="folderPath">
    /// Path to the folder in which to start the search.
    /// </param>
    /// <returns>
    /// A dictionary of (<see cref="String"/>, <see cref="SCM"/>) pairs.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="folderPath"/> path contains invalid characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="folderPath"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="PathTooLongException">
    /// If <paramref name="folderPath"/> path is longer than 260 characters.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    /// If <paramref name="folderPath"/> does not exist.
    /// </exception>
    /// <exception cref="SecurityException">
    /// The caller does not have the required permission on <paramref name="folderPath"/>.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission on <paramref name="folderPath"/>.
    /// </exception>
    public virtual IDictionary<String, SCM> FindRepositories(String folderPath) {
      return FindRepositories(new DirectoryInfo(folderPath));
    }

    /// <summary>
    /// Searches the specified folder and its sub-folders for repositories.
    /// Returns a dictionary where repository path is the key, and the
    /// <see cref="SCM"/> that detected it is the value.
    /// </summary>
    /// <remarks>
    /// When a repository is detected its sub-folders are not searched - it is
    /// highly unlikely to have one repository embedded in another.
    /// </remarks>
    /// <param name="folder">
    /// The folder in which to start the search.
    /// </param>
    /// <returns>
    /// A dictionary of (<see cref="String"/>, <see cref="SCM"/>) pairs.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <paramref name="folder"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    /// If <paramref name="folder"/> does not exist.
    /// </exception>
    /// <exception cref="SecurityException">
    /// The caller does not have the required permission on <paramref name="folder"/>.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// The caller does not have the required permission on <paramref name="folder"/>.
    /// </exception>
    public virtual IDictionary<String, SCM> FindRepositories(DirectoryInfo folder) {
      if (folder == null) {
        throw new ArgumentNullException("folder is null");
      }

      Dictionary<String, SCM> result = new Dictionary<String, SCM>(50);

      // Skip sub-folders if "this is the one".
      SCM scm = AcceptsRepository(folder);
      if (scm != null) {
        result.Add(folder.FullName, scm);
        return result;
      }

      // No, we need to check sub-folders.
      Stack<DirectoryInfo> queue = new Stack<DirectoryInfo>(100);
      queue.Push(folder);

      while (queue.Count > 0) {
        DirectoryInfo current = queue.Pop();

        DirectoryInfo[] subdirs = current.GetDirectories();
        foreach (DirectoryInfo dir in subdirs) {
          scm = AcceptsRepository(dir);
          if (scm != null) {
            result.Add(dir.FullName, scm);
          }
          else {
            queue.Push(dir);
          }
        }
      }

      return result;
    }

    /// <summary>
    /// Asks SCMs if the specified folder is a repository. If one of them
    /// responds positivelly it is returned, otherwise null is returned.
    /// </summary>
    private SCM AcceptsRepository(DirectoryInfo folder) {
      foreach (SCM scm in SCMs) {
        if (scm.IsRepository(folder.FullName)) {
          return scm;
        }
      }
      return null;
    }
  }
}
