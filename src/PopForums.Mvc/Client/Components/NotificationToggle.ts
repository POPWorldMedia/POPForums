namespace PopForums {

    export class NotificationToggle extends ElementBase {
    constructor() {
        super();
        this.userState = PopForums.userState;
        this.isInit = false;
    }
    
    get panelid(): string {
        return this.getAttribute("panelid");
    }
    
    get notificationlistid(): string {
        return this.getAttribute("notificationlistid");
    }
    
    private userState: UserState;
    private isReady: boolean;
    private panel: HTMLElement;
    private offCanvas: bootstrap.Offcanvas;
    private isInit: boolean;

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
        this.addEventListener("click", this.show);
        let list = document.getElementById(this.notificationlistid);
        this.userState.list = list;
    }

    private async show() {
        this.offCanvas.show();
        this.panel.addEventListener("hide.bs.offcanvas", event => {
            this.userState.list.removeEventListener("scroll", this.userState.ScrollLoad);
        });
        await this.userState.LoadNotifications();
        this.userState.list.addEventListener("scroll", this.userState.ScrollLoad);
    }

    getDependentReference(): [StateBase, string] {
        return [PopForums.userState, "notificationCount"];
    }

    updateUI(data: number): void {
        if (data === 0)
            this.innerHTML = `<span class="icon icon-bell-fill"></span>`;
        else {
            this.innerHTML = `<span class="icon icon-bell-fill"></span><span class="badge ms-1">${data}</span>`;
            if (this.isInit) {
                this.innerHTML = `<span class="icon icon-bell-fill"></span><span class="badge ms-1 explode">${data}</span>`;
            }
        }
        this.isInit = true;
    }
}

customElements.define('pf-notificationtoggle', NotificationToggle);

}