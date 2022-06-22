namespace PopForums {

    export class NotificationItem extends HTMLElement {
    constructor(notification: Notification) {
        super();
        this.notification = notification;
    }

    private notification: Notification;

    connectedCallback() {
        let markup: string;
        let link: string;
        switch (this.notification.notificationType) {
            case "Award":
                markup = `New award: <b>${this.notification.data.title}</b>`;
                link = "/Forums/Account/ViewProfile/" + this.notification.userID + "#Awards";
                break;
            case "QuestionAnswered":
                markup = `<b>${this.notification.data.askerName}</b> chose an answer for the question: <b>${this.notification.data.title}</b>`;
                link = "/Forums/PostLink/" + this.notification.data.postID;
                break;
            case "NewReply":
                markup = `<b>${this.notification.data.postName}</b> made a post in the topic: <b>${this.notification.data.title}</b>`;
                link = "/Forums/GoToNewestPost/" + this.notification.data.topicID;
                break;
            case "VoteUp":
                markup = `<b>${this.notification.data.voterName}</b> voted for a post in <b>${this.notification.data.title}</b>`;
                link = "/Forums/PostLink/" + this.notification.data.postID;
                break;
            default:
                console.log(`Unknown notification type: ${this.notification.notificationType}`);
        }
        let newness = "";
        if (!this.notification.isRead)
            newness = " text-bg-light border-primary";
        let template = `<div class="card mb-3${newness}">
    <div class="card-body">
        <p>${markup}</p>
    </div>
    <div class="card-footer text-end">
    </div>
    <a href="${link}" class="stretched-link"></a>
</div>`;
        this.innerHTML = template;
        let timeStamp = new FormattedTime();
        timeStamp.setAttribute("utctime", this.notification.timeStamp);
        let footer = this.querySelector(".card-footer");
        footer.append(timeStamp);
    }

}

customElements.define('pf-notificationitem', NotificationItem);

}