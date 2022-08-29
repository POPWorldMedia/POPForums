namespace PopForums {

    export class TopicForm extends HTMLElement {
        constructor() {
            super();
        }

        get templateID() {
            return this.getAttribute("templateid");
        }

        private button: HTMLInputElement;

        connectedCallback() {
            let template = document.getElementById(this.templateID) as HTMLTemplateElement;
            if (!template) {
                console.error(`Can't find templateID ${this.templateID} to make reply form.`);
                return;
            }
            this.append(template.content.cloneNode(true));
            this.button = this.querySelector("#SubmitNewTopic");
            this.button.addEventListener("click", () => {
                this.submitTopic();
            });
        }

        submitTopic() {
            this.button.setAttribute("disabled", "disabled");
            let postImageIDs = PopForums.userState.postImageIds;
            let model = {
                Title: (this.querySelector("#NewTopic #Title") as HTMLInputElement).value,
                FullText: (this.querySelector("#NewTopic #FullText")as HTMLInputElement).value,
                IncludeSignature: (this.querySelector("#NewTopic #IncludeSignature")as HTMLInputElement).checked,
                ItemID: (this.querySelector("#NewTopic #ItemID")as HTMLInputElement).value,
                IsPlainText: (this.querySelector("#NewTopic #IsPlainText")as HTMLInputElement).value.toLowerCase() === "true",
                PostImageIDs: postImageIDs
            };
            fetch(PopForums.AreaPath + "/Forum/PostTopic", {
                method: "POST",
                body: JSON.stringify(model),
                headers: {
                    "Content-Type": "application/json"
                },
            })
                .then(response => response.json())
                .then(result => {
                    switch (result.result) {
                        case true:
                            window.location = result.redirect;
                            break;
                        default:
                            var r = this.querySelector("#PostResponseMessage") as HTMLElement;
                            r.innerHTML = result.message;
                            this.button.removeAttribute("disabled");
                            r.style.display = "block";
                    }
                })
                .catch(error => {
                    var r = this.querySelector("#PostResponseMessage") as HTMLElement;
                    r.innerHTML = "There was an unknown error while trying to post";
                    this.button.removeAttribute("disabled");
                    r.style.display = "block";
                });
        };
    }
    
    customElements.define('pf-topicform', TopicForm);
}