class QuoteButton extends ElementBase {
    constructor() {
        super(null);
        let template = document.createElement("template");
        template.innerHTML = `<input type="button" />`;
        var shadow = this.attachShadow({ mode: "open" });
        shadow.append(template.content.cloneNode(true));
    }

    get name(): string {
        return this.getAttribute("name");
    }
    get containerid(): string {
        return this.getAttribute("containerid");
    }
    get buttonclass(): string {
        return this.getAttribute("buttonclass");
    }

    connectedCallback() {
        let button = this.shadowRoot.querySelector("input");
        button.value = this.getAttribute("value");
        let classes = this.getAttribute("buttonclass");
        if (classes?.length > 0)
            classes.split(" ").forEach((c) => button.classList.add(c));
        this.onclick = (e: MouseEvent) => {
            console.log(`${this.name} + ${this.containerid}`);
        };
    }

    updateUI(data: any): void {

    }
}

customElements.define('pf-quotebutton', QuoteButton);