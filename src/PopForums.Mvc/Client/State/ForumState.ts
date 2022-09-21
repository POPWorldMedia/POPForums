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
            PopForums.Ready(async () => {
                this.isNewTopicLoaded = false;
                await this.forumListen();
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

        async forumListen() {
            let service = await MessagingService.GetService();
            let connection = service.connection;
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
            await connection.invoke("listenToForum", self.forumID);
        }

        async recentListen() {
            let service = await MessagingService.GetService();
            let connection = service.connection;
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
            connection.invoke("listenRecent");
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