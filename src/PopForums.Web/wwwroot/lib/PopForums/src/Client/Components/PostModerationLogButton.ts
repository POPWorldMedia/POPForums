namespace PopForums {

    export class PostModerationLogButton extends HTMLElement {
    constructor() {
        super();
    }

    get buttonclass(): string {
        return this.getAttribute("buttonclass");
    }

    get buttontext(): string {
        return this.getAttribute("buttontext");
    }

    get postid(): string {
        return this.getAttribute("postid");
    }

    get parentSelectorToAppendTo(): string {
        return this.getAttribute("parentselectortoappendto");
    }

    connectedCallback() {
        this.innerHTML = PostModerationLogButton.template;
        let button = this.querySelector("input");
        button.value = this.buttontext;
        let classes = this.buttonclass;
        if (classes?.length > 0)
            classes.split(" ").forEach((c) => button.classList.add(c));
        let self = this;
        let container: HTMLDivElement;
        button.addEventListener("click", () => {
            if (!container) {
                let parentContainer = self.closest(this.parentSelectorToAppendTo);
                if (!parentContainer) {
                    console.error(`Can't find a parent selector "${this.parentSelectorToAppendTo}" to append post moderation log to.`);
                    return;
                }
                container = document.createElement("div");
                parentContainer.appendChild(container);
            }
            if (container.style.display !== "block")
                fetch(PopForums.AreaPath + "/Moderator/PostModerationLog/" + this.postid)
                    .then(response => response.text()
                        .then(text => {
                            container.innerHTML = text;
                            container.style.display = "block";
                        }));
            else container.style.display = "none";
        });
    }

    static template: string = `<input type="button" />`;
}

customElements.define("pf-postmoderationlogbutton", PostModerationLogButton);

}