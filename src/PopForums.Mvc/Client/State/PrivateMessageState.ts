namespace PopForums{

export class PrivateMessageState extends StateBase {
    constructor() {
        super();
        this.isStart = false;
    }

    pmID: number;
    users: PrivateMessageUser[];
    messages: PrivateMessage[];
    newestPostID: number;

    private postStream: HTMLElement;
    private connection: any;
    private isStart: boolean;

    setupPm() {
        PopForums.Ready(async () => {
            this.postStream = document.getElementById("PostStream");
            this.messages.forEach(x => {
                let messageRow = this.populateMessage(x);
                this.postStream.append(messageRow);
            });

            let service = await MessagingService.GetService();
            this.connection = service.connection;

            let self = this;
            this.connection.on("addMessage", function(message: PrivateMessage) {
                let messageRow = self.populateMessage(message);
                let parent = self.postStream.parentElement;
                let isBottom = parent.scrollHeight - parent.scrollTop - parent.clientHeight < 200;
                self.postStream.append(messageRow);
                if (isBottom)
                    parent.scrollTop = parent.scrollHeight;
                self.ackRead();
            });
            this.connection.onreconnected(async () => {
                let latestPostTime = this.messages[this.messages.length - 1].pmPostID;
                const posts = await this.connection.invoke("GetMostRecentPmPosts", this.pmID, latestPostTime) as PrivateMessage[];
                posts.reverse().forEach((item: PrivateMessage) => {
                    let m = this.populateMessage(item);
                    this.postStream.append(m);
                });
            });
            await this.connection.invoke("listenToPm", this.pmID);
            if (this.newestPostID) {
                this.scrollToElement("p" + this.newestPostID)
            }
            else {
                this.postStream.parentElement.scrollTop = this.postStream.parentElement.scrollHeight;
            }
            await this.LoadCheck();
            this.postStream.parentElement.addEventListener("scroll", this.ScrollLoad);
        });
    }

    ScrollLoad = async () => {
        await this.LoadCheck();
    }

    async LoadCheck() {
        let box = this.postStream.parentElement;
        if (!this.isStart && box.scrollTop < 250) {
            const posts = await this.GetPosts();
            let isStart = true;
            posts.reverse().forEach((item: PrivateMessage) => {
                this.messages.unshift(item);
                let m = this.populateMessage(item);
                this.postStream.prepend(m);
                isStart = false;
            });
            this.isStart = isStart;
        }
    }

    private async GetPosts() {
        let earliestPostTime = this.messages[0].postTime;
        const response = await this.connection.invoke("GetPmPosts", this.pmID, earliestPostTime) as PrivateMessage[];
        return response;
    }

    send(fullText: string) {
        if (!fullText || fullText.trim().length === 0)
            return;
        this.connection.invoke("sendPm", this.pmID, fullText);
    }

    ackRead() {
        this.connection.invoke("ackReadPm", this.pmID);
    }

    populateMessage(data: PrivateMessage) {
        let template = document.createElement("template");
        template.innerHTML = PrivateMessageState.template;
        let messageRow = template.content.cloneNode(true) as HTMLElement;

        let body = messageRow.querySelector("div > div");
        body.innerHTML = data.fullText;
        if (data.userID === PopForums.userState.userID) {
            body.classList.add("alert-secondary");
            messageRow.querySelector("div").classList.add("ms-auto");
        }
        else
            body.classList.add("alert-primary");

        let timeStamp = messageRow.querySelector("pf-formattedtime");
        timeStamp.setAttribute("utctime", data.postTime.toString());

        let name = messageRow.querySelector(".messageName");
        name.innerHTML = data.name;

        body.parentElement.id = "p" + data.pmPostID;

        return messageRow;
    };

    scrollToElement = (id: string) => {
        let e = document.getElementById(id) as HTMLElement;
        e.scrollIntoView();
    };

    static template: string = 
`<div class="w-75 mb-3">
    <span class="d-flex">
        <small class="messageName me-3"></small>
        <small class="ms-auto"><pf-formattedtime utctime=""></pf-formattedtime></small>
    </span>
    <div class="alert">

    </div>
</div>`;
}

}