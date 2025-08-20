﻿namespace PopForums.Models;

public class TopicContainer
{
	public Forum Forum { get; set; }
	public Topic Topic { get; set; }
	public List<Post> Posts { get; set; }
	public PagerContext PagerContext { get; set; }
	public ForumPermissionContext PermissionContext { get; set; }
	public Dictionary<int, string> Signatures { get; set; }
	public Dictionary<int, int> Avatars { get; set; }
	public List<int> VotedPostIDs { get; set; }
	public DateTime? LastReadTime { get; set; }
	public TopicState TopicState { get; set; }
	public List<int> IgnoreUserIDs { get; set; }
}