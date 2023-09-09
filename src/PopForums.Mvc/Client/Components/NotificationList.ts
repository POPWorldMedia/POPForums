namespace PopForums {

    export class NotificationList extends ElementBase<UserState> {
    constructor() {
        super();
    }

    connectedCallback() {
        super.connectedCallback();
    }

    getDependentReference(): [UserState, string] {
        return [PopForums.userState, "notifications"];
    }

    updateUI(data: Array<Notification>): void {
        if (!data || data.length === 0) {
            this.replaceChildren();
            return;
        }
        data.forEach(item => {
            let n = new NotificationItem(item);
            this.append(n);
        });
    }
}

customElements.define('pf-notificationlist', NotificationList);

}