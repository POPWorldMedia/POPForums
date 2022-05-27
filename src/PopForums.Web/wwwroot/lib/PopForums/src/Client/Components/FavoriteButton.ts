namespace PopForums {

    export class FavoriteButton extends ElementBase {
    constructor() {
        super(null);
    }

    get buttonclass(): string {
        return this.getAttribute("buttonclass");
    }

    get makefavoritetext(): string {
        return this.getAttribute("makefavoritetext");
    }
    get removefavoritetext(): string {
        return this.getAttribute("removefavoritetext");
    }

    connectedCallback() {
        this.innerHTML = SubscribeButton.template;
        let button: HTMLInputElement = this.querySelector("input");
        this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
        button.addEventListener("click", () => {
            fetch(PopForums.AreaPath + "/Favorites/ToggleFavorite/" + PopForums.currentTopicState.topicID, {
                method: "POST"
            })
                .then(response => response.json())
                .then(result => {
                    switch (result.data.isFavorite) {
                        case true:
                            PopForums.currentTopicState.isFavorite = true;
                            break;
                        case false:
                            PopForums.currentTopicState.isFavorite = false;
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

    updateUI(data: boolean): void {
        let button = this.querySelector("input");
        if (data)
            button.value = this.removefavoritetext;
        else
            button.value = this.makefavoritetext;
    }

    static template: string = `<input type="button" />`;
}

customElements.define('pf-favoritebutton', FavoriteButton);

}