namespace PopForums {

    export class LoginForm extends HTMLElement {
        constructor() {
            super();
        }

        get templateid() {
            return this.getAttribute("templateid");
        }
        get isExternalLogin() {
            return this.getAttribute("isexternallogin");
        }

        private button: HTMLInputElement;
        private email: HTMLInputElement;
        private password: HTMLInputElement;

        connectedCallback() {
            let template = document.getElementById(this.templateid) as HTMLTemplateElement;
            if (!template) {
                console.error(`Can't find templateID ${this.templateid} to make login form.`);
                return;
            }
            this.append(template.content.cloneNode(true));
            this.email = this.querySelector("#EmailLogin");
            this.password = this.querySelector("#PasswordLogin");
            this.button = this.querySelector("#LoginButton");
            this.button.addEventListener("click", () => {
                this.executeLogin();
            });
			this.querySelectorAll("#EmailLogin,#PasswordLogin").forEach(x =>
				x.addEventListener("keydown", (e: KeyboardEvent) => {
					if (e.code === "Enter") this.executeLogin();
				})
            );
        }

        executeLogin() {
            let path = "/Identity/Login";
            if (this.isExternalLogin.toLowerCase() === "true")
                path = "/Identity/LoginAndAssociate";
            let payload = JSON.stringify({ email: this.email.value, password: this.password.value });
            fetch(PopForums.AreaPath + path, {
                method: "POST",
                headers: {
                    'Content-Type': 'application/json'
                },
                body: payload
            })
                .then(function(response) {
                    return response.json();
            })
                .then(function (result) {
                    switch (result.result) {
                    case true:
                        let destination = (document.querySelector("#Referrer") as HTMLInputElement).value;
                        location.href = destination;
                        break;
                    default:
                        let loginResult = document.querySelector("#LoginResult");
                        loginResult.innerHTML = result.message;
                        loginResult.classList.remove("d-none");
                    }
            })
                .catch(function (error) {
                    let loginResult = document.querySelector("#LoginResult");
                    loginResult.innerHTML = "There was an unknown error while attempting login";
                    loginResult.classList.remove("d-none");
            });
        }
    }
    
    customElements.define('pf-loginform', LoginForm);
}