namespace PopForums {

    export class ReplyForm extends HTMLElement {
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
            this.button = this.querySelector("#SubmitReply");
            this.button.addEventListener("click", () => {
                this.submitReply();
            });
        }

        submitReply() {
            this.button.setAttribute("disabled", "disabled");
            let closeCheck = document.querySelector("#CloseOnReply") as HTMLInputElement;
            let closeOnReply = false;
            if (closeCheck && closeCheck.checked) closeOnReply = true;
            let postImageIDs = PopForums.userState.postImageIds;
            let model = {
                Title: (this.querySelector("#NewReply #Title") as HTMLInputElement).value,
                FullText: (this.querySelector("#NewReply #FullText") as HTMLInputElement).value,
                IncludeSignature: (this.querySelector("#NewReply #IncludeSignature") as HTMLInputElement).checked,
                ItemID: (this.querySelector("#NewReply #ItemID") as HTMLInputElement).value,
                CloseOnReply: closeOnReply,
                IsPlainText: (this.querySelector("#NewReply #IsPlainText") as HTMLInputElement).value.toLowerCase() === "true",
                ParentPostID: (this.querySelector("#NewReply #ParentPostID") as HTMLInputElement).value,
                PostImageIDs: postImageIDs
            };
            fetch(PopForums.AreaPath + "/Forum/PostReply", {
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
    
    customElements.define('pf-replyform', ReplyForm);
}