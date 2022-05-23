namespace PopForums {

    export class PMCount extends ElementBase {
    constructor() {
        super(null);
    }
    updateUI(data: any): void {
        if (data === 0)
            this.innerHTML = "";
        else
            this.innerHTML = `<span class="badge">${data}</span>`;
    }
}

customElements.define('pf-pmcount', PMCount);

}