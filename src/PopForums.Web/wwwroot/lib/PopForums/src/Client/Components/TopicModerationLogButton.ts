namespace PopForums {

    export class TopicModerationLogButton extends HTMLElement {
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

    connectedCallback() {
        this.innerHTML = TopicModerationLogButton.template;
        let button = this.querySelector("input");
        button.value = this.buttontext;
        let classes = this.buttonclass;
        if (classes?.length > 0)
            classes.split(" ").forEach((c) => button.classList.add(c));
        button.addEventListener("click", () => {
            let container = this.querySelector("div");
            if (container.style.display !== "block")
                fetch(PopForums.AreaPath + "/Moderator/TopicModerationLog/" + this.topicid)
                    .then(response => response.text()
                        .then(text => {
                            container.innerHTML = text;
                            container.style.display = "block";
                        }));
            else container.style.display = "none";
        });
    }

    static template: string = `<input class="btn btn-primary" type="button" />
    <div class="mt-3"></div>`;
}

customElements.define("pf-topicmoderationlogbutton", TopicModerationLogButton);

}