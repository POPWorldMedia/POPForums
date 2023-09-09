namespace PopForums {

export abstract class ElementBase<B extends StateBase> extends HTMLElement {

    connectedCallback() {
        if (this.state && this.propertyToWatch)
            return;
        let stateAndWatchProperty = this.getDependentReference();
        this.state = stateAndWatchProperty[0];
        this.propertyToWatch = stateAndWatchProperty[1];
        const delegate = this.update.bind(this);
        this.state.subscribe(this.propertyToWatch, delegate);
    }

    private state: B;
    private propertyToWatch: string;

    update() {
        const externalValue = this.state[this.propertyToWatch as keyof B];
        this.updateUI(externalValue);
    }

    // Implementation should return the StateBase and property (as a string) to monitor
    abstract getDependentReference(): [B, string];

    // Use in the implementation to manipulate the shadow or light DOM or straight markup as needed in response to the new data.
    abstract updateUI(data: any): void;
}

}