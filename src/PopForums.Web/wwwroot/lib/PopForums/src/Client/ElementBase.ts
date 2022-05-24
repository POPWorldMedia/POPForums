namespace PopForums {

export abstract class ElementBase extends HTMLElement {
    // Derived class constructor must call super("IDofTemplateHTML") first, or super(null) if markup is generated internally.
    constructor(templateID: string) {
        super();
        if (templateID == null)
            return;
        this.attachShadow({ mode: 'open' });
        var el = document.getElementById(templateID) as HTMLTemplateElement;
        if (!el)
            throw Error(`No template found for ID '${templateID}'. Must pass the ID of the template in constructor to base class, like super('myID');`)
        const template = el.content;
        this.shadowRoot.appendChild(template.cloneNode(true));
    }

    connectedCallback() {
        const attr = this.getAttribute('caller');
        if (!attr)
            throw Error("There is no 'caller' attribute on the component.");
        if (attr.toLowerCase() === "none")
            return;
        const varAndProp = this.parseCallerString(attr);
        const state: StateBase = PopForums[varAndProp[1]];
        const delegate = this.update.bind(this);
        state.subscribe(varAndProp[2], delegate);
    }

    update() {
        const attr = this.getAttribute('caller');
        if (!attr)
            throw Error("There is no 'caller' attribute on the component.");
        const varAndProp = this.parseCallerString(attr);
        const externalValue = PopForums[varAndProp[1]][varAndProp[2]];
        this.updateUI(externalValue);
    }

    private parseCallerString(caller: string): string[] {
        const segments = caller.split(".");
        if (segments.length !== 3 || segments[0] !== "PopForums")
            throw Error("caller attribute must follow 'PopForums.state.property' format.");
        return segments;
    }

    // Use in the implementation to manipulate the shadow or light DOM or straight markup as needed in response to the new data.
    abstract updateUI(data: any): void;
}

}