using System;
using PopForums.Models;

namespace PopForums.Services
{
	public class ForumAdapterFactory
	{
		public ForumAdapterFactory(Forum forum)
		{
			if (!String.IsNullOrWhiteSpace(forum.ForumAdapterName))
			{
				var type = Type.GetType(forum.ForumAdapterName);
				if (type == null)
					throw new Exception(String.Format("Can't find ForumAdapter \"{0}\" (Forum ID: {1}, Title: {2})", forum.ForumAdapterName, forum.ForumID, forum.Title));
				var instance = Activator.CreateInstance(type);
				if (!typeof(IForumAdapter).IsAssignableFrom(instance.GetType()))
					throw new Exception(String.Format("ForumAdapter \"{0}\" does not implement IForumAdapter (Forum ID: {1}, Title: {2})", forum.ForumAdapterName, forum.ForumID, forum.Title));
				ForumAdapter = (IForumAdapter)instance;
			}
		}

		public bool IsAdapterEnabled
		{
			get { return ForumAdapter != null; }
		}
		public IForumAdapter ForumAdapter { get; private set; }
	}
}
