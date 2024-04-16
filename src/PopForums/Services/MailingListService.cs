﻿using PopForums.Services.Interfaces;

namespace PopForums.Services;

public class MailingListService : IMailingListService
{
	private readonly IUserService _userService;
	private readonly IMailingListComposer _mailingListComposer;
	private readonly IErrorLog _errorLog;
	private static Thread _mailWorker;

	public MailingListService(IUserService userService, IMailingListComposer mailingListComposer, IErrorLog errorLog)
	{
		_userService = userService;
		_mailingListComposer = mailingListComposer;
		_errorLog = errorLog;
	}

	public void MailUsers(string subject, string body, string htmlBody, Func<User, string> unsubscribeLinkGenerator)
	{
		_mailWorker = new Thread(() =>
		{
			var users = _userService.GetSubscribedUsers().Result;

			foreach (var user in users)
			{
				var unsubLink = unsubscribeLinkGenerator(user);

				try
				{
					_mailingListComposer.ComposeAndQueue(user, subject, body, htmlBody, unsubLink);
				}
				catch (Exception exc)
				{
					_errorLog.Log(exc, ErrorSeverity.Error, "UserID: " + user.UserID);
				}
			}
		});

		_mailWorker.Start();
	}
}