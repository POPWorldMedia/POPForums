namespace PopForums {

    export class VoteCount extends HTMLElement {
    constructor() {
        super();
    }

    get votes(): string {
        return this.getAttribute("votes");
    }
    set votes(value:string) {
        this.setAttribute("votes", value);
    }

    get postid(): string {
        return this.getAttribute("postid");
    }

    get votescontainerclass(): string {
        return this.getAttribute("votescontainerclass");
    }

    get badgeclass(): string {
        return this.getAttribute("badgeclass");
    }

    get votebuttonclass(): string {
        return this.getAttribute("votebuttonclass");
    }

    get isloggedin(): string {
        return this.getAttribute("isloggedin").toLowerCase();
    }

    get isauthor(): string {
        return this.getAttribute("isauthor").toLowerCase();
    }

    get isvoted(): string {
        return this.getAttribute("isvoted").toLowerCase();
    }

    private badge: HTMLElement;
    private voterContainer: HTMLElement;
    private popover: bootstrap.Popover;
    private popoverEventHander: EventListenerOrEventListenerObject;

    connectedCallback() {
        this.innerHTML = VoteCount.template;
        this.badge = this.querySelector("div");
        this.badge.innerHTML = "+" + this.votes;
        if (this.badgeclass?.length > 0)
            this.badgeclass.split(" ").forEach((c) => this.badge.classList.add(c));
        let statusHtml = this.buttonGenerator();
        if (statusHtml != "") {
            let status = document.createElement("template");
            status.innerHTML = this.buttonGenerator();
            this.append(status.content.firstChild);
        }
        let voteButton = this.querySelector("span");
        if (voteButton) {
            if (this.votebuttonclass?.length > 0)
                this.votebuttonclass.split(" ").forEach((c) => voteButton.classList.add(c));
            type resultType = { votes: number; isVoted: boolean; }
            voteButton.addEventListener("click", () => {
                fetch(PopForums.AreaPath + "/Forum/ToggleVote/" + this.postid, { method: "POST"})
                .then(response => response.json()
                    .then((result: resultType) => {
                        this.votes = result.votes.toString();
                        this.badge.innerHTML = "+" + this.votes;
                        if (result.isVoted) {
                            voteButton.classList.remove("icon-plus-square");
                            voteButton.classList.add("icon-plus-square-fill");
                        }
                        else {
                            voteButton.classList.remove("icon-plus-square-fill");
                            voteButton.classList.add("icon-plus-square");
                        }
                        this.applyPopover();
                    }));
            })
        }
        this.setupVoterPopover();
        this.applyPopover();
    }

    private setupVoterPopover(): void {
        this.voterContainer = document.createElement("div");
        if (this.votescontainerclass?.length > 0)
            this.votescontainerclass.split(" ").forEach((c) => this.voterContainer.classList.add(c));
        this.voterContainer.innerHTML = "Loading...";
        this.popover = new bootstrap.Popover(this.badge, {
            content: this.voterContainer,
            html: true,
            trigger: "click focus"
        });
        this.popoverEventHander = (e) => {
            fetch(PopForums.AreaPath + "/Forum/Voters/" + this.postid)
            .then(response => response.text()
                .then(text => {
                    let t = document.createElement("template");
                    t.innerHTML = text.trim();
                    this.voterContainer.innerHTML = "";
                    this.voterContainer.appendChild(t.content.firstChild);
                }));
        };
        this.badge.addEventListener("shown.bs.popover", this.popoverEventHander);
    }

    private applyPopover(): void {
        if (this.votes === "0") {
            this.badge.style.cursor = "default";
            this.popover.disable();
        }
        else {
            this.badge.style.cursor = "pointer";
            this.popover.enable();
        }
    }

    private buttonGenerator(): string {
        if (this.isloggedin === "false" || this.isauthor === "true")
            return "";
        if (this.isvoted === "true")
            return VoteCount.cancelVoteButton;
        return VoteCount.voteUpButton;
    }

    static template: string = `<div></div>`;

    static voteUpButton = "<span class=\"icon icon-plus-square\"></span>";
    static cancelVoteButton = "<span class=\"icon icon-plus-square-fill\"></span>";
}

customElements.define("pf-votecount", VoteCount);

}