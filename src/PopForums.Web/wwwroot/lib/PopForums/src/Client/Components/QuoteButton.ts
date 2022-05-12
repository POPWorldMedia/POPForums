class QuoteButton extends ElementBase {
    constructor() {
        super(null);
    }

    get name(): string {
        return this.getAttribute("name");
    }
    get containerid(): string {
        return this.getAttribute("containerid");
    }

    connectedCallback() {
        this.innerHTML = `<input type="button" />`;
        let button = this.querySelector("input");
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