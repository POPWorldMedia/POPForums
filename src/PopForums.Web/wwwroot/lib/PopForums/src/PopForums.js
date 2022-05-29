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
		path += "?replyID=" + replyID;
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
	document.querySelector("#PostStream").addEventListener("click", event => {
		if (event.target.classList.contains("postNameLink")) {
			var box = event.target.closest(".postItem").querySelector(".miniProfileBox");
			var userID = event.target.closest(".postItem").getAttribute("data-userID");
			PopForums.loadMiniProfile(userID, box);
		}
	});
};

PopForums.qaTopicSetup = function (topicID) {
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