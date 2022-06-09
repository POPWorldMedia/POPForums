namespace PopForums {

    export class FeedUpdater extends HTMLElement {
        constructor() {
            super();
        }

        get templateid() {
            return this.getAttribute("templateid");
        }

        connectedCallback() {
            let connection = new signalR.HubConnectionBuilder().withUrl("/FeedHub").withAutomaticReconnect().build();
            let self = this;
            connection.on("notifyFeed", function (data: any) {
                let list = document.querySelector("#FeedList");
                let row = self.populateFeedRow(data);
                list.prepend(row);
                row.classList.remove("hidden");
            });
            connection.start()
                .then(function () {
                    return connection.invoke("listenToAll");
                });
        }

        populateFeedRow(data: any) {
            let template = document.getElementById(this.templateid);
            if (!template) {
                console.error(`Can't find ID ${this.templateid} to make feed updates.`);
                return;
            }
            let row = template.cloneNode(true) as HTMLElement;
            row.removeAttribute("id");
            row.querySelector(".feedItemText").innerHTML = data.message;
            row.querySelector("pf-formattedtime").setAttribute("utctime", data.utc);
            return row;
        };
    }
    
    customElements.define('pf-feedupdater', FeedUpdater);
}