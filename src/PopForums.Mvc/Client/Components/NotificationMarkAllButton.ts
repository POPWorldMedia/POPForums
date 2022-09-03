namespace PopForums {

    export class NotificationMarkAllButton extends HTMLElement {
    constructor() {
        super();
    }

    get buttonclass(): string {
        return this.getAttribute("buttonclass");
    }
    get buttontext(): string {
        return this.getAttribute("buttontext");
    }

    connectedCallback() {
        this.innerHTML = `<input type="button" class="${this.buttonclass}" value="${this.buttontext}" data-bs-dismiss="offcanvas" />`;
        this.querySelector("input").addEventListener("click", () => {
            PopForums.userState.MarkAllRead();
        });
    }
}

customElements.define('pf-notificationmarkallbutton', NotificationMarkAllButton);

}