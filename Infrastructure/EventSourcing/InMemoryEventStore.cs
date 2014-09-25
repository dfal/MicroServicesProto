using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Infrastructure.Messaging;

namespace Infrastructure.EventSourcing
{
	public class InMemoryEventStore: IEventStore
	{
		readonly Action<IEvent> publishAction;

		readonly IDictionary<Guid, Guid> headsStore = new ConcurrentDictionary<Guid, Guid>();
		readonly IDictionary<Guid, Commit> commitsStore = new ConcurrentDictionary<Guid, Commit>();

		public InMemoryEventStore(Action<IEvent> publishAction)
		{
			this.publishAction = publishAction;
		}

		public IEnumerable<Commit> Load(Guid sourceId, int minVersion)
		{
			Debug.Assert(sourceId != Guid.Empty);

			var commitId = GetHead(sourceId);
			var commits = new Stack<Commit>();

			while (commitId.HasValue)
			{
				var commit = GetCommit(commitId.Value);

				commits.Push(commit);

				if (commit.Changes.First().SourceVersion <= minVersion)
				{
					commit.Changes = commit.Changes.Where(x => x.SourceVersion >= minVersion).ToArray();
					break;
				}

				commitId = commit.ParentId;
			}

			return commits.ToArray();
		}

		public void Save(Commit commit)
		{
			Debug.Assert(commit != null);
			Debug.Assert(commit.SourceId != Guid.Empty);
			Debug.Assert(commit.Id != Guid.Empty);
			Debug.Assert(commit.Changes != null);
			Debug.Assert(commit.Changes.Length > 0);

			commitsStore[commit.Id] = commit;
			headsStore[commit.SourceId] = commit.Id;
			
			foreach (var @event in commit.Changes)
			{
				publishAction(@event);
			}
		}

		Guid? GetHead(Guid sourceId)
		{
			return headsStore.ContainsKey(sourceId) ? headsStore[sourceId] : (Guid?)null;
		}

		Commit GetCommit(Guid commitId)
		{
			return commitsStore[commitId];
		}
	}
}
