namespace PopForums {

    export class NotificationItem extends HTMLElement {
    constructor(notification: Notification) {
        super();
        this.notification = notification;
    }

    private notification: Notification;

    connectedCallback() {
        this.innerHTML = `<div>userid: ${this.notification.userID}, type: ${this.notification.notificationType}</div>`;
        let timeStamp = new FormattedTime();
        timeStamp.setAttribute("utctime", this.notification.timeStamp);
        this.append(timeStamp);
    }
}

customElements.define('pf-notificationitem', NotificationItem);

}