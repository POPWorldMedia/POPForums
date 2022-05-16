declare var topicState: TopicState;
declare namespace tinymce {
    function init(options:any): any;
    function get(id:string): any;
}

class FullText extends ElementBase {
    constructor() {
        super(null);
        this.internals = this.attachInternals();
        let template = document.createElement("template");
        template.innerHTML = FullText.template;
        this.attachShadow({ mode: "open" });
        this.shadowRoot.append(template.content.cloneNode(true));
        this.textBox = this.shadowRoot.getElementById(FullText.id);
        this.editorSettings.target = this.textBox;
        if (!topicState.isImageEnabled)
            this.editorSettings.toolbar = FullText.postNoImageToolbar;
        tinymce.init(this.editorSettings);
        this.hiddenInput = document.createElement("input") as HTMLInputElement;
        this.hiddenInput.id = this.id;
        this.hiddenInput.type = "hidden";
        this.appendChild(this.hiddenInput);
        let editor = tinymce.get(FullText.id);
        var self = this;
        editor.on("blur", function(e: any) {
            editor.save();
            self.value = (self.textBox as HTMLInputElement).value;
        });
    }

    get value() { return this._value;}
    set value(v: string) { this._value = v; }

    _value: string;

    static formAssociated = true;

    private internals: ElementInternals;
    private textBox: HTMLElement;
    private hiddenInput: HTMLInputElement;

    updateUI(data: any): void {
        if (data !== null && data !== undefined)
       {
           let editor = tinymce.get(FullText.id);
           var content = editor.getContent();
           content += data;
           editor.setContent(content);
           (this.textBox as HTMLInputElement).value += content;}
    }

    
    private static editorCSS = "/lib/bootstrap/dist/css/bootstrap.min.css,/lib/PopForums/dist/Editor.min.css";
    private static postNoImageToolbar = "cut copy paste | bold italic | bullist numlist blockquote removeformat | link";
    editorSettings = {
        target: null as HTMLElement,
        theme: "silver",
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
        // setup: function(editor: any) {
        //     editor.on("blur", function(e: any) {
        //         editor.save();
        //     });
        // }
    };

    static id: string = "FullText";
    static template: string = `<textarea class="form-control" id="${FullText.id}" name="${FullText.id}"></textarea>
    `;
}

customElements.define('pf-fulltext', FullText);