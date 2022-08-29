namespace PopForums {

    export class ReplyButton extends ElementBase {
    constructor() {
        super();
    }

    get buttonclass(): string {
        return this.getAttribute("buttonclass");
    }

    get buttontext(): string {
        return this.getAttribute("buttontext");
    }
    
    get topicid(): string {
        return this.getAttribute("topicid");
    }
    
    get postid(): string {
        return this.getAttribute("postid");
    }
    
    get overridedisplay(): string {
        return this.getAttribute("overridedisplay");
    }

    connectedCallback() {
        this.innerHTML = ReplyButton.template;
        let button = this.querySelector("button") as HTMLButtonElement;
        button.title = this.buttontext;
        if (this.buttonclass?.length > 0)
            this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
        if (button.classList.contains("btn"))
            button.innerText = this.buttontext;
        button.addEventListener("click", (e: MouseEvent) => {
            PopForums.currentTopicState.loadReply(Number(this.topicid), Number(this.postid), true);
        });
        super.connectedCallback();
    }

    getDependentReference(): [StateBase, string] {
        return [PopForums.currentTopicState, "isReplyLoaded"];
    }
    
    updateUI(data: boolean): void {
        if (this.overridedisplay?.toLowerCase() === "true")
            return;
        let button = this.querySelector(":first-child") as HTMLElement;
        if (data)
            button.style.display = "none";
        else
            button.style.display = "initial";
    }

    static template: string = `<button type="button"></button>`;
}

customElements.define('pf-replybutton', ReplyButton);

}