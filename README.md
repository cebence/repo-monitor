# Repository Monitor a.k.a. RepoMonitor

[![Build status](https://ci.appveyor.com/api/projects/status/9davjk5pwvcluoxm?svg=true)](https://ci.appveyor.com/project/cebence/repo-monitor)

Repository Monitor is a GUI application for monitoring the status of one or more local Mercurial repositories against its default remote repository, and notify the user of any changes.

In case of incoming changes the user can update the local repository by *pull*-ing from the remote repository, and in case of outgoing changes the user will be reminded to *push* the changes he/she made to the remote repository.

Initial implementation consists of these components:
- repomonitor-core - APIs and Mercurial implementation logic,
- repomonitor-ui - a GUI for the Core component.


## User Stories
- [ ] A user can see the list of monitored repositories.
- [ ] A user can add a repository to the list via the "Browse for Folder" dialog.
- [ ] A user can add one or more repositories to the list via the "Browse for Folder" dialog by specifying a (root) folder that will be scanned recursively for repositories.
- [ ] A user can add one or more repositories to the list via drag-and-drop (one or more drag targets). If a dropped folder is not a repository auto-scanning is offered to the user.
- [ ] A user can remove a repository from the list.
- [ ] A user can easily identify a modified repository in the list (row coloring, icons, etc.).
- [ ] A user can manually initiate polling for all monitored repositories.
- [ ] A user can configure periodic automatic polling with a customizable polling interval.
- [ ] A user will be notified (warning) if Mercurial is unavailable (not found/installed on the system using default PATH). Most of the functionality is disabled.
- [ ] A user can be notified via the System Tray icon.
- [ ] A user can group repositories in a two-level tree (e.g. by its root folder).
- [ ] A user can invoke *push* action on a repository with outgoing changes (within the application).
- [ ] A user can invoke *pull & update* action on a repository with incoming changes (within the application).
- [ ] A user can open the repository in *Hg Workbench* (within the application) - useful in cases of merge conflicts.
- [ ] A user can easily identify a repository with uncommitted changes (it could cause conflicts with automatic *pull & update*).
- [ ] A user can install the tool as a [Chocolatey](https://chocolatey.org/) package.

## Technical Stories
- [x] Implement this project hierarchy:
```
%PROJECT%
  + %PROJECT%.sln
  + src
    + main
      + %PROJECT%.csproj
    + test
      + %PROJECT%-tests.csproj
  + build
    - build.log
    - test-results.log
    + bin
      + $(Configuration)
        + $(Platform)
    + obj
      + $(Configuration)
        + $(Platform)
```
- [ ] Update `.gitignore` according to project hierarchy.
- [ ] Redirect MSBuild log to `build` folder, i.e. `%PROJECT%\build\build.log`.
- [ ] Configure MSBuild to run unit tests.
- [ ] Redirect NUnit results (`TestResult.xml`) to `build` folder, i.e. `%PROJECT%\build\test-results.xml`.
- [x] A repository is a folder containing the `.hg` sub-folder. http://mercurial.selenic.com/wiki/Repository
- [ ] Application will detect existence of Mercurial by invoking `hg --version` and parsing the output.
- [ ] Checking a repository for changes is done by invoking `hg summary --remote` in the repository folder and parsing the output.
- [ ] **Unit tests:** test the parsing logic using example `hg` output.
- [ ] **Integration tests:** use installed `hg.exe` against `test-resources`.
- [ ] **Integration tests:** embed Mercurial installation in a project sub-folder like `%UserProfile%\AppData\Local\Atlassian\SourceTree\hg_local`?
- [x] Configure Continuous Integration (CI).
- [ ] To open a repository in *Hg Workbench* issue the following command `thgw.exe -R "<full\path\to\repo>"` (or `thg.exe workbench -R "<full\path\to\repo>"`). `--newworkbench` parameter is unknown to `thg.exe`, [maybe it works](https://bitbucket.org/tortoisehg/thg/issue/4094/thgwexe-r-e-src-dev-newworkbench-does-not) in a newer version of Mercurial.
