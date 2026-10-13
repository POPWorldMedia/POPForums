namespace PopForums {

    // Uploads a profile image (avatar or photo) as soon as it's picked, and deletes it as soon as the
    // button is clicked, so there's no form or save button.
    export class ProfileImageUpload extends HTMLElement {
    constructor() {
        super();
    }

    get uploadUrl(): string {
        return this.getAttribute("uploadurl")!;
    }
    get deleteUrl(): string {
        return this.getAttribute("deleteurl")!;
    }
    // the image ID gets appended to this
    get imageUrl(): string {
        return this.getAttribute("imageurl")!;
    }
    get imageID(): string {
        return this.getAttribute("imageid") ?? "";
    }
    get isApproved(): boolean {
        return this.getAttribute("isapproved")?.toLowerCase() !== "false";
    }
    get pendingMessage(): string {
        return this.getAttribute("pendingmessage") ?? "";
    }
    get altText(): string {
        return this.getAttribute("alttext")!;
    }
    get deleteLabel(): string {
        return this.getAttribute("deletelabel")!;
    }
    get successMessage(): string {
        return this.getAttribute("successmessage")!;
    }
    get uploadingMessage(): string {
        return this.getAttribute("uploadingmessage")!;
    }
    get failedMessage(): string {
        return this.getAttribute("failedmessage")!;
    }
    get limits(): ImageLimits {
        return {
            maxWidth: Number(this.getAttribute("maxwidth")),
            maxHeight: Number(this.getAttribute("maxheight")),
            maxkBytes: Number(this.getAttribute("maxkbytes")),
            isCrop: this.getAttribute("crop")?.toLowerCase() === "true",
            quality: Number(this.getAttribute("quality"))
        };
    }

    private alertArea!: HTMLDivElement;
    private fileInput!: HTMLInputElement;
    private feedback!: HTMLDivElement;
    private status!: HTMLDivElement;
    private imageBox!: HTMLParagraphElement;
    private image!: HTMLImageElement;
    private pending!: HTMLParagraphElement;
    private deleteButton!: HTMLButtonElement;

    connectedCallback() {
        this.innerHTML = ProfileImageUpload.template;
        this.alertArea = this.querySelector(".pf-uploadAlert")!;
        this.fileInput = this.querySelector("input[type=file]")!;
        this.feedback = this.querySelector(".invalid-feedback")!;
        this.status = this.querySelector(".form-text")!;
        this.imageBox = this.querySelector(".pf-uploadImage")!;
        this.image = this.querySelector("img")!;
        this.pending = this.querySelector(".alert-warning")!;
        this.deleteButton = this.querySelector("button")!;
        // attribute values only ever go in as text, never as markup
        this.image.alt = this.altText;
        this.pending.textContent = this.pendingMessage;
        this.deleteButton.textContent = this.deleteLabel;
        this.showImage(this.imageID, this.isApproved);
        this.fileInput.addEventListener("change", () => this.upload());
        this.deleteButton.addEventListener("click", () => this.deleteImage());
    }

    private async upload() {
        const file = this.fileInput.files?.[0];
        if (!file)
            return;
        this.clearMessages();
        this.setBusy(true, this.uploadingMessage);
        try {
            let prepared: File;
            try {
                prepared = await ImageResizer.prepare(file, this.limits);
            }
            catch (error) {
                this.showError((error as Error).message);
                return;
            }
            const form = new FormData();
            form.append("file", prepared);
            const response = await fetch(this.uploadUrl, { method: "POST", body: form });
            if (!response.ok) {
                // a 400 carries the server's validation message as text
                const text = response.status === 400 ? await response.text() : "";
                this.showError(text || this.failedMessage);
                return;
            }
            const result = await response.json();
            this.showImage(String(result.id), result.isApproved);
            this.showSuccess();
        }
        catch {
            this.showError(this.failedMessage);
        }
        finally {
            // so picking the same file again still triggers an upload
            this.fileInput.value = "";
            this.setBusy(false, "");
        }
    }

    private async deleteImage() {
        this.clearMessages();
        this.setBusy(true, "");
        try {
            const response = await fetch(this.deleteUrl, { method: "POST" });
            if (!response.ok) {
                this.showError(this.failedMessage);
                return;
            }
            this.showImage("", true);
        }
        catch {
            this.showError(this.failedMessage);
        }
        finally {
            this.setBusy(false, "");
        }
    }

    private showImage(id: string, isApproved: boolean) {
        const hasImage = id !== "";
        // only set src when there's an image, so an empty one doesn't fire a request
        if (hasImage)
            this.image.src = this.imageUrl + id;
        this.imageBox.hidden = !hasImage;
        this.deleteButton.hidden = !hasImage;
        this.pending.hidden = !hasImage || isApproved || this.pendingMessage === "";
    }

    private showSuccess() {
        this.alertArea.innerHTML = ProfileImageUpload.successTemplate;
        this.alertArea.querySelector("span")!.textContent = this.successMessage;
    }

    private showError(message: string) {
        this.feedback.textContent = message;
        this.fileInput.classList.add("is-invalid");
    }

    private clearMessages() {
        this.alertArea.innerHTML = "";
        this.feedback.textContent = "";
        this.fileInput.classList.remove("is-invalid");
    }

    private setBusy(isBusy: boolean, message: string) {
        this.fileInput.disabled = isBusy;
        this.deleteButton.disabled = isBusy;
        this.status.textContent = message;
    }

    static template: string = `<div class="pf-uploadAlert"></div>
<div class="mb-3">
	<input type="file" class="form-control" accept="image/*">
	<div class="invalid-feedback"></div>
	<div class="form-text" aria-live="polite"></div>
</div>
<p class="pf-uploadImage" hidden><img></p>
<p class="alert alert-warning" hidden></p>
<button type="button" class="btn btn-outline-danger mb-3" hidden></button>`;

    static successTemplate: string = `<div class="alert alert-success alert-dismissible fade show" role="alert">
	<span></span>
	<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
</div>`;
}

customElements.define('pf-profileimageupload', ProfileImageUpload);

}
