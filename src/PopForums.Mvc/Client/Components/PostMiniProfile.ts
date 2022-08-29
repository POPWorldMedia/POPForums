namespace PopForums {

    export class PostMiniProfile extends HTMLElement {
    constructor() {
        super();
    }

    get username(): string {
        return this.getAttribute("username");
    }
    get usernameclass(): string {
        return this.getAttribute("usernameclass");
    }
    get userid(): string {
        return this.getAttribute("userid");
    }
    get miniProfileBoxClass(): string {
        return this.getAttribute("miniprofileboxclass");
    }

    private isOpen: boolean;
    private box: HTMLElement;
    private boxHeight: string;
    private isLoaded: boolean;

    connectedCallback() {
        this.isLoaded = false;
        this.innerHTML = PostMiniProfile.template;
        let nameHeader = this.querySelector("h3") as HTMLElement;
        this.usernameclass.split(" ").forEach((c) => nameHeader.classList.add(c));
        nameHeader.innerHTML = this.username;
        nameHeader.addEventListener("click", () => {
            this.toggle();
        });
        this.box = this.querySelector("div");
        this.miniProfileBoxClass.split(" ").forEach((c) => this.box.classList.add(c));
    }

    private toggle() {
        if (!this.isLoaded) {
            fetch(PopForums.AreaPath + "/Account/MiniProfile/" + this.userid)
                .then(response => response.text()
                    .then(text => {
                        let sub = this.box.querySelector("div");
                        sub.innerHTML = text;
                        const height = sub.getBoundingClientRect().height;
                        this.boxHeight = `${height}px`;
                        this.box.style.height = this.boxHeight;
                        this.isOpen = true;
                        this.isLoaded = true;
                    }));
        }
        else if (!this.isOpen) {
            this.box.style.height = this.boxHeight;
            this.isOpen = true;
        }
        else {
            this.box.style.height = "0";
            this.isOpen = false;
        }
    }

    static template: string = `<h3></h3>
<div>
    <div class="py-1 px-3 mb-2"></div>
</div>`;
}

customElements.define('pf-postminiprofile', PostMiniProfile);

}