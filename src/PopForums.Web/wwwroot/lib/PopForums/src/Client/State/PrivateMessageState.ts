namespace PopForums{

export class PrivateMessageState extends StateBase {
    constructor() {
        super();
    }

    pmID: number;
    users: PrivateMessageUser[];
    messages: PrivateMessage[];

    private postStream: HTMLElement;

    setupPm() {
        PopForums.Ready(() => {
            this.postStream = document.getElementById("PostStream");
            this.messages.forEach(x => {
                var messageRow = this.populateMessage(x);
                this.postStream.append(messageRow);
            });
        });
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