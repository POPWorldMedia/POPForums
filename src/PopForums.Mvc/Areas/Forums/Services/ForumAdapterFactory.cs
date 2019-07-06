using System;
using PopForums.Models;

namespace PopForums.Mvc.Areas.Forums.Services
{
	public class ForumAdapterFactory
	{
		public ForumAdapterFactory(Forum forum)
		{
			if (!string.IsNullOrWhiteSpace(forum.ForumAdapterName))
			{
				var type = Type.GetType(forum.ForumAdapterName);
				if (type == null)
					throw new Exception($"Can't find ForumAdapter \"{forum.ForumAdapterName}\" (Forum ID: {forum.ForumID}, Title: {forum.Title})");
				var instance = Activator.CreateInstance(type);
				if (!typeof(IForumAdapter).IsAssignableFrom(instance.GetType()))
					throw new Exception($"ForumAdapter \"{forum.ForumAdapterName}\" does not implement IForumAdapter (Forum ID: {forum.ForumID}, Title: {forum.Title})");
				ForumAdapter = (IForumAdapter)instance;
			}
		}

		public bool IsAdapterEnabled => ForumAdapter != null;
		public IForumAdapter ForumAdapter { get; }
	}
}