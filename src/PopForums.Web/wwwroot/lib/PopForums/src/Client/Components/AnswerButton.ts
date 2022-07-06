namespace PopForums {

    export class AnswerButton extends ElementBase {
        constructor() {
            super();
        }

        get answerstatusclass(): string {
            return this.getAttribute("answerstatusclass");
        }
        get chooseanswertext(): string {
            return this.getAttribute("chooseanswertext");
        }
        get topicid(): string {
            return this.getAttribute("topicid");
        }
        get postid(): string {
            return this.getAttribute("postid");
        }
        get answerpostid(): string {
            return this.getAttribute("answerpostid");
        }
        get userid(): string {
            return this.getAttribute("userid");
        }
        get startedbyuserid(): string {
            return this.getAttribute("startedbyuserid");
        }
        get isfirstintopic(): string {
            return this.getAttribute("isfirstintopic");
        }

        private button: HTMLElement;

        connectedCallback() {
            this.button = document.createElement("p");
            this.button.classList.add("icon");
            this.answerstatusclass.split(" ").forEach((c) => this.button.classList.add(c));
            if (this.isfirstintopic.toLowerCase() === "false" && this.userid === this.startedbyuserid) {
                // make it a button for author
                this.button.addEventListener("click", () => {
                    PopForums.currentTopicState.setAnswer(Number(this.postid), Number(this.topicid));
                });
            }
            this.appendChild(this.button);
            super.connectedCallback();
        }

        getDependentReference(): [StateBase, string] {
            return [PopForums.currentTopicState, "answerPostID"];
        }
        
        updateUI(answerPostID: number): void {
            if (this.isfirstintopic.toLowerCase() === "false" && this.userid === this.startedbyuserid) {
                // this is question author
                this.button.classList.add("asnswerButton", "fs-1", "my-3");
                if (answerPostID && this.postid === answerPostID.toString()) {
                    this.button.classList.remove("icon-check-circle");
                    this.button.classList.remove("text-muted");
                    this.button.classList.add("icon-check-circle-fill");
                    this.button.classList.add("text-success");
                    this.style.cursor = "default";
                }
                else {
                    this.button.classList.remove("icon-check-circle-fill");
                    this.button.classList.remove("text-success");
                    this.button.classList.add("icon-check-circle");
                    this.button.classList.add("text-muted");
                    this.style.cursor = "pointer";
                }
            }
            else if (answerPostID && this.postid === answerPostID.toString()) {
                // not the question author, but it is the answer
                this.button.classList.add("icon-check-circle-fill");
                this.button.classList.add("text-success");
                this.style.cursor = "default";
            }
        }
}

customElements.define('pf-answerbutton', AnswerButton);

}