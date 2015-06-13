using NUnit.Framework;
using RepoMonitor.Core;
using System;
using System.Collections.Generic;

namespace RepoMonitor.Core.UnitTests
{
	/// <summary>
	/// Class under test: <see cref="Repository"/>
	/// </summary>
	[TestFixture]
	public class RepositoryTest
	{
		#region Test data
		private const String REPO_URL = "local/filesystem/path";
		private const String REPO_NAME = "path";
		#endregion

		#region Property names
		private const String PROP_INCOMING = "Incoming";
		private const String PROP_NAME = "Name";
		private const String PROP_OUTGOING = "Outgoing";
		private const String PROP_STATUS = "Status";
		private const String PROP_URL = "Url";
		#endregion

		/// <summary>
		/// Confirm the newly created <see cref="Repository"/> is
		/// correctly initialized:
		/// - it has the specified URL,
		/// - name is exctracted from the URL,
		/// - no incoming or outgoing changes,
		/// - status is "synced",
		/// - last updated is never, i.e. DateTime.MinValue.
		/// </summary>
		[Test]
		public void NewRepositoryIsInitialized()
		{
			Repository repo = new Repository(REPO_URL);

			Assert.AreEqual(REPO_URL, repo.Url);
			Assert.AreEqual(REPO_NAME, repo.Name);
			Assert.AreEqual(0, repo.Incoming);
			Assert.AreEqual(0, repo.Outgoing);
			Assert.AreEqual(RepoStatus.Synced, repo.Status);
			Assert.AreEqual(DateTime.MinValue, repo.LastUpdate);
		}

		/// <summary>
		/// Confirm the `PropertyChanged` event is generated **twice** when
		/// repository URL is modified - once for `Url`, once for `Name`.
		/// Also confirm `Name` is extracted from the URL.
		/// </summary>
		[Test]
		public void ChangingUrlPropertyChangesNameAndGeneratesTwoOnChangeEvents()
		{
			const String newUrl = "some/other/repo";
			List<String> events = new List<string>(2);

			Repository repo = new Repository(REPO_URL);
			repo.PropertyChanged += (obj, args) => { events.Add(args.PropertyName); };

			repo.Url = newUrl;

			Assert.AreEqual(2, events.Count);
			Assert.IsTrue(events.Contains(PROP_URL));
			Assert.IsTrue(events.Contains(PROP_NAME));
			Assert.AreEqual(newUrl, repo.Url);
			Assert.AreEqual("repo", repo.Name);
		}

		/// <summary>
		/// Confirm the `PropertyChanged` event is generated **twice** when
		/// `Incoming` property is modified - once for `Incoming`,
		/// once for `Status`.
		/// Also confirm `Status` is set to `Incoming`.
		/// </summary>
		[Test]
		public void ChangingIncomingPropertyChangesStatusAndGeneratesTwoOnChangeEvents()
		{
			const int newValue = 1;
			List<String> events = new List<string>(2);

			Repository repo = new Repository(REPO_URL);
			repo.PropertyChanged += (obj, args) => { events.Add(args.PropertyName); };

			repo.Incoming = newValue;

			Assert.AreEqual(2, events.Count);
			Assert.IsTrue(events.Contains(PROP_INCOMING));
			Assert.IsTrue(events.Contains(PROP_STATUS));
			Assert.AreEqual(newValue, repo.Incoming);
			Assert.AreEqual(RepoStatus.Incoming, repo.Status);
		}

		/// <summary>
		/// Confirm the `PropertyChanged` event is generated **twice** when
		/// `Outgoing` property is modified - once for `Outgoing`,
		/// once for `Status`.
		/// Also confirm `Status` is set to `Outgoing`.
		/// </summary>
		[Test]
		public void ChangingOutgoingPropertyChangesStatusAndGeneratesTwoOnChangeEvents()
		{
			const int newValue = 1;
			List<String> events = new List<string>(2);

			Repository repo = new Repository(REPO_URL);
			repo.PropertyChanged += (obj, args) => { events.Add(args.PropertyName); };

			repo.Outgoing = newValue;

			Assert.AreEqual(2, events.Count);
			Assert.IsTrue(events.Contains(PROP_OUTGOING));
			Assert.IsTrue(events.Contains(PROP_STATUS));
			Assert.AreEqual(newValue, repo.Outgoing);
			Assert.AreEqual(RepoStatus.Outgoing, repo.Status);
		}

		/// <summary>
		/// Confirm `Status` is set to `AllDirty` when both
		/// `Incoming` and `Outgoing` properties are modified.
		/// </summary>
		[Test]
		public void ChangingIncomingAndOutgoingMakesStatusAllDirty()
		{
			const int newValue = 1;

			Repository repo = new Repository(REPO_URL);

			repo.Incoming = newValue;
			repo.Outgoing = newValue;

			Assert.AreEqual(newValue, repo.Incoming);
			Assert.AreEqual(newValue, repo.Outgoing);
			Assert.AreEqual(RepoStatus.AllDirty, repo.Status);
		}

		/// <summary>
		/// Confirm the `PropertyChanged` event is generated when
		/// `LastUpdate` property is modified.
		/// </summary>
		[Test]
		public void ChangingLastUpdatePropertyGeneratesOnChangeEvent()
		{
			DateTime newValue = DateTime.Now;
			Boolean listChanged = false;

			Repository repo = new Repository(REPO_URL);
			repo.PropertyChanged += (obj, args) => { listChanged = true; };

			repo.LastUpdate = newValue;

			Assert.IsTrue(listChanged);
			Assert.AreEqual(newValue, repo.LastUpdate);
		}
	}
}
