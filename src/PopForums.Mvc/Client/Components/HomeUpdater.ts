namespace PopForums {

    export class HomeUpdater extends HTMLElement {
        constructor() {
            super();
        }

        async connectedCallback() {
            let service = await MessagingService.GetService();
            let connection = service.connection;
            let self = this;
            connection.on("notifyForumUpdate", function (data: any) {
                self.updateForumStats(data);
            });
            await connection.invoke("listenToAllForums");
        }

        updateForumStats(data: any) {
            let row = document.querySelector("[data-forumid='" + data.forumID + "']");
            row.querySelector(".topicCount").innerHTML = data.topicCount;
            row.querySelector(".postCount").innerHTML = data.postCount;
            row.querySelector(".lastPostName").innerHTML = data.lastPostName;
            row.querySelector("pf-formattedtime").setAttribute("utctime", data.utc);
            row.querySelector(".newIndicator .icon").classList.remove("text-muted", "icon-file-earmark-text");
            row.querySelector(".newIndicator .icon").classList.add("text-warning", "icon-file-earmark-text-fill");
        };
    }
    
    customElements.define('pf-homeupdater', HomeUpdater);
}