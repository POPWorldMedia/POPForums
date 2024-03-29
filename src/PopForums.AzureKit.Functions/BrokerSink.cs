﻿using PopForums.Messaging;
using PopForums.Models;

namespace PopForums.AzureKit.Functions;

public class BrokerSink : IBroker
{
	public void NotifyNewPosts(Topic topic, int lasPostID)
	{
		throw new System.NotImplementedException();
	}

	public void NotifyForumUpdate(Forum forum)
	{
		throw new System.NotImplementedException();
	}

	public void NotifyTopicUpdate(Topic topic, Forum forum, string topicLink)
	{
		throw new System.NotImplementedException();
	}

	public void NotifyNewPost(Topic topic, int postID)
	{
		throw new System.NotImplementedException();
	}

	public void NotifyPMCount(int userID, int pmCount)
	{
		throw new System.NotImplementedException();
	}

	public void NotifyUser(Notification notification)
	{
		throw new System.NotImplementedException();
	}

	public void NotifyUser(Notification notification, string tenantID)
	{
		throw new System.NotImplementedException();
	}

	public void SendPMMessage(PrivateMessagePost post)
	{
		throw new System.NotImplementedException();
	}
}
