namespace PopForums {

export class AnyBool extends ElementBase {
    constructor() {
        super(null);
    }

    private markup: HTMLElement;

    private template: HTMLTemplateElement;

    connectedCallback(): void {
        super.connectedCallback();
        this.template = document.createElement("template");
        this.template.innerHTML = `<slot name="markup">`;
        this.attachShadow({ mode: "open" });
        this.shadowRoot.append(this.template.content.cloneNode(true));
        this.markup = this.shadowRoot.querySelector("slot[name='markup']");
    }

    updateUI(data: any): void {
        if (data as boolean !== true)
            this.markup.style.display = "none";
        else
            this.markup.style.display = this.style.display;
    }
}

customElements.define('pf-anybool', AnyBool);

}