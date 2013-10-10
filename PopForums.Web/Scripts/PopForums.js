$(function () {
	$("table.stripe tr:even").addClass("stripe");

	var crumbs = $("#ForumContainer #TopBreadcrumb");
	if (crumbs.length > 0) {
		PopForums.crumbTop = crumbs.offset().top + PopForums.navOffset;
		crumbs.css("width", $("#ForumContainer").width());
		$(window).scroll(function () {
			PopForums.checkScroll();
		});
		PopForums.checkScroll();
	}

	if (window.location.hash) {
		var hash = window.location.hash;
		while (hash.charAt(0) == '#') hash = hash.substr(1);
		var tag = $("div[postID='" + hash + "']");
		if ($("#PostStream").has(tag).length > 0) {
			var offset = tag.offset();
			if (offset) {
				var crumb = $("#TopBreadcrumb");
				var height = crumb.outerHeight();
				var margin = parseInt($(".postItem").css("margin-top"), 10);
				var tagTop = offset.top;
				var newPosition = tagTop - height - margin + PopForums.navOffset;
				$("html,body").animate({ scrollTop: newPosition }, "fast");
			}
		}
	}
});

var PopForums = {};

PopForums.areaPath = "/Forums";
PopForums.contentPath = "/Content/PopForums";
PopForums.currentTopicState = null;
PopForums.navOffset = 0;

PopForums.checkScroll = function () {
	var crumb = $("#ForumContainer #TopBreadcrumb");
	var scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
	if (scrollTop > PopForums.crumbTop && crumb.hasClass("navAbsolute")) {
		var holder = $("<div id='TopBreadcrumbHolder'/>");
		holder.height(crumb.height());
		crumb.addClass("navFixed");
		crumb.removeClass("navAbsolute");
		crumb.after(holder);
	}
	else if (scrollTop < PopForums.crumbTop && crumb.hasClass("navFixed")) {
		crumb.addClass("navAbsolute");
		crumb.removeClass("navFixed");
		$("#TopBreadcrumbHolder").remove();
	}
};

PopForums.processLogin = function() {
	PopForums.processLoginBase("/Authorization/Login");
};

PopForums.processLoginExternal = function () {
	PopForums.processLoginBase("/Authorization/LoginAndAssociate");
};

PopForums.processLoginBase = function (path) {
	$.ajax({
		url: PopForums.areaPath + path,
		type: "POST",
		data: { email: $("#EmailLogin").val(), password: $("#PasswordLogin").val(), persistCookie: $("#PersistCookie").is(":checked") },
		dataType: "json",
		success: function (result) {
			var loginResult = $("#LoginResult");
			switch (result.Result) {
				case true:
					var destination = $("#Referrer").val();
					location = destination;
					break;
				default:
					loginResult.html(result.Message);
			}
		},
		error: function () {
			var loginResult = $("#LoginResult");
			loginResult.html("There was an unknown error while attempting login");
		}
	});
};

PopForums.topicListSetup = function (forumID) {
	PopForums.topicPreviewSetup();
	PopForums.startTimeUpdater();
	var b = $("#NewTopicButton");
	b.click(function () {
		var n = $("#NewTopic");
		n.load(PopForums.areaPath + "/Forum/PostTopic/" + forumID, function () {
			var usePlainText = ($("#IsPlainText").val().toLowerCase() == "true");
			if (!usePlainText) {
				$("#NewTopic #FullText").tinymce({
					script_url: "/scripts/tiny_mce/tiny_mce.js",
					theme: "advanced",
					plugins: "paste,inlinepopups",
					content_css: "/Content/PopForums/Style.css",
					gecko_spellcheck: true,
					mode: "exact",
					init_instance_callback: "PopForums.richEditorComplete"
				});
			}
			n.slideDown();
			b.hide();
		});
	});
};

PopForums.loadReply = function (topicID, postID, replyID) {
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
		var usePlainText = ($("#IsPlainText").val().toLowerCase() == "true");
		if (!usePlainText) {
			$("#NewReply #FullText").tinymce({
				script_url: "/scripts/tiny_mce/tiny_mce.js",
				theme: "advanced",
				plugins: "paste,inlinepopups",
				content_css: "/Content/PopForums/Style.css",
				gecko_spellcheck: true,
				mode: "exact",
				init_instance_callback: "PopForums.richEditorComplete"
			});
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
				stuff.effect("highlight", 1500);
				links.replaceAll(".pagerLinks");
				$(".postItem img:not('.avatar')").addClass("postImage");
				$(".morePostsButton").remove();
				PopForums.setReplyMorePosts(PopForums.currentTopicState.lastVisiblePost);
			});
		});

		var hub = $.connection.topics; // already started in topicsetup
		hub.server.getLastPostID(topicID).done(function (result) {
			PopForums.setReplyMorePosts(result);
		});

		PopForums.TopicState.replyLoaded = true;
	});
};

PopForums.loadFeed = function () {
	var hub = $.connection.feed;
	hub.client.notifyFeed = function (data) {
		var list = $("#FeedList");
		var row = PopForums.populateFeedRow(data);
		list.prepend(row);
		row.fadeIn();
	};
	$.connection.hub.start();
	PopForums.startTimeUpdater();
};

PopForums.populateFeedRow = function (data) {
	var row = $("#ActivityFeedTemplate").clone();
	row.removeAttr("id");
	row.find(".feedItemText").html(data.Message);
	row.find(".fTime").attr("data-utc", data.Utc);
	row.find(".fTime").text(data.TimeStamp);
	return row;
};

PopForums.setReplyMorePosts = function (lastPostID) {
	var lastPostLoaded = lastPostID == PopForums.currentTopicState.lastVisiblePost;
	if (lastPostLoaded)
		$("#MorePostsBeforeReplyButton").css("visibility", "hidden");
	else
		$("#MorePostsBeforeReplyButton").css("visibility", "visible");
};

PopForums.richEditorComplete = function (instance) {
	var isImageEnabled = ($("#IsImageEnabled").val().toLowerCase() == "true");
	if (!isImageEnabled) {
		var imageButton = $(".mce_image");
		imageButton.remove();
	}
};

PopForums.topicSetup = function (topicID, pageIndex, pageCount, replyID) {
	PopForums.startTimeUpdater();
	var lastPostID = $("#LastPostID").val();
	PopForums.currentTopicState = new PopForums.TopicState(pageIndex, lastPostID, pageCount, topicID);

	var topicHub = $.connection.topics;
	topicHub.client.fetchNewPost = function (postID) {
		if (!PopForums.TopicState.replyLoaded && PopForums.currentTopicState.highPage == PopForums.currentTopicState.pageCount) {
			$.get(PopForums.areaPath + "/Forum/Post/" + postID, function (data) {
				var post = $(data);
				post.appendTo("#PostStream");
				post.effect("highlight", 1500);
			});
			$("#LastPostID").val(postID);
			PopForums.currentTopicState.lastVisiblePost = postID;
		}
	};
	topicHub.client.notifyNewPosts = function (theLastPostID) {
		PopForums.setReplyMorePosts(theLastPostID);
	};
	$.connection.hub.start().done(function () {
		topicHub.server.listenTo(topicID);
	});

	$(".postItem img:not('.avatar')").addClass("postImage");
	$(document).on("click", "#ReplyButton,.replyLink", function () {
		PopForums.loadReply(topicID, null, replyID);
	});
	$(document).on("click", ".quoteLink", function () {
		var postID = $(this).parents(".postItem").attr("postID");
		PopForums.loadReply(topicID, postID, replyID);
	});
	$(document).on("click", ".postNameLink", function () {
		var box = $(this).parents(".postItem").find(".miniProfileBox");
		var userID = $(this).parents(".postItem").attr("userID");
		PopForums.loadMiniProfile(userID, box);
	});
	$(document).on("click", ".voteCount", function () {
		var parent = $(this).parents(".postItem");
		var postID = parent.attr("postID");
		parent.find(".voters").slideDown(function () {
			$(this).load(PopForums.areaPath + "/Forum/Voters/" + postID);
			$(this).mouseleave(function () {
				$(this).hide();
			});
		}).css("display", "block");
	});
	$(document).on("click", ".voteUp", function () {
		var parent = $(this).parents(".postItem");
		var postID = parent.attr("postID");
		var countBox = parent.children(".voteCount");
		$.ajax({
			url: PopForums.areaPath + "/Forum/VotePost/" + postID,
			type: "POST",
			success: function (result) {
				countBox.html(result);
				var voted = parent.find(".voteUp");
				voted.replaceWith('Voted');
			}
		});
	});
	var s = $("#SubscribeButton");
	s.click(function () {
		var asyncResult = $("#AsyncResponse");
		$.ajax({
			url: PopForums.areaPath + "/Subscription/ToggleSubscription/" + topicID,
			type: "POST",
			dataType: "json",
			success: function (result) {
				switch (result.Data.isSubscribed) {
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
	var f = $("#FavoriteButton");
	f.click(function () {
		var asyncResult = $("#AsyncResponse");
		$.ajax({
			url: PopForums.areaPath + "/Favorites/ToggleFavorite/" + topicID,
			type: "POST",
			dataType: "json",
			success: function (result) {
				switch (result.Data.isFavorite) {
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
	$("#TopicModLogButton").click(function () {
		var l = $("#TopicModerationLog");
		if (l.is(":hidden"))
			l.load(PopForums.areaPath + "/Moderator/TopicModerationLog/" + topicID, function () {
				l.slideDown();
			});
		else l.slideUp();
	});
	$(document).on("click", ".postModLogButton", function () {
		var id = $(this).attr("postID");
		var l = $(this).next();
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
		$.get(PopForums.areaPath + "/Forum/TopicPage/" + id + "?page=" + nextPage + "&low=" + PopForums.currentTopicState.lowPage + "&high=" + PopForums.currentTopicState.highPage, function (result) {
			var stuff = $(result);
			var links = stuff.find(".pagerLinks").detach();
			postStream.prepend(stuff);
			stuff.effect("highlight", 1500);
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
	$.get(PopForums.areaPath + "/Forum/TopicPage/" + id + "?page=" + nextPage + "&low=" + PopForums.currentTopicState.lowPage + "&high=" + PopForums.currentTopicState.highPage, function (result) {
		var stuff = $(result);
		var links = stuff.find(".pagerLinks").detach();
		var newLastPostID = stuff.find(".lastPostID").detach();
		var newPageCount = stuff.find(".pageCount").detach();
		PopForums.currentTopicState.lastVisiblePost = newLastPostID.val();
		PopForums.currentTopicState.pageCount = newPageCount.val();
		stuff = $("<div>").append(stuff);
		postStream.append(stuff);
		stuff.effect("highlight", 1500);
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

PopForums.topicPreviewSetup = function () {
	$(document).on("click", ".topicPreviewButton", function () {
		var id = $(this).attr("topicID");
		var preview = $("#TopicPreview" + id);
		if (preview.is(":hidden")) {
			preview.html("<p>Loading...</p>");
			preview.slideDown();
			$.ajax({
				url: PopForums.areaPath + "/Forum/FirstPostPreview/" + id,
				type: "GET",
				dataType: "json",
				success: function (result) {
					preview.html(result.Data.FullText);
				},
				error: function () {
					preview.html("<p>There was an unknown error getting the preview</p>");
				}
			});
		} else
			preview.slideUp();
	});
};

PopForums.postNewTopic = function () {
	$("SubmitNewTopic").attr("disabled", "disabled");
	$.ajax({
		url: PopForums.areaPath + "/Forum/PostTopic",
		type: "POST",
		data: { Title: $("#NewTopic #Title").val(), FullText: $("#NewTopic #FullText").val(), IncludeSignature: $("#NewTopic #IncludeSignature").is(":checked"), ItemID: $("#NewTopic #ItemID").val(), IsPlainText: $("#NewTopic #IsPlainText").val() },
		dataType: "json",
		success: function (result) {
			var r = $("#PostResponseMessage");
			switch (result.Result) {
				case true:
					window.location = result.Redirect;
					break;
				default:
					r.html(result.Message);
					$("#SubmitNewTopic").removeAttr("disabled");
			}
		},
		error: function () {
			var r = $("#PostResponseMessage");
			r.html("There was an unknown error while trying to post");
			$("#SubmitNewTopic").removeAttr("disabled");
		}
	});
};

PopForums.postReply = function () {
	$("#SubmitReply").attr("disabled", "disabled");
	$.ajax({
		url: PopForums.areaPath + "/Forum/PostReply",
		type: "POST",
		data: { Title: $("#NewReply #Title").val(), FullText: $("#NewReply #FullText").val(), IncludeSignature: $("#NewReply #IncludeSignature").is(":checked"), ItemID: $("#NewReply #ItemID").val(), CloseOnReply: $("#CloseOnReply").is(":checked"), IsPlainText: $("#NewReply #IsPlainText").val(), ParentPostID: $("#NewReply #ParentPostID").val() },
		dataType: "json",
		success: function (result) {
			var r = $("#PostResponseMessage");
			switch (result.Result) {
				case true:
					window.location = result.Redirect;
					break;
				default:
					r.html(result.Message);
					$("#SubmitReply").removeAttr("disabled");
			}
		},
		error: function () {
			var r = $("#PostResponseMessage");
			r.html("There was an unknown error while trying to post");
			$("#SubmitReply").removeAttr("disabled");
		}
	});
};

PopForums.loadMiniProfile = function (userID, d) {
	if (d.is(":hidden"))
		d.load(PopForums.areaPath + "/Account/MiniProfile/" + userID, function () {
			d.slideDown();
		});
	else d.slideUp();
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
	t += PopForums.navOffset;
	scrollTo(0, t);
};

PopForums.homeSetup = function () {
	var hub = $.connection.forums;
	hub.client.notifyForumUpdate = function (data) {
		PopForums.updateForumStats(data);
	};
	$.connection.hub.start();
	PopForums.startTimeUpdater();
};

PopForums.recentListen = function (pageSize) {
	var hub = $.connection.recent;
	hub.client.notifyRecentUpdate = function (data) {
		var removal = $('.forumGrid tr[data-topicID="' + data.TopicID + '"]');
		if (removal.length != 0) {
			removal.fadeOut();
			removal.remove();
		} else {
			var rows = $(".forumGrid tr:not('#TopicTemplate')");
			if (rows.length == pageSize)
				rows.last().remove();
		}
		var row = PopForums.populateTopicRow(data).hide();
		$(".forumGrid").prepend(row);
		row.fadeIn();
	};
	$.connection.hub.start().done(function () {
		hub.server.register();
	});
};

PopForums.forumListen = function (pageSize, forumID) {
	var hub = $.connection.forums;
	hub.client.notifyUpdatedTopic = function (data) {
		var removal = $('.forumGrid tr[data-topicID="' + data.TopicID + '"]');
		if (removal.length != 0) {
			removal.fadeOut();
			removal.remove();
		} else {
			var rows = $(".forumGrid tr:not('#TopicTemplate')");
			if (rows.length == pageSize)
				rows.last().remove();
		}
		var row = PopForums.populateTopicRow(data).hide();
		$(".forumGrid").prepend(row);
		row.fadeIn();
	};
	$.connection.hub.start().done(function () {
		hub.server.listenTo(forumID);
	});
};

PopForums.populateTopicRow = function (data) {
	var row = $("#TopicTemplate").clone();
	row.attr("data-topicID", data.TopicID);
	row.removeAttr("id");
	row.find(".startedByName").text(data.StartedByName);
	row.find(".indicatorLink").attr("href", data.Link);
	row.find(".titleLink").text(data.Title);
	row.find(".titleLink").attr("href", data.Link);
	row.find(".topicPreview").attr("id", "TopicPreview" + data.TopicID);
	row.find(".topicPreviewButton").attr("topicID", data.TopicID);
	row.find(".forumTitle").text(data.ForumTitle);
	row.find(".viewCount").text(data.ViewCount);
	row.find(".replyCount").text(data.ReplyCount);
	row.find(".lastPostTime").text(data.LastPostTime);
	row.find(".lastPostName").text(data.LastPostName);
	row.find(".fTime").attr("data-utc", data.Utc);
	row.find(".newIndicator img").attr("src", PopForums.contentPath + "/" + data.Image);
	return row;
};

PopForums.updateForumStats = function (data) {
	var row = $("[data-forumID='" + data.ForumID + "']");
	row.find(".topicCount").text(data.TopicCount);
	row.find(".postCount").text(data.PostCount);
	row.find(".lastPostTime").text(data.LastPostTime);
	row.find(".lastPostName").text(data.LastPostName);
	row.find(".fTime").attr("data-utc", data.Utc);
	row.find(".newIndicator img").attr("src", PopForums.contentPath + "/" + data.Image);
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
					$(".fTime[data-utc='" + this.Key + "']").text(this.Value);
				});
			},
			error: function () {
			}
		});
};