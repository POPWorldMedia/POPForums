namespace PopForums {

export abstract class ElementBase extends HTMLElement {

    connectedCallback() {
        if (this.state && this.propertyToWatch)
            return;
        let stateAndWatchProperty = this.getDependentReference();
        this.state = stateAndWatchProperty[0];
        this.propertyToWatch = stateAndWatchProperty[1];
        const delegate = this.update.bind(this);
        this.state.subscribe(this.propertyToWatch, delegate);
    }

    private state: StateBase;
    private propertyToWatch: string;

    update() {
        const externalValue = this.state[this.propertyToWatch];
        this.updateUI(externalValue);
    }

    // Implementation should return the StateBase and property (as a string) to monitor
    abstract getDependentReference(): [StateBase, string];

    // Use in the implementation to manipulate the shadow or light DOM or straight markup as needed in response to the new data.
    abstract updateUI(data: any): void;
}

}