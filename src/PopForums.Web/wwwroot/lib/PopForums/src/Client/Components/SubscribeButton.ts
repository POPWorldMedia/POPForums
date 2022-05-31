namespace PopForums {

    export class SubscribeButton extends ElementBase {
    constructor() {
        super();
    }

    get buttonclass(): string {
        return this.getAttribute("buttonclass");
    }
    
    get subscribetext(): string {
        return this.getAttribute("subscribetext");
    }
    get unsubscribetext(): string {
        return this.getAttribute("unsubscribetext");
    }

    connectedCallback() {
        this.innerHTML = SubscribeButton.template;
        let button: HTMLInputElement = this.querySelector("input");
        this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
        button.addEventListener("click", () => {
            fetch(PopForums.AreaPath + "/Subscription/ToggleSubscription/" + PopForums.currentTopicState.topicID, {
                method: "POST"
            })
                .then(response => response.json())
                .then(result => {
                    switch (result.data.isSubscribed) {
                        case true:
                            PopForums.currentTopicState.isSubscribed = true;
                            break;
                        case false:
                            PopForums.currentTopicState.isSubscribed = false;
                            break;
                        default:
                            // TODO: something else
                    }
                })
                .catch(() => {
                    // TODO: handle error
                });
        });
        super.connectedCallback();
    }

    getDependentReference(): [StateBase, string] {
        return [PopForums.currentTopicState, "isSubscribed"];
    }

    updateUI(data: boolean): void {
        let button = this.querySelector("input");
        if (data)
            button.value = this.unsubscribetext;
        else
            button.value = this.subscribetext;
    }

    static template: string = `<input type="button" />`;
}

customElements.define('pf-subscribebutton', SubscribeButton);

}