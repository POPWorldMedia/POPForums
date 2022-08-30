namespace PopForums {

    export class MorePostsButton extends ElementBase {
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
        this.innerHTML = MorePostsButton.template;
        let button = this.querySelector("input") as HTMLInputElement;
        button.value = this.buttontext;
        if (this.buttonclass?.length > 0)
            this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
        button.addEventListener("click", (e: MouseEvent) => {
            PopForums.currentTopicState.loadMorePosts();
        });
        super.connectedCallback();
    }

    getDependentReference(): [StateBase, string] {
        return [PopForums.currentTopicState, "highPage"];
    }
    
    updateUI(data: number): void {
        let button = this.querySelector("input");
        if (PopForums.currentTopicState.pageCount === 1 || data === PopForums.currentTopicState.pageCount)
            button.style.visibility = "hidden";
        else
            button.style.visibility = "visible";
    }

    static template: string = `<input type="button" />`;
}

customElements.define('pf-morepostsbutton', MorePostsButton);

}