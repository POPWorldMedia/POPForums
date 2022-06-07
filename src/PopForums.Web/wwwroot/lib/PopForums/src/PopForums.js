var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var PopForums;
(function (PopForums) {
    PopForums.AreaPath = "/Forums";
    function Ready(callback) {
        if (document.readyState != "loading")
            callback();
        else
            document.addEventListener("DOMContentLoaded", callback);
    }
    PopForums.Ready = Ready;
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class ElementBase extends HTMLElement {
        connectedCallback() {
            if (this.state && this.propertyToWatch)
                return;
            let stateAndWatchProperty = this.getDependentReference();
            this.state = stateAndWatchProperty[0];
            this.propertyToWatch = stateAndWatchProperty[1];
            const delegate = this.update.bind(this);
            this.state.subscribe(this.propertyToWatch, delegate);
        }
        update() {
            const externalValue = this.state[this.propertyToWatch];
            this.updateUI(externalValue);
        }
    }
    PopForums.ElementBase = ElementBase;
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    // Properties to watch require the @WatchProperty attribute.
    class StateBase {
        constructor() {
            this._subs = new Map();
        }
        subscribe(propertyName, eventHandler) {
            if (!this._subs.has(propertyName))
                this._subs.set(propertyName, new Array());
            const callbacks = this._subs.get(propertyName);
            callbacks.push(eventHandler);
            eventHandler();
        }
        notify(propertyName) {
            const callbacks = this._subs.get(propertyName);
            if (callbacks)
                for (let i of callbacks) {
                    i();
                }
        }
    }
    PopForums.StateBase = StateBase;
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    PopForums.WatchProperty = (target, memberName) => {
        let currentValue = target[memberName];
        Object.defineProperty(target, memberName, {
            set(newValue) {
                currentValue = newValue;
                this.notify(memberName);
            },
            get() { return currentValue; }
        });
    };
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class AnswerButton extends PopForums.ElementBase {
        constructor() {
            super();
        }
        get answerstatusclass() {
            return this.getAttribute("answerstatusclass");
        }
        get chooseanswertext() {
            return this.getAttribute("chooseanswertext");
        }
        get topicid() {
            return this.getAttribute("topicid");
        }
        get postid() {
            return this.getAttribute("postid");
        }
        get answerpostid() {
            return this.getAttribute("answerpostid");
        }
        get userid() {
            return this.getAttribute("userid");
        }
        get startedbyuserid() {
            return this.getAttribute("startedbyuserid");
        }
        get isfirstintopic() {
            return this.getAttribute("isfirstintopic");
        }
        connectedCallback() {
            this.button = document.createElement("p");
            this.answerstatusclass.split(" ").forEach((c) => this.button.classList.add(c));
            if (this.isfirstintopic.toLowerCase() === "false" && this.userid === this.startedbyuserid) {
                // make it a button for author
                this.button.addEventListener("click", () => {
                    PopForums.currentTopicState.setAnswer(Number(this.postid), Number(this.topicid));
                });
            }
            this.appendChild(this.button);
            super.connectedCallback();
        }
        getDependentReference() {
            return [PopForums.currentTopicState, "answerPostID"];
        }
        updateUI(answerPostID) {
            if (this.isfirstintopic.toLowerCase() === "false" && this.userid === this.startedbyuserid) {
                // this is question author
                this.button.classList.add("asnswerButton");
                if (answerPostID && this.postid === answerPostID.toString()) {
                    this.button.classList.remove("icon-checkmark2");
                    this.button.classList.remove("text-muted");
                    this.button.classList.add("icon-checkmark");
                    this.button.classList.add("text-success");
                    this.style.cursor = "default";
                }
                else {
                    this.button.classList.remove("icon-checkmark");
                    this.button.classList.remove("text-success");
                    this.button.classList.add("icon-checkmark2");
                    this.button.classList.add("text-muted");
                    this.style.cursor = "pointer";
                }
            }
            else if (answerPostID && this.postid === answerPostID.toString()) {
                // not the question author, but it is the answer
                this.button.classList.add("icon-checkmark");
                this.button.classList.add("text-success");
                this.style.cursor = "default";
            }
        }
    }
    PopForums.AnswerButton = AnswerButton;
    customElements.define('pf-answerbutton', AnswerButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class CommentButton extends PopForums.ElementBase {
        constructor() {
            super();
        }
        get buttonclass() {
            return this.getAttribute("buttonclass");
        }
        get buttontext() {
            return this.getAttribute("buttontext");
        }
        get topicid() {
            return this.getAttribute("topicid");
        }
        get postid() {
            return this.getAttribute("postid");
        }
        connectedCallback() {
            var _a;
            this.innerHTML = CommentButton.template;
            let button = this.querySelector("input");
            button.value = this.buttontext;
            if (((_a = this.buttonclass) === null || _a === void 0 ? void 0 : _a.length) > 0)
                this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
            button.addEventListener("click", (e) => {
                PopForums.currentTopicState.loadComment(Number(this.topicid), Number(this.postid));
            });
            super.connectedCallback();
        }
        getDependentReference() {
            return [PopForums.currentTopicState, "commentReplyID"];
        }
        updateUI(data) {
            let button = this.querySelector("input");
            if (data !== undefined) {
                button.disabled = true;
                button.style.cursor = "default";
            }
            else
                button.disabled = false;
        }
    }
    CommentButton.template = `<input type="button" />`;
    PopForums.CommentButton = CommentButton;
    customElements.define('pf-commentbutton', CommentButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class FavoriteButton extends PopForums.ElementBase {
        constructor() {
            super();
        }
        get buttonclass() {
            return this.getAttribute("buttonclass");
        }
        get makefavoritetext() {
            return this.getAttribute("makefavoritetext");
        }
        get removefavoritetext() {
            return this.getAttribute("removefavoritetext");
        }
        connectedCallback() {
            this.innerHTML = PopForums.SubscribeButton.template;
            let button = this.querySelector("input");
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
        getDependentReference() {
            return [PopForums.currentTopicState, "isFavorite"];
        }
        updateUI(data) {
            let button = this.querySelector("input");
            if (data)
                button.value = this.removefavoritetext;
            else
                button.value = this.makefavoritetext;
        }
    }
    FavoriteButton.template = `<input type="button" />`;
    PopForums.FavoriteButton = FavoriteButton;
    customElements.define('pf-favoritebutton', FavoriteButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class FeedUpdater extends HTMLElement {
        constructor() {
            super();
        }
        get templateid() {
            return this.getAttribute("templateid");
        }
        connectedCallback() {
            let connection = new signalR.HubConnectionBuilder().withUrl("/FeedHub").build();
            let self = this;
            connection.on("notifyFeed", function (data) {
                let list = document.querySelector("#FeedList");
                let row = self.populateFeedRow(data);
                list.prepend(row);
                row.classList.remove("hidden");
            });
            connection.start()
                .then(function () {
                return connection.invoke("listenToAll");
            });
        }
        populateFeedRow(data) {
            let template = document.getElementById(this.templateid);
            if (!template) {
                console.error(`Can't find ID ${this.templateid} to make feed updates.`);
                return;
            }
            let row = template.cloneNode(true);
            row.removeAttribute("id");
            row.querySelector(".feedItemText").innerHTML = data.message;
            row.querySelector(".fTime").setAttribute("data-utc", data.utc);
            row.querySelector(".fTime").innerHTML = data.timeStamp;
            return row;
        }
        ;
    }
    PopForums.FeedUpdater = FeedUpdater;
    customElements.define('pf-feedupdater', FeedUpdater);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class FullText extends PopForums.ElementBase {
        constructor() {
            super();
            this.editorSettings = {
                target: null,
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
                browser_spellcheck: true,
                object_resizing: false,
                relative_urls: false,
                remove_script_host: false,
                contextmenu: "",
                paste_as_text: true,
                paste_data_images: false,
                setup: null
            };
        }
        get overridelistener() {
            return this.getAttribute("overridelistener");
        }
        get formID() { return this.getAttribute("formid"); }
        ;
        get value() { return this._value; }
        set value(v) { this._value = v; }
        connectedCallback() {
            var _a, _b;
            var initialValue = this.getAttribute("value");
            if (initialValue)
                this.value = initialValue;
            if (PopForums.userState.isPlainText) {
                this.externalFormElement = document.createElement("textarea");
                this.externalFormElement.id = this.formID;
                this.externalFormElement.setAttribute("name", this.formID);
                this.externalFormElement.classList.add("form-control");
                if (this.value)
                    this.externalFormElement.value = this.value;
                this.externalFormElement.rows = 12;
                let self = this;
                this.externalFormElement.addEventListener("change", () => {
                    self.value = this.externalFormElement.value;
                });
                this.appendChild(this.externalFormElement);
                if (((_a = this.overridelistener) === null || _a === void 0 ? void 0 : _a.toLowerCase()) !== "true")
                    super.connectedCallback();
                return;
            }
            let template = document.createElement("template");
            template.innerHTML = FullText.template;
            this.attachShadow({ mode: "open" });
            this.shadowRoot.append(template.content.cloneNode(true));
            this.textBox = this.shadowRoot.querySelector("#editor");
            if (this.value)
                this.textBox.innerText = this.value;
            this.editorSettings.target = this.textBox;
            if (!PopForums.userState.isImageEnabled)
                this.editorSettings.toolbar = FullText.postNoImageToolbar;
            var self = this;
            this.editorSettings.setup = function (editor) {
                editor.on("init", function () {
                    this.on("focusout", function (e) {
                        editor.save();
                        self.value = self.textBox.value;
                        self.externalFormElement.value = self.value;
                    });
                    this.on("blur", function (e) {
                        editor.save();
                        self.value = self.textBox.value;
                        self.externalFormElement.value = self.value;
                    });
                    editor.save();
                    self.value = self.textBox.value;
                    self.externalFormElement.value = self.value;
                });
            };
            tinymce.init(this.editorSettings);
            this.externalFormElement = document.createElement("input");
            this.externalFormElement.id = this.formID;
            this.externalFormElement.setAttribute("name", this.formID);
            this.externalFormElement.type = "hidden";
            this.appendChild(this.externalFormElement);
            if (((_b = this.overridelistener) === null || _b === void 0 ? void 0 : _b.toLowerCase()) !== "true")
                super.connectedCallback();
        }
        getDependentReference() {
            return [PopForums.currentTopicState, "nextQuote"];
        }
        updateUI(data) {
            if (data !== null && data !== undefined) {
                if (PopForums.userState.isPlainText) {
                    this.externalFormElement.value += data;
                    this.value = this.externalFormElement.value;
                }
                else {
                    let editor = tinymce.get("editor");
                    var content = editor.getContent();
                    content += data;
                    editor.setContent(content);
                    this.textBox.value += content;
                    editor.save();
                    this.value = this.textBox.value;
                    this.externalFormElement.value = this.value;
                }
            }
        }
    }
    FullText.formAssociated = true;
    FullText.editorCSS = "/lib/bootstrap/dist/css/bootstrap.min.css,/lib/PopForums/dist/Editor.min.css";
    FullText.postNoImageToolbar = "cut copy paste | bold italic | bullist numlist blockquote removeformat | link";
    FullText.id = "FullText";
    FullText.template = `<textarea id="editor"></textarea>
    `;
    PopForums.FullText = FullText;
    customElements.define('pf-fulltext', FullText);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class HomeUpdater extends HTMLElement {
        constructor() {
            super();
        }
        connectedCallback() {
            let connection = new signalR.HubConnectionBuilder().withUrl("/ForumsHub").build();
            let self = this;
            connection.on("notifyForumUpdate", function (data) {
                self.updateForumStats(data);
            });
            connection.start()
                .then(function () {
                return connection.invoke("listenToAll");
            });
        }
        updateForumStats(data) {
            let row = document.querySelector("[data-forumid='" + data.forumID + "']");
            row.querySelector(".topicCount").innerHTML = data.topicCount;
            row.querySelector(".postCount").innerHTML = data.postCount;
            row.querySelector(".lastPostTime").innerHTML = data.lastPostTime;
            row.querySelector(".lastPostName").innerHTML = data.lastPostName;
            row.querySelector(".fTime").setAttribute("data-utc", data.utc);
            row.querySelector(".newIndicator .icon-file-text2").classList.remove("text-muted");
            row.querySelector(".newIndicator .icon-file-text2").classList.add("text-warning");
        }
        ;
    }
    PopForums.HomeUpdater = HomeUpdater;
    customElements.define('pf-homeupdater', HomeUpdater);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class LoginForm extends HTMLElement {
        constructor() {
            super();
        }
        get templateid() {
            return this.getAttribute("templateid");
        }
        get isExternalLogin() {
            return this.getAttribute("isexternallogin");
        }
        connectedCallback() {
            let template = document.getElementById(this.templateid);
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
            this.querySelectorAll("#EmailLogin,#PasswordLogin").forEach(x => x.addEventListener("keydown", (e) => {
                if (e.code === "Enter")
                    this.executeLogin();
            }));
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
                .then(function (response) {
                return response.json();
            })
                .then(function (result) {
                switch (result.result) {
                    case true:
                        let destination = document.querySelector("#Referrer").value;
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
    PopForums.LoginForm = LoginForm;
    customElements.define('pf-loginform', LoginForm);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class MorePostsBeforeReplyButton extends PopForums.ElementBase {
        constructor() {
            super();
        }
        get buttonclass() {
            return this.getAttribute("buttonclass");
        }
        get buttontext() {
            return this.getAttribute("buttontext");
        }
        connectedCallback() {
            var _a;
            this.innerHTML = MorePostsBeforeReplyButton.template;
            let button = this.querySelector("input");
            button.value = this.buttontext;
            if (((_a = this.buttonclass) === null || _a === void 0 ? void 0 : _a.length) > 0)
                this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
            button.addEventListener("click", (e) => {
                PopForums.currentTopicState.loadMorePosts();
            });
            super.connectedCallback();
        }
        getDependentReference() {
            return [PopForums.currentTopicState, "isNewerPostsAvailable"];
        }
        updateUI(data) {
            let button = this.querySelector("input");
            if (!data)
                button.style.visibility = "hidden";
            else
                button.style.visibility = "visible";
        }
    }
    MorePostsBeforeReplyButton.template = `<input type="button" />`;
    PopForums.MorePostsBeforeReplyButton = MorePostsBeforeReplyButton;
    customElements.define('pf-morepostsbeforereplybutton', MorePostsBeforeReplyButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class MorePostsButton extends PopForums.ElementBase {
        constructor() {
            super();
        }
        get buttonclass() {
            return this.getAttribute("buttonclass");
        }
        get buttontext() {
            return this.getAttribute("buttontext");
        }
        connectedCallback() {
            var _a;
            this.innerHTML = MorePostsButton.template;
            let button = this.querySelector("input");
            button.value = this.buttontext;
            if (((_a = this.buttonclass) === null || _a === void 0 ? void 0 : _a.length) > 0)
                this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
            button.addEventListener("click", (e) => {
                PopForums.currentTopicState.loadMorePosts();
            });
            super.connectedCallback();
        }
        getDependentReference() {
            return [PopForums.currentTopicState, "highPage"];
        }
        updateUI(data) {
            let button = this.querySelector("input");
            if (PopForums.currentTopicState.pageCount === 1 || data === PopForums.currentTopicState.pageCount)
                button.style.visibility = "hidden";
            else
                button.style.visibility = "visible";
        }
    }
    MorePostsButton.template = `<input type="button" />`;
    PopForums.MorePostsButton = MorePostsButton;
    customElements.define('pf-morepostsbutton', MorePostsButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class PMCount extends PopForums.ElementBase {
        constructor() {
            super();
        }
        getDependentReference() {
            return [PopForums.userState, "newPmCount"];
        }
        updateUI(data) {
            if (data === 0)
                this.innerHTML = "";
            else
                this.innerHTML = `<span class="badge">${data}</span>`;
        }
    }
    PopForums.PMCount = PMCount;
    customElements.define('pf-pmcount', PMCount);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class PostMiniProfile extends HTMLElement {
        constructor() {
            super();
        }
        get username() {
            return this.getAttribute("username");
        }
        get usernameclass() {
            return this.getAttribute("usernameclass");
        }
        get userid() {
            return this.getAttribute("userid");
        }
        get miniProfileBoxClass() {
            return this.getAttribute("miniprofileboxclass");
        }
        connectedCallback() {
            this.isLoaded = false;
            this.innerHTML = PostMiniProfile.template;
            let nameHeader = this.querySelector("h3");
            this.usernameclass.split(" ").forEach((c) => nameHeader.classList.add(c));
            nameHeader.innerHTML = this.username;
            nameHeader.addEventListener("click", () => {
                this.toggle();
            });
            this.box = this.querySelector("div");
            this.miniProfileBoxClass.split(" ").forEach((c) => this.box.classList.add(c));
        }
        toggle() {
            if (!this.isLoaded) {
                fetch(PopForums.AreaPath + "/Account/MiniProfile/" + this.userid)
                    .then(response => response.text()
                    .then(text => {
                    let sub = this.box.querySelector("div");
                    sub.innerHTML = text;
                    const height = sub.getBoundingClientRect().height;
                    this.boxHeight = `${height}px`;
                    this.box.style.height = this.boxHeight;
                    this.isOpen = true;
                    this.isLoaded = true;
                }));
            }
            else if (!this.isOpen) {
                this.box.style.height = this.boxHeight;
                this.isOpen = true;
            }
            else {
                this.box.style.height = "0";
                this.isOpen = false;
            }
        }
    }
    PostMiniProfile.template = `<h3></h3>
<div>
    <div class="py-1 px-3 mb-2"></div>
</div>`;
    PopForums.PostMiniProfile = PostMiniProfile;
    customElements.define('pf-postminiprofile', PostMiniProfile);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class PostModerationLogButton extends HTMLElement {
        constructor() {
            super();
        }
        get buttonclass() {
            return this.getAttribute("buttonclass");
        }
        get buttontext() {
            return this.getAttribute("buttontext");
        }
        get postid() {
            return this.getAttribute("postid");
        }
        get parentSelectorToAppendTo() {
            return this.getAttribute("parentselectortoappendto");
        }
        connectedCallback() {
            this.innerHTML = PopForums.TopicModerationLogButton.template;
            let button = this.querySelector("input");
            button.value = this.buttontext;
            let classes = this.buttonclass;
            if ((classes === null || classes === void 0 ? void 0 : classes.length) > 0)
                classes.split(" ").forEach((c) => button.classList.add(c));
            let self = this;
            let container;
            button.addEventListener("click", () => {
                if (!container) {
                    let parentContainer = self.closest(this.parentSelectorToAppendTo);
                    if (!parentContainer) {
                        console.error(`Can't find a parent selector "${this.parentSelectorToAppendTo}" to append post moderation log to.`);
                        return;
                    }
                    container = document.createElement("div");
                    parentContainer.appendChild(container);
                }
                if (container.style.display !== "block")
                    fetch(PopForums.AreaPath + "/Moderator/PostModerationLog/" + this.postid)
                        .then(response => response.text()
                        .then(text => {
                        container.innerHTML = text;
                        container.style.display = "block";
                    }));
                else
                    container.style.display = "none";
            });
        }
    }
    PostModerationLogButton.template = `<input type="button" />`;
    PopForums.PostModerationLogButton = PostModerationLogButton;
    customElements.define("pf-postmoderationlogbutton", PostModerationLogButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class PreviewButton extends HTMLElement {
        constructor() {
            super();
        }
        get labelText() {
            return this.getAttribute("labeltext");
        }
        get textSourceSelector() {
            return this.getAttribute("textsourceselector");
        }
        get isPlainTextSelector() {
            return this.getAttribute("isplaintextselector");
        }
        connectedCallback() {
            this.innerHTML = PreviewButton.template;
            let button = this.querySelector("input");
            button.value = this.labelText;
            let headText = this.querySelector("h4");
            headText.innerText = this.labelText;
            var modal = this.querySelector(".modal");
            modal.addEventListener("shown.bs.modal", () => {
                this.openModal();
            });
        }
        openModal() {
            tinymce.triggerSave();
            let fullText = document.querySelector(this.textSourceSelector);
            let model = {
                FullText: fullText.value,
                IsPlainText: document.querySelector(this.isPlainTextSelector).value.toLowerCase() === "true"
            };
            fetch(PopForums.AreaPath + "/Forum/PreviewText", {
                method: "POST",
                body: JSON.stringify(model),
                headers: {
                    "Content-Type": "application/json"
                }
            })
                .then(response => response.text()
                .then(text => {
                let r = this.querySelector(".parsedFullText");
                r.innerHTML = text;
            }));
        }
    }
    PreviewButton.template = `<input type="button" class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#PreviewModal">
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
    PopForums.PreviewButton = PreviewButton;
    customElements.define('pf-previewbutton', PreviewButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class PreviousPostsButton extends PopForums.ElementBase {
        constructor() {
            super();
        }
        get buttonclass() {
            return this.getAttribute("buttonclass");
        }
        get buttontext() {
            return this.getAttribute("buttontext");
        }
        connectedCallback() {
            var _a;
            this.innerHTML = PreviousPostsButton.template;
            let button = this.querySelector("input");
            button.value = this.buttontext;
            if (((_a = this.buttonclass) === null || _a === void 0 ? void 0 : _a.length) > 0)
                this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
            button.addEventListener("click", (e) => {
                PopForums.currentTopicState.loadPreviousPosts();
            });
            super.connectedCallback();
        }
        getDependentReference() {
            return [PopForums.currentTopicState, "lowPage"];
        }
        updateUI(data) {
            let button = this.querySelector("input");
            if (PopForums.currentTopicState.pageCount === 1 || data === 1)
                button.style.visibility = "hidden";
            else
                button.style.visibility = "visible";
        }
    }
    PreviousPostsButton.template = `<input type="button" />`;
    PopForums.PreviousPostsButton = PreviousPostsButton;
    customElements.define('pf-previouspostsbutton', PreviousPostsButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class QuoteButton extends HTMLElement {
        constructor() {
            super();
        }
        get name() {
            return this.getAttribute("name");
        }
        get containerid() {
            return this.getAttribute("containerid");
        }
        get buttonclass() {
            return this.getAttribute("buttonclass");
        }
        get buttontext() {
            return this.getAttribute("buttontext");
        }
        get tip() {
            return this.getAttribute("tip");
        }
        get postID() {
            return this.getAttribute("postid");
        }
        connectedCallback() {
            let targetText = document.getElementById(this.containerid);
            this.innerHTML = QuoteButton.template;
            let button = this.querySelector("input");
            button.title = this.tip;
            ["mousedown", "touchstart"].forEach((e) => targetText.addEventListener(e, () => { if (this._tip)
                this._tip.hide(); }));
            button.value = this.buttontext;
            let classes = this.buttonclass;
            if ((classes === null || classes === void 0 ? void 0 : classes.length) > 0)
                classes.split(" ").forEach((c) => button.classList.add(c));
            this.onclick = (e) => {
                let selection = document.getSelection();
                if (selection.rangeCount === 0 || selection.getRangeAt(0).toString().length === 0) {
                    // prompt to select
                    this._tip = new bootstrap.Tooltip(button, { trigger: "manual" });
                    this._tip.show();
                    selection.removeAllRanges();
                    return;
                }
                let range = selection.getRangeAt(0);
                let fragment = range.cloneContents();
                let div = document.createElement("div");
                div.appendChild(fragment);
                // is selection in the container?
                let ancestor = range.commonAncestorContainer;
                while (ancestor['id'] !== this.containerid && ancestor.parentElement !== null) {
                    ancestor = ancestor.parentElement;
                }
                let isInText = ancestor['id'] === this.containerid;
                // if not, is it partially in the container?
                if (!isInText) {
                    let container = div.querySelector("#" + this.containerid);
                    if (container !== null && container !== undefined) {
                        // it's partially in the container, so just get that part
                        div.innerHTML = container.innerHTML;
                        isInText = true;
                    }
                }
                selection.removeAllRanges();
                if (isInText) {
                    // activate or add to quote
                    let result;
                    if (PopForums.userState.isPlainText)
                        result = `[quote][i]${this.name}:[/i]\r\n ${div.innerText}[/quote]`;
                    else
                        result = `<blockquote><p><i>${this.name}:</i></p>${div.innerHTML}</blockquote><p></p>`;
                    PopForums.currentTopicState.nextQuote = result;
                    if (!PopForums.currentTopicState.isReplyLoaded)
                        PopForums.currentTopicState.loadReply(PopForums.currentTopicState.topicID, Number(this.postID), true);
                }
            };
        }
    }
    QuoteButton.template = `<input type="button" data-bs-toggle="tooltip" title="" />`;
    PopForums.QuoteButton = QuoteButton;
    customElements.define('pf-quotebutton', QuoteButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class ReplyButton extends PopForums.ElementBase {
        constructor() {
            super();
        }
        get buttonclass() {
            return this.getAttribute("buttonclass");
        }
        get buttontext() {
            return this.getAttribute("buttontext");
        }
        get topicid() {
            return this.getAttribute("topicid");
        }
        get postid() {
            return this.getAttribute("postid");
        }
        get overridedisplay() {
            return this.getAttribute("overridedisplay");
        }
        connectedCallback() {
            var _a;
            this.innerHTML = ReplyButton.template;
            let button = this.querySelector("input");
            button.value = this.buttontext;
            if (((_a = this.buttonclass) === null || _a === void 0 ? void 0 : _a.length) > 0)
                this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
            button.addEventListener("click", (e) => {
                PopForums.currentTopicState.loadReply(Number(this.topicid), Number(this.postid), true);
            });
            super.connectedCallback();
        }
        getDependentReference() {
            return [PopForums.currentTopicState, "isReplyLoaded"];
        }
        updateUI(data) {
            var _a;
            if (((_a = this.overridedisplay) === null || _a === void 0 ? void 0 : _a.toLowerCase()) === "true")
                return;
            let button = this.querySelector("input");
            if (data)
                button.style.display = "none";
            else
                button.style.display = "initial";
        }
    }
    ReplyButton.template = `<input type="button" />`;
    PopForums.ReplyButton = ReplyButton;
    customElements.define('pf-replybutton', ReplyButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class ReplyForm extends HTMLElement {
        constructor() {
            super();
        }
        get templateID() {
            return this.getAttribute("templateid");
        }
        connectedCallback() {
            let template = document.getElementById(this.templateID);
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
            var closeCheck = document.querySelector("#CloseOnReply");
            var closeOnReply = false;
            if (closeCheck && closeCheck.checked)
                closeOnReply = true;
            var model = {
                Title: this.querySelector("#NewReply #Title").value,
                FullText: this.querySelector("#NewReply #FullText").value,
                IncludeSignature: this.querySelector("#NewReply #IncludeSignature").checked,
                ItemID: this.querySelector("#NewReply #ItemID").value,
                CloseOnReply: closeOnReply,
                IsPlainText: this.querySelector("#NewReply #IsPlainText").value.toLowerCase() === "true",
                ParentPostID: this.querySelector("#NewReply #ParentPostID").value
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
                        var r = this.querySelector("#PostResponseMessage");
                        r.innerHTML = result.message;
                        this.button.removeAttribute("disabled");
                        r.style.display = "block";
                }
            })
                .catch(error => {
                var r = this.querySelector("#PostResponseMessage");
                r.innerHTML = "There was an unknown error while trying to post";
                this.button.removeAttribute("disabled");
                r.style.display = "block";
            });
        }
        ;
    }
    PopForums.ReplyForm = ReplyForm;
    customElements.define('pf-replyform', ReplyForm);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class SubscribeButton extends PopForums.ElementBase {
        constructor() {
            super();
        }
        get buttonclass() {
            return this.getAttribute("buttonclass");
        }
        get subscribetext() {
            return this.getAttribute("subscribetext");
        }
        get unsubscribetext() {
            return this.getAttribute("unsubscribetext");
        }
        connectedCallback() {
            this.innerHTML = SubscribeButton.template;
            let button = this.querySelector("input");
            this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
            button.addEventListener("click", () => {
                fetch(PopForums.AreaPath + "/Subscription/ToggleSubscription/" + PopForums.currentTopicState.topicID, {
                    method: "POST"
                })
                    .then(response => response.json())
                    .then(result => {
                    switch (result.data.isSubscribed) {
                        case true:
                            PopForums.currentTopicState.isSubscribed = true;
                            break;
                        case false:
                            PopForums.currentTopicState.isSubscribed = false;
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
        getDependentReference() {
            return [PopForums.currentTopicState, "isSubscribed"];
        }
        updateUI(data) {
            let button = this.querySelector("input");
            if (data)
                button.value = this.unsubscribetext;
            else
                button.value = this.subscribetext;
        }
    }
    SubscribeButton.template = `<input type="button" />`;
    PopForums.SubscribeButton = SubscribeButton;
    customElements.define('pf-subscribebutton', SubscribeButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class TopicButton extends PopForums.ElementBase {
        constructor() {
            super();
        }
        get buttonclass() {
            return this.getAttribute("buttonclass");
        }
        get buttontext() {
            return this.getAttribute("buttontext");
        }
        get forumid() {
            return this.getAttribute("forumid");
        }
        connectedCallback() {
            var _a;
            this.innerHTML = TopicButton.template;
            let button = this.querySelector("input");
            button.value = this.buttontext;
            if (((_a = this.buttonclass) === null || _a === void 0 ? void 0 : _a.length) > 0)
                this.buttonclass.split(" ").forEach((c) => button.classList.add(c));
            button.addEventListener("click", () => {
                PopForums.currentForumState.loadNewTopic();
            });
            super.connectedCallback();
        }
        getDependentReference() {
            return [PopForums.currentForumState, "isNewTopicLoaded"];
        }
        updateUI(data) {
            if (data)
                this.style.display = "none";
            else
                this.style.display = "initial";
        }
    }
    TopicButton.template = `<input type="button" />`;
    PopForums.TopicButton = TopicButton;
    customElements.define('pf-topicbutton', TopicButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class TopicForm extends HTMLElement {
        constructor() {
            super();
        }
        get templateID() {
            return this.getAttribute("templateid");
        }
        connectedCallback() {
            let template = document.getElementById(this.templateID);
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
            var model = {
                Title: this.querySelector("#NewTopic #Title").value,
                FullText: this.querySelector("#NewTopic #FullText").value,
                IncludeSignature: this.querySelector("#NewTopic #IncludeSignature").checked,
                ItemID: this.querySelector("#NewTopic #ItemID").value,
                IsPlainText: this.querySelector("#NewTopic #IsPlainText").value.toLowerCase() === "true"
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
                        var r = this.querySelector("#PostResponseMessage");
                        r.innerHTML = result.message;
                        this.button.removeAttribute("disabled");
                        r.style.display = "block";
                }
            })
                .catch(error => {
                var r = this.querySelector("#PostResponseMessage");
                r.innerHTML = "There was an unknown error while trying to post";
                this.button.removeAttribute("disabled");
                r.style.display = "block";
            });
        }
        ;
    }
    PopForums.TopicForm = TopicForm;
    customElements.define('pf-topicform', TopicForm);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class TopicModerationLogButton extends HTMLElement {
        constructor() {
            super();
        }
        get buttonclass() {
            return this.getAttribute("buttonclass");
        }
        get buttontext() {
            return this.getAttribute("buttontext");
        }
        get topicid() {
            return this.getAttribute("topicid");
        }
        connectedCallback() {
            this.innerHTML = TopicModerationLogButton.template;
            let button = this.querySelector("input");
            button.value = this.buttontext;
            let classes = this.buttonclass;
            if ((classes === null || classes === void 0 ? void 0 : classes.length) > 0)
                classes.split(" ").forEach((c) => button.classList.add(c));
            button.addEventListener("click", () => {
                let container = this.querySelector("div");
                if (container.style.display !== "block")
                    fetch(PopForums.AreaPath + "/Moderator/TopicModerationLog/" + this.topicid)
                        .then(response => response.text()
                        .then(text => {
                        container.innerHTML = text;
                        container.style.display = "block";
                    }));
                else
                    container.style.display = "none";
            });
        }
    }
    TopicModerationLogButton.template = `<input type="button" />
    <div></div>`;
    PopForums.TopicModerationLogButton = TopicModerationLogButton;
    customElements.define("pf-topicmoderationlogbutton", TopicModerationLogButton);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class VoteCount extends HTMLElement {
        constructor() {
            super();
        }
        get votes() {
            return this.getAttribute("votes");
        }
        set votes(value) {
            this.setAttribute("votes", value);
        }
        get postid() {
            return this.getAttribute("postid");
        }
        get votescontainerclass() {
            return this.getAttribute("votescontainerclass");
        }
        get badgeclass() {
            return this.getAttribute("badgeclass");
        }
        get votebuttonclass() {
            return this.getAttribute("votebuttonclass");
        }
        get isloggedin() {
            return this.getAttribute("isloggedin").toLowerCase();
        }
        get isauthor() {
            return this.getAttribute("isauthor").toLowerCase();
        }
        get isvoted() {
            return this.getAttribute("isvoted").toLowerCase();
        }
        connectedCallback() {
            var _a, _b;
            this.innerHTML = VoteCount.template;
            this.badge = this.querySelector("div");
            this.badge.innerHTML = "+" + this.votes;
            if (((_a = this.badgeclass) === null || _a === void 0 ? void 0 : _a.length) > 0)
                this.badgeclass.split(" ").forEach((c) => this.badge.classList.add(c));
            var statusHtml = this.buttonGenerator();
            if (statusHtml != "") {
                let status = document.createElement("template");
                status.innerHTML = this.buttonGenerator();
                this.append(status.content.firstChild);
            }
            let voteButton = this.querySelector("span");
            if (voteButton) {
                if (((_b = this.votebuttonclass) === null || _b === void 0 ? void 0 : _b.length) > 0)
                    this.votebuttonclass.split(" ").forEach((c) => voteButton.classList.add(c));
                voteButton.addEventListener("click", () => {
                    fetch(PopForums.AreaPath + "/Forum/ToggleVote/" + this.postid, { method: "POST" })
                        .then(response => response.json()
                        .then((result) => {
                        this.votes = result.votes.toString();
                        this.badge.innerHTML = "+" + this.votes;
                        if (result.isVoted) {
                            voteButton.classList.remove("icon-plus");
                            voteButton.classList.add("icon-cancel-circle");
                        }
                        else {
                            voteButton.classList.remove("icon-cancel-circle");
                            voteButton.classList.add("icon-plus");
                        }
                        this.applyPopover();
                    }));
                });
            }
            this.setupVoterPopover();
            this.applyPopover();
        }
        setupVoterPopover() {
            var _a;
            this.voterContainer = document.createElement("div");
            if (((_a = this.votescontainerclass) === null || _a === void 0 ? void 0 : _a.length) > 0)
                this.votescontainerclass.split(" ").forEach((c) => this.voterContainer.classList.add(c));
            this.voterContainer.innerHTML = "Loading...";
            this.popover = new bootstrap.Popover(this.badge, {
                content: this.voterContainer,
                html: true,
                trigger: "click focus"
            });
            this.popoverEventHander = (e) => {
                fetch(PopForums.AreaPath + "/Forum/Voters/" + this.postid)
                    .then(response => response.text()
                    .then(text => {
                    var t = document.createElement("template");
                    t.innerHTML = text.trim();
                    this.voterContainer.innerHTML = "";
                    this.voterContainer.appendChild(t.content.firstChild);
                }));
            };
            this.badge.addEventListener("shown.bs.popover", this.popoverEventHander);
        }
        applyPopover() {
            if (this.votes === "0") {
                this.badge.style.cursor = "default";
                this.popover.disable();
            }
            else {
                this.badge.style.cursor = "pointer";
                this.popover.enable();
            }
        }
        buttonGenerator() {
            if (this.isloggedin === "false" || this.isauthor === "true")
                return "";
            if (this.isvoted === "true")
                return VoteCount.cancelVoteButton;
            return VoteCount.voteUpButton;
        }
    }
    VoteCount.template = `<div></div>`;
    VoteCount.voteUpButton = "<span class=\"icon-plus\"></span>";
    VoteCount.cancelVoteButton = "<span class=\"icon-cancel-circle\"></span>";
    PopForums.VoteCount = VoteCount;
    customElements.define("pf-votecount", VoteCount);
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class NotificationService {
        constructor(userState) {
            this.userState = userState;
            let self = this;
            this.connection = new signalR.HubConnectionBuilder().withUrl("/NotificationHub").build();
            this.connection.on("updatePMCount", function (pmCount) {
                self.userState.newPmCount = pmCount;
            });
            this.connection.start();
        }
    }
    PopForums.NotificationService = NotificationService;
})(PopForums || (PopForums = {}));
// TODO: Move this to an open websockets connection
var PopForums;
(function (PopForums) {
    class TimeUpdater {
        Start() {
            PopForums.Ready(() => {
                this.StartUpdater();
            });
        }
        PopulateArray() {
            this.timeArray = [];
            let times = document.querySelectorAll(".fTime");
            times.forEach(time => {
                var t = time.getAttribute("data-utc");
                if (((new Date().getDate() - new Date(t + "Z").getDate()) / 3600000) < 48)
                    this.timeArray.push(t);
            });
        }
        StartUpdater() {
            setTimeout(() => {
                this.StartUpdater();
                this.PopulateArray();
                this.CallForUpdate();
            }, 60000);
        }
        CallForUpdate() {
            if (!this.timeArray || this.timeArray.length === 0)
                return;
            let serialized = JSON.stringify(this.timeArray);
            fetch(PopForums.AreaPath + "/Time/GetTimes", {
                method: "POST",
                body: serialized,
                headers: {
                    "Content-Type": "application/json"
                }
            })
                .then(response => response.json())
                .then(data => {
                data.forEach((t) => {
                    document.querySelector(".fTime[data-utc='" + t.key + "']").innerHTML = t.value;
                });
            })
                .catch(error => { console.log("Time update failure: " + error); });
        }
    }
    PopForums.TimeUpdater = TimeUpdater;
})(PopForums || (PopForums = {}));
var timeUpdater = new PopForums.TimeUpdater();
timeUpdater.Start();
var PopForums;
(function (PopForums) {
    class ForumState extends PopForums.StateBase {
        constructor() {
            super();
            this.populateTopicRow = function (data) {
                let row = document.querySelector("#TopicTemplate").cloneNode(true);
                row.setAttribute("data-topicid", data.topicID);
                row.removeAttribute("id");
                row.querySelector(".startedByName").innerHTML = data.startedByName;
                row.querySelector(".indicatorLink").setAttribute("href", data.link);
                row.querySelector(".titleLink").innerHTML = data.title;
                row.querySelector(".titleLink").setAttribute("href", data.link);
                var forumTitle = row.querySelector(".forumTitle");
                if (forumTitle)
                    forumTitle.innerHTML = data.forumTitle;
                row.querySelector(".viewCount").innerHTML = data.viewCount;
                row.querySelector(".replyCount").innerHTML = data.replyCount;
                row.querySelector(".lastPostTime").innerHTML = data.lastPostTime;
                row.querySelector(".lastPostName").innerHTML = data.lastPostName;
                row.querySelector(".fTime").setAttribute("data-utc", data.utc);
                return row;
            };
        }
        setupForum() {
            PopForums.Ready(() => {
                this.isNewTopicLoaded = false;
                this.forumListen();
            });
        }
        loadNewTopic() {
            fetch(PopForums.AreaPath + "/Forum/PostTopic/" + this.forumID)
                .then((response) => {
                return response.text();
            })
                .then((body) => {
                var n = document.querySelector("#NewTopic");
                if (!n)
                    throw ("Can't find a #NewTopic element to load in the new topic form.");
                n.innerHTML = body;
                n.style.display = "block";
                this.isNewTopicLoaded = true;
            });
        }
        forumListen() {
            let connection = new signalR.HubConnectionBuilder().withUrl("/ForumsHub").build();
            let self = this;
            connection.on("notifyUpdatedTopic", function (data) {
                let removal = document.querySelector('#TopicList tr[data-topicID="' + data.topicID + '"]');
                if (removal) {
                    removal.remove();
                }
                else {
                    let rows = document.querySelectorAll("#TopicList tr:not(#TopicTemplate)");
                    if (rows.length == self.pageSize)
                        rows[rows.length - 1].remove();
                }
                let row = self.populateTopicRow(data);
                row.classList.remove("hidden");
                document.querySelector("#TopicList tbody").prepend(row);
            });
            connection.start()
                .then(function () {
                return connection.invoke("listenTo", self.forumID);
            });
        }
        recentListen() {
            var connection = new signalR.HubConnectionBuilder().withUrl("/RecentHub").build();
            let self = this;
            connection.on("notifyRecentUpdate", function (data) {
                var removal = document.querySelector('#TopicList tr[data-topicID="' + data.topicID + '"]');
                if (removal) {
                    removal.remove();
                }
                else {
                    var rows = document.querySelectorAll("#TopicList tr:not(#TopicTemplate)");
                    if (rows.length == self.pageSize)
                        rows[rows.length - 1].remove();
                }
                var row = self.populateTopicRow(data);
                row.classList.remove("hidden");
                document.querySelector("#TopicList tbody").prepend(row);
            });
            connection.start()
                .then(function () {
                return connection.invoke("register");
            });
        }
    }
    __decorate([
        PopForums.WatchProperty
    ], ForumState.prototype, "isNewTopicLoaded", void 0);
    PopForums.ForumState = ForumState;
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class TopicState extends PopForums.StateBase {
        constructor() {
            super();
            this.loadingPosts = false;
            this.isScrollAdjusted = false;
            // this is intended to be called when the reply box is open
            this.setMorePostsAvailable = (newestPostIDonServer) => {
                this.isNewerPostsAvailable = newestPostIDonServer !== this.lastVisiblePostID;
            };
            this.loadMorePosts = () => {
                let topicPagePath;
                if (this.highPage === this.pageCount) {
                    topicPagePath = PopForums.AreaPath + "/Forum/TopicPartial/" + this.topicID + "?lastPost=" + this.lastVisiblePostID + "&lowPage=" + this.lowPage;
                }
                else {
                    this.highPage++;
                    topicPagePath = PopForums.AreaPath + "/Forum/TopicPage/" + this.topicID + "?pageNumber=" + this.highPage + "&low=" + this.lowPage + "&high=" + this.highPage;
                }
                fetch(topicPagePath)
                    .then(response => response.text()
                    .then(text => {
                    let t = document.createElement("template");
                    t.innerHTML = text.trim();
                    let stuff = t.content.firstChild;
                    let links = stuff.querySelector(".pagerLinks");
                    stuff.removeChild(links);
                    let lastPostID = stuff.querySelector(".lastPostID");
                    stuff.removeChild(lastPostID);
                    let newPageCount = stuff.querySelector(".pageCount");
                    stuff.removeChild(newPageCount);
                    this.lastVisiblePostID = Number(lastPostID.value);
                    this.pageCount = Number(newPageCount.value);
                    let postStream = document.querySelector("#PostStream");
                    postStream.append(stuff);
                    document.querySelectorAll(".pagerLinks").forEach(x => x.replaceWith(links.cloneNode(true)));
                    document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));
                    if (this.highPage == this.pageCount && this.lowPage == 1) {
                        document.querySelectorAll(".pagerLinks").forEach(x => x.remove());
                    }
                    this.loadingPosts = false;
                    if (!this.isScrollAdjusted) {
                        this.scrollToPostFromHash();
                    }
                    if (this.isReplyLoaded) {
                        let self = this;
                        this.connection.invoke("getLastPostID", this.topicID)
                            .then(function (result) {
                            self.setMorePostsAvailable(result);
                        });
                    }
                }));
            };
            this.loadPreviousPosts = () => {
                this.lowPage--;
                let topicPagePath = PopForums.AreaPath + "/Forum/TopicPage/" + this.topicID + "?pageNumber=" + this.lowPage + "&low=" + this.lowPage + "&high=" + this.highPage;
                fetch(topicPagePath)
                    .then(response => response.text()
                    .then(text => {
                    let t = document.createElement("template");
                    t.innerHTML = text.trim();
                    var stuff = t.content.firstChild;
                    var links = stuff.querySelector(".pagerLinks");
                    stuff.removeChild(links);
                    var postStream = document.querySelector("#PostStream");
                    postStream.prepend(stuff);
                    document.querySelectorAll(".pagerLinks").forEach(x => x.replaceWith(links.cloneNode(true)));
                    document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));
                    if (this.highPage == this.pageCount && this.lowPage == 1) {
                        document.querySelectorAll(".pagerLinks").forEach(x => x.remove());
                    }
                }));
            };
            this.scrollLoad = () => {
                let streamEnd = document.querySelector("#StreamBottom");
                if (!streamEnd)
                    return; // this is a QA topic, no continuous post stream
                let top = streamEnd.offsetTop;
                let viewEnd = window.scrollY + window.outerHeight;
                let distance = top - viewEnd;
                if (!this.loadingPosts && distance < 250 && this.highPage < this.pageCount) {
                    this.loadingPosts = true;
                    this.loadMorePosts();
                }
            };
            this.scrollToElement = (id) => {
                let e = document.getElementById(id);
                let t = 0;
                if (e.offsetParent) {
                    while (e.offsetParent) {
                        t += e.offsetTop;
                        e = e.offsetParent;
                    }
                }
                else if (e.getBoundingClientRect().y) {
                    t += e.getBoundingClientRect().y;
                }
                let crumb = document.querySelector("#TopBreadcrumb");
                if (crumb)
                    t -= crumb.offsetHeight;
                scrollTo(0, t);
            };
            this.scrollToPostFromHash = () => {
                if (window.location.hash) {
                    Promise.all(Array.from(document.querySelectorAll("#PostStream img"))
                        .filter(img => !img.complete)
                        .map(img => new Promise(resolve => { img.onload = img.onerror = resolve; })))
                        .then(() => {
                        let hash = window.location.hash;
                        while (hash.charAt(0) === '#')
                            hash = hash.substring(1);
                        let tag = document.querySelector("div[data-postID='" + hash + "']");
                        if (tag) {
                            let tagPosition = tag.getBoundingClientRect().top;
                            let crumb = document.querySelector("#ForumContainer #TopBreadcrumb");
                            let crumbHeight = crumb.getBoundingClientRect().height;
                            let e = getComputedStyle(document.querySelector(".postItem"));
                            let margin = parseFloat(e.marginTop);
                            let newPosition = tagPosition - crumbHeight - margin;
                            window.scrollBy({ top: newPosition, behavior: 'auto' });
                        }
                        this.isScrollAdjusted = true;
                    });
                }
            };
        }
        setupTopic() {
            PopForums.Ready(() => {
                this.isReplyLoaded = false;
                this.isNewerPostsAvailable = false;
                this.lowPage = this.pageIndex;
                this.highPage = this.pageIndex;
                // signalR connections
                let connection = new signalR.HubConnectionBuilder().withUrl("/TopicsHub").build();
                let self = this;
                // for all posts loaded but reply not open
                connection.on("fetchNewPost", function (postID) {
                    if (!self.isReplyLoaded && self.highPage === self.pageCount) {
                        fetch(PopForums.AreaPath + "/Forum/Post/" + postID)
                            .then(response => response.text()
                            .then(text => {
                            var t = document.createElement("template");
                            t.innerHTML = text.trim();
                            document.querySelector("#PostStream").appendChild(t.content.firstChild);
                        }));
                        self.lastVisiblePostID = postID;
                    }
                });
                // for reply already open
                connection.on("notifyNewPosts", function (theLastPostID) {
                    self.setMorePostsAvailable(theLastPostID);
                });
                connection.start()
                    .then(function () {
                    return connection.invoke("listenTo", self.topicID);
                })
                    .then(function () {
                    self.connection = connection;
                });
                document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));
                this.scrollToPostFromHash();
                window.addEventListener("scroll", this.scrollLoad);
            });
        }
        loadReply(topicID, replyID, setupMorePosts) {
            if (this.isReplyLoaded) {
                this.scrollToElement("NewReply");
                return;
            }
            window.removeEventListener("scroll", this.scrollLoad);
            var path = PopForums.AreaPath + "/Forum/PostReply/" + topicID;
            if (replyID != null) {
                path += "?replyID=" + replyID;
            }
            fetch(path)
                .then(response => response.text()
                .then(text => {
                let n = document.querySelector("#NewReply");
                n.innerHTML = text;
                n.style.display = "block";
                this.scrollToElement("NewReply");
                this.isReplyLoaded = true;
                if (setupMorePosts) {
                    let self = this;
                    this.connection.invoke("getLastPostID", this.topicID)
                        .then(function (result) {
                        self.setMorePostsAvailable(result);
                    });
                }
                this.isReplyLoaded = true;
                this.commentReplyID = 0;
            }));
        }
        loadComment(topicID, replyID) {
            var n = document.querySelector("[data-postid*='" + replyID + "'] .commentHolder");
            const boxid = "commentbox";
            n.id = boxid;
            var path = PopForums.AreaPath + "/Forum/PostReply/" + topicID + "?replyID=" + replyID;
            this.commentReplyID = replyID;
            this.isReplyLoaded = true;
            fetch(path)
                .then(response => response.text()
                .then(text => {
                n.innerHTML = text;
                this.scrollToElement(boxid);
            }));
        }
        ;
        setAnswer(postID, topicID) {
            var model = { postID: postID, topicID: topicID };
            fetch(PopForums.AreaPath + "/Forum/SetAnswer/", {
                method: "POST",
                body: JSON.stringify(model),
                headers: {
                    "Content-Type": "application/json"
                }
            })
                .then(response => {
                this.answerPostID = postID;
            });
        }
    }
    __decorate([
        PopForums.WatchProperty
    ], TopicState.prototype, "isReplyLoaded", void 0);
    __decorate([
        PopForums.WatchProperty
    ], TopicState.prototype, "answerPostID", void 0);
    __decorate([
        PopForums.WatchProperty
    ], TopicState.prototype, "lowPage", void 0);
    __decorate([
        PopForums.WatchProperty
    ], TopicState.prototype, "highPage", void 0);
    __decorate([
        PopForums.WatchProperty
    ], TopicState.prototype, "isNewerPostsAvailable", void 0);
    __decorate([
        PopForums.WatchProperty
    ], TopicState.prototype, "commentReplyID", void 0);
    __decorate([
        PopForums.WatchProperty
    ], TopicState.prototype, "nextQuote", void 0);
    __decorate([
        PopForums.WatchProperty
    ], TopicState.prototype, "isSubscribed", void 0);
    __decorate([
        PopForums.WatchProperty
    ], TopicState.prototype, "isFavorite", void 0);
    PopForums.TopicState = TopicState;
})(PopForums || (PopForums = {}));
var PopForums;
(function (PopForums) {
    class UserState extends PopForums.StateBase {
        constructor() {
            super();
            this.isPlainText = false;
            this.newPmCount = 0;
            this.notificationService = new PopForums.NotificationService(this);
        }
    }
    __decorate([
        PopForums.WatchProperty
    ], UserState.prototype, "newPmCount", void 0);
    PopForums.UserState = UserState;
})(PopForums || (PopForums = {}));
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiUG9wRm9ydW1zLmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXMiOlsiQ2xpZW50L0RlY2xhcmF0aW9ucy50cyIsIkNsaWVudC9FbGVtZW50QmFzZS50cyIsIkNsaWVudC9TdGF0ZUJhc2UudHMiLCJDbGllbnQvV2F0Y2hQcm9wZXJ0eUF0dHJpYnV0ZS50cyIsIkNsaWVudC9Db21wb25lbnRzL0Fuc3dlckJ1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL0NvbW1lbnRCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9GYXZvcml0ZUJ1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL0ZlZWRVcGRhdGVyLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvRnVsbFRleHQudHMiLCJDbGllbnQvQ29tcG9uZW50cy9Ib21lVXBkYXRlci50cyIsIkNsaWVudC9Db21wb25lbnRzL0xvZ2luRm9ybS50cyIsIkNsaWVudC9Db21wb25lbnRzL01vcmVQb3N0c0JlZm9yZVJlcGx5QnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvTW9yZVBvc3RzQnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvUE1Db3VudC50cyIsIkNsaWVudC9Db21wb25lbnRzL1Bvc3RNaW5pUHJvZmlsZS50cyIsIkNsaWVudC9Db21wb25lbnRzL1Bvc3RNb2RlcmF0aW9uTG9nQnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvUHJldmlld0J1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL1ByZXZpb3VzUG9zdHNCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9RdW90ZUJ1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL1JlcGx5QnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvUmVwbHlGb3JtLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvU3Vic2NyaWJlQnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvVG9waWNCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9Ub3BpY0Zvcm0udHMiLCJDbGllbnQvQ29tcG9uZW50cy9Ub3BpY01vZGVyYXRpb25Mb2dCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9Wb3RlQ291bnQudHMiLCJDbGllbnQvU2VydmljZXMvTm90aWZpY2F0aW9uU2VydmljZS50cyIsIkNsaWVudC9TZXJ2aWNlcy9UaW1lVXBkYXRlci50cyIsIkNsaWVudC9TdGF0ZS9Gb3J1bVN0YXRlLnRzIiwiQ2xpZW50L1N0YXRlL1RvcGljU3RhdGUudHMiLCJDbGllbnQvU3RhdGUvVXNlclN0YXRlLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7Ozs7OztBQUFBLElBQVUsU0FBUyxDQVVsQjtBQVZELFdBQVUsU0FBUztJQUNGLGtCQUFRLEdBQUcsU0FBUyxDQUFDO0lBS2xDLFNBQWdCLEtBQUssQ0FBQyxRQUFhO1FBQy9CLElBQUksUUFBUSxDQUFDLFVBQVUsSUFBSSxTQUFTO1lBQUUsUUFBUSxFQUFFLENBQUM7O1lBQzVDLFFBQVEsQ0FBQyxnQkFBZ0IsQ0FBQyxrQkFBa0IsRUFBRSxRQUFRLENBQUMsQ0FBQztJQUNqRSxDQUFDO0lBSGUsZUFBSyxRQUdwQixDQUFBO0FBQ0wsQ0FBQyxFQVZTLFNBQVMsS0FBVCxTQUFTLFFBVWxCO0FDVkQsSUFBVSxTQUFTLENBNkJsQjtBQTdCRCxXQUFVLFNBQVM7SUFFbkIsTUFBc0IsV0FBWSxTQUFRLFdBQVc7UUFFakQsaUJBQWlCO1lBQ2IsSUFBSSxJQUFJLENBQUMsS0FBSyxJQUFJLElBQUksQ0FBQyxlQUFlO2dCQUNsQyxPQUFPO1lBQ1gsSUFBSSxxQkFBcUIsR0FBRyxJQUFJLENBQUMscUJBQXFCLEVBQUUsQ0FBQztZQUN6RCxJQUFJLENBQUMsS0FBSyxHQUFHLHFCQUFxQixDQUFDLENBQUMsQ0FBQyxDQUFDO1lBQ3RDLElBQUksQ0FBQyxlQUFlLEdBQUcscUJBQXFCLENBQUMsQ0FBQyxDQUFDLENBQUM7WUFDaEQsTUFBTSxRQUFRLEdBQUcsSUFBSSxDQUFDLE1BQU0sQ0FBQyxJQUFJLENBQUMsSUFBSSxDQUFD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LENBQUMsV0FBVyxHQUFHLEtBQUssQ0FBQztZQUN6QixJQUFJLENBQUMsVUFBVSxHQUFHLENBQUMsQ0FBQztZQUNwQixJQUFJLENBQUMsbUJBQW1CLEdBQUcsSUFBSSxVQUFBLG1CQUFtQixDQUFDLElBQUksQ0FBQyxDQUFDO1FBQzdELENBQUM7S0FTSjtJQURHO1FBREMsVUFBQSxhQUFhO2lEQUNLO0lBZFYsbUJBQVMsWUFlckIsQ0FBQTtBQUVELENBQUMsRUFuQlMsU0FBUyxLQUFULFNBQVMsUUFtQmxCIiwic291cmNlc0NvbnRlbnQiOlsibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcbiAgICBleHBvcnQgY29uc3QgQXJlYVBhdGggPSBcIi9Gb3J1bXNcIjtcclxuICAgIGV4cG9ydCB2YXIgY3VycmVudFRvcGljU3RhdGU6IFRvcGljU3RhdGU7XHJcbiAgICBleHBvcnQgdmFyIGN1cnJlbnRGb3J1bVN0YXRlOiBGb3J1bVN0YXRlO1xyXG4gICAgZXhwb3J0IHZhciB1c2VyU3RhdGU6IFVzZXJTdGF0ZTtcclxuXHJcbiAgICBleHBvcnQgZnVuY3Rpb24gUmVhZHkoY2FsbGJhY2s6IGFueSk6IHZvaWQge1xyXG4gICAgICAgIGlmIChkb2N1bWVudC5yZWFkeVN0YXRlICE9IFwibG9hZGluZ1wiKSBjYWxsYmFjaygpO1xyXG4gICAgICAgIGVsc2UgZG9jdW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcIkRPTUNvbnRlbnRMb2FkZWRcIiwgY2FsbGJhY2spO1xyXG4gICAgfVxyXG59XHJcblxyXG5cclxuZGVjbGFyZSBuYW1lc3BhY2UgdGlueW1jZSB7XHJcbiAgICBmdW5jdGlvbiBpbml0KG9wdGlvbnM6YW55KTogYW55O1xyXG4gICAgZnVuY3Rpb24gZ2V0KGlkOnN0cmluZyk6IGFueTtcclxuICAgIGZ1bmN0aW9uIHRyaWdnZXJTYXZlKCk6IGFueTtcclxufVxyXG5cclxuZGVjbGFyZSBuYW1lc3BhY2UgYm9vdHN0cmFwIHtcclxuICAgIGNsYXNzIFRvb2x0aXAge1xyXG4gICAgICAgIGNvbnN0cnVjdG9yKGVsOiBFbGVtZW50LCBvcHRpb25zOmFueSk7XHJcbiAgICB9XHJcbiAgICBjbGFzcyBQb3BvdmVyIHtcclxuICAgICAgICBjb25zdHJ1Y3RvcihlbDogRWxlbWVudCwgb3B0aW9uczphbnkpO1xyXG4gICAgICAgIGVuYWJsZSgpOiB2b2lkO1xyXG4gICAgICAgIGRpc2FibGUoKTogdm9pZDtcclxuICAgIH1cclxufVxyXG5cclxuZGVjbGFyZSBuYW1lc3BhY2Ugc2lnbmFsUiB7XHJcbiAgICBjbGFzcyBIdWJDb25uZWN0aW9uQnVpbGRlciB7XHJcbiAgICAgICAgd2l0aFVybCh1cmw6IHN0cmluZyk6IGFueTtcclxuICAgIH1cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuZXhwb3J0IGFic3RyYWN0IGNsYXNzIEVsZW1lbnRCYXNlIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIGlmICh0aGlzLnN0YXRlICYmIHRoaXMucHJvcGVydHlUb1dhdGNoKVxyXG4gICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgbGV0IHN0YXRlQW5kV2F0Y2hQcm9wZXJ0eSA9IHRoaXMuZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk7XHJcbiAgICAgICAgdGhpcy5zdGF0ZSA9IHN0YXRlQW5kV2F0Y2hQcm9wZXJ0eVswXTtcclxuICAgICAgICB0aGlzLnByb3BlcnR5VG9XYXRjaCA9IHN0YXRlQW5kV2F0Y2hQcm9wZXJ0eVsxXTtcclxuICAgICAgICBjb25zdCBkZWxlZ2F0ZSA9IHRoaXMudXBkYXRlLmJpbmQodGhpcyk7XHJcbiAgICAgICAgdGhpcy5zdGF0ZS5zdWJzY3JpYmUodGhpcy5wcm9wZXJ0eVRvV2F0Y2gsIGRlbGVnYXRlKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIHN0YXRlOiBTdGF0ZUJhc2U7XHJcbiAgICBwcml2YXRlIHByb3BlcnR5VG9XYXRjaDogc3RyaW5nO1xyXG5cclxuICAgIHVwZGF0ZSgpIHtcclxuICAgICAgICBjb25zdCBleHRlcm5hbFZhbHVlID0gdGhpcy5zdGF0ZVt0aGlzLnByb3BlcnR5VG9XYXRjaF07XHJcbiAgICAgICAgdGhpcy51cGRhdGVVSShleHRlcm5hbFZhbHVlKTtcclxuICAgIH1cclxuXHJcbiAgICAvLyBJbXBsZW1lbnRhdGlvbiBzaG91bGQgcmV0dXJuIHRoZSBTdGF0ZUJhc2UgYW5kIHByb3BlcnR5IChhcyBhIHN0cmluZykgdG8gbW9uaXRvclxyXG4gICAgYWJzdHJhY3QgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ107XHJcblxyXG4gICAgLy8gVXNlIGluIHRoZSBpbXBsZW1lbnRhdGlvbiB0byBtYW5pcHVsYXRlIHRoZSBzaGFkb3cgb3IgbGlnaHQgRE9NIG9yIHN0cmFpZ2h0IG1hcmt1cCBhcyBuZWVkZWQgaW4gcmVzcG9uc2UgdG8gdGhlIG5ldyBkYXRhLlxyXG4gICAgYWJzdHJhY3QgdXBkYXRlVUkoZGF0YTogYW55KTogdm9pZDtcclxufVxyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuLy8gUHJvcGVydGllcyB0byB3YXRjaCByZXF1aXJlIHRoZSBAV2F0Y2hQcm9wZXJ0eSBhdHRyaWJ1dGUuXHJcbmV4cG9ydCBjbGFzcyBTdGF0ZUJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgdGhpcy5fc3VicyA9IG5ldyBNYXA8c3RyaW5nLCBBcnJheTxGdW5jdGlvbj4+KCk7XHJcbiAgICB9XHJcblxyXG4gICAgcHJpdmF0ZSBfc3ViczogTWFwPHN0cmluZywgQXJyYXk8RnVuY3Rpb24+PjtcclxuXHJcbiAgICBzdWJzY3JpYmUocHJvcGVydHlOYW1lOiBzdHJpbmcsIGV2ZW50SGFuZGxlcjogRnVuY3Rpb24pIHtcclxuICAgICAgICBpZiAoIXRoaXMuX3N1YnMuaGFzKHByb3BlcnR5TmFtZSkpXHJcbiAgICAgICAgICAgIHRoaXMuX3N1YnMuc2V0KHByb3BlcnR5TmFtZSwgbmV3IEFycmF5PEZ1bmN0aW9uPigpKTtcclxuICAgICAgICBjb25zdCBjYWxsYmFja3MgPSB0aGlzLl9zdWJzLmdldChwcm9wZXJ0eU5hbWUpO1xyXG4gICAgICAgIGNhbGxiYWNrcy5wdXNoKGV2ZW50SGFuZGxlcik7XHJcbiAgICAgICAgZXZlbnRIYW5kbGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgbm90aWZ5KHByb3BlcnR5TmFtZTogc3RyaW5nKSB7XHJcbiAgICAgICAgY29uc3QgY2FsbGJhY2tzID0gdGhpcy5fc3Vicy5nZXQocHJvcGVydHlOYW1lKTtcclxuICAgICAgICBpZiAoY2FsbGJhY2tzKVxyXG4gICAgICAgICAgICBmb3IgKGxldCBpIG9mIGNhbGxiYWNrcykge1xyXG4gICAgICAgICAgICAgICAgaSgpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICB9XHJcbn1cclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbmV4cG9ydCBjb25zdCBXYXRjaFByb3BlcnR5ID0gKHRhcmdldDogYW55LCBtZW1iZXJOYW1lOiBzdHJpbmcpID0+IHtcclxuICAgIGxldCBjdXJyZW50VmFsdWU6IGFueSA9IHRhcmdldFttZW1iZXJOYW1lXTsgIFxyXG4gICAgT2JqZWN0LmRlZmluZVByb3BlcnR5KHRhcmdldCwgbWVtYmVyTmFtZSwge1xyXG4gICAgICAgIHNldCh0aGlzOiBhbnksIG5ld1ZhbHVlOiBhbnkpIHtcclxuICAgICAgICAgICAgY3VycmVudFZhbHVlID0gbmV3VmFsdWU7XHJcbiAgICAgICAgICAgIHRoaXMubm90aWZ5KG1lbWJlck5hbWUpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgZ2V0KCkge3JldHVybiBjdXJyZW50VmFsdWU7fVxyXG4gICAgfSk7XHJcbn07XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIEFuc3dlckJ1dHRvbiBleHRlbmRzIEVsZW1lbnRCYXNlIHtcclxuICAgICAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICAgICAgc3VwZXIoKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldCBhbnN3ZXJzdGF0dXNjbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJhbnN3ZXJzdGF0dXNjbGFzc1wiKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZ2V0IGNob29zZWFuc3dlcnRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiY2hvb3NlYW5zd2VydGV4dFwiKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZ2V0IHRvcGljaWQoKTogc3RyaW5nIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidG9waWNpZFwiKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZ2V0IHBvc3RpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJwb3N0aWRcIik7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGdldCBhbnN3ZXJwb3N0aWQoKTogc3RyaW5nIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYW5zd2VycG9zdGlkXCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBnZXQgdXNlcmlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInVzZXJpZFwiKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZ2V0IHN0YXJ0ZWRieXVzZXJpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJzdGFydGVkYnl1c2VyaWRcIik7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGdldCBpc2ZpcnN0aW50b3BpYygpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJpc2ZpcnN0aW50b3BpY1wiKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgYnV0dG9uOiBIVE1MRWxlbWVudDtcclxuXHJcbiAgICAgICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChcInBcIik7XHJcbiAgICAgICAgICAgIHRoaXMuYW5zd2Vyc3RhdHVzY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgICAgICBpZiAodGhpcy5pc2ZpcnN0aW50b3BpYy50b0xvd2VyQ2FzZSgpID09PSBcImZhbHNlXCIgJiYgdGhpcy51c2VyaWQgPT09IHRoaXMuc3RhcnRlZGJ5dXNlcmlkKSB7XHJcbiAgICAgICAgICAgICAgICAvLyBtYWtlIGl0IGEgYnV0dG9uIGZvciBhdXRob3JcclxuICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLnNldEFuc3dlcihOdW1iZXIodGhpcy5wb3N0aWQpLCBOdW1iZXIodGhpcy50b3BpY2lkKSk7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB0aGlzLmFwcGVuZENoaWxkKHRoaXMuYnV0dG9uKTtcclxuICAgICAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICAgICAgcmV0dXJuIFtQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUsIFwiYW5zd2VyUG9zdElEXCJdO1xyXG4gICAgICAgIH1cclxuICAgICAgICBcclxuICAgICAgICB1cGRhdGVVSShhbnN3ZXJQb3N0SUQ6IG51bWJlcik6IHZvaWQge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5pc2ZpcnN0aW50b3BpYy50b0xvd2VyQ2FzZSgpID09PSBcImZhbHNlXCIgJiYgdGhpcy51c2VyaWQgPT09IHRoaXMuc3RhcnRlZGJ5dXNlcmlkKSB7XHJcbiAgICAgICAgICAgICAgICAvLyB0aGlzIGlzIHF1ZXN0aW9uIGF1dGhvclxyXG4gICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LmFkZChcImFzbnN3ZXJCdXR0b25cIik7XHJcbiAgICAgICAgICAgICAgICBpZiAoYW5zd2VyUG9zdElEICYmIHRoaXMucG9zdGlkID09PSBhbnN3ZXJQb3N0SUQudG9TdHJpbmcoKSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5yZW1vdmUoXCJpY29uLWNoZWNrbWFyazJcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LnJlbW92ZShcInRleHQtbXV0ZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LmFkZChcImljb24tY2hlY2ttYXJrXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJ0ZXh0LXN1Y2Nlc3NcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5zdHlsZS5jdXJzb3IgPSBcImRlZmF1bHRcIjtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5yZW1vdmUoXCJpY29uLWNoZWNrbWFya1wiKTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmJ1dHRvbi5jbGFzc0xpc3QucmVtb3ZlKFwidGV4dC1zdWNjZXNzXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJpY29uLWNoZWNrbWFyazJcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LmFkZChcInRleHQtbXV0ZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5zdHlsZS5jdXJzb3IgPSBcInBvaW50ZXJcIjtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBlbHNlIGlmIChhbnN3ZXJQb3N0SUQgJiYgdGhpcy5wb3N0aWQgPT09IGFuc3dlclBvc3RJRC50b1N0cmluZygpKSB7XHJcbiAgICAgICAgICAgICAgICAvLyBub3QgdGhlIHF1ZXN0aW9uIGF1dGhvciwgYnV0IGl0IGlzIHRoZSBhbnN3ZXJcclxuICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJpY29uLWNoZWNrbWFya1wiKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJ0ZXh0LXN1Y2Nlc3NcIik7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnN0eWxlLmN1cnNvciA9IFwiZGVmYXVsdFwiO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLWFuc3dlcmJ1dHRvbicsIEFuc3dlckJ1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIENvbW1lbnRCdXR0b24gZXh0ZW5kcyBFbGVtZW50QmFzZSB7XHJcbiAgICAgICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgICAgIHN1cGVyKCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBnZXQgYnV0dG9udGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b250ZXh0XCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBcclxuICAgICAgICBnZXQgdG9waWNpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0b3BpY2lkXCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBcclxuICAgICAgICBnZXQgcG9zdGlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInBvc3RpZFwiKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgICAgICB0aGlzLmlubmVySFRNTCA9IENvbW1lbnRCdXR0b24udGVtcGxhdGU7XHJcbiAgICAgICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmJ1dHRvbnRleHQ7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmJ1dHRvbmNsYXNzPy5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICAgICAgdGhpcy5idXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgICAgICBidXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIChlOiBNb3VzZUV2ZW50KSA9PiB7XHJcbiAgICAgICAgICAgICAgICBQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUubG9hZENvbW1lbnQoTnVtYmVyKHRoaXMudG9waWNpZCksIE51bWJlcih0aGlzLnBvc3RpZCkpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICAgICAgcmV0dXJuIFtQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUsIFwiY29tbWVudFJlcGx5SURcIl07XHJcbiAgICAgICAgfVxyXG4gICAgICAgIFxyXG4gICAgICAgIHVwZGF0ZVVJKGRhdGE6IG51bWJlcik6IHZvaWQge1xyXG4gICAgICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgICAgIGlmIChkYXRhICE9PSB1bmRlZmluZWQpIHtcclxuICAgICAgICAgICAgICAgIGJ1dHRvbi5kaXNhYmxlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICBidXR0b24uc3R5bGUuY3Vyc29yID0gXCJkZWZhdWx0XCI7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgYnV0dG9uLmRpc2FibGVkID0gZmFsc2U7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIC8+YDtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1jb21tZW50YnV0dG9uJywgQ29tbWVudEJ1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIEZhdm9yaXRlQnV0dG9uIGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgbWFrZWZhdm9yaXRldGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcIm1ha2VmYXZvcml0ZXRleHRcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgcmVtb3ZlZmF2b3JpdGV0ZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwicmVtb3ZlZmF2b3JpdGV0ZXh0XCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIHRoaXMuaW5uZXJIVE1MID0gU3Vic2NyaWJlQnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b246IEhUTUxJbnB1dEVsZW1lbnQgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICB0aGlzLmJ1dHRvbmNsYXNzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiBidXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0Zhdm9yaXRlcy9Ub2dnbGVGYXZvcml0ZS9cIiArIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS50b3BpY0lELCB7XHJcbiAgICAgICAgICAgICAgICBtZXRob2Q6IFwiUE9TVFwiXHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS5qc29uKCkpXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXN1bHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIHN3aXRjaCAocmVzdWx0LmRhdGEuaXNGYXZvcml0ZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjYXNlIHRydWU6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUuaXNGYXZvcml0ZSA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSBmYWxzZTpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5pc0Zhdm9yaXRlID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgZGVmYXVsdDpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vIFRPRE86IHNvbWV0aGluZyBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC5jYXRjaCgoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgLy8gVE9ETzogaGFuZGxlIGVycm9yXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9KTtcclxuICAgICAgICBzdXBlci5jb25uZWN0ZWRDYWxsYmFjaygpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZSwgXCJpc0Zhdm9yaXRlXCJdO1xyXG4gICAgfVxyXG5cclxuICAgIHVwZGF0ZVVJKGRhdGE6IGJvb2xlYW4pOiB2b2lkIHtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgaWYgKGRhdGEpXHJcbiAgICAgICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMucmVtb3ZlZmF2b3JpdGV0ZXh0O1xyXG4gICAgICAgIGVsc2VcclxuICAgICAgICAgICAgYnV0dG9uLnZhbHVlID0gdGhpcy5tYWtlZmF2b3JpdGV0ZXh0O1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgLz5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLWZhdm9yaXRlYnV0dG9uJywgRmF2b3JpdGVCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBGZWVkVXBkYXRlciBleHRlbmRzIEhUTUxFbGVtZW50IHtcclxuICAgICAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICAgICAgc3VwZXIoKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldCB0ZW1wbGF0ZWlkKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0ZW1wbGF0ZWlkXCIpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgICAgIGxldCBjb25uZWN0aW9uID0gbmV3IHNpZ25hbFIuSHViQ29ubmVjdGlvbkJ1aWxkZXIoKS53aXRoVXJsKFwiL0ZlZWRIdWJcIikuYnVpbGQoKTtcclxuICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICBjb25uZWN0aW9uLm9uKFwibm90aWZ5RmVlZFwiLCBmdW5jdGlvbiAoZGF0YTogYW55KSB7XHJcbiAgICAgICAgICAgICAgICBsZXQgbGlzdCA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjRmVlZExpc3RcIik7XHJcbiAgICAgICAgICAgICAgICBsZXQgcm93ID0gc2VsZi5wb3B1bGF0ZUZlZWRSb3coZGF0YSk7XHJcbiAgICAgICAgICAgICAgICBsaXN0LnByZXBlbmQocm93KTtcclxuICAgICAgICAgICAgICAgIHJvdy5jbGFzc0xpc3QucmVtb3ZlKFwiaGlkZGVuXCIpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgY29ubmVjdGlvbi5zdGFydCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbm5lY3Rpb24uaW52b2tlKFwibGlzdGVuVG9BbGxcIik7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHBvcHVsYXRlRmVlZFJvdyhkYXRhOiBhbnkpIHtcclxuICAgICAgICAgICAgbGV0IHRlbXBsYXRlID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQodGhpcy50ZW1wbGF0ZWlkKTtcclxuICAgICAgICAgICAgaWYgKCF0ZW1wbGF0ZSkge1xyXG4gICAgICAgICAgICAgICAgY29uc29sZS5lcnJvcihgQ2FuJ3QgZmluZCBJRCAke3RoaXMudGVtcGxhdGVpZH0gdG8gbWFrZSBmZWVkIHVwZGF0ZXMuYCk7XHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgbGV0IHJvdyA9IHRlbXBsYXRlLmNsb25lTm9kZSh0cnVlKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgcm93LnJlbW92ZUF0dHJpYnV0ZShcImlkXCIpO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5mZWVkSXRlbVRleHRcIikuaW5uZXJIVE1MID0gZGF0YS5tZXNzYWdlO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5mVGltZVwiKS5zZXRBdHRyaWJ1dGUoXCJkYXRhLXV0Y1wiLCBkYXRhLnV0Yyk7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLmZUaW1lXCIpLmlubmVySFRNTCA9IGRhdGEudGltZVN0YW1wO1xyXG4gICAgICAgICAgICByZXR1cm4gcm93O1xyXG4gICAgICAgIH07XHJcbiAgICB9XHJcbiAgICBcclxuICAgIGN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtZmVlZHVwZGF0ZXInLCBGZWVkVXBkYXRlcik7XHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgRnVsbFRleHQgZXh0ZW5kcyBFbGVtZW50QmFzZSB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBvdmVycmlkZWxpc3RlbmVyKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwib3ZlcnJpZGVsaXN0ZW5lclwiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgZm9ybUlEKCkgeyByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJmb3JtaWRcIikgfTtcclxuXHJcbiAgICBnZXQgdmFsdWUoKSB7IHJldHVybiB0aGlzLl92YWx1ZTt9XHJcbiAgICBzZXQgdmFsdWUodjogc3RyaW5nKSB7IHRoaXMuX3ZhbHVlID0gdjsgfVxyXG5cclxuICAgIF92YWx1ZTogc3RyaW5nO1xyXG5cclxuICAgIHN0YXRpYyBmb3JtQXNzb2NpYXRlZCA9IHRydWU7XHJcblxyXG4gICAgcHJpdmF0ZSB0ZXh0Qm94OiBIVE1MRWxlbWVudDtcclxuICAgIHByaXZhdGUgZXh0ZXJuYWxGb3JtRWxlbWVudDogSFRNTEVsZW1lbnQ7XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdmFyIGluaXRpYWxWYWx1ZSA9IHRoaXMuZ2V0QXR0cmlidXRlKFwidmFsdWVcIik7XHJcbiAgICAgICAgaWYgKGluaXRpYWxWYWx1ZSlcclxuICAgICAgICAgICAgdGhpcy52YWx1ZSA9IGluaXRpYWxWYWx1ZTtcclxuICAgICAgICBpZiAodXNlclN0YXRlLmlzUGxhaW5UZXh0KSB7XHJcbiAgICAgICAgICAgIHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ0ZXh0YXJlYVwiKTtcclxuICAgICAgICAgICAgdGhpcy5leHRlcm5hbEZvcm1FbGVtZW50LmlkID0gdGhpcy5mb3JtSUQ7XHJcbiAgICAgICAgICAgIHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudC5zZXRBdHRyaWJ1dGUoXCJuYW1lXCIsIHRoaXMuZm9ybUlEKTtcclxuICAgICAgICAgICAgdGhpcy5leHRlcm5hbEZvcm1FbGVtZW50LmNsYXNzTGlzdC5hZGQoXCJmb3JtLWNvbnRyb2xcIik7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLnZhbHVlKVxyXG4gICAgICAgICAgICAodGhpcy5leHRlcm5hbEZvcm1FbGVtZW50IGFzIEhUTUxUZXh0QXJlYUVsZW1lbnQpLnZhbHVlID0gdGhpcy52YWx1ZTtcclxuICAgICAgICAgICAgKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBIVE1MVGV4dEFyZWFFbGVtZW50KS5yb3dzID0gMTI7XHJcbiAgICAgICAgICAgIGxldCBzZWxmID0gdGhpcztcclxuICAgICAgICAgICAgdGhpcy5leHRlcm5hbEZvcm1FbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJjaGFuZ2VcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgc2VsZi52YWx1ZSA9ICh0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQgYXMgSFRNTFRleHRBcmVhRWxlbWVudCkudmFsdWU7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB0aGlzLmFwcGVuZENoaWxkKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCk7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLm92ZXJyaWRlbGlzdGVuZXI/LnRvTG93ZXJDYXNlKCkgIT09IFwidHJ1ZVwiKVxyXG4gICAgICAgICAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuICAgICAgICBsZXQgdGVtcGxhdGUgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwidGVtcGxhdGVcIik7XHJcbiAgICAgICAgdGVtcGxhdGUuaW5uZXJIVE1MID0gRnVsbFRleHQudGVtcGxhdGU7XHJcbiAgICAgICAgdGhpcy5hdHRhY2hTaGFkb3coeyBtb2RlOiBcIm9wZW5cIiB9KTtcclxuICAgICAgICB0aGlzLnNoYWRvd1Jvb3QuYXBwZW5kKHRlbXBsYXRlLmNvbnRlbnQuY2xvbmVOb2RlKHRydWUpKTtcclxuICAgICAgICB0aGlzLnRleHRCb3ggPSB0aGlzLnNoYWRvd1Jvb3QucXVlcnlTZWxlY3RvcihcIiNlZGl0b3JcIik7XHJcbiAgICAgICAgaWYgKHRoaXMudmFsdWUpXHJcbiAgICAgICAgICAgICh0aGlzLnRleHRCb3ggYXMgSFRNTFRleHRBcmVhRWxlbWVudCkuaW5uZXJUZXh0ID0gdGhpcy52YWx1ZTtcclxuICAgICAgICB0aGlzLmVkaXRvclNldHRpbmdzLnRhcmdldCA9IHRoaXMudGV4dEJveDtcclxuICAgICAgICBpZiAoIXVzZXJTdGF0ZS5pc0ltYWdlRW5hYmxlZClcclxuICAgICAgICAgICAgdGhpcy5lZGl0b3JTZXR0aW5ncy50b29sYmFyID0gRnVsbFRleHQucG9zdE5vSW1hZ2VUb29sYmFyO1xyXG4gICAgICAgIHZhciBzZWxmID0gdGhpcztcclxuICAgICAgICB0aGlzLmVkaXRvclNldHRpbmdzLnNldHVwID0gZnVuY3Rpb24gKGVkaXRvcjogYW55KSB7XHJcbiAgICAgICAgICAgIGVkaXRvci5vbihcImluaXRcIiwgZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgIHRoaXMub24oXCJmb2N1c291dFwiLCBmdW5jdGlvbihlOiBhbnkpIHtcclxuICAgICAgICAgICAgICAgIGVkaXRvci5zYXZlKCk7XHJcbiAgICAgICAgICAgICAgICBzZWxmLnZhbHVlID0gKHNlbGYudGV4dEJveCBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZTtcclxuICAgICAgICAgICAgICAgIChzZWxmLmV4dGVybmFsRm9ybUVsZW1lbnQgYXMgYW55KS52YWx1ZSA9IHNlbGYudmFsdWU7XHJcbiAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgdGhpcy5vbihcImJsdXJcIiwgZnVuY3Rpb24oZTogYW55KSB7XHJcbiAgICAgICAgICAgICAgICBlZGl0b3Iuc2F2ZSgpO1xyXG4gICAgICAgICAgICAgICAgc2VsZi52YWx1ZSA9IChzZWxmLnRleHRCb3ggYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWU7XHJcbiAgICAgICAgICAgICAgICAoc2VsZi5leHRlcm5hbEZvcm1FbGVtZW50IGFzIGFueSkudmFsdWUgPSBzZWxmLnZhbHVlO1xyXG4gICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgIGVkaXRvci5zYXZlKCk7XHJcbiAgICAgICAgICAgICAgc2VsZi52YWx1ZSA9IChzZWxmLnRleHRCb3ggYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWU7XHJcbiAgICAgICAgICAgICAgKHNlbGYuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBhbnkpLnZhbHVlID0gc2VsZi52YWx1ZTtcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICB9O1xyXG4gICAgICAgIHRpbnltY2UuaW5pdCh0aGlzLmVkaXRvclNldHRpbmdzKTtcclxuICAgICAgICB0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwiaW5wdXRcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICB0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQuaWQgPSB0aGlzLmZvcm1JRDtcclxuICAgICAgICB0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQuc2V0QXR0cmlidXRlKFwibmFtZVwiLCB0aGlzLmZvcm1JRCk7XHJcbiAgICAgICAgKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBIVE1MSW5wdXRFbGVtZW50KS50eXBlID0gXCJoaWRkZW5cIjtcclxuICAgICAgICB0aGlzLmFwcGVuZENoaWxkKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCk7XHJcbiAgICAgICAgaWYgKHRoaXMub3ZlcnJpZGVsaXN0ZW5lcj8udG9Mb3dlckNhc2UoKSAhPT0gXCJ0cnVlXCIpXHJcbiAgICAgICAgICAgIHN1cGVyLmNvbm5lY3RlZENhbGxiYWNrKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgIHJldHVybiBbUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLCBcIm5leHRRdW90ZVwiXTtcclxuICAgIH1cclxuXHJcbiAgICB1cGRhdGVVSShkYXRhOiBhbnkpOiB2b2lkIHtcclxuICAgICAgICBpZiAoZGF0YSAhPT0gbnVsbCAmJiBkYXRhICE9PSB1bmRlZmluZWQpXHJcbiAgICAgICAge1xyXG4gICAgICAgICAgICBpZiAodXNlclN0YXRlLmlzUGxhaW5UZXh0KSB7XHJcbiAgICAgICAgICAgICAgICAodGhpcy5leHRlcm5hbEZvcm1FbGVtZW50IGFzIEhUTUxUZXh0QXJlYUVsZW1lbnQpLnZhbHVlICs9IGRhdGE7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnZhbHVlID0gKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBIVE1MVGV4dEFyZWFFbGVtZW50KS52YWx1ZTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICAgICAgIGxldCBlZGl0b3IgPSB0aW55bWNlLmdldChcImVkaXRvclwiKTtcclxuICAgICAgICAgICAgICAgIHZhciBjb250ZW50ID0gZWRpdG9yLmdldENvbnRlbnQoKTtcclxuICAgICAgICAgICAgICAgIGNvbnRlbnQgKz0gZGF0YTtcclxuICAgICAgICAgICAgICAgIGVkaXRvci5zZXRDb250ZW50KGNvbnRlbnQpO1xyXG4gICAgICAgICAgICAgICAgKHRoaXMudGV4dEJveCBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZSArPSBjb250ZW50O1xyXG4gICAgICAgICAgICAgICAgZWRpdG9yLnNhdmUoKTtcclxuICAgICAgICAgICAgICAgIHRoaXMudmFsdWUgPSAodGhpcy50ZXh0Qm94IGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlO1xyXG4gICAgICAgICAgICAgICAgKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZSA9IHRoaXMudmFsdWU7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgXHJcbiAgICBwcml2YXRlIHN0YXRpYyBlZGl0b3JDU1MgPSBcIi9saWIvYm9vdHN0cmFwL2Rpc3QvY3NzL2Jvb3RzdHJhcC5taW4uY3NzLC9saWIvUG9wRm9ydW1zL2Rpc3QvRWRpdG9yLm1pbi5jc3NcIjtcclxuICAgIHByaXZhdGUgc3RhdGljIHBvc3ROb0ltYWdlVG9vbGJhciA9IFwiY3V0IGNvcHkgcGFzdGUgfCBib2xkIGl0YWxpYyB8IGJ1bGxpc3QgbnVtbGlzdCBibG9ja3F1b3RlIHJlbW92ZWZvcm1hdCB8IGxpbmtcIjtcclxuICAgIGVkaXRvclNldHRpbmdzID0ge1xyXG4gICAgICAgIHRhcmdldDogbnVsbCBhcyBIVE1MRWxlbWVudCxcclxuICAgICAgICBwbHVnaW5zOiBcImxpc3RzIGltYWdlIGxpbmtcIixcclxuICAgICAgICBjb250ZW50X2NzczogRnVsbFRleHQuZWRpdG9yQ1NTLFxyXG4gICAgICAgIG1lbnViYXI6IGZhbHNlLFxyXG4gICAgICAgIHRvb2xiYXI6IFwiY3V0IGNvcHkgcGFzdGUgfCBib2xkIGl0YWxpYyB8IGJ1bGxpc3QgbnVtbGlzdCBibG9ja3F1b3RlIHJlbW92ZWZvcm1hdCB8IGxpbmsgfCBpbWFnZVwiLFxyXG4gICAgICAgIHN0YXR1c2JhcjogZmFsc2UsXHJcbiAgICAgICAgbGlua190YXJnZXRfbGlzdDogZmFsc2UsXHJcbiAgICAgICAgbGlua190aXRsZTogZmFsc2UsXHJcbiAgICAgICAgaW1hZ2VfZGVzY3JpcHRpb246IGZhbHNlLFxyXG4gICAgICAgIGltYWdlX2RpbWVuc2lvbnM6IGZhbHNlLFxyXG4gICAgICAgIGltYWdlX3RpdGxlOiBmYWxzZSxcclxuICAgICAgICBpbWFnZV91cGxvYWR0YWI6IGZhbHNlLFxyXG4gICAgICAgIGltYWdlc19maWxlX3R5cGVzOiAnanBlZyxqcGcscG5nLGdpZicsXHJcbiAgICAgICAgYXV0b21hdGljX3VwbG9hZHM6IGZhbHNlLFxyXG4gICAgICAgIGJyb3dzZXJfc3BlbGxjaGVjayA6IHRydWUsXHJcbiAgICAgICAgb2JqZWN0X3Jlc2l6aW5nOiBmYWxzZSxcclxuICAgICAgICByZWxhdGl2ZV91cmxzOiBmYWxzZSxcclxuICAgICAgICByZW1vdmVfc2NyaXB0X2hvc3Q6IGZhbHNlLFxyXG4gICAgICAgIGNvbnRleHRtZW51OiBcIlwiLFxyXG4gICAgICAgIHBhc3RlX2FzX3RleHQ6IHRydWUsXHJcbiAgICAgICAgcGFzdGVfZGF0YV9pbWFnZXM6IGZhbHNlLFxyXG4gICAgICAgIHNldHVwOiBudWxsIGFzIEZ1bmN0aW9uXHJcbiAgICB9O1xyXG5cclxuICAgIHN0YXRpYyBpZDogc3RyaW5nID0gXCJGdWxsVGV4dFwiO1xyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPHRleHRhcmVhIGlkPVwiZWRpdG9yXCI+PC90ZXh0YXJlYT5cclxuICAgIGA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtZnVsbHRleHQnLCBGdWxsVGV4dCk7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIEhvbWVVcGRhdGVyIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgICAgICBzdXBlcigpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgICAgIGxldCBjb25uZWN0aW9uID0gbmV3IHNpZ25hbFIuSHViQ29ubmVjdGlvbkJ1aWxkZXIoKS53aXRoVXJsKFwiL0ZvcnVtc0h1YlwiKS5idWlsZCgpO1xyXG4gICAgICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgICAgIGNvbm5lY3Rpb24ub24oXCJub3RpZnlGb3J1bVVwZGF0ZVwiLCBmdW5jdGlvbiAoZGF0YTogYW55KSB7XHJcbiAgICAgICAgICAgICAgICBzZWxmLnVwZGF0ZUZvcnVtU3RhdHMoZGF0YSk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICBjb25uZWN0aW9uLnN0YXJ0KClcclxuICAgICAgICAgICAgICAgIC50aGVuKGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gY29ubmVjdGlvbi5pbnZva2UoXCJsaXN0ZW5Ub0FsbFwiKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgdXBkYXRlRm9ydW1TdGF0cyhkYXRhOiBhbnkpIHtcclxuICAgICAgICAgICAgbGV0IHJvdyA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCJbZGF0YS1mb3J1bWlkPSdcIiArIGRhdGEuZm9ydW1JRCArIFwiJ11cIik7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLnRvcGljQ291bnRcIikuaW5uZXJIVE1MID0gZGF0YS50b3BpY0NvdW50O1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5wb3N0Q291bnRcIikuaW5uZXJIVE1MID0gZGF0YS5wb3N0Q291bnQ7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLmxhc3RQb3N0VGltZVwiKS5pbm5lckhUTUwgPSBkYXRhLmxhc3RQb3N0VGltZTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIubGFzdFBvc3ROYW1lXCIpLmlubmVySFRNTCA9IGRhdGEubGFzdFBvc3ROYW1lO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5mVGltZVwiKS5zZXRBdHRyaWJ1dGUoXCJkYXRhLXV0Y1wiLCBkYXRhLnV0Yyk7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLm5ld0luZGljYXRvciAuaWNvbi1maWxlLXRleHQyXCIpLmNsYXNzTGlzdC5yZW1vdmUoXCJ0ZXh0LW11dGVkXCIpO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5uZXdJbmRpY2F0b3IgLmljb24tZmlsZS10ZXh0MlwiKS5jbGFzc0xpc3QuYWRkKFwidGV4dC13YXJuaW5nXCIpO1xyXG4gICAgICAgIH07XHJcbiAgICB9XHJcbiAgICBcclxuICAgIGN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtaG9tZXVwZGF0ZXInLCBIb21lVXBkYXRlcik7XHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgTG9naW5Gb3JtIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgICAgICBzdXBlcigpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZ2V0IHRlbXBsYXRlaWQoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInRlbXBsYXRlaWRcIik7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGdldCBpc0V4dGVybmFsTG9naW4oKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImlzZXh0ZXJuYWxsb2dpblwiKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgYnV0dG9uOiBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgIHByaXZhdGUgZW1haWw6IEhUTUxJbnB1dEVsZW1lbnQ7XHJcbiAgICAgICAgcHJpdmF0ZSBwYXNzd29yZDogSFRNTElucHV0RWxlbWVudDtcclxuXHJcbiAgICAgICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgICAgIGxldCB0ZW1wbGF0ZSA9IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKHRoaXMudGVtcGxhdGVpZCkgYXMgSFRNTFRlbXBsYXRlRWxlbWVudDtcclxuICAgICAgICAgICAgaWYgKCF0ZW1wbGF0ZSkge1xyXG4gICAgICAgICAgICAgICAgY29uc29sZS5lcnJvcihgQ2FuJ3QgZmluZCB0ZW1wbGF0ZUlEICR7dGhpcy50ZW1wbGF0ZWlkfSB0byBtYWtlIGxvZ2luIGZvcm0uYCk7XHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgdGhpcy5hcHBlbmQodGVtcGxhdGUuY29udGVudC5jbG9uZU5vZGUodHJ1ZSkpO1xyXG4gICAgICAgICAgICB0aGlzLmVtYWlsID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI0VtYWlsTG9naW5cIik7XHJcbiAgICAgICAgICAgIHRoaXMucGFzc3dvcmQgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjUGFzc3dvcmRMb2dpblwiKTtcclxuICAgICAgICAgICAgdGhpcy5idXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTG9naW5CdXR0b25cIik7XHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmV4ZWN1dGVMb2dpbigpO1xyXG4gICAgICAgICAgICB9KTtcclxuXHRcdFx0dGhpcy5xdWVyeVNlbGVjdG9yQWxsKFwiI0VtYWlsTG9naW4sI1Bhc3N3b3JkTG9naW5cIikuZm9yRWFjaCh4ID0+XHJcblx0XHRcdFx0eC5hZGRFdmVudExpc3RlbmVyKFwia2V5ZG93blwiLCAoZTogS2V5Ym9hcmRFdmVudCkgPT4ge1xyXG5cdFx0XHRcdFx0aWYgKGUuY29kZSA9PT0gXCJFbnRlclwiKSB0aGlzLmV4ZWN1dGVMb2dpbigpO1xyXG5cdFx0XHRcdH0pXHJcbiAgICAgICAgICAgICk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBleGVjdXRlTG9naW4oKSB7XHJcbiAgICAgICAgICAgIGxldCBwYXRoID0gXCIvSWRlbnRpdHkvTG9naW5cIjtcclxuICAgICAgICAgICAgaWYgKHRoaXMuaXNFeHRlcm5hbExvZ2luLnRvTG93ZXJDYXNlKCkgPT09IFwidHJ1ZVwiKVxyXG4gICAgICAgICAgICAgICAgcGF0aCA9IFwiL0lkZW50aXR5L0xvZ2luQW5kQXNzb2NpYXRlXCI7XHJcbiAgICAgICAgICAgIGxldCBwYXlsb2FkID0gSlNPTi5zdHJpbmdpZnkoeyBlbWFpbDogdGhpcy5lbWFpbC52YWx1ZSwgcGFzc3dvcmQ6IHRoaXMucGFzc3dvcmQudmFsdWUgfSk7XHJcbiAgICAgICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIHBhdGgsIHtcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCIsXHJcbiAgICAgICAgICAgICAgICBoZWFkZXJzOiB7XHJcbiAgICAgICAgICAgICAgICAgICAgJ0NvbnRlbnQtVHlwZSc6ICdhcHBsaWNhdGlvbi9qc29uJ1xyXG4gICAgICAgICAgICAgICAgfSxcclxuICAgICAgICAgICAgICAgIGJvZHk6IHBheWxvYWRcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC50aGVuKGZ1bmN0aW9uKHJlc3BvbnNlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIHJlc3BvbnNlLmpzb24oKTtcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC50aGVuKGZ1bmN0aW9uIChyZXN1bHQpIHtcclxuICAgICAgICAgICAgICAgICAgICBzd2l0Y2ggKHJlc3VsdC5yZXN1bHQpIHtcclxuICAgICAgICAgICAgICAgICAgICBjYXNlIHRydWU6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGxldCBkZXN0aW5hdGlvbiA9IChkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI1JlZmVycmVyXCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBsb2NhdGlvbi5ocmVmID0gZGVzdGluYXRpb247XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgICAgIGRlZmF1bHQ6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGxldCBsb2dpblJlc3VsdCA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjTG9naW5SZXN1bHRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGxvZ2luUmVzdWx0LmlubmVySFRNTCA9IHJlc3VsdC5tZXNzYWdlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBsb2dpblJlc3VsdC5jbGFzc0xpc3QucmVtb3ZlKFwiZC1ub25lXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC5jYXRjaChmdW5jdGlvbiAoZXJyb3IpIHtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgbG9naW5SZXN1bHQgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI0xvZ2luUmVzdWx0XCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIGxvZ2luUmVzdWx0LmlubmVySFRNTCA9IFwiVGhlcmUgd2FzIGFuIHVua25vd24gZXJyb3Igd2hpbGUgYXR0ZW1wdGluZyBsb2dpblwiO1xyXG4gICAgICAgICAgICAgICAgICAgIGxvZ2luUmVzdWx0LmNsYXNzTGlzdC5yZW1vdmUoXCJkLW5vbmVcIik7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuICAgIH1cclxuICAgIFxyXG4gICAgY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1sb2dpbmZvcm0nLCBMb2dpbkZvcm0pO1xyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIE1vcmVQb3N0c0JlZm9yZVJlcGx5QnV0dG9uIGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuICAgIGdldCBidXR0b250ZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9udGV4dFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IE1vcmVQb3N0c0JlZm9yZVJlcGx5QnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuYnV0dG9udGV4dDtcclxuICAgICAgICBpZiAodGhpcy5idXR0b25jbGFzcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgdGhpcy5idXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKGU6IE1vdXNlRXZlbnQpID0+IHtcclxuICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLmxvYWRNb3JlUG9zdHMoKTtcclxuICAgICAgICB9KTtcclxuICAgICAgICBzdXBlci5jb25uZWN0ZWRDYWxsYmFjaygpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZSwgXCJpc05ld2VyUG9zdHNBdmFpbGFibGVcIl07XHJcbiAgICB9XHJcbiAgICBcclxuICAgIHVwZGF0ZVVJKGRhdGE6IGJvb2xlYW4pOiB2b2lkIHtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgaWYgKCFkYXRhKVxyXG4gICAgICAgICAgICBidXR0b24uc3R5bGUudmlzaWJpbGl0eSA9IFwiaGlkZGVuXCI7XHJcbiAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICBidXR0b24uc3R5bGUudmlzaWJpbGl0eSA9IFwidmlzaWJsZVwiO1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgLz5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLW1vcmVwb3N0c2JlZm9yZXJlcGx5YnV0dG9uJywgTW9yZVBvc3RzQmVmb3JlUmVwbHlCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBNb3JlUG9zdHNCdXR0b24gZXh0ZW5kcyBFbGVtZW50QmFzZSB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBidXR0b25jbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbmNsYXNzXCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IGJ1dHRvbnRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b250ZXh0XCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIHRoaXMuaW5uZXJIVE1MID0gTW9yZVBvc3RzQnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuYnV0dG9udGV4dDtcclxuICAgICAgICBpZiAodGhpcy5idXR0b25jbGFzcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgdGhpcy5idXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKGU6IE1vdXNlRXZlbnQpID0+IHtcclxuICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLmxvYWRNb3JlUG9zdHMoKTtcclxuICAgICAgICB9KTtcclxuICAgICAgICBzdXBlci5jb25uZWN0ZWRDYWxsYmFjaygpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZSwgXCJoaWdoUGFnZVwiXTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgdXBkYXRlVUkoZGF0YTogbnVtYmVyKTogdm9pZCB7XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpO1xyXG4gICAgICAgIGlmIChQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUucGFnZUNvdW50ID09PSAxIHx8IGRhdGEgPT09IFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5wYWdlQ291bnQpXHJcbiAgICAgICAgICAgIGJ1dHRvbi5zdHlsZS52aXNpYmlsaXR5ID0gXCJoaWRkZW5cIjtcclxuICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgIGJ1dHRvbi5zdHlsZS52aXNpYmlsaXR5ID0gXCJ2aXNpYmxlXCI7XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGlucHV0IHR5cGU9XCJidXR0b25cIiAvPmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtbW9yZXBvc3RzYnV0dG9uJywgTW9yZVBvc3RzQnV0dG9uKTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgUE1Db3VudCBleHRlbmRzIEVsZW1lbnRCYXNlIHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgIHJldHVybiBbUG9wRm9ydW1zLnVzZXJTdGF0ZSwgXCJuZXdQbUNvdW50XCJdO1xyXG4gICAgfVxyXG5cclxuICAgIHVwZGF0ZVVJKGRhdGE6IG51bWJlcik6IHZvaWQge1xyXG4gICAgICAgIGlmIChkYXRhID09PSAwKVxyXG4gICAgICAgICAgICB0aGlzLmlubmVySFRNTCA9IFwiXCI7XHJcbiAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICB0aGlzLmlubmVySFRNTCA9IGA8c3BhbiBjbGFzcz1cImJhZGdlXCI+JHtkYXRhfTwvc3Bhbj5gO1xyXG4gICAgfVxyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXBtY291bnQnLCBQTUNvdW50KTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgUG9zdE1pbmlQcm9maWxlIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgdXNlcm5hbWUoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ1c2VybmFtZVwiKTtcclxuICAgIH1cclxuICAgIGdldCB1c2VybmFtZWNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidXNlcm5hbWVjbGFzc1wiKTtcclxuICAgIH1cclxuICAgIGdldCB1c2VyaWQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ1c2VyaWRcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgbWluaVByb2ZpbGVCb3hDbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcIm1pbmlwcm9maWxlYm94Y2xhc3NcIik7XHJcbiAgICB9XHJcblxyXG4gICAgcHJpdmF0ZSBpc09wZW46IGJvb2xlYW47XHJcbiAgICBwcml2YXRlIGJveDogSFRNTEVsZW1lbnQ7XHJcbiAgICBwcml2YXRlIGJveEhlaWdodDogc3RyaW5nO1xyXG4gICAgcHJpdmF0ZSBpc0xvYWRlZDogYm9vbGVhbjtcclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlzTG9hZGVkID0gZmFsc2U7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBQb3N0TWluaVByb2ZpbGUudGVtcGxhdGU7XHJcbiAgICAgICAgbGV0IG5hbWVIZWFkZXIgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJoM1wiKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICB0aGlzLnVzZXJuYW1lY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IG5hbWVIZWFkZXIuY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgbmFtZUhlYWRlci5pbm5lckhUTUwgPSB0aGlzLnVzZXJuYW1lO1xyXG4gICAgICAgIG5hbWVIZWFkZXIuYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IHtcclxuICAgICAgICAgICAgdGhpcy50b2dnbGUoKTtcclxuICAgICAgICB9KTtcclxuICAgICAgICB0aGlzLmJveCA9IHRoaXMucXVlcnlTZWxlY3RvcihcImRpdlwiKTtcclxuICAgICAgICB0aGlzLm1pbmlQcm9maWxlQm94Q2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IHRoaXMuYm94LmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgfVxyXG5cclxuICAgIHByaXZhdGUgdG9nZ2xlKCkge1xyXG4gICAgICAgIGlmICghdGhpcy5pc0xvYWRlZCkge1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9BY2NvdW50L01pbmlQcm9maWxlL1wiICsgdGhpcy51c2VyaWQpXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS50ZXh0KClcclxuICAgICAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbGV0IHN1YiA9IHRoaXMuYm94LnF1ZXJ5U2VsZWN0b3IoXCJkaXZcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHN1Yi5pbm5lckhUTUwgPSB0ZXh0O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjb25zdCBoZWlnaHQgPSBzdWIuZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCkuaGVpZ2h0O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmJveEhlaWdodCA9IGAke2hlaWdodH1weGA7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuYm94LnN0eWxlLmhlaWdodCA9IHRoaXMuYm94SGVpZ2h0O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmlzT3BlbiA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuaXNMb2FkZWQgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgICAgIH0pKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZWxzZSBpZiAoIXRoaXMuaXNPcGVuKSB7XHJcbiAgICAgICAgICAgIHRoaXMuYm94LnN0eWxlLmhlaWdodCA9IHRoaXMuYm94SGVpZ2h0O1xyXG4gICAgICAgICAgICB0aGlzLmlzT3BlbiA9IHRydWU7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGVsc2Uge1xyXG4gICAgICAgICAgICB0aGlzLmJveC5zdHlsZS5oZWlnaHQgPSBcIjBcIjtcclxuICAgICAgICAgICAgdGhpcy5pc09wZW4gPSBmYWxzZTtcclxuICAgICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGgzPjwvaDM+XHJcbjxkaXY+XHJcbiAgICA8ZGl2IGNsYXNzPVwicHktMSBweC0zIG1iLTJcIj48L2Rpdj5cclxuPC9kaXY+YDtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1wb3N0bWluaXByb2ZpbGUnLCBQb3N0TWluaVByb2ZpbGUpO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBQb3N0TW9kZXJhdGlvbkxvZ0J1dHRvbiBleHRlbmRzIEhUTUxFbGVtZW50IHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbnRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b250ZXh0XCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBwb3N0aWQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJwb3N0aWRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IHBhcmVudFNlbGVjdG9yVG9BcHBlbmRUbygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInBhcmVudHNlbGVjdG9ydG9hcHBlbmR0b1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFRvcGljTW9kZXJhdGlvbkxvZ0J1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgYnV0dG9uLnZhbHVlID0gdGhpcy5idXR0b250ZXh0O1xyXG4gICAgICAgIGxldCBjbGFzc2VzID0gdGhpcy5idXR0b25jbGFzcztcclxuICAgICAgICBpZiAoY2xhc3Nlcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgY2xhc3Nlcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIGxldCBzZWxmID0gdGhpcztcclxuICAgICAgICBsZXQgY29udGFpbmVyOiBIVE1MRGl2RWxlbWVudDtcclxuICAgICAgICBidXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IHtcclxuICAgICAgICAgICAgaWYgKCFjb250YWluZXIpIHtcclxuICAgICAgICAgICAgICAgIGxldCBwYXJlbnRDb250YWluZXIgPSBzZWxmLmNsb3Nlc3QodGhpcy5wYXJlbnRTZWxlY3RvclRvQXBwZW5kVG8pO1xyXG4gICAgICAgICAgICAgICAgaWYgKCFwYXJlbnRDb250YWluZXIpIHtcclxuICAgICAgICAgICAgICAgICAgICBjb25zb2xlLmVycm9yKGBDYW4ndCBmaW5kIGEgcGFyZW50IHNlbGVjdG9yIFwiJHt0aGlzLnBhcmVudFNlbGVjdG9yVG9BcHBlbmRUb31cIiB0byBhcHBlbmQgcG9zdCBtb2RlcmF0aW9uIGxvZyB0by5gKTtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBjb250YWluZXIgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwiZGl2XCIpO1xyXG4gICAgICAgICAgICAgICAgcGFyZW50Q29udGFpbmVyLmFwcGVuZENoaWxkKGNvbnRhaW5lcik7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgaWYgKGNvbnRhaW5lci5zdHlsZS5kaXNwbGF5ICE9PSBcImJsb2NrXCIpXHJcbiAgICAgICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Nb2RlcmF0b3IvUG9zdE1vZGVyYXRpb25Mb2cvXCIgKyB0aGlzLnBvc3RpZClcclxuICAgICAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS50ZXh0KClcclxuICAgICAgICAgICAgICAgICAgICAgICAgLnRoZW4odGV4dCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb250YWluZXIuaW5uZXJIVE1MID0gdGV4dDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNvbnRhaW5lci5zdHlsZS5kaXNwbGF5ID0gXCJibG9ja1wiO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICAgICAgICAgIGVsc2UgY29udGFpbmVyLnN0eWxlLmRpc3BsYXkgPSBcIm5vbmVcIjtcclxuICAgICAgICB9KTtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIC8+YDtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKFwicGYtcG9zdG1vZGVyYXRpb25sb2didXR0b25cIiwgUG9zdE1vZGVyYXRpb25Mb2dCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBQcmV2aWV3QnV0dG9uIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgbGFiZWxUZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwibGFiZWx0ZXh0XCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IHRleHRTb3VyY2VTZWxlY3RvcigpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInRleHRzb3VyY2VzZWxlY3RvclwiKTtcclxuICAgIH1cclxuICAgIGdldCBpc1BsYWluVGV4dFNlbGVjdG9yKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiaXNwbGFpbnRleHRzZWxlY3RvclwiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFByZXZpZXdCdXR0b24udGVtcGxhdGU7XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpIGFzIEhUTUxCdXR0b25FbGVtZW50O1xyXG4gICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMubGFiZWxUZXh0O1xyXG4gICAgICAgIGxldCBoZWFkVGV4dCA9IHRoaXMucXVlcnlTZWxlY3RvcihcImg0XCIpIGFzIEhUTUxIZWFkRWxlbWVudDtcclxuICAgICAgICBoZWFkVGV4dC5pbm5lclRleHQgPSB0aGlzLmxhYmVsVGV4dDtcclxuICAgICAgICB2YXIgbW9kYWwgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIubW9kYWxcIik7XHJcbiAgICAgICAgbW9kYWwuYWRkRXZlbnRMaXN0ZW5lcihcInNob3duLmJzLm1vZGFsXCIsICgpID0+IHtcclxuICAgICAgICAgICAgdGhpcy5vcGVuTW9kYWwoKTtcclxuICAgICAgICB9KTtcclxuICAgIH1cclxuXHJcbiAgICBvcGVuTW9kYWwoKSB7XHJcbiAgICAgICAgdGlueW1jZS50cmlnZ2VyU2F2ZSgpO1xyXG4gICAgICAgIGxldCBmdWxsVGV4dCA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IodGhpcy50ZXh0U291cmNlU2VsZWN0b3IpIGFzIGFueTtcclxuICAgICAgICBsZXQgbW9kZWwgPSB7XHJcbiAgICAgICAgICAgIEZ1bGxUZXh0OiBmdWxsVGV4dC52YWx1ZSxcclxuICAgICAgICAgICAgSXNQbGFpblRleHQ6IChkb2N1bWVudC5xdWVyeVNlbGVjdG9yKHRoaXMuaXNQbGFpblRleHRTZWxlY3RvcikgYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWUudG9Mb3dlckNhc2UoKSA9PT0gXCJ0cnVlXCJcclxuICAgICAgICB9O1xyXG4gICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0ZvcnVtL1ByZXZpZXdUZXh0XCIsIHtcclxuICAgICAgICAgICAgbWV0aG9kOiBcIlBPU1RcIixcclxuICAgICAgICAgICAgYm9keTogSlNPTi5zdHJpbmdpZnkobW9kZWwpLFxyXG4gICAgICAgICAgICBoZWFkZXJzOiB7XHJcbiAgICAgICAgICAgICAgICBcIkNvbnRlbnQtVHlwZVwiOiBcImFwcGxpY2F0aW9uL2pzb25cIlxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSlcclxuICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgciA9IHRoaXMucXVlcnlTZWxlY3RvcihcIi5wYXJzZWRGdWxsVGV4dFwiKSBhcyBIVE1MRGl2RWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICByLmlubmVySFRNTCA9IHRleHQ7XHJcbiAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGlucHV0IHR5cGU9XCJidXR0b25cIiBjbGFzcz1cImJ0biBidG4tcHJpbWFyeVwiIGRhdGEtYnMtdG9nZ2xlPVwibW9kYWxcIiBkYXRhLWJzLXRhcmdldD1cIiNQcmV2aWV3TW9kYWxcIj5cclxuPGRpdiBjbGFzcz1cIm1vZGFsIGZhZGVcIiBpZD1cIlByZXZpZXdNb2RhbFwiIHRhYmluZGV4PVwiLTFcIiByb2xlPVwiZGlhbG9nXCI+XHJcblx0PGRpdiBjbGFzcz1cIm1vZGFsLWRpYWxvZ1wiPlxyXG5cdFx0PGRpdiBjbGFzcz1cIm1vZGFsLWNvbnRlbnRcIj5cclxuXHRcdFx0PGRpdiBjbGFzcz1cIm1vZGFsLWhlYWRlclwiPlxyXG5cdFx0XHRcdDxoNCBjbGFzcz1cIm1vZGFsLXRpdGxlXCI+PC9oND5cclxuXHRcdFx0XHQ8YnV0dG9uIHR5cGU9XCJidXR0b25cIiBjbGFzcz1cImJ0bi1jbG9zZVwiIGRhdGEtYnMtZGlzbWlzcz1cIm1vZGFsXCIgYXJpYS1sYWJlbD1cIkNsb3NlXCI+PC9idXR0b24+XHJcblx0XHRcdDwvZGl2PlxyXG5cdFx0XHQ8ZGl2IGNsYXNzPVwibW9kYWwtYm9keVwiPlxyXG5cdFx0XHRcdDxkaXYgY2xhc3M9XCJwb3N0SXRlbSBwYXJzZWRGdWxsVGV4dFwiPjwvZGl2PlxyXG5cdFx0XHQ8L2Rpdj5cclxuXHRcdFx0PGRpdiBjbGFzcz1cIm1vZGFsLWZvb3RlclwiPlxyXG5cdFx0XHRcdDxidXR0b24gdHlwZT1cImJ1dHRvblwiIGNsYXNzPVwiYnRuIGJ0bi1wcmltYXJ5XCIgZGF0YS1icy1kaXNtaXNzPVwibW9kYWxcIj5DbG9zZTwvYnV0dG9uPlxyXG5cdFx0XHQ8L2Rpdj5cclxuXHRcdDwvZGl2PlxyXG5cdDwvZGl2PlxyXG48L2Rpdj5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXByZXZpZXdidXR0b24nLCBQcmV2aWV3QnV0dG9uKTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgUHJldmlvdXNQb3N0c0J1dHRvbiBleHRlbmRzIEVsZW1lbnRCYXNlIHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgYnV0dG9udGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbnRleHRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBQcmV2aW91c1Bvc3RzQnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuYnV0dG9udGV4dDtcclxuICAgICAgICBpZiAodGhpcy5idXR0b25jbGFzcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgdGhpcy5idXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKGU6IE1vdXNlRXZlbnQpID0+IHtcclxuICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLmxvYWRQcmV2aW91c1Bvc3RzKCk7XHJcbiAgICAgICAgfSk7XHJcbiAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXREZXBlbmRlbnRSZWZlcmVuY2UoKTogW1N0YXRlQmFzZSwgc3RyaW5nXSB7XHJcbiAgICAgICAgcmV0dXJuIFtQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUsIFwibG93UGFnZVwiXTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgdXBkYXRlVUkoZGF0YTogbnVtYmVyKTogdm9pZCB7XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpO1xyXG4gICAgICAgIGlmIChQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUucGFnZUNvdW50ID09PSAxIHx8IGRhdGEgPT09IDEpXHJcbiAgICAgICAgICAgIGJ1dHRvbi5zdHlsZS52aXNpYmlsaXR5ID0gXCJoaWRkZW5cIjtcclxuICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgIGJ1dHRvbi5zdHlsZS52aXNpYmlsaXR5ID0gXCJ2aXNpYmxlXCI7XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGlucHV0IHR5cGU9XCJidXR0b25cIiAvPmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtcHJldmlvdXNwb3N0c2J1dHRvbicsIFByZXZpb3VzUG9zdHNCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBRdW90ZUJ1dHRvbiBleHRlbmRzIEhUTUxFbGVtZW50IHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IG5hbWUoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJuYW1lXCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IGNvbnRhaW5lcmlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiY29udGFpbmVyaWRcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuICAgIGdldCBidXR0b250ZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9udGV4dFwiKTtcclxuICAgIH1cclxuICAgIGdldCB0aXAoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0aXBcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgcG9zdElEKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwicG9zdGlkXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIHByaXZhdGUgX3RpcDogYW55O1xyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIGxldCB0YXJnZXRUZXh0ID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQodGhpcy5jb250YWluZXJpZCk7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBRdW90ZUJ1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgYnV0dG9uLnRpdGxlID0gdGhpcy50aXA7XHJcbiAgICAgICAgW1wibW91c2Vkb3duXCIsXCJ0b3VjaHN0YXJ0XCJdLmZvckVhY2goKGU6c3RyaW5nKSA9PiBcclxuICAgICAgICAgICAgdGFyZ2V0VGV4dC5hZGRFdmVudExpc3RlbmVyKGUsICgpID0+IFxyXG4gICAgICAgICAgICAgICAgeyBpZiAodGhpcy5fdGlwKSB0aGlzLl90aXAuaGlkZSgpIH0pKTtcclxuICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmJ1dHRvbnRleHQ7XHJcbiAgICAgICAgbGV0IGNsYXNzZXMgPSB0aGlzLmJ1dHRvbmNsYXNzO1xyXG4gICAgICAgIGlmIChjbGFzc2VzPy5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICBjbGFzc2VzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiBidXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgdGhpcy5vbmNsaWNrID0gKGU6IE1vdXNlRXZlbnQpID0+IHtcclxuICAgICAgICAgICAgbGV0IHNlbGVjdGlvbiA9IGRvY3VtZW50LmdldFNlbGVjdGlvbigpO1xyXG4gICAgICAgICAgICBpZiAoc2VsZWN0aW9uLnJhbmdlQ291bnQgPT09IDAgfHwgc2VsZWN0aW9uLmdldFJhbmdlQXQoMCkudG9TdHJpbmcoKS5sZW5ndGggPT09IDApIHtcclxuICAgICAgICAgICAgICAgIC8vIHByb21wdCB0byBzZWxlY3RcclxuICAgICAgICAgICAgICAgIHRoaXMuX3RpcCA9IG5ldyBib290c3RyYXAuVG9vbHRpcChidXR0b24sIHt0cmlnZ2VyOiBcIm1hbnVhbFwifSk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLl90aXAuc2hvdygpO1xyXG4gICAgICAgICAgICAgICAgc2VsZWN0aW9uLnJlbW92ZUFsbFJhbmdlcygpO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGxldCByYW5nZSA9IHNlbGVjdGlvbi5nZXRSYW5nZUF0KDApO1xyXG4gICAgICAgICAgICBsZXQgZnJhZ21lbnQgPSByYW5nZS5jbG9uZUNvbnRlbnRzKCk7XHJcbiAgICAgICAgICAgIGxldCBkaXYgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwiZGl2XCIpO1xyXG4gICAgICAgICAgICBkaXYuYXBwZW5kQ2hpbGQoZnJhZ21lbnQpO1xyXG4gICAgICAgICAgICAvLyBpcyBzZWxlY3Rpb24gaW4gdGhlIGNvbnRhaW5lcj9cclxuICAgICAgICAgICAgbGV0IGFuY2VzdG9yID0gcmFuZ2UuY29tbW9uQW5jZXN0b3JDb250YWluZXI7XHJcbiAgICAgICAgICAgIHdoaWxlIChhbmNlc3RvclsnaWQnXSAhPT0gdGhpcy5jb250YWluZXJpZCAmJiBhbmNlc3Rvci5wYXJlbnRFbGVtZW50ICE9PSBudWxsKSB7XHJcbiAgICAgICAgICAgICAgICBhbmNlc3RvciA9IGFuY2VzdG9yLnBhcmVudEVsZW1lbnQ7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgbGV0IGlzSW5UZXh0ID0gYW5jZXN0b3JbJ2lkJ10gPT09IHRoaXMuY29udGFpbmVyaWQ7XHJcbiAgICAgICAgICAgIC8vIGlmIG5vdCwgaXMgaXQgcGFydGlhbGx5IGluIHRoZSBjb250YWluZXI/XHJcbiAgICAgICAgICAgIGlmICghaXNJblRleHQpIHtcclxuICAgICAgICAgICAgICAgIGxldCBjb250YWluZXIgPSBkaXYucXVlcnlTZWxlY3RvcihcIiNcIiArIHRoaXMuY29udGFpbmVyaWQpO1xyXG4gICAgICAgICAgICAgICAgaWYgKGNvbnRhaW5lciAhPT0gbnVsbCAmJiBjb250YWluZXIgIT09IHVuZGVmaW5lZCkge1xyXG4gICAgICAgICAgICAgICAgICAgIC8vIGl0J3MgcGFydGlhbGx5IGluIHRoZSBjb250YWluZXIsIHNvIGp1c3QgZ2V0IHRoYXQgcGFydFxyXG4gICAgICAgICAgICAgICAgICAgIGRpdi5pbm5lckhUTUwgPSBjb250YWluZXIuaW5uZXJIVE1MO1xyXG4gICAgICAgICAgICAgICAgICAgIGlzSW5UZXh0ID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBzZWxlY3Rpb24ucmVtb3ZlQWxsUmFuZ2VzKCk7XHJcbiAgICAgICAgICAgIGlmIChpc0luVGV4dCkge1xyXG4gICAgICAgICAgICAgICAgLy8gYWN0aXZhdGUgb3IgYWRkIHRvIHF1b3RlXHJcbiAgICAgICAgICAgICAgICBsZXQgcmVzdWx0OiBzdHJpbmc7XHJcbiAgICAgICAgICAgICAgICBpZiAodXNlclN0YXRlLmlzUGxhaW5UZXh0KVxyXG4gICAgICAgICAgICAgICAgICAgIHJlc3VsdCA9IGBbcXVvdGVdW2ldJHt0aGlzLm5hbWV9OlsvaV1cXHJcXG4gJHtkaXYuaW5uZXJUZXh0fVsvcXVvdGVdYDtcclxuICAgICAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICByZXN1bHQgPSBgPGJsb2NrcXVvdGU+PHA+PGk+JHt0aGlzLm5hbWV9OjwvaT48L3A+JHtkaXYuaW5uZXJIVE1MfTwvYmxvY2txdW90ZT48cD48L3A+YDtcclxuICAgICAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5uZXh0UXVvdGUgPSByZXN1bHQ7XHJcbiAgICAgICAgICAgICAgICBpZiAoIVBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5pc1JlcGx5TG9hZGVkKVxyXG4gICAgICAgICAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5sb2FkUmVwbHkoUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLnRvcGljSUQsIE51bWJlcih0aGlzLnBvc3RJRCksIHRydWUpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfTtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIGRhdGEtYnMtdG9nZ2xlPVwidG9vbHRpcFwiIHRpdGxlPVwiXCIgLz5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXF1b3RlYnV0dG9uJywgUXVvdGVCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBSZXBseUJ1dHRvbiBleHRlbmRzIEVsZW1lbnRCYXNlIHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbnRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b250ZXh0XCIpO1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICBnZXQgdG9waWNpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInRvcGljaWRcIik7XHJcbiAgICB9XHJcbiAgICBcclxuICAgIGdldCBwb3N0aWQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJwb3N0aWRcIik7XHJcbiAgICB9XHJcbiAgICBcclxuICAgIGdldCBvdmVycmlkZWRpc3BsYXkoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJvdmVycmlkZWRpc3BsYXlcIik7XHJcbiAgICB9XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBSZXBseUJ1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmJ1dHRvbnRleHQ7XHJcbiAgICAgICAgaWYgKHRoaXMuYnV0dG9uY2xhc3M/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IGJ1dHRvbi5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICBidXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIChlOiBNb3VzZUV2ZW50KSA9PiB7XHJcbiAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5sb2FkUmVwbHkoTnVtYmVyKHRoaXMudG9waWNpZCksIE51bWJlcih0aGlzLnBvc3RpZCksIHRydWUpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgICAgIHN1cGVyLmNvbm5lY3RlZENhbGxiYWNrKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgIHJldHVybiBbUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLCBcImlzUmVwbHlMb2FkZWRcIl07XHJcbiAgICB9XHJcbiAgICBcclxuICAgIHVwZGF0ZVVJKGRhdGE6IGJvb2xlYW4pOiB2b2lkIHtcclxuICAgICAgICBpZiAodGhpcy5vdmVycmlkZWRpc3BsYXk/LnRvTG93ZXJDYXNlKCkgPT09IFwidHJ1ZVwiKVxyXG4gICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpO1xyXG4gICAgICAgIGlmIChkYXRhKVxyXG4gICAgICAgICAgICBidXR0b24uc3R5bGUuZGlzcGxheSA9IFwibm9uZVwiO1xyXG4gICAgICAgIGVsc2VcclxuICAgICAgICAgICAgYnV0dG9uLnN0eWxlLmRpc3BsYXkgPSBcImluaXRpYWxcIjtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIC8+YDtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1yZXBseWJ1dHRvbicsIFJlcGx5QnV0dG9uKTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgUmVwbHlGb3JtIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgICAgICBzdXBlcigpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZ2V0IHRlbXBsYXRlSUQoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInRlbXBsYXRlaWRcIik7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBwcml2YXRlIGJ1dHRvbjogSFRNTElucHV0RWxlbWVudDtcclxuXHJcbiAgICAgICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgICAgIGxldCB0ZW1wbGF0ZSA9IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKHRoaXMudGVtcGxhdGVJRCkgYXMgSFRNTFRlbXBsYXRlRWxlbWVudDtcclxuICAgICAgICAgICAgaWYgKCF0ZW1wbGF0ZSkge1xyXG4gICAgICAgICAgICAgICAgY29uc29sZS5lcnJvcihgQ2FuJ3QgZmluZCB0ZW1wbGF0ZUlEICR7dGhpcy50ZW1wbGF0ZUlEfSB0byBtYWtlIHJlcGx5IGZvcm0uYCk7XHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgdGhpcy5hcHBlbmQodGVtcGxhdGUuY29udGVudC5jbG9uZU5vZGUodHJ1ZSkpO1xyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcIiNTdWJtaXRSZXBseVwiKTtcclxuICAgICAgICAgICAgdGhpcy5idXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IHtcclxuICAgICAgICAgICAgICAgIHRoaXMuc3VibWl0UmVwbHkoKTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBzdWJtaXRSZXBseSgpIHtcclxuICAgICAgICAgICAgdGhpcy5idXR0b24uc2V0QXR0cmlidXRlKFwiZGlzYWJsZWRcIiwgXCJkaXNhYmxlZFwiKTtcclxuICAgICAgICAgICAgdmFyIGNsb3NlQ2hlY2sgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI0Nsb3NlT25SZXBseVwiKSBhcyBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgICAgICB2YXIgY2xvc2VPblJlcGx5ID0gZmFsc2U7XHJcbiAgICAgICAgICAgIGlmIChjbG9zZUNoZWNrICYmIGNsb3NlQ2hlY2suY2hlY2tlZCkgY2xvc2VPblJlcGx5ID0gdHJ1ZTtcclxuICAgICAgICAgICAgdmFyIG1vZGVsID0ge1xyXG4gICAgICAgICAgICAgICAgVGl0bGU6ICh0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTmV3UmVwbHkgI1RpdGxlXCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlLFxyXG4gICAgICAgICAgICAgICAgRnVsbFRleHQ6ICh0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTmV3UmVwbHkgI0Z1bGxUZXh0XCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlLFxyXG4gICAgICAgICAgICAgICAgSW5jbHVkZVNpZ25hdHVyZTogKHRoaXMucXVlcnlTZWxlY3RvcihcIiNOZXdSZXBseSAjSW5jbHVkZVNpZ25hdHVyZVwiKSBhcyBIVE1MSW5wdXRFbGVtZW50KS5jaGVja2VkLFxyXG4gICAgICAgICAgICAgICAgSXRlbUlEOiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1JlcGx5ICNJdGVtSURcIikgYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWUsXHJcbiAgICAgICAgICAgICAgICBDbG9zZU9uUmVwbHk6IGNsb3NlT25SZXBseSxcclxuICAgICAgICAgICAgICAgIElzUGxhaW5UZXh0OiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1JlcGx5ICNJc1BsYWluVGV4dFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZS50b0xvd2VyQ2FzZSgpID09PSBcInRydWVcIixcclxuICAgICAgICAgICAgICAgIFBhcmVudFBvc3RJRDogKHRoaXMucXVlcnlTZWxlY3RvcihcIiNOZXdSZXBseSAjUGFyZW50UG9zdElEXCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlXHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0ZvcnVtL1Bvc3RSZXBseVwiLCB7XHJcbiAgICAgICAgICAgICAgICBtZXRob2Q6IFwiUE9TVFwiLFxyXG4gICAgICAgICAgICAgICAgYm9keTogSlNPTi5zdHJpbmdpZnkobW9kZWwpLFxyXG4gICAgICAgICAgICAgICAgaGVhZGVyczoge1xyXG4gICAgICAgICAgICAgICAgICAgIFwiQ29udGVudC1UeXBlXCI6IFwiYXBwbGljYXRpb24vanNvblwiXHJcbiAgICAgICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UuanNvbigpKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4ocmVzdWx0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBzd2l0Y2ggKHJlc3VsdC5yZXN1bHQpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSB0cnVlOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgd2luZG93LmxvY2F0aW9uID0gcmVzdWx0LnJlZGlyZWN0O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRlZmF1bHQ6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgciA9IHRoaXMucXVlcnlTZWxlY3RvcihcIiNQb3N0UmVzcG9uc2VNZXNzYWdlXCIpIGFzIEhUTUxFbGVtZW50O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgci5pbm5lckhUTUwgPSByZXN1bHQubWVzc2FnZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLnJlbW92ZUF0dHJpYnV0ZShcImRpc2FibGVkXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgci5zdHlsZS5kaXNwbGF5ID0gXCJibG9ja1wiO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAuY2F0Y2goZXJyb3IgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciByID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI1Bvc3RSZXNwb25zZU1lc3NhZ2VcIikgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgICAgICAgICAgICAgci5pbm5lckhUTUwgPSBcIlRoZXJlIHdhcyBhbiB1bmtub3duIGVycm9yIHdoaWxlIHRyeWluZyB0byBwb3N0XCI7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24ucmVtb3ZlQXR0cmlidXRlKFwiZGlzYWJsZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgci5zdHlsZS5kaXNwbGF5ID0gXCJibG9ja1wiO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1yZXBseWZvcm0nLCBSZXBseUZvcm0pO1xyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFN1YnNjcmliZUJ1dHRvbiBleHRlbmRzIEVsZW1lbnRCYXNlIHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICB9XHJcbiAgICBcclxuICAgIGdldCBzdWJzY3JpYmV0ZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwic3Vic2NyaWJldGV4dFwiKTtcclxuICAgIH1cclxuICAgIGdldCB1bnN1YnNjcmliZXRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ1bnN1YnNjcmliZXRleHRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBTdWJzY3JpYmVCdXR0b24udGVtcGxhdGU7XHJcbiAgICAgICAgbGV0IGJ1dHRvbjogSFRNTElucHV0RWxlbWVudCA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpO1xyXG4gICAgICAgIHRoaXMuYnV0dG9uY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IGJ1dHRvbi5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICBidXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IHtcclxuICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvU3Vic2NyaXB0aW9uL1RvZ2dsZVN1YnNjcmlwdGlvbi9cIiArIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS50b3BpY0lELCB7XHJcbiAgICAgICAgICAgICAgICBtZXRob2Q6IFwiUE9TVFwiXHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS5qc29uKCkpXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXN1bHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIHN3aXRjaCAocmVzdWx0LmRhdGEuaXNTdWJzY3JpYmVkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGNhc2UgdHJ1ZTpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5pc1N1YnNjcmliZWQgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGNhc2UgZmFsc2U6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUuaXNTdWJzY3JpYmVkID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgZGVmYXVsdDpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vIFRPRE86IHNvbWV0aGluZyBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC5jYXRjaCgoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgLy8gVE9ETzogaGFuZGxlIGVycm9yXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9KTtcclxuICAgICAgICBzdXBlci5jb25uZWN0ZWRDYWxsYmFjaygpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZSwgXCJpc1N1YnNjcmliZWRcIl07XHJcbiAgICB9XHJcblxyXG4gICAgdXBkYXRlVUkoZGF0YTogYm9vbGVhbik6IHZvaWQge1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICBpZiAoZGF0YSlcclxuICAgICAgICAgICAgYnV0dG9uLnZhbHVlID0gdGhpcy51bnN1YnNjcmliZXRleHQ7XHJcbiAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLnN1YnNjcmliZXRleHQ7XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGlucHV0IHR5cGU9XCJidXR0b25cIiAvPmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtc3Vic2NyaWJlYnV0dG9uJywgU3Vic2NyaWJlQnV0dG9uKTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgVG9waWNCdXR0b24gZXh0ZW5kcyBFbGVtZW50QmFzZSB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBidXR0b25jbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbmNsYXNzXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBidXR0b250ZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9udGV4dFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgZm9ydW1pZCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImZvcnVtaWRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBUb3BpY0J1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmJ1dHRvbnRleHQ7XHJcbiAgICAgICAgaWYgKHRoaXMuYnV0dG9uY2xhc3M/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IGJ1dHRvbi5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICBidXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IHtcclxuICAgICAgICAgICAgY3VycmVudEZvcnVtU3RhdGUubG9hZE5ld1RvcGljKCk7XHJcbiAgICAgICAgfSk7XHJcbiAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXREZXBlbmRlbnRSZWZlcmVuY2UoKTogW1N0YXRlQmFzZSwgc3RyaW5nXSB7XHJcbiAgICAgICAgcmV0dXJuIFtQb3BGb3J1bXMuY3VycmVudEZvcnVtU3RhdGUsIFwiaXNOZXdUb3BpY0xvYWRlZFwiXTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgdXBkYXRlVUkoZGF0YTogYm9vbGVhbik6IHZvaWQge1xyXG4gICAgICAgIGlmIChkYXRhKVxyXG4gICAgICAgICAgICB0aGlzLnN0eWxlLmRpc3BsYXkgPSBcIm5vbmVcIjtcclxuICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgIHRoaXMuc3R5bGUuZGlzcGxheSA9IFwiaW5pdGlhbFwiO1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgLz5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXRvcGljYnV0dG9uJywgVG9waWNCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBUb3BpY0Zvcm0gZXh0ZW5kcyBIVE1MRWxlbWVudCB7XHJcbiAgICAgICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgICAgIHN1cGVyKCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBnZXQgdGVtcGxhdGVJRCgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidGVtcGxhdGVpZFwiKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgYnV0dG9uOiBIVE1MSW5wdXRFbGVtZW50O1xyXG5cclxuICAgICAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICAgICAgbGV0IHRlbXBsYXRlID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQodGhpcy50ZW1wbGF0ZUlEKSBhcyBIVE1MVGVtcGxhdGVFbGVtZW50O1xyXG4gICAgICAgICAgICBpZiAoIXRlbXBsYXRlKSB7XHJcbiAgICAgICAgICAgICAgICBjb25zb2xlLmVycm9yKGBDYW4ndCBmaW5kIHRlbXBsYXRlSUQgJHt0aGlzLnRlbXBsYXRlSUR9IHRvIG1ha2UgcmVwbHkgZm9ybS5gKTtcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB0aGlzLmFwcGVuZCh0ZW1wbGF0ZS5jb250ZW50LmNsb25lTm9kZSh0cnVlKSk7XHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI1N1Ym1pdE5ld1RvcGljXCIpO1xyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5zdWJtaXRUb3BpYygpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHN1Ym1pdFRvcGljKCkge1xyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbi5zZXRBdHRyaWJ1dGUoXCJkaXNhYmxlZFwiLCBcImRpc2FibGVkXCIpO1xyXG4gICAgICAgICAgICB2YXIgbW9kZWwgPSB7XHJcbiAgICAgICAgICAgICAgICBUaXRsZTogKHRoaXMucXVlcnlTZWxlY3RvcihcIiNOZXdUb3BpYyAjVGl0bGVcIikgYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWUsXHJcbiAgICAgICAgICAgICAgICBGdWxsVGV4dDogKHRoaXMucXVlcnlTZWxlY3RvcihcIiNOZXdUb3BpYyAjRnVsbFRleHRcIilhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZSxcclxuICAgICAgICAgICAgICAgIEluY2x1ZGVTaWduYXR1cmU6ICh0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTmV3VG9waWMgI0luY2x1ZGVTaWduYXR1cmVcIilhcyBIVE1MSW5wdXRFbGVtZW50KS5jaGVja2VkLFxyXG4gICAgICAgICAgICAgICAgSXRlbUlEOiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1RvcGljICNJdGVtSURcIilhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZSxcclxuICAgICAgICAgICAgICAgIElzUGxhaW5UZXh0OiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1RvcGljICNJc1BsYWluVGV4dFwiKWFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlLnRvTG93ZXJDYXNlKCkgPT09IFwidHJ1ZVwiXHJcbiAgICAgICAgICAgIH07XHJcbiAgICAgICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0ZvcnVtL1Bvc3RUb3BpY1wiLCB7XHJcbiAgICAgICAgICAgICAgICBtZXRob2Q6IFwiUE9TVFwiLFxyXG4gICAgICAgICAgICAgICAgYm9keTogSlNPTi5zdHJpbmdpZnkobW9kZWwpLFxyXG4gICAgICAgICAgICAgICAgaGVhZGVyczoge1xyXG4gICAgICAgICAgICAgICAgICAgIFwiQ29udGVudC1UeXBlXCI6IFwiYXBwbGljYXRpb24vanNvblwiXHJcbiAgICAgICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UuanNvbigpKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4ocmVzdWx0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBzd2l0Y2ggKHJlc3VsdC5yZXN1bHQpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSB0cnVlOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgd2luZG93LmxvY2F0aW9uID0gcmVzdWx0LnJlZGlyZWN0O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRlZmF1bHQ6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgciA9IHRoaXMucXVlcnlTZWxlY3RvcihcIiNQb3N0UmVzcG9uc2VNZXNzYWdlXCIpIGFzIEhUTUxFbGVtZW50O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgci5pbm5lckhUTUwgPSByZXN1bHQubWVzc2FnZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLnJlbW92ZUF0dHJpYnV0ZShcImRpc2FibGVkXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgci5zdHlsZS5kaXNwbGF5ID0gXCJibG9ja1wiO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAuY2F0Y2goZXJyb3IgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciByID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI1Bvc3RSZXNwb25zZU1lc3NhZ2VcIikgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgICAgICAgICAgICAgci5pbm5lckhUTUwgPSBcIlRoZXJlIHdhcyBhbiB1bmtub3duIGVycm9yIHdoaWxlIHRyeWluZyB0byBwb3N0XCI7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24ucmVtb3ZlQXR0cmlidXRlKFwiZGlzYWJsZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgci5zdHlsZS5kaXNwbGF5ID0gXCJibG9ja1wiO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi10b3BpY2Zvcm0nLCBUb3BpY0Zvcm0pO1xyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFRvcGljTW9kZXJhdGlvbkxvZ0J1dHRvbiBleHRlbmRzIEhUTUxFbGVtZW50IHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbnRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b250ZXh0XCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCB0b3BpY2lkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidG9waWNpZFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFRvcGljTW9kZXJhdGlvbkxvZ0J1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgYnV0dG9uLnZhbHVlID0gdGhpcy5idXR0b250ZXh0O1xyXG4gICAgICAgIGxldCBjbGFzc2VzID0gdGhpcy5idXR0b25jbGFzcztcclxuICAgICAgICBpZiAoY2xhc3Nlcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgY2xhc3Nlcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICBsZXQgY29udGFpbmVyID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiZGl2XCIpO1xyXG4gICAgICAgICAgICBpZiAoY29udGFpbmVyLnN0eWxlLmRpc3BsYXkgIT09IFwiYmxvY2tcIilcclxuICAgICAgICAgICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL01vZGVyYXRvci9Ub3BpY01vZGVyYXRpb25Mb2cvXCIgKyB0aGlzLnRvcGljaWQpXHJcbiAgICAgICAgICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIC50aGVuKHRleHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgY29udGFpbmVyLmlubmVySFRNTCA9IHRleHQ7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb250YWluZXIuc3R5bGUuZGlzcGxheSA9IFwiYmxvY2tcIjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfSkpO1xyXG4gICAgICAgICAgICBlbHNlIGNvbnRhaW5lci5zdHlsZS5kaXNwbGF5ID0gXCJub25lXCI7XHJcbiAgICAgICAgfSk7XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGlucHV0IHR5cGU9XCJidXR0b25cIiAvPlxyXG4gICAgPGRpdj48L2Rpdj5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoXCJwZi10b3BpY21vZGVyYXRpb25sb2didXR0b25cIiwgVG9waWNNb2RlcmF0aW9uTG9nQnV0dG9uKTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgVm90ZUNvdW50IGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgdm90ZXMoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ2b3Rlc1wiKTtcclxuICAgIH1cclxuICAgIHNldCB2b3Rlcyh2YWx1ZTpzdHJpbmcpIHtcclxuICAgICAgICB0aGlzLnNldEF0dHJpYnV0ZShcInZvdGVzXCIsIHZhbHVlKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgcG9zdGlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwicG9zdGlkXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCB2b3Rlc2NvbnRhaW5lcmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidm90ZXNjb250YWluZXJjbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYmFkZ2VjbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJhZGdlY2xhc3NcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IHZvdGVidXR0b25jbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInZvdGVidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgaXNsb2dnZWRpbigpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImlzbG9nZ2VkaW5cIikudG9Mb3dlckNhc2UoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgaXNhdXRob3IoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJpc2F1dGhvclwiKS50b0xvd2VyQ2FzZSgpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBpc3ZvdGVkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiaXN2b3RlZFwiKS50b0xvd2VyQ2FzZSgpO1xyXG4gICAgfVxyXG5cclxuICAgIHByaXZhdGUgYmFkZ2U6IEhUTUxFbGVtZW50O1xyXG4gICAgcHJpdmF0ZSB2b3RlckNvbnRhaW5lcjogSFRNTEVsZW1lbnQ7XHJcbiAgICBwcml2YXRlIHBvcG92ZXI6IGJvb3RzdHJhcC5Qb3BvdmVyO1xyXG4gICAgcHJpdmF0ZSBwb3BvdmVyRXZlbnRIYW5kZXI6IEV2ZW50TGlzdGVuZXJPckV2ZW50TGlzdGVuZXJPYmplY3Q7XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBWb3RlQ291bnQudGVtcGxhdGU7XHJcbiAgICAgICAgdGhpcy5iYWRnZSA9IHRoaXMucXVlcnlTZWxlY3RvcihcImRpdlwiKTtcclxuICAgICAgICB0aGlzLmJhZGdlLmlubmVySFRNTCA9IFwiK1wiICsgdGhpcy52b3RlcztcclxuICAgICAgICBpZiAodGhpcy5iYWRnZWNsYXNzPy5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICB0aGlzLmJhZGdlY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IHRoaXMuYmFkZ2UuY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgdmFyIHN0YXR1c0h0bWwgPSB0aGlzLmJ1dHRvbkdlbmVyYXRvcigpO1xyXG4gICAgICAgIGlmIChzdGF0dXNIdG1sICE9IFwiXCIpIHtcclxuICAgICAgICAgICAgbGV0IHN0YXR1cyA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ0ZW1wbGF0ZVwiKTtcclxuICAgICAgICAgICAgc3RhdHVzLmlubmVySFRNTCA9IHRoaXMuYnV0dG9uR2VuZXJhdG9yKCk7XHJcbiAgICAgICAgICAgIHRoaXMuYXBwZW5kKHN0YXR1cy5jb250ZW50LmZpcnN0Q2hpbGQpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBsZXQgdm90ZUJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcInNwYW5cIik7XHJcbiAgICAgICAgaWYgKHZvdGVCdXR0b24pIHtcclxuICAgICAgICAgICAgaWYgKHRoaXMudm90ZWJ1dHRvbmNsYXNzPy5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICAgICAgdGhpcy52b3RlYnV0dG9uY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IHZvdGVCdXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgICAgIHR5cGUgcmVzdWx0VHlwZSA9IHsgdm90ZXM6IG51bWJlcjsgaXNWb3RlZDogYm9vbGVhbjsgfVxyXG4gICAgICAgICAgICB2b3RlQnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9Ub2dnbGVWb3RlL1wiICsgdGhpcy5wb3N0aWQsIHsgbWV0aG9kOiBcIlBPU1RcIn0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS5qc29uKClcclxuICAgICAgICAgICAgICAgICAgICAudGhlbigocmVzdWx0OiByZXN1bHRUeXBlKSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMudm90ZXMgPSByZXN1bHQudm90ZXMudG9TdHJpbmcoKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5iYWRnZS5pbm5lckhUTUwgPSBcIitcIiArIHRoaXMudm90ZXM7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmIChyZXN1bHQuaXNWb3RlZCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdm90ZUJ1dHRvbi5jbGFzc0xpc3QucmVtb3ZlKFwiaWNvbi1wbHVzXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdm90ZUJ1dHRvbi5jbGFzc0xpc3QuYWRkKFwiaWNvbi1jYW5jZWwtY2lyY2xlXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdm90ZUJ1dHRvbi5jbGFzc0xpc3QucmVtb3ZlKFwiaWNvbi1jYW5jZWwtY2lyY2xlXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdm90ZUJ1dHRvbi5jbGFzc0xpc3QuYWRkKFwiaWNvbi1wbHVzXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuYXBwbHlQb3BvdmVyKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgfSkpO1xyXG4gICAgICAgICAgICB9KVxyXG4gICAgICAgIH1cclxuICAgICAgICB0aGlzLnNldHVwVm90ZXJQb3BvdmVyKCk7XHJcbiAgICAgICAgdGhpcy5hcHBseVBvcG92ZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIHNldHVwVm90ZXJQb3BvdmVyKCk6IHZvaWQge1xyXG4gICAgICAgIHRoaXMudm90ZXJDb250YWluZXIgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwiZGl2XCIpO1xyXG4gICAgICAgIGlmICh0aGlzLnZvdGVzY29udGFpbmVyY2xhc3M/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgIHRoaXMudm90ZXNjb250YWluZXJjbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gdGhpcy52b3RlckNvbnRhaW5lci5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICB0aGlzLnZvdGVyQ29udGFpbmVyLmlubmVySFRNTCA9IFwiTG9hZGluZy4uLlwiO1xyXG4gICAgICAgIHRoaXMucG9wb3ZlciA9IG5ldyBib290c3RyYXAuUG9wb3Zlcih0aGlzLmJhZGdlLCB7XHJcbiAgICAgICAgICAgIGNvbnRlbnQ6IHRoaXMudm90ZXJDb250YWluZXIsXHJcbiAgICAgICAgICAgIGh0bWw6IHRydWUsXHJcbiAgICAgICAgICAgIHRyaWdnZXI6IFwiY2xpY2sgZm9jdXNcIlxyXG4gICAgICAgIH0pO1xyXG4gICAgICAgIHRoaXMucG9wb3ZlckV2ZW50SGFuZGVyID0gKGUpID0+IHtcclxuICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vVm90ZXJzL1wiICsgdGhpcy5wb3N0aWQpXHJcbiAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLnRleHQoKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4odGV4dCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHQgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwidGVtcGxhdGVcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdC5pbm5lckhUTUwgPSB0ZXh0LnRyaW0oKTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLnZvdGVyQ29udGFpbmVyLmlubmVySFRNTCA9IFwiXCI7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy52b3RlckNvbnRhaW5lci5hcHBlbmRDaGlsZCh0LmNvbnRlbnQuZmlyc3RDaGlsZCk7XHJcbiAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICAgICAgfTtcclxuICAgICAgICB0aGlzLmJhZGdlLmFkZEV2ZW50TGlzdGVuZXIoXCJzaG93bi5icy5wb3BvdmVyXCIsIHRoaXMucG9wb3ZlckV2ZW50SGFuZGVyKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIGFwcGx5UG9wb3ZlcigpOiB2b2lkIHtcclxuICAgICAgICBpZiAodGhpcy52b3RlcyA9PT0gXCIwXCIpIHtcclxuICAgICAgICAgICAgdGhpcy5iYWRnZS5zdHlsZS5jdXJzb3IgPSBcImRlZmF1bHRcIjtcclxuICAgICAgICAgICAgdGhpcy5wb3BvdmVyLmRpc2FibGUoKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZWxzZSB7XHJcbiAgICAgICAgICAgIHRoaXMuYmFkZ2Uuc3R5bGUuY3Vyc29yID0gXCJwb2ludGVyXCI7XHJcbiAgICAgICAgICAgIHRoaXMucG9wb3Zlci5lbmFibGUoKTtcclxuICAgICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgcHJpdmF0ZSBidXR0b25HZW5lcmF0b3IoKTogc3RyaW5nIHtcclxuICAgICAgICBpZiAodGhpcy5pc2xvZ2dlZGluID09PSBcImZhbHNlXCIgfHwgdGhpcy5pc2F1dGhvciA9PT0gXCJ0cnVlXCIpXHJcbiAgICAgICAgICAgIHJldHVybiBcIlwiO1xyXG4gICAgICAgIGlmICh0aGlzLmlzdm90ZWQgPT09IFwidHJ1ZVwiKVxyXG4gICAgICAgICAgICByZXR1cm4gVm90ZUNvdW50LmNhbmNlbFZvdGVCdXR0b247XHJcbiAgICAgICAgcmV0dXJuIFZvdGVDb3VudC52b3RlVXBCdXR0b247XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGRpdj48L2Rpdj5gO1xyXG5cclxuICAgIHN0YXRpYyB2b3RlVXBCdXR0b24gPSBcIjxzcGFuIGNsYXNzPVxcXCJpY29uLXBsdXNcXFwiPjwvc3Bhbj5cIjtcclxuICAgIHN0YXRpYyBjYW5jZWxWb3RlQnV0dG9uID0gXCI8c3BhbiBjbGFzcz1cXFwiaWNvbi1jYW5jZWwtY2lyY2xlXFxcIj48L3NwYW4+XCI7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZShcInBmLXZvdGVjb3VudFwiLCBWb3RlQ291bnQpO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBOb3RpZmljYXRpb25TZXJ2aWNlIHtcclxuICAgICAgICBjb25zdHJ1Y3Rvcih1c2VyU3RhdGU6IFVzZXJTdGF0ZSkge1xyXG4gICAgICAgICAgICB0aGlzLnVzZXJTdGF0ZSA9IHVzZXJTdGF0ZTtcclxuICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICB0aGlzLmNvbm5lY3Rpb24gPSBuZXcgc2lnbmFsUi5IdWJDb25uZWN0aW9uQnVpbGRlcigpLndpdGhVcmwoXCIvTm90aWZpY2F0aW9uSHViXCIpLmJ1aWxkKCk7XHJcbiAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbi5vbihcInVwZGF0ZVBNQ291bnRcIiwgZnVuY3Rpb24ocG1Db3VudDogbnVtYmVyKSB7XHJcbiAgICAgICAgICAgICAgICBzZWxmLnVzZXJTdGF0ZS5uZXdQbUNvdW50ID0gcG1Db3VudDtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbi5zdGFydCgpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHJpdmF0ZSB1c2VyU3RhdGU6IFVzZXJTdGF0ZTtcclxuICAgICAgICBwcml2YXRlIGNvbm5lY3Rpb246IGFueTtcclxuICAgIH1cclxufSIsIi8vIFRPRE86IE1vdmUgdGhpcyB0byBhbiBvcGVuIHdlYnNvY2tldHMgY29ubmVjdGlvblxyXG5cclxubmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcbiAgICBleHBvcnQgY2xhc3MgVGltZVVwZGF0ZXIge1xyXG4gICAgICAgIFN0YXJ0KCkge1xyXG4gICAgICAgICAgICBSZWFkeSgoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLlN0YXJ0VXBkYXRlcigpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgdGltZUFycmF5OiBzdHJpbmdbXTtcclxuXHJcbiAgICAgICAgcHJpdmF0ZSBQb3B1bGF0ZUFycmF5KCk6IHZvaWQge1xyXG4gICAgICAgICAgICB0aGlzLnRpbWVBcnJheSA9IFtdO1xyXG4gICAgICAgICAgICBsZXQgdGltZXMgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLmZUaW1lXCIpO1xyXG4gICAgICAgICAgICB0aW1lcy5mb3JFYWNoKHRpbWUgPT4ge1xyXG4gICAgICAgICAgICAgICAgdmFyIHQgPSB0aW1lLmdldEF0dHJpYnV0ZShcImRhdGEtdXRjXCIpO1xyXG4gICAgICAgICAgICAgICAgaWYgKCgobmV3IERhdGUoKS5nZXREYXRlKCkgLSBuZXcgRGF0ZSh0ICsgXCJaXCIpLmdldERhdGUoKSkgLyAzNjAwMDAwKSA8IDQ4KVxyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMudGltZUFycmF5LnB1c2godCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHJpdmF0ZSBTdGFydFVwZGF0ZXIoKTogdm9pZCB7XHJcbiAgICAgICAgICAgIHNldFRpbWVvdXQoKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5TdGFydFVwZGF0ZXIoKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuUG9wdWxhdGVBcnJheSgpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5DYWxsRm9yVXBkYXRlKCk7XHJcbiAgICAgICAgICAgIH0sIDYwMDAwKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgQ2FsbEZvclVwZGF0ZSgpOiB2b2lkIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLnRpbWVBcnJheSB8fCB0aGlzLnRpbWVBcnJheS5sZW5ndGggPT09IDApXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIGxldCBzZXJpYWxpemVkID0gSlNPTi5zdHJpbmdpZnkodGhpcy50aW1lQXJyYXkpO1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9UaW1lL0dldFRpbWVzXCIsIHtcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCIsXHJcbiAgICAgICAgICAgICAgICBib2R5OiBzZXJpYWxpemVkLFxyXG4gICAgICAgICAgICAgICAgaGVhZGVyczoge1xyXG4gICAgICAgICAgICAgICAgICAgIFwiQ29udGVudC1UeXBlXCI6IFwiYXBwbGljYXRpb24vanNvblwiXHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS5qc29uKCkpXHJcbiAgICAgICAgICAgICAgICAudGhlbihkYXRhID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBkYXRhLmZvckVhY2goKHQ6IHsga2V5OiBzdHJpbmc7IHZhbHVlOiBzdHJpbmc7IH0pID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIi5mVGltZVtkYXRhLXV0Yz0nXCIgKyB0LmtleSArIFwiJ11cIikuaW5uZXJIVE1MID0gdC52YWx1ZTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAuY2F0Y2goZXJyb3IgPT4geyBjb25zb2xlLmxvZyhcIlRpbWUgdXBkYXRlIGZhaWx1cmU6IFwiICsgZXJyb3IpOyB9KTtcclxuICAgICAgICB9XHJcbiAgICB9XHJcbn1cclxuXHJcbnZhciB0aW1lVXBkYXRlciA9IG5ldyBQb3BGb3J1bXMuVGltZVVwZGF0ZXIoKTtcclxudGltZVVwZGF0ZXIuU3RhcnQoKTsiLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgRm9ydW1TdGF0ZSBleHRlbmRzIFN0YXRlQmFzZSB7XHJcbiAgICAgICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgICAgIHN1cGVyKCk7XHJcbiAgICAgICAgfVxyXG4gICAgXHJcbiAgICAgICAgZm9ydW1JRDogbnVtYmVyO1xyXG4gICAgICAgIHBhZ2VTaXplOiBudW1iZXI7XHJcbiAgICAgICAgcGFnZUluZGV4OiBudW1iZXI7XHJcbiAgICAgICAgQFdhdGNoUHJvcGVydHlcclxuICAgICAgICBpc05ld1RvcGljTG9hZGVkOiBib29sZWFuO1xyXG5cclxuICAgICAgICBzZXR1cEZvcnVtKCkge1xyXG4gICAgICAgICAgICBQb3BGb3J1bXMuUmVhZHkoKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5pc05ld1RvcGljTG9hZGVkID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmZvcnVtTGlzdGVuKCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgbG9hZE5ld1RvcGljKCkge1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9Qb3N0VG9waWMvXCIgKyB0aGlzLmZvcnVtSUQpXHJcbiAgICAgICAgICAgICAgICAudGhlbigocmVzcG9uc2UpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gcmVzcG9uc2UudGV4dCgpO1xyXG4gICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC50aGVuKChib2R5KSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIG4gPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI05ld1RvcGljXCIpIGFzIEhUTUxFbGVtZW50O1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICghbilcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhyb3coXCJDYW4ndCBmaW5kIGEgI05ld1RvcGljIGVsZW1lbnQgdG8gbG9hZCBpbiB0aGUgbmV3IHRvcGljIGZvcm0uXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIG4uaW5uZXJIVE1MID0gYm9keTtcclxuICAgICAgICAgICAgICAgICAgICBuLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5pc05ld1RvcGljTG9hZGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZm9ydW1MaXN0ZW4oKSB7XHJcbiAgICAgICAgICAgIGxldCBjb25uZWN0aW9uID0gbmV3IHNpZ25hbFIuSHViQ29ubmVjdGlvbkJ1aWxkZXIoKS53aXRoVXJsKFwiL0ZvcnVtc0h1YlwiKS5idWlsZCgpO1xyXG4gICAgICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgICAgIGNvbm5lY3Rpb24ub24oXCJub3RpZnlVcGRhdGVkVG9waWNcIiwgZnVuY3Rpb24gKGRhdGE6IGFueSkgeyAvLyBUT0RPOiByZWZhY3RvciB0byBzdHJvbmcgdHlwZVxyXG4gICAgICAgICAgICAgICAgbGV0IHJlbW92YWwgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKCcjVG9waWNMaXN0IHRyW2RhdGEtdG9waWNJRD1cIicgKyBkYXRhLnRvcGljSUQgKyAnXCJdJyk7XHJcbiAgICAgICAgICAgICAgICBpZiAocmVtb3ZhbCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJlbW92YWwucmVtb3ZlKCk7XHJcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIGxldCByb3dzID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIiNUb3BpY0xpc3QgdHI6bm90KCNUb3BpY1RlbXBsYXRlKVwiKTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAocm93cy5sZW5ndGggPT0gc2VsZi5wYWdlU2l6ZSlcclxuICAgICAgICAgICAgICAgICAgICAgICAgcm93c1tyb3dzLmxlbmd0aCAtIDFdLnJlbW92ZSgpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgbGV0IHJvdyA9IHNlbGYucG9wdWxhdGVUb3BpY1JvdyhkYXRhKTtcclxuICAgICAgICAgICAgICAgIHJvdy5jbGFzc0xpc3QucmVtb3ZlKFwiaGlkZGVuXCIpO1xyXG4gICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNUb3BpY0xpc3QgdGJvZHlcIikucHJlcGVuZChyb3cpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgY29ubmVjdGlvbi5zdGFydCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbm5lY3Rpb24uaW52b2tlKFwibGlzdGVuVG9cIiwgc2VsZi5mb3J1bUlEKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcmVjZW50TGlzdGVuKCkge1xyXG4gICAgICAgICAgICB2YXIgY29ubmVjdGlvbiA9IG5ldyBzaWduYWxSLkh1YkNvbm5lY3Rpb25CdWlsZGVyKCkud2l0aFVybChcIi9SZWNlbnRIdWJcIikuYnVpbGQoKTtcclxuICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICBjb25uZWN0aW9uLm9uKFwibm90aWZ5UmVjZW50VXBkYXRlXCIsIGZ1bmN0aW9uIChkYXRhOiBhbnkpIHtcclxuICAgICAgICAgICAgICAgIHZhciByZW1vdmFsID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcignI1RvcGljTGlzdCB0cltkYXRhLXRvcGljSUQ9XCInICsgZGF0YS50b3BpY0lEICsgJ1wiXScpO1xyXG4gICAgICAgICAgICAgICAgaWYgKHJlbW92YWwpIHtcclxuICAgICAgICAgICAgICAgICAgICByZW1vdmFsLnJlbW92ZSgpO1xyXG4gICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgcm93cyA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIjVG9waWNMaXN0IHRyOm5vdCgjVG9waWNUZW1wbGF0ZSlcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHJvd3MubGVuZ3RoID09IHNlbGYucGFnZVNpemUpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJvd3Nbcm93cy5sZW5ndGggLSAxXS5yZW1vdmUoKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIHZhciByb3cgPSBzZWxmLnBvcHVsYXRlVG9waWNSb3coZGF0YSk7XHJcbiAgICAgICAgICAgICAgICByb3cuY2xhc3NMaXN0LnJlbW92ZShcImhpZGRlblwiKTtcclxuICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjVG9waWNMaXN0IHRib2R5XCIpLnByZXBlbmQocm93KTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIGNvbm5lY3Rpb24uc3RhcnQoKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4oZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBjb25uZWN0aW9uLmludm9rZShcInJlZ2lzdGVyXCIpO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBwb3B1bGF0ZVRvcGljUm93ID0gZnVuY3Rpb24gKGRhdGE6IGFueSkge1xyXG4gICAgICAgICAgICBsZXQgcm93ID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNUb3BpY1RlbXBsYXRlXCIpLmNsb25lTm9kZSh0cnVlKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgcm93LnNldEF0dHJpYnV0ZShcImRhdGEtdG9waWNpZFwiLCBkYXRhLnRvcGljSUQpO1xyXG4gICAgICAgICAgICByb3cucmVtb3ZlQXR0cmlidXRlKFwiaWRcIik7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLnN0YXJ0ZWRCeU5hbWVcIikuaW5uZXJIVE1MID0gZGF0YS5zdGFydGVkQnlOYW1lO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5pbmRpY2F0b3JMaW5rXCIpLnNldEF0dHJpYnV0ZShcImhyZWZcIiwgZGF0YS5saW5rKTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIudGl0bGVMaW5rXCIpLmlubmVySFRNTCA9IGRhdGEudGl0bGU7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLnRpdGxlTGlua1wiKS5zZXRBdHRyaWJ1dGUoXCJocmVmXCIsIGRhdGEubGluayk7XHJcbiAgICAgICAgICAgIHZhciBmb3J1bVRpdGxlID0gcm93LnF1ZXJ5U2VsZWN0b3IoXCIuZm9ydW1UaXRsZVwiKTtcclxuICAgICAgICAgICAgaWYgKGZvcnVtVGl0bGUpIGZvcnVtVGl0bGUuaW5uZXJIVE1MID0gZGF0YS5mb3J1bVRpdGxlO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi52aWV3Q291bnRcIikuaW5uZXJIVE1MID0gZGF0YS52aWV3Q291bnQ7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLnJlcGx5Q291bnRcIikuaW5uZXJIVE1MID0gZGF0YS5yZXBseUNvdW50O1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5sYXN0UG9zdFRpbWVcIikuaW5uZXJIVE1MID0gZGF0YS5sYXN0UG9zdFRpbWU7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLmxhc3RQb3N0TmFtZVwiKS5pbm5lckhUTUwgPSBkYXRhLmxhc3RQb3N0TmFtZTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIuZlRpbWVcIikuc2V0QXR0cmlidXRlKFwiZGF0YS11dGNcIiwgZGF0YS51dGMpO1xyXG4gICAgICAgICAgICByZXR1cm4gcm93O1xyXG4gICAgICAgIH07XHJcbiAgICB9XHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbmV4cG9ydCBjbGFzcyBUb3BpY1N0YXRlIGV4dGVuZHMgU3RhdGVCYXNlIHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgdG9waWNJRDogbnVtYmVyO1xyXG4gICAgaXNJbWFnZUVuYWJsZWQ6IGJvb2xlYW47XHJcbiAgICBAV2F0Y2hQcm9wZXJ0eVxyXG4gICAgaXNSZXBseUxvYWRlZDogYm9vbGVhbjtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcbiAgICBhbnN3ZXJQb3N0SUQ6IG51bWJlcjtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcblx0bG93UGFnZTpudW1iZXI7XHJcbiAgICBAV2F0Y2hQcm9wZXJ0eVxyXG5cdGhpZ2hQYWdlOiBudW1iZXI7XHJcblx0bGFzdFZpc2libGVQb3N0SUQ6IG51bWJlcjtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcbiAgICBpc05ld2VyUG9zdHNBdmFpbGFibGU6IGJvb2xlYW47XHJcbiAgICBwYWdlSW5kZXg6IG51bWJlcjtcclxuXHRwYWdlQ291bnQ6IG51bWJlcjtcclxuXHRsb2FkaW5nUG9zdHM6IGJvb2xlYW4gPSBmYWxzZTtcclxuXHRpc1Njcm9sbEFkanVzdGVkOiBib29sZWFuID0gZmFsc2U7XHJcbiAgICBAV2F0Y2hQcm9wZXJ0eVxyXG4gICAgY29tbWVudFJlcGx5SUQ6IG51bWJlcjtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcbiAgICBuZXh0UXVvdGU6IHN0cmluZztcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcbiAgICBpc1N1YnNjcmliZWQ6IGJvb2xlYW47XHJcbiAgICBAV2F0Y2hQcm9wZXJ0eVxyXG4gICAgaXNGYXZvcml0ZTogYm9vbGVhbjtcclxuXHJcbiAgICBzZXR1cFRvcGljKCkge1xyXG4gICAgICAgIFBvcEZvcnVtcy5SZWFkeSgoKSA9PiB7XHJcbiAgICAgICAgICAgIHRoaXMuaXNSZXBseUxvYWRlZCA9IGZhbHNlO1xyXG4gICAgICAgICAgICB0aGlzLmlzTmV3ZXJQb3N0c0F2YWlsYWJsZSA9IGZhbHNlO1xyXG4gICAgICAgICAgICB0aGlzLmxvd1BhZ2UgPSB0aGlzLnBhZ2VJbmRleDtcclxuICAgICAgICAgICAgdGhpcy5oaWdoUGFnZSA9IHRoaXMucGFnZUluZGV4O1xyXG5cclxuICAgICAgICAgICAgLy8gc2lnbmFsUiBjb25uZWN0aW9uc1xyXG4gICAgICAgICAgICBsZXQgY29ubmVjdGlvbiA9IG5ldyBzaWduYWxSLkh1YkNvbm5lY3Rpb25CdWlsZGVyKCkud2l0aFVybChcIi9Ub3BpY3NIdWJcIikuYnVpbGQoKTtcclxuICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICAvLyBmb3IgYWxsIHBvc3RzIGxvYWRlZCBidXQgcmVwbHkgbm90IG9wZW5cclxuICAgICAgICAgICAgY29ubmVjdGlvbi5vbihcImZldGNoTmV3UG9zdFwiLCBmdW5jdGlvbiAocG9zdElEOiBudW1iZXIpIHtcclxuICAgICAgICAgICAgICAgIGlmICghc2VsZi5pc1JlcGx5TG9hZGVkICYmIHNlbGYuaGlnaFBhZ2UgPT09IHNlbGYucGFnZUNvdW50KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vUG9zdC9cIiArIHBvc3RJRClcclxuICAgICAgICAgICAgICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgdCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ0ZW1wbGF0ZVwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0LmlubmVySFRNTCA9IHRleHQudHJpbSgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjUG9zdFN0cmVhbVwiKS5hcHBlbmRDaGlsZCh0LmNvbnRlbnQuZmlyc3RDaGlsZCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICAgICAgICAgICAgICAgICAgc2VsZi5sYXN0VmlzaWJsZVBvc3RJRCA9IHBvc3RJRDtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIC8vIGZvciByZXBseSBhbHJlYWR5IG9wZW5cclxuICAgICAgICAgICAgY29ubmVjdGlvbi5vbihcIm5vdGlmeU5ld1Bvc3RzXCIsIGZ1bmN0aW9uICh0aGVMYXN0UG9zdElEOiBudW1iZXIpIHtcclxuICAgICAgICAgICAgICAgIHNlbGYuc2V0TW9yZVBvc3RzQXZhaWxhYmxlKHRoZUxhc3RQb3N0SUQpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgY29ubmVjdGlvbi5zdGFydCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbm5lY3Rpb24uaW52b2tlKFwibGlzdGVuVG9cIiwgc2VsZi50b3BpY0lEKTtcclxuICAgICAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgc2VsZi5jb25uZWN0aW9uID0gY29ubmVjdGlvblxyXG4gICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLnBvc3RJdGVtIGltZzpub3QoLmF2YXRhcilcIikuZm9yRWFjaCh4ID0+IHguY2xhc3NMaXN0LmFkZChcInBvc3RJbWFnZVwiKSk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLnNjcm9sbFRvUG9zdEZyb21IYXNoKCk7XHJcbiAgICAgICAgICAgIHdpbmRvdy5hZGRFdmVudExpc3RlbmVyKFwic2Nyb2xsXCIsIHRoaXMuc2Nyb2xsTG9hZCk7XHJcbiAgICAgICAgfSk7XHJcbiAgICB9XHJcblxyXG4gICAgbG9hZFJlcGx5KHRvcGljSUQ6bnVtYmVyLCByZXBseUlEOm51bWJlciwgc2V0dXBNb3JlUG9zdHM6Ym9vbGVhbik6dm9pZCB7XHJcbiAgICAgICAgaWYgKHRoaXMuaXNSZXBseUxvYWRlZCkge1xyXG4gICAgICAgICAgICB0aGlzLnNjcm9sbFRvRWxlbWVudChcIk5ld1JlcGx5XCIpO1xyXG4gICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG4gICAgICAgIHdpbmRvdy5yZW1vdmVFdmVudExpc3RlbmVyKFwic2Nyb2xsXCIsIHRoaXMuc2Nyb2xsTG9hZCk7XHJcbiAgICAgICAgdmFyIHBhdGggPSBQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9Qb3N0UmVwbHkvXCIgKyB0b3BpY0lEO1xyXG4gICAgICAgIGlmIChyZXBseUlEICE9IG51bGwpIHtcclxuICAgICAgICAgICAgcGF0aCArPSBcIj9yZXBseUlEPVwiICsgcmVwbHlJRDtcclxuICAgICAgICB9XHJcbiAgICBcclxuICAgICAgICBmZXRjaChwYXRoKVxyXG4gICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS50ZXh0KClcclxuICAgICAgICAgICAgICAgIC50aGVuKHRleHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIGxldCBuID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNOZXdSZXBseVwiKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICBuLmlubmVySFRNTCA9IHRleHQ7XHJcbiAgICAgICAgICAgICAgICAgICAgbi5zdHlsZS5kaXNwbGF5ID0gXCJibG9ja1wiO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuc2Nyb2xsVG9FbGVtZW50KFwiTmV3UmVwbHlcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5pc1JlcGx5TG9hZGVkID0gdHJ1ZTtcclxuICAgIFxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChzZXR1cE1vcmVQb3N0cykge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbi5pbnZva2UoXCJnZXRMYXN0UG9zdElEXCIsIHRoaXMudG9waWNJRClcclxuICAgICAgICAgICAgICAgICAgICAgICAgLnRoZW4oZnVuY3Rpb24gKHJlc3VsdDogbnVtYmVyKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxmLnNldE1vcmVQb3N0c0F2YWlsYWJsZShyZXN1bHQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5pc1JlcGx5TG9hZGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmNvbW1lbnRSZXBseUlEID0gMDtcclxuICAgICAgICAgICAgICAgIH0pKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIGNvbm5lY3Rpb246IGFueTtcclxuXHJcbiAgICAvLyB0aGlzIGlzIGludGVuZGVkIHRvIGJlIGNhbGxlZCB3aGVuIHRoZSByZXBseSBib3ggaXMgb3BlblxyXG4gICAgcHJpdmF0ZSBzZXRNb3JlUG9zdHNBdmFpbGFibGUgPSAobmV3ZXN0UG9zdElEb25TZXJ2ZXI6IG51bWJlcikgPT4ge1xyXG4gICAgICAgIHRoaXMuaXNOZXdlclBvc3RzQXZhaWxhYmxlID0gbmV3ZXN0UG9zdElEb25TZXJ2ZXIgIT09IHRoaXMubGFzdFZpc2libGVQb3N0SUQ7XHJcbiAgICB9XHJcblxyXG4gICAgbG9hZENvbW1lbnQodG9waWNJRDogbnVtYmVyLCByZXBseUlEOiBudW1iZXIpOiB2b2lkIHtcclxuICAgICAgICB2YXIgbiA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCJbZGF0YS1wb3N0aWQqPSdcIiArIHJlcGx5SUQgKyBcIiddIC5jb21tZW50SG9sZGVyXCIpO1xyXG4gICAgICAgIGNvbnN0IGJveGlkID0gXCJjb21tZW50Ym94XCI7XHJcbiAgICAgICAgbi5pZCA9IGJveGlkO1xyXG4gICAgICAgIHZhciBwYXRoID0gUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vUG9zdFJlcGx5L1wiICsgdG9waWNJRCArIFwiP3JlcGx5SUQ9XCIgKyByZXBseUlEO1xyXG4gICAgICAgIHRoaXMuY29tbWVudFJlcGx5SUQgPSByZXBseUlEO1xyXG4gICAgICAgIHRoaXMuaXNSZXBseUxvYWRlZCA9IHRydWU7XHJcbiAgICAgICAgZmV0Y2gocGF0aClcclxuICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBuLmlubmVySFRNTCA9IHRleHQ7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5zY3JvbGxUb0VsZW1lbnQoYm94aWQpO1xyXG4gICAgICAgICAgICAgICAgfSkpO1xyXG4gICAgfTtcclxuXHJcbiAgICBsb2FkTW9yZVBvc3RzID0gKCkgPT4ge1xyXG4gICAgICAgIGxldCB0b3BpY1BhZ2VQYXRoOiBzdHJpbmc7XHJcbiAgICAgICAgaWYgKHRoaXMuaGlnaFBhZ2UgPT09IHRoaXMucGFnZUNvdW50KSB7XHJcbiAgICAgICAgICAgIHRvcGljUGFnZVBhdGggPSBQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9Ub3BpY1BhcnRpYWwvXCIgKyB0aGlzLnRvcGljSUQgKyBcIj9sYXN0UG9zdD1cIiArIHRoaXMubGFzdFZpc2libGVQb3N0SUQgKyBcIiZsb3dQYWdlPVwiICsgdGhpcy5sb3dQYWdlO1xyXG4gICAgICAgIH1cclxuICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICAgdGhpcy5oaWdoUGFnZSsrO1xyXG4gICAgICAgICAgICB0b3BpY1BhZ2VQYXRoID0gUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vVG9waWNQYWdlL1wiICsgdGhpcy50b3BpY0lEICsgXCI/cGFnZU51bWJlcj1cIiArIHRoaXMuaGlnaFBhZ2UgKyBcIiZsb3c9XCIgKyB0aGlzLmxvd1BhZ2UgKyBcIiZoaWdoPVwiICsgdGhpcy5oaWdoUGFnZTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZmV0Y2godG9waWNQYWdlUGF0aClcclxuICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgdCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ0ZW1wbGF0ZVwiKTtcclxuICAgICAgICAgICAgICAgICAgICB0LmlubmVySFRNTCA9IHRleHQudHJpbSgpO1xyXG4gICAgICAgICAgICAgICAgICAgIGxldCBzdHVmZiA9IHQuY29udGVudC5maXJzdENoaWxkIGFzIEhUTUxFbGVtZW50O1xyXG4gICAgICAgICAgICAgICAgICAgIGxldCBsaW5rcyA9IHN0dWZmLnF1ZXJ5U2VsZWN0b3IoXCIucGFnZXJMaW5rc1wiKTtcclxuICAgICAgICAgICAgICAgICAgICBzdHVmZi5yZW1vdmVDaGlsZChsaW5rcyk7XHJcbiAgICAgICAgICAgICAgICAgICAgbGV0IGxhc3RQb3N0SUQgPSBzdHVmZi5xdWVyeVNlbGVjdG9yKFwiLmxhc3RQb3N0SURcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICBzdHVmZi5yZW1vdmVDaGlsZChsYXN0UG9zdElEKTtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgbmV3UGFnZUNvdW50ID0gc3R1ZmYucXVlcnlTZWxlY3RvcihcIi5wYWdlQ291bnRcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICBzdHVmZi5yZW1vdmVDaGlsZChuZXdQYWdlQ291bnQpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMubGFzdFZpc2libGVQb3N0SUQgPSBOdW1iZXIobGFzdFBvc3RJRC52YWx1ZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5wYWdlQ291bnQgPSBOdW1iZXIobmV3UGFnZUNvdW50LnZhbHVlKTtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgcG9zdFN0cmVhbSA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjUG9zdFN0cmVhbVwiKTtcclxuICAgICAgICAgICAgICAgICAgICBwb3N0U3RyZWFtLmFwcGVuZChzdHVmZik7XHJcbiAgICAgICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIi5wYWdlckxpbmtzXCIpLmZvckVhY2goeCA9PiB4LnJlcGxhY2VXaXRoKGxpbmtzLmNsb25lTm9kZSh0cnVlKSkpO1xyXG4gICAgICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIucG9zdEl0ZW0gaW1nOm5vdCguYXZhdGFyKVwiKS5mb3JFYWNoKHggPT4geC5jbGFzc0xpc3QuYWRkKFwicG9zdEltYWdlXCIpKTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAodGhpcy5oaWdoUGFnZSA9PSB0aGlzLnBhZ2VDb3VudCAmJiB0aGlzLmxvd1BhZ2UgPT0gMSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLnBhZ2VyTGlua3NcIikuZm9yRWFjaCh4ID0+IHgucmVtb3ZlKCkpO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmxvYWRpbmdQb3N0cyA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICghdGhpcy5pc1Njcm9sbEFkanVzdGVkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuc2Nyb2xsVG9Qb3N0RnJvbUhhc2goKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHRoaXMuaXNSZXBseUxvYWRlZCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbi5pbnZva2UoXCJnZXRMYXN0UG9zdElEXCIsIHRoaXMudG9waWNJRClcclxuICAgICAgICAgICAgICAgICAgICAgICAgLnRoZW4oZnVuY3Rpb24gKHJlc3VsdDogbnVtYmVyKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxmLnNldE1vcmVQb3N0c0F2YWlsYWJsZShyZXN1bHQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICB9O1xyXG5cclxuICAgIGxvYWRQcmV2aW91c1Bvc3RzID0gKCkgPT4ge1xyXG4gICAgICAgIHRoaXMubG93UGFnZS0tO1xyXG4gICAgICAgIGxldCB0b3BpY1BhZ2VQYXRoID0gUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vVG9waWNQYWdlL1wiICsgdGhpcy50b3BpY0lEICsgXCI/cGFnZU51bWJlcj1cIiArIHRoaXMubG93UGFnZSArIFwiJmxvdz1cIiArIHRoaXMubG93UGFnZSArIFwiJmhpZ2g9XCIgKyB0aGlzLmhpZ2hQYWdlO1xyXG4gICAgICAgIGZldGNoKHRvcGljUGFnZVBhdGgpXHJcbiAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLnRleHQoKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4odGV4dCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgbGV0IHQgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwidGVtcGxhdGVcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdC5pbm5lckhUTUwgPSB0ZXh0LnRyaW0oKTtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgc3R1ZmYgPSB0LmNvbnRlbnQuZmlyc3RDaGlsZCBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgbGlua3MgPSBzdHVmZi5xdWVyeVNlbGVjdG9yKFwiLnBhZ2VyTGlua3NcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgc3R1ZmYucmVtb3ZlQ2hpbGQobGlua3MpO1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBwb3N0U3RyZWFtID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNQb3N0U3RyZWFtXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHBvc3RTdHJlYW0ucHJlcGVuZChzdHVmZik7XHJcbiAgICAgICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIi5wYWdlckxpbmtzXCIpLmZvckVhY2goeCA9PiB4LnJlcGxhY2VXaXRoKGxpbmtzLmNsb25lTm9kZSh0cnVlKSkpO1xyXG4gICAgICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIucG9zdEl0ZW0gaW1nOm5vdCguYXZhdGFyKVwiKS5mb3JFYWNoKHggPT4geC5jbGFzc0xpc3QuYWRkKFwicG9zdEltYWdlXCIpKTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAodGhpcy5oaWdoUGFnZSA9PSB0aGlzLnBhZ2VDb3VudCAmJiB0aGlzLmxvd1BhZ2UgPT0gMSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLnBhZ2VyTGlua3NcIikuZm9yRWFjaCh4ID0+IHgucmVtb3ZlKCkpO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH0pKTtcclxuICAgIH1cclxuXHJcbiAgICBzY3JvbGxMb2FkID0gKCkgPT4ge1xyXG4gICAgICAgIGxldCBzdHJlYW1FbmQgPSAoZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNTdHJlYW1Cb3R0b21cIikgYXMgSFRNTEVsZW1lbnQpO1xyXG4gICAgICAgIGlmICghc3RyZWFtRW5kKVxyXG4gICAgICAgICAgICByZXR1cm47IC8vIHRoaXMgaXMgYSBRQSB0b3BpYywgbm8gY29udGludW91cyBwb3N0IHN0cmVhbVxyXG4gICAgICAgIGxldCB0b3AgPSBzdHJlYW1FbmQub2Zmc2V0VG9wO1xyXG4gICAgICAgIGxldCB2aWV3RW5kID0gd2luZG93LnNjcm9sbFkgKyB3aW5kb3cub3V0ZXJIZWlnaHQ7XHJcbiAgICAgICAgbGV0IGRpc3RhbmNlID0gdG9wIC0gdmlld0VuZDtcclxuICAgICAgICBpZiAoIXRoaXMubG9hZGluZ1Bvc3RzICYmIGRpc3RhbmNlIDwgMjUwICYmIHRoaXMuaGlnaFBhZ2UgPCB0aGlzLnBhZ2VDb3VudCkge1xyXG4gICAgICAgICAgICB0aGlzLmxvYWRpbmdQb3N0cyA9IHRydWU7XHJcbiAgICAgICAgICAgIHRoaXMubG9hZE1vcmVQb3N0cygpO1xyXG4gICAgICAgIH1cclxuICAgIH07XHJcblxyXG4gICAgc2Nyb2xsVG9FbGVtZW50ID0gKGlkOiBzdHJpbmcpID0+IHtcclxuICAgICAgICBsZXQgZSA9IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKGlkKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICBsZXQgdCA9IDA7XHJcbiAgICAgICAgaWYgKGUub2Zmc2V0UGFyZW50KSB7XHJcbiAgICAgICAgICAgIHdoaWxlIChlLm9mZnNldFBhcmVudCkge1xyXG4gICAgICAgICAgICAgICAgdCArPSBlLm9mZnNldFRvcDtcclxuICAgICAgICAgICAgICAgIGUgPSBlLm9mZnNldFBhcmVudCBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0gZWxzZSBpZiAoZS5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKS55KSB7XHJcbiAgICAgICAgICAgIHQgKz0gZS5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKS55O1xyXG4gICAgICAgIH1cclxuICAgICAgICBsZXQgY3J1bWIgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI1RvcEJyZWFkY3J1bWJcIikgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgaWYgKGNydW1iKVxyXG4gICAgICAgICAgICB0IC09IGNydW1iLm9mZnNldEhlaWdodDtcclxuICAgICAgICBzY3JvbGxUbygwLCB0KTtcclxuICAgIH07XHJcblxyXG4gICAgc2Nyb2xsVG9Qb3N0RnJvbUhhc2ggPSAoKSA9PiB7XHJcbiAgICAgICAgaWYgKHdpbmRvdy5sb2NhdGlvbi5oYXNoKSB7XHJcbiAgICAgICAgICAgIFByb21pc2UuYWxsKEFycmF5LmZyb20oZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIiNQb3N0U3RyZWFtIGltZ1wiKSlcclxuICAgICAgICAgICAgICAgIC5maWx0ZXIoaW1nID0+ICEoaW1nIGFzIEhUTUxJbWFnZUVsZW1lbnQpLmNvbXBsZXRlKVxyXG4gICAgICAgICAgICAgICAgLm1hcChpbWcgPT4gbmV3IFByb21pc2UocmVzb2x2ZSA9PiB7IChpbWcgYXMgSFRNTEltYWdlRWxlbWVudCkub25sb2FkID0gKGltZyBhcyBIVE1MSW1hZ2VFbGVtZW50KS5vbmVycm9yID0gcmVzb2x2ZTsgfSkpKVxyXG4gICAgICAgICAgICAgICAgICAgIC50aGVuKCgpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbGV0IGhhc2ggPSB3aW5kb3cubG9jYXRpb24uaGFzaDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgd2hpbGUgKGhhc2guY2hhckF0KDApID09PSAnIycpIGhhc2ggPSBoYXNoLnN1YnN0cmluZygxKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbGV0IHRhZyA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCJkaXZbZGF0YS1wb3N0SUQ9J1wiICsgaGFzaCArIFwiJ11cIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmICh0YWcpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGxldCB0YWdQb3NpdGlvbiA9IHRhZy5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKS50b3A7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBsZXQgY3J1bWIgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI0ZvcnVtQ29udGFpbmVyICNUb3BCcmVhZGNydW1iXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGV0IGNydW1iSGVpZ2h0ID0gY3J1bWIuZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCkuaGVpZ2h0O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGV0IGUgPSBnZXRDb21wdXRlZFN0eWxlKGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIucG9zdEl0ZW1cIikpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGV0IG1hcmdpbiA9IHBhcnNlRmxvYXQoZS5tYXJnaW5Ub3ApO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGV0IG5ld1Bvc2l0aW9uID0gdGFnUG9zaXRpb24gLSBjcnVtYkhlaWdodCAtIG1hcmdpbjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHdpbmRvdy5zY3JvbGxCeSh7IHRvcDogbmV3UG9zaXRpb24sIGJlaGF2aW9yOiAnYXV0bycgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5pc1Njcm9sbEFkanVzdGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcbiAgICB9O1xyXG5cclxuICAgIHNldEFuc3dlcihwb3N0SUQ6IG51bWJlciwgdG9waWNJRDogbnVtYmVyKSB7XHJcbiAgICAgICAgdmFyIG1vZGVsID0geyBwb3N0SUQ6IHBvc3RJRCwgdG9waWNJRDogdG9waWNJRCB9O1xyXG4gICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0ZvcnVtL1NldEFuc3dlci9cIiwge1xyXG4gICAgICAgICAgICBtZXRob2Q6IFwiUE9TVFwiLFxyXG4gICAgICAgICAgICBib2R5OiBKU09OLnN0cmluZ2lmeShtb2RlbCksXHJcbiAgICAgICAgICAgIGhlYWRlcnM6IHtcclxuICAgICAgICAgICAgICAgIFwiQ29udGVudC1UeXBlXCI6IFwiYXBwbGljYXRpb24vanNvblwiXHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KVxyXG4gICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmFuc3dlclBvc3RJRCA9IHBvc3RJRDtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICB9XHJcbn1cclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbmV4cG9ydCBjbGFzcyBVc2VyU3RhdGUgZXh0ZW5kcyBTdGF0ZUJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgICAgICB0aGlzLmlzUGxhaW5UZXh0ID0gZmFsc2U7XHJcbiAgICAgICAgdGhpcy5uZXdQbUNvdW50ID0gMDtcclxuICAgICAgICB0aGlzLm5vdGlmaWNhdGlvblNlcnZpY2UgPSBuZXcgTm90aWZpY2F0aW9uU2VydmljZSh0aGlzKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIG5vdGlmaWNhdGlvblNlcnZpY2U6IE5vdGlmaWNhdGlvblNlcnZpY2U7XHJcbiAgICBcclxuICAgIGlzUGxhaW5UZXh0OiBib29sZWFuO1xyXG4gICAgaXNJbWFnZUVuYWJsZWQ6IGJvb2xlYW47XHJcblxyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuICAgIG5ld1BtQ291bnQ6IG51bWJlcjtcclxufVxyXG5cclxufSJdfQ==