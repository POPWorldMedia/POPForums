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
            let selection = document.getSelection();
            if (selection.rangeCount === 0) {
                // prompt to select
                return;
            }
            let fragment = selection.getRangeAt(0).cloneContents();
            let div = document.createElement("div");
            div.appendChild(fragment);
            console.log(div.innerHTML);
            let container = div.querySelector("#" + this.containerid);
            if (container !== null && container !== undefined) {
                div.innerHTML = container.innerHTML;
            }
            selection.removeAllRanges();
            console.log(`${this.name} + ${this.containerid}
${div.innerHTML}`);
        };
    }

    updateUI(data: any): void {

    }
}

customElements.define('pf-quotebutton', QuoteButton);