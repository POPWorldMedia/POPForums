namespace PopForums {

    export class MorePostsBeforeReplyButton extends ElementBase<TopicState> {
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
        this.innerHTML = MorePostsBeforeReplyButton.template;
        let button = this.querySelector("input") as HTMLInputElement;
        button.value = this.buttontext;
        if (this.buttonclass?.length > 0)
            this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
        button.addEventListener("click", (e: MouseEvent) => {
            PopForums.currentTopicState.loadMorePosts();
        });
        super.connectedCallback();
    }

    getDependentReference(): [TopicState, string] {
        return [PopForums.currentTopicState, "isNewerPostsAvailable"];
    }
    
    updateUI(data: boolean): void {
        let button = this.querySelector("input");
        if (!data)
            button.style.visibility = "hidden";
        else
            button.style.visibility = "visible";
    }

    static template: string = `<input type="button" />`;
}

customElements.define('pf-morepostsbeforereplybutton', MorePostsBeforeReplyButton);

}