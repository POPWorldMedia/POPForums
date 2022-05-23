namespace PopForums {

    export class PreviewButton extends HTMLElement {
    constructor() {
        super();
    }

    get labelText(): string {
        return this.getAttribute("labeltext");
    }
    get textSourceSelector(): string {
        return this.getAttribute("textsourceselector");
    }
    get isPlainTextSelector(): string {
        return this.getAttribute("isplaintextselector");
    }

    connectedCallback() {
        this.innerHTML = PreviewButton.template;
        let button = this.querySelector("input") as HTMLButtonElement;
        button.value = this.labelText;
        let headText = this.querySelector("h4") as HTMLHeadElement;
        headText.innerText = this.labelText;
        var modal = this.querySelector(".modal");
        modal.addEventListener("shown.bs.modal", () => {
            this.openModal();
        });
    }

    openModal() {
        tinymce.triggerSave();
        let fullText = document.querySelector(this.textSourceSelector) as any;
        let model = {
            FullText: fullText.value,
            IsPlainText: (document.querySelector(this.isPlainTextSelector) as HTMLInputElement).value.toLowerCase() === "true"
        };
        fetch(PopForums.areaPath + "/Forum/PreviewText", {
            method: "POST",
            body: JSON.stringify(model),
            headers: {
                "Content-Type": "application/json"
            }
        })
            .then(response => response.text()
                .then(text => {
                    let r = this.querySelector(".parsedFullText") as HTMLDivElement;
                    r.innerHTML = text;
                }));
    }

    static template: string = `<input type="button" class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#PreviewModal">
<div class="modal fade" id="PreviewModal" tabindex="-1" role="dialog">
	<div class="modal-dialog">
		<div class="modal-content">
			<div class="modal-header">
				<h4 class="modal-title"></h4>
				<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
			</div>
			<div class="modal-body">
				<div class="postItem parsedFullText"></div>
			</div>
			<div class="modal-footer">
				<button type="button" class="btn btn-primary" data-bs-dismiss="modal">Close</button>
			</div>
		</div>
	</div>
</div>`;
}

customElements.define('pf-previewbutton', PreviewButton);

}