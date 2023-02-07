namespace PopForums {

export class PMCount extends ElementBase {
    constructor() {
        super();
        this.isInit = false;
    }
    
    private isInit: boolean;

    getDependentReference(): [StateBase, string] {
        return [PopForums.userState, "newPmCount"];
    }

    updateUI(data: number): void {
        if (data === 0)
            this.innerHTML = "";
        else {
            this.innerHTML = `<span class="badge">${data}</span>`;
            if (this.isInit)
                this.innerHTML = `<span class="badge explode">${data}</span>`;
        }
        this.isInit = true;
    }
}

customElements.define('pf-pmcount', PMCount);

}