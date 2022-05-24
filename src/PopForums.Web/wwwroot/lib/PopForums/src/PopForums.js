var ready = (callback) => {
	if (document.readyState != "loading") callback();
	else document.addEventListener("DOMContentLoaded", callback);
}

var PopForums = {};

PopForums.areaPath = "/Forums"; // TODO: migrate this
PopForums.theTopicState = null;

PopForums.processLogin = function () {
	PopForums.processLoginBase("/Identity/Login");
};

PopForums.processLoginExternal = function () {
	PopForums.processLoginBase("/Identity/LoginAndAssociate");
};

PopForums.processLoginBase = function (path) {
	var email = document.querySelector("#EmailLogin").value;
	var password = document.querySelector("#PasswordLogin").value;
	fetch(PopForums.areaPath + path, {
		method: "POST",
		headers: {
			'Content-Type': 'application/json'
		},
		body: JSON.stringify({ email: email, password: password })
	})
		.then(function(response) {
			return response.json();
	})
		.then(function (result) {
			var loginResult = document.querySelector("#LoginResult");
			switch (result.result) {
			case true:
				var destination = document.querySelector("#Referrer").value;
				location = destination;
				break;
			default:
				loginResult.innerHTML = result.message;
				loginResult.classList.remove("d-none");
			}
	})
		.catch(function (error) {
			var loginResult = document.querySelector("#LoginResult");
			loginResult.innerHTML = "There was an unknown error while attempting login";
			loginResult.classList.remove("d-none");
	});
};

PopForums.scrollToPostFromHash = () => {
	if (window.location.hash) {
		var hash = window.location.hash;
		while (hash.charAt(0) === '#') hash = hash.substr(1);
		var tag = document.querySelector("div[data-postID='" + hash + "']");
		if (tag) {
			var tagPosition = tag.getBoundingClientRect().top;
			var crumb = document.querySelector("#ForumContainer #TopBreadcrumb");
			var crumbHeight = crumb.getBoundingClientRect().height;
			var e = getComputedStyle(document.querySelector(".postItem"));
			var margin = parseFloat(e.marginTop, 10);
			var newPosition = tagPosition - crumbHeight - margin;
			window.scrollBy({ top: newPosition, behavior: 'auto' });
		}
		PopForums.theTopicState.isScrollAdjusted = true;
	}
};

PopForums.topicListSetup = function (forumID) {
	PopForums.startTimeUpdater();
	var b = document.querySelector("#NewTopicButton");
	b?.addEventListener("click", () => {
		var n = document.querySelector("#NewTopic");
		fetch(PopForums.areaPath + "/Forum/PostTopic/" + forumID)
			.then((response) => {
				return response.text();
			})
			.then((body) => {
				n.innerHTML = body;
				n.style.display = "block"; // TODO: animate?
				b.style.display = "none";
			});
	});
};

PopForums.loadReply = function (topicID, replyID, setupMorePosts) {
	window.removeEventListener("scroll", PopForums.ScrollLoad);
	var path = PopForums.areaPath + "/Forum/PostReply/" + topicID;
	if (replyID != null) {
		path += "&replyID=" + replyID;
	}

	fetch(path)
		.then(response => response.text()
			.then(text => {
				var n = document.querySelector("#NewReply");
				n.innerHTML = text;
				n.style.display = "block";
				// legacy tiny was init here
				document.querySelector("#ReplyButton").style.display = "none";
				PopForums.scrollToElement("NewReply");

				document.querySelector("#MorePostsBeforeReplyButton")?.addEventListener("click", (e) => {
					var topicPartialPath = PopForums.areaPath + "/Forum/TopicPartial/" + topicID + "?lastPost=" + PopForums.theTopicState.lastVisiblePost + "&lowpage=" + PopForums.theTopicState.lowPage;
					fetch(topicPartialPath)
						.then(response => response.text()
							.then(text => {
								var t = document.createElement("template");
								t.innerHTML = text.trim();
								var stuff = t.content.firstChild;
								var links = stuff.querySelector(".pagerLinks");
								stuff.removeChild(links);
								var lastPostID = stuff.querySelector(".lastPostID");
								stuff.removeChild(lastPostID);
								PopForums.theTopicState.lastVisiblePost = lastPostID.value;
								var postStream = document.querySelector("#PostStream");
								postStream.append(stuff);
								document.querySelectorAll(".pagerLinks").forEach(x => x.replaceWith(links.cloneNode(true)));
								document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));
								document.querySelector("#MorePostsBeforeReplyButton").style.visibility = "hidden";
								var moreButton = document.querySelector(".morePostsButton");
								if (moreButton)
									moreButton.remove();
								PopForums.setReplyMorePosts(PopForums.theTopicState.lastVisiblePost);
							}));
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

				PopForums.theTopicState.replyLoaded = true;
				PopForums.currentTopicState.isReplyLoaded = true; // TODO: temporary for new library
			}));
};

PopForums.loadComment = function (topicID, replyID) {
	const d = document;
	var p = d.querySelector("[data-postid*='" + replyID + "']");
	var n = p.querySelector(".commentHolder");
	var path = PopForums.areaPath + "/Forum/PostReply/" + topicID;
	if (replyID != null)
		path += "?replyID=" + replyID;

	fetch(path)
		.then(response => response.text()
			.then(text => {
				n.innerHTML = text;
				// legacy tiny init here
			}));
};

PopForums.loadFeed = function () {
	var connection = new signalR.HubConnectionBuilder().withUrl("/FeedHub").build();
	connection.on("notifyFeed", function (data) {
		var list = document.querySelector("#FeedList");
		var row = PopForums.populateFeedRow(data);
		list.prepend(row);
		row.classList.remove("hidden");
	});
	connection.start()
		.then(function () {
			return connection.invoke("listenToAll");
		});
	PopForums.startTimeUpdater();
};

PopForums.populateFeedRow = function (data) {
	var row = document.querySelector("#ActivityFeedTemplate").cloneNode(true);
	row.removeAttribute("id");
	row.querySelector(".feedItemText").innerHTML = data.message;
	row.querySelector(".fTime").setAttribute("data-utc", data.utc);
	row.querySelector(".fTime").innerHTML = data.timeStamp;
	return row;
};

PopForums.setReplyMorePosts = function (lastPostID) {
	var lastPostLoaded = lastPostID == PopForums.theTopicState.lastVisiblePost;
	var button = document.querySelector("#MorePostsBeforeReplyButton");
	if (lastPostLoaded)
		button.style.visibility = "hidden";
	else
		button.style.visibility = "visible";
};

PopForums.topicSetup = function (topicID, pageIndex, pageCount, replyID) {
	PopForums.startTimeUpdater();
	var lastPostID = document.querySelector("#LastPostID").value;
	PopForums.theTopicState = new PopForums.oldTopicState(pageIndex, lastPostID, pageCount, topicID);

	var connection = new signalR.HubConnectionBuilder().withUrl("/TopicsHub").build();
	connection.on("fetchNewPost", function (postID) {
		if (!PopForums.theTopicState.replyLoaded && PopForums.theTopicState.highPage == PopForums.theTopicState.pageCount) {
			fetch(PopForums.areaPath + "/Forum/Post/" + postID)
				.then(response => response.text()
					.then(text => {
						var t = document.createElement("template");
						t.innerHTML = text.trim();
						document.querySelector("#PostStream").appendChild(t.content.firstChild);
					}));
			document.querySelector("#LastPostID").value = postID;
			PopForums.theTopicState.lastVisiblePost = postID;
		}
	});
	connection.on("notifyNewPosts", function (theLastPostID) {
		PopForums.setReplyMorePosts(theLastPostID);
	});
	connection.start()
		.then(function () {
			return connection.invoke("listenTo", topicID);
		});

	document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));

	document.querySelector("#ReplyButton")?.addEventListener("click", event => {
		PopForums.loadReply(topicID, replyID, true);
	});
	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("replyLink")) {
			PopForums.loadReply(topicID, replyID, true);
		}
	});
	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("postNameLink")) {
			var box = event.target.closest(".postItem").querySelector(".miniProfileBox");
			var userID = event.target.closest(".postItem").getAttribute("data-userID");
			PopForums.loadMiniProfile(userID, box);
		}
	});
	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("voteCount")) {
			var parent = event.target.closest(".postItem");
			var postID = parent.getAttribute("data-postID");
			var voters = parent.querySelector(".voters");
			if (voters) {
				if (voters.style.display === "block")
					voters.style.display = "none";
				else {
					fetch(PopForums.areaPath + "/Forum/Voters/" + postID)
						.then(response => response.text()
							.then(text => {
								var t = document.createElement("template");
								t.innerHTML = text.trim();
								voters.innerHTML = "";
								voters.appendChild(t.content.firstChild);
								voters.style.display = "block";
							}));
				}
			}
		}
	});
	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("voteUp")) {
			var parent = event.target.closest(".postItem");
			var postID = parent.getAttribute("data-postID");
			var countBox = parent.querySelector(".voteCount");
			fetch(PopForums.areaPath + "/Forum/VotePost/" + postID, {
				method: "POST"
			})
				.then(response => response.text()
					.then(text => {
						countBox.innerHTML = text;
						parent.querySelector(".voteUp").outerHTML = '<li class="list-inline-item">Voted</li>';
					}));
		}
	});
	document.querySelector("#TopicModLogButton")?.addEventListener("click", () => {
		var l = document.querySelector("#TopicModerationLog");
		if (l.style.display != "block")
			fetch(PopForums.areaPath + "/Moderator/TopicModerationLog/" + topicID)
				.then(response => response.text()
					.then(text => {
						l.innerHTML = text;
						l.style.display = "block";
					}));
		else l.style.display = "none";
	});
	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("postModLogButton")) {
			var id = event.target.getAttribute("data-postid");
			var l = event.target.closest(".postToolContainer").querySelector(".moderationLog");
			if (l.style.display != "block")
				fetch(PopForums.areaPath + "/Moderator/PostModerationLog/" + id)
					.then(response => response.text()
						.then(text => {
							l.innerHTML = text;
							l.style.display = "block";
						}));
			else l.style.display = "none";
		}
	});
	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("morePostsButton")) {
			PopForums.LoadMorePosts(topicID, event.target);
		}
	});
	document.querySelector("#PostStream").addEventListener("click", e => {
		if (event.target.classList.contains("previousPostsButton")) {
			PopForums.theTopicState.addStartPage();
			var nextPage = PopForums.theTopicState.lowPage;
			var id = topicID;
			e.target.remove();
			var topicPartialPath = PopForums.areaPath + "/Forum/TopicPage/" + id + "?pageNumber=" + nextPage + "&low=" + PopForums.theTopicState.lowPage + "&high=" + PopForums.theTopicState.highPage;
			fetch(topicPartialPath)
				.then(response => response.text()
					.then(text => {
						var t = document.createElement("template");
						t.innerHTML = text.trim();
						var stuff = t.content.firstChild;
						var links = stuff.querySelector(".pagerLinks");
						stuff.removeChild(links);
						var postStream = document.querySelector("#PostStream");
						postStream.prepend(stuff);
						if (PopForums.theTopicState.lowPage > 1)
							postStream.prepend(e.target);
						document.querySelectorAll(".pagerLinks").forEach(x => x.replaceWith(links.cloneNode(true)));
						document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));
						if (PopForums.theTopicState.highPage == PopForums.theTopicState.pageCount && PopForums.theTopicState.lowPage == 1) {
							document.querySelectorAll(".pagerLinks").forEach(x => x.remove());
						}
					}));
		}
	});
	PopForums.scrollToPostFromHash();
	window.addEventListener("scroll", PopForums.ScrollLoad);
};

PopForums.qaTopicSetup = function (topicID) {
	PopForums.startTimeUpdater();

	document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));

	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("commentLink")) {
			var replyID = event.target.closest(".postItem").getAttribute("data-postid");
			PopForums.loadComment(topicID, replyID);
		}
	});
	document.querySelector("#ReplyButton")?.addEventListener("click", event => {
		PopForums.loadReply(topicID, null);
	});
	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("postNameLink")) {
			var box = event.target.closest(".postUserData").querySelector(".miniProfileBox");
			var userID = event.target.closest(".postUserData").getAttribute("data-userID");
			PopForums.loadMiniProfile(userID, box);
		}
	});
	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("voteUp")) {
			var parent = event.target.closest(".postItem");
			var postID = parent.getAttribute("data-postID");
			var countBox = parent.querySelector(".answerData").querySelector(".badge");
			fetch(PopForums.areaPath + "/Forum/VotePost/" + postID, {
				method: "POST"
			})
				.then(response => response.text()
					.then(text => {
						countBox.innerHTML = text;
						parent.querySelector(".voteUp").outerHTML = '<li class="list-inline-item">Voted</li>';
					}));
		}
	});
	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("answerButton")) {
			var button = event.target;
			var parent = button.closest(".postItem");
			var postID = parent.getAttribute("data-postid");
			var topicID = parent.getAttribute("data-topicid");
			var model = { postID: postID, topicID: topicID };
			fetch(PopForums.areaPath + "/Forum/SetAnswer/", {
				method: "POST",
				body: JSON.stringify(model),
				headers: {
					"Content-Type": "application/json"
				}
			})
				.then(response => {
					document.querySelectorAll(".answerStatus").forEach(x => {
						x.classList.remove("icon-checkmark");
						x.classList.remove("text-success");
						x.classList.add("icon-checkmark2");
						x.classList.add("text-muted");
					});
					button.classList.remove("icon-checkmark2");
					button.classList.remove("text-muted");
					button.classList.add("icon-checkmark");
					button.classList.add("text-success");
				});
		}
	});
	document.querySelector("#TopicModLogButton")?.addEventListener("click", () => {
		var l = document.querySelector("#TopicModerationLog");
		if (l.style.display != "block")
			fetch(PopForums.areaPath + "/Moderator/TopicModerationLog/" + topicID)
				.then(response => response.text()
					.then(text => {
						l.innerHTML = text;
						l.style.display = "block";
					}));
		else l.style.display = "none";
	});
	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("postModLogButton")) {
			var id = event.target.getAttribute("data-postid");
			var l = event.target.closest(".postToolContainer").querySelector(".moderationLog");
			if (l.style.display != "block")
				fetch(PopForums.areaPath + "/Moderator/PostModerationLog/" + id)
					.then(response => response.text()
						.then(text => {
							l.innerHTML = text;
							l.style.display = "block";
						}));
			else l.style.display = "none";
		}
	});
};

PopForums.ScrollLoad = function () {
	var streamEnd = document.querySelector("#StreamBottom").offsetTop;
	var viewEnd = window.scrollY + window.outerHeight;
	var distance = streamEnd - viewEnd;
	if (!PopForums.theTopicState.loadingPosts && distance < 250 && PopForums.theTopicState.highPage < PopForums.theTopicState.pageCount) {
		PopForums.theTopicState.loadingPosts = true;
		var button = document.querySelector(".morePostsButton");
		PopForums.LoadMorePosts(PopForums.theTopicState.topicID, button);
	}
};

PopForums.LoadMorePosts = function (topicID, clickedButton) {
	PopForums.theTopicState.addEndPage();
	var nextPage = PopForums.theTopicState.highPage;
	var id = topicID;
	var topicPartialPath = PopForums.areaPath + "/Forum/TopicPage/" + id + "?pageNumber=" + nextPage + "&low=" + PopForums.theTopicState.lowPage + "&high=" + PopForums.theTopicState.highPage;
	fetch(topicPartialPath)
		.then(response => response.text()
			.then(text => {
				var t = document.createElement("template");
				t.innerHTML = text.trim();
				var stuff = t.content.firstChild;
				var links = stuff.querySelector(".pagerLinks");
				stuff.removeChild(links);
				var lastPostID = stuff.querySelector(".lastPostID");
				stuff.removeChild(lastPostID);
				var newPageCount = stuff.querySelector(".pageCount");
				stuff.removeChild(newPageCount);
				PopForums.theTopicState.lastVisiblePost = lastPostID.value;
				PopForums.theTopicState.pageCount = newPageCount.value;
				var postStream = document.querySelector("#PostStream");
				postStream.append(stuff);
				document.querySelectorAll(".pagerLinks").forEach(x => x.replaceWith(links.cloneNode(true)));
				document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));
				clickedButton.remove();
				if (PopForums.theTopicState.highPage != PopForums.theTopicState.pageCount)
					postStream.append(clickedButton);
				if (PopForums.theTopicState.highPage == PopForums.theTopicState.pageCount && PopForums.theTopicState.lowPage == 1) {
					document.querySelectorAll(".pagerLinks").forEach(x => x.remove());
				}
				PopForums.theTopicState.loadingPosts = false;
				if (!PopForums.theTopicState.isScrollAdjusted) {
					PopForums.scrollToPostFromHash();
					PopForums.theTopicState.isScrollAdjusted = true;
				}
			}));
};

PopForums.oldTopicState = function (startPageIndex, lastVisiblePost, pageCount, topicID) {
	this.lowPage = startPageIndex;
	this.highPage = startPageIndex;
	this.lastVisiblePost = lastVisiblePost;
	this.pageCount = pageCount;
	this.loadingPosts = false;
	this.topicID = topicID;
	this.replyLoaded = false;
	this.isScrollAdjusted = false;
};
PopForums.oldTopicState.prototype.addEndPage = function () { this.highPage++; };
PopForums.oldTopicState.prototype.addStartPage = function () { this.lowPage--; };

PopForums.postNewTopic = function () {
	const d = document;
	d.querySelector("#SubmitNewTopic").setAttribute("disabled", "disabled");
	var model = {
		Title: d.querySelector("#NewTopic #Title").value,
		FullText: d.querySelector("#NewTopic #FullText").value,
		IncludeSignature: d.querySelector("#NewTopic #IncludeSignature").checked,
		ItemID: d.querySelector("#NewTopic #ItemID").value,
		IsPlainText: d.querySelector("#NewTopic #IsPlainText").value.toLowerCase() === "true"
	};
	fetch(PopForums.areaPath + "/Forum/PostTopic", {
		method: "POST",
		body: JSON.stringify(model),
		headers: {
			"Content-Type": "application/json"
		},
	})
		.then(response => response.json())
		.then(result => {
			switch (result.result) {
				case true:
					window.location = result.redirect;
					break;
				default:
					var r = d.querySelector("#PostResponseMessage");
					r.innerHTML = result.message;
					d.querySelector("#SubmitNewTopic").removeAttribute("disabled");
					r.style.display = "block";
			}
		})
		.catch(error => {
			var r = d.querySelector("#PostResponseMessage");
			r.innerHTML = "There was an unknown error while trying to post";
			d.querySelector("#SubmitNewTopic").removeAttribute("disabled");
			r.style.display = "block";
		});
};

PopForums.postReply = function () {
	const d = document;
	d.querySelector("#SubmitReply").setAttribute("disabled", "disabled");
	var closeCheck = d.querySelector("#CloseOnReply");
	var closeOnReply = false;
	if (closeCheck && closeCheck.checked) closeOnReply = true;
	var model = {
		Title: d.querySelector("#NewReply #Title").value,
		FullText: d.querySelector("#NewReply #FullText").value,
		IncludeSignature: d.querySelector("#NewReply #IncludeSignature").checked,
		ItemID: d.querySelector("#NewReply #ItemID").value,
		CloseOnReply: closeOnReply,
		IsPlainText: d.querySelector("#NewReply #IsPlainText").value.toLowerCase() === "true",
		ParentPostID: d.querySelector("#NewReply #ParentPostID").value
	};
	fetch(PopForums.areaPath + "/Forum/PostReply", {
		method: "POST",
		body: JSON.stringify(model),
		headers: {
			"Content-Type": "application/json"
		},
	})
		.then(response => response.json())
		.then(result => {
			switch (result.result) {
				case true:
					window.location = result.redirect;
					break;
				default:
					var r = d.querySelector("#PostResponseMessage");
					r.innerHTML = result.message;
					d.querySelector("#SubmitReply").removeAttribute("disabled");
					r.style.display = "block";
			}
		})
		.catch(error => {
			var r = d.querySelector("#PostResponseMessage");
			r.innerHTML = "There was an unknown error while trying to post";
			d.querySelector("#SubmitReply").removeAttribute("disabled");
			r.style.display = "block";
		});
};

PopForums.loadMiniProfile = function (userID, d) {
	if (!d.classList.contains("open")) {
		fetch(PopForums.areaPath + "/Account/MiniProfile/" + userID)
			.then(response => response.text()
				.then(text => {
					var sub = d.querySelector("div");
					sub.innerHTML = text;
					const height = sub.getBoundingClientRect().height;
					d.style.height = `${height}px`;
					d.classList.add("open");
				}));
	}
	else {
		d.classList.remove("open");
		d.style.height = 0;
	}
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
	var crumb = document.querySelector("#TopBreadcrumb");
	if (crumb)
		t -= crumb.offsetHeight;
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
		var removal = document.querySelector('#TopicList tr[data-topicID="' + data.topicID + '"]');
		if (removal) {
			removal.remove();
		} else {
			var rows = document.querySelectorAll("#TopicList tr:not(#TopicTemplate)");
			if (rows.length == pageSize)
				rows[rows.length - 1].remove();
		}
		var row = PopForums.populateTopicRow(data);
		row.classList.remove("hidden");
		document.querySelector("#TopicList tbody").prepend(row);
	});
	connection.start()
		.then(function () {
			return connection.invoke("register");
		});
};

PopForums.forumListen = function (pageSize, forumID) {
	var connection = new signalR.HubConnectionBuilder().withUrl("/ForumsHub").build();
	connection.on("notifyUpdatedTopic", function (data) {
		var removal = document.querySelector('#TopicList tr[data-topicID="' + data.topicID + '"]');
		if (removal) {
			removal.remove();
		} else {
			var rows = document.querySelectorAll("#TopicList tr:not(#TopicTemplate)");
			if (rows.length == pageSize)
				rows[rows.length - 1].remove();
		}
		var row = PopForums.populateTopicRow(data);
		row.classList.remove("hidden");
		document.querySelector("#TopicList tbody").prepend(row);
	});
	connection.start()
		.then(function () {
			return connection.invoke("listenTo", forumID);
		});
};

PopForums.populateTopicRow = function (data) {
	var row = document.querySelector("#TopicTemplate").cloneNode(true);
	row.setAttribute("data-topicid", data.topicID);
	row.removeAttribute("id");
	row.querySelector(".startedByName").innerHTML = data.startedByName;
	row.querySelector(".indicatorLink").setAttribute("href", data.link);
	row.querySelector(".titleLink").innerHTML = data.title;
	row.querySelector(".titleLink").setAttribute("href", data.link);
	var forumTitle = row.querySelector(".forumTitle");
	if (forumTitle) forumTitle.innerHTML = data.forumTitle;
	row.querySelector(".viewCount").innerHTML = data.viewCount;
	row.querySelector(".replyCount").innerHTML = data.replyCount;
	row.querySelector(".lastPostTime").innerHTML = data.lastPostTime;
	row.querySelector(".lastPostName").innerHTML = data.lastPostName;
	row.querySelector(".fTime").setAttribute("data-utc", data.utc);
	return row;
};

PopForums.updateForumStats = function (data) {
	var row = document.querySelector("[data-forumid='" + data.forumID + "']");
	row.querySelector(".topicCount").innerHTML = data.topicCount;
	row.querySelector(".postCount").innerHTML = data.postCount;
	row.querySelector(".lastPostTime").innerHTML = data.lastPostTime;
	row.querySelector(".lastPostName").innerHTML = data.lastPostName;
	row.querySelector(".fTime").setAttribute("data-utc", data.utc);
	row.querySelector(".newIndicator .icon-file-text2").classList.remove("text-muted");
	row.querySelector(".newIndicator .icon-file-text2").classList.add("text-warning");
};

PopForums.startTimeUpdater = function () {
	setTimeout(function () {
		PopForums.startTimeUpdater();
		PopForums.updateTimes();
	}, 60000);
};

PopForums.updateTimes = function () {
	var a = [];
	var times = document.querySelectorAll(".fTime");
	times.forEach(time => {
		var t = time.getAttribute("data-utc");
		if (((new Date() - new Date(t + "Z")) / 3600000) < 48)
			a.push(t);
	});
	if (a.length > 0) {
		var formData = new FormData();
		a.forEach(t => formData.append("times", t));
		fetch(PopForums.areaPath + "/Time/GetTimes", {
			method: "POST",
			body: formData
		})
			.then(response => response.json())
			.then(data => {
				data.forEach(t => {
					document.querySelector(".fTime[data-utc='" + t.key + "']").innerHTML = t.value;
				});
			})
			.catch(error => { console.log("Time update failure: " + error); });
	}
};