namespace PopForums {

    export class TopicButton extends ElementBase {
    constructor() {
        super();
    }

    get buttonclass(): string {
        return this.getAttribute("buttonclass");
    }

    get buttontext(): string {
        return this.getAttribute("buttontext");
    }

    get forumid(): string {
        return this.getAttribute("forumid");
    }

    connectedCallback() {
        this.innerHTML = TopicButton.template;
        let button = this.querySelector("input") as HTMLInputElement;
        button.value = this.buttontext;
        if (this.buttonclass?.length > 0)
            this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
        button.addEventListener("click", () => {
            currentForumState.loadNewTopic();
        });
        super.connectedCallback();
    }
    
    updateUI(data: boolean): void {
        if (data)
            this.style.display = "none";
        else
            this.style.display = "initial";
    }

    static template: string = `<input type="button" />`;
}

customElements.define('pf-topicbutton', TopicButton);

}