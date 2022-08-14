namespace PopForums{

export class PrivateMessageState extends StateBase {
    constructor() {
        super();
    }

    pmID: number;
    users: PrivateMessageUser[];
    messages: PrivateMessage[];

    private postStream: HTMLElement;
    private connection: any;

    setupPm() {
        PopForums.Ready(() => {
            this.postStream = document.getElementById("PostStream");
            this.messages.forEach(x => {
                let messageRow = this.populateMessage(x);
                this.postStream.append(messageRow);
            });
            
            this.connection = new signalR.HubConnectionBuilder().withUrl("/PMHub").withAutomaticReconnect().build();
            let self = this;
            this.connection.on("addMessage", function(message: PrivateMessage) {
                let messageRow = self.populateMessage(message);
                let parent = self.postStream.parentElement;
                let isBottom = parent.scrollHeight - parent.scrollTop - parent.clientHeight < 1;
                self.postStream.append(messageRow);
                if (isBottom)
                    (self.postStream.lastChild as HTMLElement).scrollIntoView(true);
                self.ackRead();
            });
            this.connection.start()
                .then(function () {
                    return self.connection.invoke("listenTo", self.pmID);
                });
        });
    }

    send(fullText: string) {
        if (!fullText || fullText.trim().length === 0)
            return;
        this.connection.invoke("send", this.pmID, fullText);
    }

    ackRead() {
        this.connection.invoke("ackRead", this.pmID);
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
        timeStamp.setAttribute("utctime", data.postTime);

        let name = messageRow.querySelector(".messageName");
        name.innerHTML = data.name;

        return messageRow;
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