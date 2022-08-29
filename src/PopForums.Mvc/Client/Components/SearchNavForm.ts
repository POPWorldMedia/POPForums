namespace PopForums {

    export class SearchNavForm extends HTMLElement {
        constructor() {
            super();
        }

        get templateid() {
            return this.getAttribute("templateid");
        }
        get textboxid() {
            return this.getAttribute("textboxid");
        }
        get dropdownid() {
            return this.getAttribute("dropdownid");
        }

        private searchBox: HTMLInputElement;
        private dropdown: HTMLElement;

        connectedCallback() {
            let template = document.getElementById(this.templateid) as HTMLTemplateElement;
            if (!template) {
                console.error(`Can't find templateID ${this.templateid} to make search form.`);
                return;
            }
            this.append(template.content.cloneNode(true));
            this.searchBox = this.querySelector("#" + this.textboxid);
            this.dropdown = this.querySelector("#" + this.dropdownid);
            this.dropdown.addEventListener("shown.bs.dropdown", () => {
                this.searchBox.focus();
            });
        }
    }
    
    customElements.define('pf-searchnavform', SearchNavForm);
}