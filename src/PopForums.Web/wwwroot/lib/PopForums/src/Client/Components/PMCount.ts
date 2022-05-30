namespace PopForums {

    export class PMCount extends ElementBase {
    constructor() {
        super();
    }
    updateUI(data: number): void {
        if (data === 0)
            this.innerHTML = "";
        else
            this.innerHTML = `<span class="badge">${data}</span>`;
    }
}

customElements.define('pf-pmcount', PMCount);

}