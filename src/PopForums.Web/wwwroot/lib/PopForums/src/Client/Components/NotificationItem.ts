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
            case 3: // Award
                markup = `${PopForums.localizations.award}: <b>${this.notification.data.title}</b>`;
                link = "/Forums/Account/ViewProfile/" + this.notification.userID + "#Awards";
                break;
            case 2: // QuestionAnswered
                markup = PopForums.localizations.questionAnsweredNotification
                    .replace("{0}", this.notification.data.askerName)
                    .replace("{1}", this.notification.data.title);
                link = "/Forums/PostLink/" + this.notification.data.postID;
                break;
            case 0: // NewReply
                markup = PopForums.localizations.newReplyNotification
                    .replace("{0}", this.notification.data.postName)
                    .replace("{1}", this.notification.data.title);
                link = "/Forums/Forum/GoToNewestPost/" + this.notification.data.topicID;
                break;
            case 1: // VoteUp
                markup = PopForums.localizations.voteUpNotification
                    .replace("{0}", this.notification.data.voterName)
                    .replace("{1}", this.notification.data.title);
                link = "/Forums/PostLink/" + this.notification.data.postID;
                break;
            default:
                console.log(`Unknown notification type: ${this.notification.notificationType}`);
        }
        let newness = " border border-2";
        if (!this.notification.isRead)
            newness = " text-bg-light border-primary border border-2";
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

        this.querySelector("a").addEventListener("click", (e) => {
            PopForums.userState.MarkRead(this.notification.contextID, this.notification.notificationType);
        });
    }

    MarkRead() {
        let box = this.querySelector(".card");
        if (box) {
            box.classList.remove("text-bg-light", "border-primary");
        }
    }

}

customElements.define('pf-notificationitem', NotificationItem);

}