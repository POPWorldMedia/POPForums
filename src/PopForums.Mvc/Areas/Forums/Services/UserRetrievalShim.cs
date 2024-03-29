﻿namespace PopForums.Mvc.Areas.Forums.Services;

public class UserRetrievalShim : IUserRetrievalShim
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public UserRetrievalShim(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public User GetUser()
	{
		var user = _httpContextAccessor.HttpContext?.Items["PopForumsUser"] as User;
		return user;
	}

	public Profile GetProfile()
	{
		var profile = _httpContextAccessor.HttpContext?.Items["PopForumsProfile"] as Profile;
		return profile;
	}
}