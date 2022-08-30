namespace PopForums {

    export class ForumState extends StateBase {
        constructor() {
            super();
        }
    
        forumID: number;
        pageSize: number;
        pageIndex: number;
        @WatchProperty
        isNewTopicLoaded: boolean;

        setupForum() {
            PopForums.Ready(() => {
                this.isNewTopicLoaded = false;
                this.forumListen();
            });
        }

        loadNewTopic() {
            fetch(PopForums.AreaPath + "/Forum/PostTopic/" + this.forumID)
                .then((response) => {
                    return response.text();
                })
                .then((body) => {
                    var n = document.querySelector("#NewTopic") as HTMLElement;
                    if (!n)
                        throw("Can't find a #NewTopic element to load in the new topic form.");
                    n.innerHTML = body;
                    n.style.display = "block";
                    this.isNewTopicLoaded = true;
                });
        }

        forumListen() {
            let connection = new signalR.HubConnectionBuilder().withUrl("/ForumsHub").withAutomaticReconnect().build();
            let self = this;
            connection.on("notifyUpdatedTopic", function (data: any) { // TODO: refactor to strong type
                let removal = document.querySelector('#TopicList tr[data-topicID="' + data.topicID + '"]');
                if (removal) {
                    removal.remove();
                } else {
                    let rows = document.querySelectorAll("#TopicList tr:not(#TopicTemplate)");
                    if (rows.length == self.pageSize)
                        rows[rows.length - 1].remove();
                }
                let row = self.populateTopicRow(data);
                row.classList.remove("hidden");
                document.querySelector("#TopicList tbody").prepend(row);
            });
            connection.start()
                .then(function () {
                    return connection.invoke("listenTo", self.forumID);
                });
        }

        recentListen() {
            var connection = new signalR.HubConnectionBuilder().withUrl("/RecentHub").withAutomaticReconnect().build();
            let self = this;
            connection.on("notifyRecentUpdate", function (data: any) {
                var removal = document.querySelector('#TopicList tr[data-topicID="' + data.topicID + '"]');
                if (removal) {
                    removal.remove();
                } else {
                    var rows = document.querySelectorAll("#TopicList tr:not(#TopicTemplate)");
                    if (rows.length == self.pageSize)
                        rows[rows.length - 1].remove();
                }
                var row = self.populateTopicRow(data);
                row.classList.remove("hidden");
                document.querySelector("#TopicList tbody").prepend(row);
            });
            connection.start()
                .then(function () {
                    return connection.invoke("register");
                });
        }

        populateTopicRow = function (data: any) {
            let row = document.querySelector("#TopicTemplate").cloneNode(true) as HTMLElement;
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
            row.querySelector(".lastPostName").innerHTML = data.lastPostName;
            row.querySelector("pf-formattedtime").setAttribute("utctime", data.utc);
            return row;
        };
    }
}