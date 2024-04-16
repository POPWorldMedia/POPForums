namespace PopForums.Services.Interfaces;

public interface IForumService
{
    Task<Forum> Get(int forumID);

    Task<Forum> Get(string urlName);

    Task<Forum> Create(int categoryID, string title, string description, bool isVisible, bool isArchived, int sortOrder, string forumAdapterName, bool isQAForum);
   
	Task UpdateLast(Forum forum);
   
	Task UpdateLast(Forum forum, DateTime lastTime, string lastName);
    
	void UpdateCounts(Forum forum);
    
	Task<CategorizedForumContainer> GetCategorizedForumContainer();
   
	Task<List<CategoryContainerWithForums>> GetCategoryContainersWithForums();
    
	Task<CategorizedForumContainer> GetCategorizedForumContainerFilteredForUser(User user);

    Task<List<int>> GetNonViewableForumIDs(User user);

    Task Update(Forum forum, int categoryID, string title, string description, bool isVisible, bool isArchived, string forumAdapterName, bool isQAForum);
   
	Task MoveForumUp(int forumID);

    Task MoveForumDown(int forumID);

    Task<List<string>> GetForumPostRoles(Forum forum);

    Task<List<string>> GetForumViewRoles(Forum forum);

    Dictionary<int, string> GetAllForumTitles();

    Task<Tuple<List<Topic>, PagerContext>> GetRecentTopics(User user, bool includeDeleted, int pageIndex);

    Task<int> GetAggregateTopicCount();

    Task<int> GetAggregatePostCount();

    Task<List<int>> GetViewableForumIDsFromViewRestrictedForums(User user);

    TopicContainerForQA MapTopicContainerForQA(TopicContainer topicContainer);

    Task ModifyForumRoles(ModifyForumRolesContainer container);
}
