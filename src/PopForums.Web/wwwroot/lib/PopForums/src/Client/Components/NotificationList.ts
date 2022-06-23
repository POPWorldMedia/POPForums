namespace PopForums {

    export class NotificationList extends ElementBase {
    constructor() {
        super();
    }

    connectedCallback() {
        super.connectedCallback();
    }

    getDependentReference(): [StateBase, string] {
        return [PopForums.userState, "notifications"];
    }

    updateUI(data: Array<Notification>): void {
        this.replaceChildren();
        data.forEach(item => {
            let n = new NotificationItem(item);
            this.append(n);
        });
    }
}

customElements.define('pf-notificationlist', NotificationList);

}