var ready = (callback) => {
	if (document.readyState != "loading") callback();
	else document.addEventListener("DOMContentLoaded", callback);
}

var PopForums = {};

PopForums.areaPath = "/Forums";

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