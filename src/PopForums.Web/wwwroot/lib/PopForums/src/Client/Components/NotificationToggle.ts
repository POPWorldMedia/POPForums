namespace PopForums {

    export class NotificationToggle extends ElementBase {
    constructor() {
        super();
    }
    
    get panelid(): string {
        return this.getAttribute("panelid");
    }
    
    private isReady: boolean;
    private panel: HTMLElement;
    private offCanvas: bootstrap.Offcanvas;

    connectedCallback() {
        const delegate = this.ready.bind(this);
        this.isReady = LocalizationService.subscribe(delegate);
        if (this.isReady)
            this.ready();
        super.connectedCallback();
    }

    ready() {
        this.title = PopForums.localizations.notifications;
        this.panel = document.getElementById(this.panelid);
        this.offCanvas = new bootstrap.Offcanvas(this.panel);
        this.addEventListener("click", this.toggle);
    }

    private toggle() {
        this.offCanvas.show();
    }

    getDependentReference(): [StateBase, string] {
        return [PopForums.userState, "notificationCount"];
    }

    updateUI(data: number): void {
        if (data === 0)
            this.innerHTML = `<span class="icon-bell"></span>`;
        else
            this.innerHTML = `<span class="icon-bell"></span><span class="badge ms-1">${data}</span>`;
    }
}

customElements.define('pf-notificationtoggle', NotificationToggle);

}