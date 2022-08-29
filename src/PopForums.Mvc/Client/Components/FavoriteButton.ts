namespace PopForums {

    export class FavoriteButton extends ElementBase {
    constructor() {
        super();
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
        let button: HTMLButtonElement = this.querySelector("button");
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

    getDependentReference(): [StateBase, string] {
        return [PopForums.currentTopicState, "isFavorite"];
    }

    updateUI(data: boolean): void {
        let button = this.querySelector("button");
        if (data) {
            button.title = this.removefavoritetext;
            button.classList.remove("icon-star", "text-muted");
            button.classList.add("icon-star-fill", "text-warning");
        }
        else {
            button.title = this.makefavoritetext;
            button.classList.remove("icon-star-fill", "text-warning");
            button.classList.add("icon-star", "text-muted");
        }
    }

    static template: string = `<button type="button" class="btn-link icon"></button>`;
}

customElements.define('pf-favoritebutton', FavoriteButton);

}