namespace PopForums {

    export class FullText extends ElementBase {
    constructor() {
        super(null);
    }

    get formID() { return this.getAttribute("formid") };

    get value() { return this._value;}
    set value(v: string) { this._value = v; }

    _value: string;

    static formAssociated = true;

    private textBox: HTMLElement;
    private externalFormElement: HTMLElement;

    connectedCallback() {
        var initialValue = this.getAttribute("value");
        if (initialValue)
            this.value = initialValue;
        if (userState.isPlainText) {
            this.externalFormElement = document.createElement("textarea");
            this.externalFormElement.id = this.formID;
            this.externalFormElement.setAttribute("name", this.formID);
            this.externalFormElement.classList.add("form-control");
            if (this.value)
            (this.externalFormElement as HTMLTextAreaElement).value = this.value;
            (this.externalFormElement as HTMLTextAreaElement).rows = 12;
            let self = this;
            this.externalFormElement.addEventListener("change", () => {
                self.value = (this.externalFormElement as HTMLTextAreaElement).value;
            });
            this.appendChild(this.externalFormElement);
            super.connectedCallback();
            return;
        }
        let template = document.createElement("template");
        template.innerHTML = FullText.template;
        this.attachShadow({ mode: "open" });
        this.shadowRoot.append(template.content.cloneNode(true));
        this.textBox = this.shadowRoot.querySelector("#editor");
        if (this.value)
            (this.textBox as HTMLTextAreaElement).innerText = this.value;
        this.editorSettings.target = this.textBox;
        if (!userState.isImageEnabled)
            this.editorSettings.toolbar = FullText.postNoImageToolbar;
        var self = this;
        this.editorSettings.setup = function (editor: any) {
            editor.on("init", function () {
              this.on("blur", function(e: any) {
                editor.save();
                self.value = (self.textBox as HTMLInputElement).value;
                (self.externalFormElement as any).value = self.value;
              })
            })
        };
        tinymce.init(this.editorSettings);
        this.externalFormElement = document.createElement("input") as HTMLInputElement;
        this.externalFormElement.id = this.formID;
        this.externalFormElement.setAttribute("name", this.formID);
        (this.externalFormElement as HTMLInputElement).type = "hidden";
        this.appendChild(this.externalFormElement);
        super.connectedCallback();
    }

    updateUI(data: any): void {
        if (data !== null && data !== undefined)
        {
            if (userState.isPlainText) {
                (this.externalFormElement as HTMLTextAreaElement).value += data;
                this.value = (this.externalFormElement as HTMLTextAreaElement).value;
            }
            else {
                let editor = tinymce.get("editor");
                var content = editor.getContent();
                content += data;
                editor.setContent(content);
                (this.textBox as HTMLInputElement).value += content;
                editor.save();
                this.value = (this.textBox as HTMLInputElement).value;
            }
        }
    }

    
    private static editorCSS = "/lib/bootstrap/dist/css/bootstrap.min.css,/lib/PopForums/dist/Editor.min.css";
    private static postNoImageToolbar = "cut copy paste | bold italic | bullist numlist blockquote removeformat | link";
    editorSettings = {
        target: null as HTMLElement,
        plugins: "lists image link",
        content_css: FullText.editorCSS,
        menubar: false,
        toolbar: "cut copy paste | bold italic | bullist numlist blockquote removeformat | link | image",
        statusbar: false,
        link_target_list: false,
        link_title: false,
        image_description: false,
        image_dimensions: false,
        image_title: false,
        image_uploadtab: false,
        images_file_types: 'jpeg,jpg,png,gif',
        automatic_uploads: false,
        browser_spellcheck : true,
        object_resizing: false,
        relative_urls: false,
        remove_script_host: false,
        contextmenu: "",
        paste_as_text: true,
        paste_data_images: false,
        setup: null as Function
    };

    static id: string = "FullText";
    static template: string = `<textarea id="editor"></textarea>
    `;
}

customElements.define('pf-fulltext', FullText);

}