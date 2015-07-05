using Moq;
using System;
using System.Collections.Generic;
using System.IO;

namespace RepoMonitor.Core.UnitTests {
  /// <summary>
  /// Common methods to access resources under the "test-resources" folder, etc.
  /// NOTE: "test-resources" folder should be copied to "OutputPath"!
  /// </summary>
  internal class TestUtil {
    #region "test-resources" folder
    /// <summary>
    /// Returns the full path to the "test-resources" folder by searching
    /// upward from the given directory or <see langword="null"/>
    /// if it couldn't be found.
    /// </summary>
    /// <param name="startingPath">
    /// Directory to start from (included in the search).
    /// If <see langword="null"/> current directory is used.
    /// </param>
    /// <returns>
    /// Path to the "test-resources" folder, if found, or <see langword="null"/>.
    /// </returns>
    public static String FindTestResourcesFolder(String startingPath = null) {
      if (startingPath == null) {
          startingPath = Directory.GetCurrentDirectory();
      }
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

    private class TestResPathSingleton {
      public static readonly String TestResPath = FindTestResourcesFolder();
      static TestResPathSingleton() {}
    }

    /// <summary>
    /// Full path to the "test-resources" folder (searched upward from the
    /// current working directory, cached), or <see langword="null"/>
    /// if it couldn't be found.
    /// </summary>
    public static String TestResourcesPath {
      get {
        return TestResPathSingleton.TestResPath;
      }
    }
    #endregion

    #region "packages" folder
    /// <summary>
    /// Returns the full path to solution's "packages" folder (where NuGet
    /// dependencies are kept) by searching upward from the given directory or
    /// <see langword="null"/> if it couldn't be found.
    /// </summary>
    /// <param name="startingPath">
    /// Directory to start from (included in the search).
    /// If <see langword="null"/> current directory is used.
    /// </param>
    /// <returns>
    /// Full path to the "packages" folder, if found, or <see langword="null"/>
    /// </returns>
    public static String FindPackagesFolder(String startingPath = null) {
      if (startingPath == null) {
          startingPath = Directory.GetCurrentDirectory();
      }
      if (Directory.Exists(startingPath)) {
        DirectoryInfo current = new DirectoryInfo(startingPath);
        while (current != null) {
          String path = Path.Combine(current.FullName, "packages");
          if (Directory.Exists(path)) {
            return path;
          }
          current = current.Parent;
        }
      }
      return null;
    }

    private class PackagesPathSingleton {
      public static readonly String PackagesPath = FindPackagesFolder();
      static PackagesPathSingleton() {}
    }

    /// <summary>
    /// Full path to the NuGet "packages" folder (searched upward from the
    /// current working directory, cached), or <see langword="null"/>
    /// if it couldn't be found.
    /// </summary>
    public static String PackagesPath {
      get {
        return PackagesPathSingleton.PackagesPath;
      }
    }
    #endregion

    #region "echoer.exe" tool
    /// <summary>
    /// Returns the full path to the "echoer.exe" file (inside solution's
    /// NuGet packages) by searching upward from the given directory or
    /// <see langword="null"/> if it couldn't be found.
    /// </summary>
    /// <param name="startingPath">
    /// Directory to start from (included in the search).
    /// If <see langword="null"/> current directory is used.
    /// </param>
    /// <returns>
    /// Full path to the "echoer.exe", if found, or <see langword="null"/>
    /// </returns>
    public static String FindEchoerTool(String startingPath = null) {
      String packages = startingPath == null
          ? PackagesPath
          : FindPackagesFolder(startingPath);
      if (packages != null) {
        String[] dirs = Directory.GetDirectories(packages, "echoer.*");
        if (dirs.Length > 0) {
          String echoer = Path.Combine(dirs[0], "lib", "net45", "echoer.exe");
          if (File.Exists(echoer)) {
            return echoer;
          }
        }
      }
      return null;
    }

    private class EchoerPathSingleton {
      public static readonly String EchoerPath = FindEchoerTool();
      static EchoerPathSingleton() {}
    }

    /// <summary>
    /// Full path to the "echoer" tool (searched in the NuGet packages folder,
    /// cached), or <see langword="null"/> if it couldn't be found.
    /// </summary>
    public static String EchoerPath {
      get {
        return EchoerPathSingleton.EchoerPath;
      }
    }
    #endregion

    #region SCM mocks
    public const String HG_REPO_1 = "test-repo";
    public const String HG_REPO_2 = "test-repo-clone";
    public const String GIT_REPO_1 = "git-repo";
    public const String GIT_REPO_2 = "git-repo-clone";

    /// <summary>
    /// Creates a Mercurial (Hg) mock configured to recognize test repositories.
    /// </summary>
    public static Mock<SCM> CreateHgMockOnTestRepos() {
      Mock<SCM> mock = new Mock<SCM>();
      mock.Setup(scm => scm.IsRepository(It.Is<String>(
          s => s.EndsWith(HG_REPO_1) || s.EndsWith(HG_REPO_2)))).Returns(true);
      return mock;
    }

    /// <summary>
    /// Creates a Git mock configured to recognize test repositories.
    /// </summary>
    public static Mock<SCM> CreateGitMockOnTestRepos() {
      Mock<SCM> mock = new Mock<SCM>();
      mock.Setup(scm => scm.IsRepository(It.Is<String>(
          s => s.EndsWith(GIT_REPO_1) || s.EndsWith(GIT_REPO_2)))).Returns(true);
      return mock;
    }

    /// <summary>
    /// Creates a read-only collection with Hg and Git mocked SCMs (not mocks!).
    /// </summary>
    public static ICollection<SCM> CreateArrayOfMockedSCMsOnTestRepos() {
      SCM[] scms = new SCM[2];
      scms[0] = CreateHgMockOnTestRepos().Object;
      scms[1] = CreateGitMockOnTestRepos().Object;

      return Array.AsReadOnly(scms);
    }

    /// <summary>
    /// Creates a read-only collection of SCMs with specified mocks.
    /// </summary>
    public static ICollection<SCM> CreateArrayOfMockedSCMs(Mock<SCM>[] mocks) {
      SCM[] scms = new SCM[mocks.Length];
      for (int i = 0; i < mocks.Length; i++) {
        scms[i] = mocks[i].Object;
      }
      return Array.AsReadOnly(scms);
    }
    #endregion
  }
}
