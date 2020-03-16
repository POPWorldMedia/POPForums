$(function () {
	if (window.location.hash) {
		var hash = window.location.hash;
		while (hash.charAt(0) == '#') hash = hash.substr(1);
		var tag = $("div[data-postID='" + hash + "']");
		if ($("#PostStream").has(tag).length > 0) {
			var offset = tag.offset();
			if (offset) {
				var crumb = $("#ForumContainer #TopBreadcrumb");
				var height = crumb.outerHeight();
				var margin = parseInt($(".postItem").css("margin-top"), 10);
				var tagTop = offset.top;
				var newPosition = tagTop - height - margin;
				$("html,body").animate({ scrollTop: newPosition }, "fast");
			}
		}
	}
});

var PopForums = {};

PopForums.areaPath = "/Forums";
PopForums.currentTopicState = null;
PopForums.editorCSS = "/lib/bootstrap/dist/css/bootstrap.min.css,/lib/PopForums/dist/Editor.min.css";
PopForums.postNoImageToolbar = "cut copy paste | bold italic | bullist numlist blockquote removeformat | link";

PopForums.editorSettings = {
	theme: "silver",
	plugins: "paste lists image link",
	content_css: PopForums.editorCSS,
	menubar: false,
	toolbar: "cut copy paste | bold italic | bullist numlist blockquote removeformat | link | image",
	statusbar: false,
	target_list: false,
	link_title: false,
	image_description: false,
	image_dimensions: false,
	browser_spellcheck : true,
	object_resizing: false,
	relative_urls: false,
	remove_script_host: false,
	contextmenu: "",
	mobile: {
		theme: "silver"
	},
	paste_as_text: true
};

PopForums.processLogin = function () {
	PopForums.processLoginBase("/Identity/Login");
};

PopForums.processLoginExternal = function () {
	PopForums.processLoginBase("/Identity/LoginAndAssociate");
};

PopForums.processLoginBase = function (path) {
	$.ajax({
		url: PopForums.areaPath + path,
		type: "POST",
		data: { email: $("#EmailLogin").val(), password: $("#PasswordLogin").val(), persistCookie: $("#PersistCookie").is(":checked") },
		dataType: "json",
		success: function (result) {
			var loginResult = $("#LoginResult");
			switch (result.result) {
				case true:
					var destination = $("#Referrer").val();
					location = destination;
					break;
				default:
					loginResult.html(result.message);
					loginResult.removeClass("d-none");
			}
		},
		error: function () {
			var loginResult = $("#LoginResult");
			loginResult.html("There was an unknown error while attempting login");
			loginResult.removeClass("d-none");
		}
	});
};

PopForums.topicListSetup = function (forumID) {
	PopForums.startTimeUpdater();
	var b = $("#NewTopicButton");
	b.click(function () {
		var n = $("#NewTopic");
		n.load(PopForums.areaPath + "/Forum/PostTopic/" + forumID, function () {
			var allowImage = ($("#IsImageEnabled").val().toLowerCase() == "true");
			if (!allowImage) {
				PopForums.editorSettings.toolbar = PopForums.postNoImageToolbar;
			}
			var usePlainText = ($("#IsPlainText").val().toLowerCase() == "true");
			if (!usePlainText) {
				PopForums.editorSettings.selector = "#NewTopic #FullText";
				tinyMCE.init(PopForums.editorSettings);
			}
			n.slideDown();
			b.hide();

			$("#PreviewModal").on("shown.bs.modal", function () {
				PopForums.previewPost();
			});
		});
	});
};

PopForums.loadReply = function (topicID, postID, replyID, setupMorePosts) {
	$(window).off("scroll", PopForums.ScrollLoad);
	var n = $("#NewReply");
	var path = PopForums.areaPath + "/Forum/PostReply/" + topicID;
	if (postID != null)
		path += "?quotePostID=" + postID;
	if (replyID != null) {
		if (postID == null)
			path += "?replyID=" + replyID;
		else
			path += "&replyID=" + replyID;
	}
	n.load(path, function () {
		var allowImage = ($("#IsImageEnabled").val().toLowerCase() == "true");
		if (!allowImage) {
			PopForums.editorSettings.toolbar = PopForums.postNoImageToolbar;
		}
		var usePlainText = ($("#IsPlainText").val().toLowerCase() == "true");
		if (!usePlainText) {
			PopForums.editorSettings.selector = "#NewReply #FullText";
			tinyMCE.init(PopForums.editorSettings);
		}
		n.slideDown();
		$("#ReplyButton").hide();
		PopForums.scrollToElement("NewReply");
		$("#MorePostsBeforeReplyButton").click(function () {
			$.get(PopForums.areaPath + "/Forum/TopicPartial/" + topicID + "?lastPost=" + PopForums.currentTopicState.lastVisiblePost + "&lowpage=" + PopForums.currentTopicState.lowPage, function (result) {
				var stuff = $(result);
				var links = stuff.find(".pagerLinks").detach();
				var lastPostID = stuff.find(".lastPostID").detach();
				PopForums.currentTopicState.lastVisiblePost = lastPostID.val();
				var postStream = $("#PostStream");
				postStream.append(stuff);
				links.replaceAll(".pagerLinks");
				$(".postItem img:not('.avatar')").addClass("postImage");
				$(".morePostsButton").remove();
				PopForums.setReplyMorePosts(PopForums.currentTopicState.lastVisiblePost);
			});
		});

		if (setupMorePosts) {
			var connection = new signalR.HubConnectionBuilder().withUrl("/TopicsHub").build();
			connection.start()
				.then(function () {
					var result = connection.invoke("getLastPostID", topicID)
						.then(function (result) {
							PopForums.setReplyMorePosts(result);
						});
				});
		}

		$("#PreviewModal").on("shown.bs.modal", function () {
			PopForums.previewPost();
		});

		PopForums.TopicState.replyLoaded = true;
	});
};

PopForums.loadComment = function (topicID, replyID) {
	var p = $("[data-postid*='" + replyID + "']");
	var n = p.find(".commentHolder");
	var path = PopForums.areaPath + "/Forum/PostReply/" + topicID;
	if (replyID != null)
		path += "?replyID=" + replyID;
	n.load(path, function () {
		var allowImage = ($("#IsImageEnabled").val().toLowerCase() == "true");
		if (!allowImage) {
			PopForums.editorSettings.toolbar = PopForums.postNoImageToolbar;
		}
		var usePlainText = ($("#IsPlainText").val().toLowerCase() == "true");
		if (!usePlainText) {
			PopForums.editorSettings.selector = ".postForm #FullText";
			tinyMCE.init(PopForums.editorSettings);
		}
		n.slideDown();

		$("#PreviewModal").on("shown.bs.modal", function () {
			PopForums.previewPost();
		});
	});
};

PopForums.previewPost = function () {
	tinyMCE.triggerSave();
	var r = $("#ParsedFullText");
	$.ajax({
		url: PopForums.areaPath + "/Forum/PreviewText",
		type: "POST",
		data: { FullText: $(".postForm #FullText").val(), IsPlainText: $(".postForm #IsPlainText").val() },
		dataType: "json",
		converters: { "text json": true },
		success: function (result) {
			r.html(result);
		},
		error: function () {
			r.html("There was a problem getting the preview.");
		}
	});
};

PopForums.loadFeed = function () {
	var connection = new signalR.HubConnectionBuilder().withUrl("/FeedHub").build();
	connection.on("notifyFeed", function (data) {
		var list = $("#FeedList");
		var row = PopForums.populateFeedRow(data);
		list.prepend(row);
		row.fadeIn();
	});
	connection.start()
		.then(function () {
			return connection.invoke("listenToAll");
		});
	PopForums.startTimeUpdater();
};

PopForums.populateFeedRow = function (data) {
	var row = $("#ActivityFeedTemplate").clone();
	row.removeAttr("id");
	row.find(".feedItemText").html(data.message);
	row.find(".fTime").attr("data-utc", data.utc);
	row.find(".fTime").text(data.timeStamp);
	return row;
};

PopForums.setReplyMorePosts = function (lastPostID) {
	var lastPostLoaded = lastPostID == PopForums.currentTopicState.lastVisiblePost;
	if (lastPostLoaded)
		$("#MorePostsBeforeReplyButton").css("visibility", "hidden");
	else
		$("#MorePostsBeforeReplyButton").css("visibility", "visible");
};

PopForums.topicSetup = function (topicID, pageIndex, pageCount, replyID) {
	PopForums.startTimeUpdater();
	var lastPostID = $("#LastPostID").val();
	PopForums.currentTopicState = new PopForums.TopicState(pageIndex, lastPostID, pageCount, topicID);

	var connection = new signalR.HubConnectionBuilder().withUrl("/TopicsHub").build();
	connection.on("fetchNewPost", function (postID) {
		if (!PopForums.TopicState.replyLoaded && PopForums.currentTopicState.highPage == PopForums.currentTopicState.pageCount) {
			$.get(PopForums.areaPath + "/Forum/Post/" + postID, function (data) {
				var post = $(data);
				post.appendTo("#PostStream");
			});
			$("#LastPostID").val(postID);
			PopForums.currentTopicState.lastVisiblePost = postID;
		}
	});
	connection.on("notifyNewPosts", function (theLastPostID) {
		PopForums.setReplyMorePosts(theLastPostID);
	});
	connection.start()
		.then(function () {
			return connection.invoke("listenTo", topicID);
		});

	$(".postItem img:not('.avatar')").addClass("postImage");
	$(document).on("click", "#ReplyButton,.replyLink", function () {
		PopForums.loadReply(topicID, null, replyID, true);
	});
	$(document).on("click", ".quoteLink", function () {
		var postID = $(this).parents(".postItem").attr("data-postID");
		PopForums.loadReply(topicID, postID, replyID, true);
	});
	$(document).on("click", ".postNameLink", function () {
		var box = $(this).parents(".postItem").find(".miniProfileBox");
		var userID = $(this).parents(".postItem").attr("data-userID");
		var chev = $(this).find(".twirl");
		PopForums.loadMiniProfile(userID, box, chev);
	});
	$(document).on("click", ".voteCount", function () {
		var parent = $(this).parents(".postItem");
		var postID = parent.attr("data-postID");
		parent.find(".voters").slideDown(function () {
			$(this).load(PopForums.areaPath + "/Forum/Voters/" + postID);
			$(this).mouseleave(function () {
				$(this).hide();
			});
			$(this).css("display", "block");
		}).css("display", "block");
	});
	$(document).on("click", ".voteUp", function () {
		var parent = $(this).parents(".postItem");
		var postID = parent.attr("data-postID");
		var countBox = $(this).closest(".postToolContainer").children(".voteCount");
		$.ajax({
			url: PopForums.areaPath + "/Forum/VotePost/" + postID,
			type: "POST",
			success: function (result) {
				countBox.html(result);
				var voted = parent.find(".voteUp");
				voted.replaceWith('<li class="list-inline-item">Voted</li>');
			}
		});
	});
	PopForums.SetupSubscribeButton(topicID);
	PopForums.SetupFavoriteButton(topicID);
	$("#TopicModLogButton").click(function () {
		var l = $("#TopicModerationLog");
		if (l.is(":hidden"))
			l.load(PopForums.areaPath + "/Moderator/TopicModerationLog/" + topicID, function () {
				l.slideDown();
			});
		else l.slideUp();
	});
	$(document).on("click", ".postModLogButton", function () {
		var id = $(this).attr("data-postID");
		var l = $(this).closest(".postToolContainer").find(".moderationLog");
		if (l.is(":hidden"))
			l.load(PopForums.areaPath + "/Moderator/PostModerationLog/" + id, function () {
				l.slideDown();
			});
		else l.slideUp();
	});
	$(document).on("click", ".morePostsButton", function () {
		PopForums.LoadMorePosts(topicID, this);
	});
	$(document).on("click", ".previousPostsButton", function () {
		PopForums.currentTopicState.addStartPage();
		var nextPage = PopForums.currentTopicState.lowPage;
		var id = topicID;
		var postStream = $("#PostStream");
		var button = $(this).detach();
		$.get(PopForums.areaPath + "/Forum/TopicPage/" + id + "?pageNumber=" + nextPage + "&low=" + PopForums.currentTopicState.lowPage + "&high=" + PopForums.currentTopicState.highPage, function (result) {
			var stuff = $(result);
			var links = stuff.find(".pagerLinks").detach();
			postStream.prepend(stuff);
			links.replaceAll(".pagerLinks");
			if (PopForums.currentTopicState.lowPage > 1)
				postStream.prepend(button);
			$(".postItem img:not('.avatar')").addClass("postImage");
			if (PopForums.currentTopicState.highPage == PopForums.currentTopicState.pageCount && PopForums.currentTopicState.lowPage == 1) {
				$(".pagerLinks").remove();
			}
		});
	});
	$(window).on("scroll", PopForums.ScrollLoad);
};

PopForums.qaTopicSetup = function (topicID) {
	PopForums.startTimeUpdater();

	$(".postItem img:not('.avatar')").addClass("postImage");
	$(document).on("click", ".commentLink", function () {
		var replyID = $(this).parents(".postItem").attr("data-postid");
		PopForums.loadComment(topicID, replyID);
	});
	$(document).on("click", "#ReplyButton", function () {
		var replyID = $(this).parents(".postContainer").attr("data-postid");
		PopForums.loadReply(topicID, null, replyID);
	});
	$(document).on("click", ".postNameLink", function () {
		var box = $(this).parents(".postUserData").find(".miniProfileBox");
		var userID = $(this).parents(".postUserData").attr("data-userID");
		var chev = $(this).find(".twirl");
		PopForums.loadMiniProfile(userID, box, chev);
	});
	$(document).on("click", ".voteUp", function () {
		var parent = $(this).parents(".postItem");
		var postID = parent.attr("data-postid");
		var countBox = $(this).parents(".answerData").find(".badge");
		$.ajax({
			url: PopForums.areaPath + "/Forum/VotePost/" + postID,
			type: "POST",
			success: function (result) {
				countBox.html(result);
				var voted = parent.find(".voteUp");
				voted.html("Voted");
				voted.removeClass("btn-link");
			}
		});
	});
	$(document).on("click", ".answerButton", function () {
		var button = $(this);
		var parent = button.parents(".postItem");
		var postID = parent.attr("data-postid");
		var topicID = parent.attr("data-topicid");
		$.ajax({
			url: PopForums.areaPath + "/Forum/SetAnswer/",
			type: "POST",
			data: {postID: postID, topicID: topicID},
			success: function () {
				$(".answerStatus").removeClass("icon-checkmark text-success").addClass("icon-checkmark2 text-muted");
				button.removeClass("icon-checkmark2 text-muted").addClass("icon-checkmark text-success");
			}
		});
	});
	PopForums.SetupSubscribeButton(topicID);
	PopForums.SetupFavoriteButton(topicID);
	$("#TopicModLogButton").click(function () {
		var l = $("#TopicModerationLog");
		if (l.is(":none"))
			l.load(PopForums.areaPath + "/Moderator/TopicModerationLog/" + topicID, function () {
				l.slideDown();
			});
		else l.slideUp();
	});
	$(document).on("click", ".postModLogButton", function () {
		var id = $(this).attr("data-postID");
		var l = $(this).closest(".postToolContainer").find(".moderationLog");
		if (l.is(":hidden"))
			l.load(PopForums.areaPath + "/Moderator/PostModerationLog/" + id, function () {
				l.slideDown();
			});
		else l.slideUp();
	});
};

PopForums.SetupSubscribeButton = function (topicID) {
	var s = $("#SubscribeButton");
	s.click(function () {
		var asyncResult = $("#AsyncResponse");
		$.ajax({
			url: PopForums.areaPath + "/Subscription/ToggleSubscription/" + topicID,
			type: "POST",
			dataType: "json",
			success: function (result) {
				switch (result.data.isSubscribed) {
					case true:
						s.val("Unsubscribe");
						break;
					case false:
						s.val("Subscribe");
						break;
					default:
						asyncResult.html(result.Message);
				}
			},
			error: function () {
				asyncResult.html("There was an unknown error while attempting to use subscription");
			}
		});
	});
};

PopForums.SetupFavoriteButton = function(topicID) {
	var f = $("#FavoriteButton");
	f.click(function () {
		var asyncResult = $("#AsyncResponse");
		$.ajax({
			url: PopForums.areaPath + "/Favorites/ToggleFavorite/" + topicID,
			type: "POST",
			dataType: "json",
			success: function (result) {
				switch (result.data.isFavorite) {
					case true:
						f.val("Remove From Favorites");
						break;
					case false:
						f.val("Make Favorite");
						break;
					default:
						asyncResult.html(result.Message);
				}
			},
			error: function () {
				asyncResult.html("There was an unknown error while attempting to use favorites");
			}
		});
	});
};

PopForums.ScrollLoad = function () {
	var win = $(window);
	var streamEnd = $("#StreamBottom").offset().top;
	var viewEnd = win.scrollTop() + win.height();
	var distance = streamEnd - viewEnd;
	if (!PopForums.currentTopicState.loadingPosts && distance < 250 && PopForums.currentTopicState.highPage < PopForums.currentTopicState.pageCount) {
		PopForums.currentTopicState.loadingPosts = true;
		var button = $(".morePostsButton");
		PopForums.LoadMorePosts(PopForums.currentTopicState.topicID, button);
	}
};

PopForums.LoadMorePosts = function (topicID, clickedButton) {
	PopForums.currentTopicState.addEndPage();
	var nextPage = PopForums.currentTopicState.highPage;
	var id = topicID;
	var postStream = $("#PostStream");
	var button = $(clickedButton).detach();
	$.get(PopForums.areaPath + "/Forum/TopicPage/" + id + "?pageNumber=" + nextPage + "&low=" + PopForums.currentTopicState.lowPage + "&high=" + PopForums.currentTopicState.highPage, function (result) {
		var stuff = $(result);
		var links = stuff.find(".pagerLinks").detach();
		var newLastPostID = stuff.find(".lastPostID").detach();
		var newPageCount = stuff.find(".pageCount").detach();
		PopForums.currentTopicState.lastVisiblePost = newLastPostID.val();
		PopForums.currentTopicState.pageCount = newPageCount.val();
		stuff = $("<div>").append(stuff);
		postStream.append(stuff);
		links.replaceAll(".pagerLinks");
		if (PopForums.currentTopicState.highPage != PopForums.currentTopicState.pageCount)
			postStream.append(button);
		if (PopForums.currentTopicState.highPage == PopForums.currentTopicState.pageCount && PopForums.currentTopicState.lowPage == 1) {
			$(".pagerLinks").remove();
		}
		$(".postItem img:not('.avatar')").addClass("postImage");
		PopForums.currentTopicState.loadingPosts = false;
	});
};

PopForums.TopicState = function (startPageIndex, lastVisiblePost, pageCount, topicID) {
	this.lowPage = startPageIndex;
	this.highPage = startPageIndex;
	this.lastVisiblePost = lastVisiblePost;
	this.pageCount = pageCount;
	this.loadingPosts = false;
	this.topicID = topicID;
	this.replyLoaded = false;
};
PopForums.TopicState.prototype.addEndPage = function () { this.highPage++; };
PopForums.TopicState.prototype.addStartPage = function () { this.lowPage--; };

PopForums.postNewTopic = function () {
	$("SubmitNewTopic").attr("disabled", "disabled");
	tinyMCE.triggerSave();
	$.ajax({
		url: PopForums.areaPath + "/Forum/PostTopic",
		type: "POST",
		data: { Title: $("#NewTopic #Title").val(), FullText: $("#NewTopic #FullText").val(), IncludeSignature: $("#NewTopic #IncludeSignature").is(":checked"), ItemID: $("#NewTopic #ItemID").val(), IsPlainText: $("#NewTopic #IsPlainText").val() },
		dataType: "json",
		success: function (result) {
			var r = $("#PostResponseMessage");
			switch (result.result) {
				case true:
					window.location = result.redirect;
					break;
				default:
					r.html(result.message);
					$("#SubmitNewTopic").removeAttr("disabled");
					r.show();
			}
		},
		error: function () {
			var r = $("#PostResponseMessage");
			r.html("There was an unknown error while trying to post");
			$("#SubmitNewTopic").removeAttr("disabled");
			r.show();
		}
	});
};

PopForums.postReply = function () {
	$("#SubmitReply").attr("disabled", "disabled");
	tinyMCE.triggerSave();
	$.ajax({
		url: PopForums.areaPath + "/Forum/PostReply",
		type: "POST",
		data: { Title: $("#NewReply #Title").val(), FullText: $("#NewReply #FullText").val(), IncludeSignature: $("#NewReply #IncludeSignature").is(":checked"), ItemID: $("#NewReply #ItemID").val(), CloseOnReply: $("#CloseOnReply").is(":checked"), IsPlainText: $("#NewReply #IsPlainText").val(), ParentPostID: $("#NewReply #ParentPostID").val() },
		dataType: "json",
		success: function (result) {
			var r = $("#PostResponseMessage");
			switch (result.result) {
				case true:
					window.location = result.redirect;
					break;
				default:
					r.html(result.message);
					$("#SubmitReply").removeAttr("disabled");
					r.show();
			}
		},
		error: function () {
			var r = $("#PostResponseMessage");
			r.html("There was an unknown error while trying to post");
			$("#SubmitReply").removeAttr("disabled");
			r.show();
		}
	});
};

PopForums.loadMiniProfile = function (userID, d, chev) {
	if (d.is(":hidden"))
		d.load(PopForums.areaPath + "/Account/MiniProfile/" + userID, function () {
			d.slideDown();
		});
	else {
		d.slideUp();
	}
	chev.toggleClass("glyphicon-chevron-right glyphicon-chevron-down");
};

PopForums.scrollToElement = function (id) {
	var e = document.getElementById(id);
	var t = 0;
	if (e.offsetParent) {
		while (e.offsetParent) {
			t += e.offsetTop;
			e = e.offsetParent;
		}
	} else if (e.y) {
		t += e.y;
	}
	var crumb = $("#TopBreadcrumb");
	if (crumb)
		t -= crumb.outerHeight();
	scrollTo(0, t);
};

PopForums.homeSetup = function () {
	var connection = new signalR.HubConnectionBuilder().withUrl("/ForumsHub").build();
	connection.on("notifyForumUpdate", function (data) {
		PopForums.updateForumStats(data);
	});
	connection.start()
		.then(function () {
			return connection.invoke("listenToAll");
		});
	PopForums.startTimeUpdater();
};

PopForums.recentListen = function (pageSize) {
	var connection = new signalR.HubConnectionBuilder().withUrl("/RecentHub").build();
	connection.on("notifyRecentUpdate", function (data) {
		var removal = $('#TopicList tr[data-topicID="' + data.topicID + '"]');
		if (removal.length != 0) {
			removal.fadeOut();
			removal.remove();
		} else {
			var rows = $("#TopicList tr:not('#TopicTemplate')");
			if (rows.length == pageSize)
				rows.last().remove();
		}
		var row = PopForums.populateTopicRow(data).hide();
		row.removeClass("hidden");
		$("#TopicList").prepend(row);
		row.fadeIn();
	});
	connection.start()
		.then(function () {
			return connection.invoke("register");
		});
};

PopForums.forumListen = function (pageSize, forumID) {
	var connection = new signalR.HubConnectionBuilder().withUrl("/ForumsHub").build();
	connection.on("notifyUpdatedTopic", function (data) {
		var removal = $('#TopicList tr[data-topicID="' + data.topicID + '"]');
		if (removal.length != 0) {
			removal.fadeOut();
			removal.remove();
		} else {
			var rows = $("#TopicList tr:not('#TopicTemplate')");
			if (rows.length == pageSize)
				rows.last().remove();
		}
		var row = PopForums.populateTopicRow(data).hide();
		row.removeClass("hidden");
		$("#TopicList").prepend(row);
		row.fadeIn();
	});
	connection.start()
		.then(function () {
			return connection.invoke("listenTo", forumID);
		});
};

PopForums.populateTopicRow = function (data) {
	var row = $("#TopicTemplate").clone();
	row.attr("data-topicid", data.topicID);
	row.removeAttr("id");
	row.find(".startedByName").text(data.startedByName);
	row.find(".indicatorLink").attr("href", data.link);
	row.find(".titleLink").text(data.title);
	row.find(".titleLink").attr("href", data.link);
	row.find(".forumTitle").text(data.forumTitle);
	row.find(".viewCount").text(data.viewCount);
	row.find(".replyCount").text(data.replyCount);
	row.find(".lastPostTime").text(data.lastPostTime);
	row.find(".lastPostName").text(data.lastPostName);
	row.find(".fTime").attr("data-utc", data.utc);
	return row;
};

PopForums.updateForumStats = function (data) {
	var row = $("[data-forumid='" + data.forumID + "']");
	row.find(".topicCount").text(data.topicCount);
	row.find(".postCount").text(data.postCount);
	row.find(".lastPostTime").text(data.lastPostTime);
	row.find(".lastPostName").text(data.lastPostName);
	row.find(".fTime").attr("data-utc", data.utc);
	row.find(".newIndicator .icon-file-text2").removeClass("text-muted").addClass("text-warning");
};

PopForums.startTimeUpdater = function () {
	setTimeout(function () {
		PopForums.startTimeUpdater();
		PopForums.updateTimes();
	}, 60000);
};

PopForums.updateTimes = function () {
	var a = [];
	var times = $(".fTime");
	$.each(times, function () {
		var t = $(this).attr("data-utc");
		if (((new Date() - new Date(t + "Z")) / 3600000) < 48)
			a.push(t);
	});
	if (a.length > 0)
		$.ajax({
			url: PopForums.areaPath + "/Time/GetTimes",
			type: "POST",
			data: { times: a },
			dataType: "json",
			traditional: true,
			success: function (result) {
				$.each(result, function () {
					$(".fTime[data-utc='" + this.key + "']").text(this.value);
				});
			},
			error: function () {
			}
		});
};