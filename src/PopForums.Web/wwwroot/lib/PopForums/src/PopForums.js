var ready = (callback) => {
	if (document.readyState != "loading") callback();
	else document.addEventListener("DOMContentLoaded", callback);
}

var PopForums = {};

PopForums.areaPath = "/Forums";

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