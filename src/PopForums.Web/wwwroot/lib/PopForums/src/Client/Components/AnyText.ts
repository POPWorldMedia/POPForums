namespace PopForums {

    export class AnyText extends ElementBase {
    constructor() {
        super(null);
    }
    updateUI(data: any): void {
        this.textContent = data;
    }
}

customElements.define('pf-anytext', AnyText);

}