namespace PopForums {

    export class PreviousPostsButton extends ElementBase {
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
        this.innerHTML = PreviousPostsButton.template;
        let button = this.querySelector("input") as HTMLInputElement;
        button.value = this.buttontext;
        if (this.buttonclass?.length > 0)
            this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
        button.addEventListener("click", (e: MouseEvent) => {
            PopForums.currentTopicState.loadPreviousPosts();
        });
        super.connectedCallback();
    }
    
    updateUI(data: number): void {
        let button = this.querySelector("input");
        if (PopForums.currentTopicState.pageCount === 1 || data === 1)
            button.style.visibility = "hidden";
        else
            button.style.visibility = "visible";
    }

    static template: string = `<input type="button" />`;
}

customElements.define('pf-previouspostsbutton', PreviousPostsButton);

}