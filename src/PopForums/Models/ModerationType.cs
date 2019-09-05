namespace PopForums.Models
{
	public enum ModerationType
	{
		NotSet = 0,
		PostEdit = 1,
		PostDelete = 2,
		PostDeletePermanently = 3,
		TopicEdit = 4,
		TopicDelete = 5,
		TopicDeletePermanently = 6,
		TopicClose = 7,
		TopicOpen = 8,
		TopicPin = 9,
		TopicUnpin = 10,
		TopicMoved = 11,
		TopicUndelete = 12,
		PostUndelete = 13,
		TopicRenamed = 14,
		TopicCloseAuto = 15
	}
}