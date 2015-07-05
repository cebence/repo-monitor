# RepoMonitor.Core

Repository Monitor's Core class library consists of public APIs and the
implementation for monitoring Mercurial repositories.

### How to build the project

**Required software**
- Visual Studio (2013) or
- MSBuild and .NET Framework 4.5 SDK
- NuGet

You can build the project from *Visual Studio* by opening the `repoMonitor-core.sln` and issuing *Build* > *Build solution (F6)*.

If you don't have *Visual Studio* installed, or simply like working from the command-line, you can run `build.bat release` to build the release version of the project.

### How to run unit tests

Depending on your *Visual Studio* setup you could run unit tests directly from IDE.

But it's easier to just use `test.bat release` from the command-line - it will first build the project and the tests, and if successful run the tests.
Afterwards you can run `code-coverage.bat release` to get project's code coverage report (`build\bin\Release\coverage-report\index.html`).
