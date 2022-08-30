namespace PopForums {

    export class CommentButton extends ElementBase {
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

        connectedCallback() {
            this.innerHTML = CommentButton.template;
            let button = this.querySelector("button") as HTMLButtonElement;
            button.title = this.buttontext;
            if (this.buttonclass?.length > 0)
                this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
            if (button.classList.contains("btn"))
                button.innerText = this.buttontext;
            button.addEventListener("click", (e: MouseEvent) => {
                PopForums.currentTopicState.loadComment(Number(this.topicid), Number(this.postid));
            });
            super.connectedCallback();
        }

        getDependentReference(): [StateBase, string] {
            return [PopForums.currentTopicState, "commentReplyID"];
        }
        
        updateUI(data: number): void {
            let button = this.querySelector("button");
            if (data !== undefined) {
                button.disabled = true;
                button.style.cursor = "default";
            }
            else
                button.disabled = false;
        }

        static template: string = `<button type="button"></button>`;
}

customElements.define('pf-commentbutton', CommentButton);

}