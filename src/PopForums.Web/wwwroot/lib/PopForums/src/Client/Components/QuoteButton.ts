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
            let range = selection.getRangeAt(0);
            let fragment = range.cloneContents();
            let div = document.createElement("div");
            div.appendChild(fragment);
            // is selection in the container?
            let ancestor = range.commonAncestorContainer;
            while (ancestor['id'] !== this.containerid && ancestor.parentElement !== null) {
                ancestor = ancestor.parentElement;
            }
            let isInText = ancestor['id'] === this.containerid;
            // if not, is it partially in the container?
            if (!isInText) {
                let container = div.querySelector("#" + this.containerid);
                if (container !== null && container !== undefined) {
                    // it's partially in the container, so just get that part
                    div.innerHTML = container.innerHTML;
                    isInText = true;
                }
            }
            selection.removeAllRanges();
            console.log("isChildOfText: " + isInText);
            console.log(`${this.name} + ${this.containerid}
${div.innerHTML}`);
            if (isInText) {
                // activate or add to quote
            }
        };
    }

    updateUI(data: any): void {

    }
}

customElements.define('pf-quotebutton', QuoteButton);