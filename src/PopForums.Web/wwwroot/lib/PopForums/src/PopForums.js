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
// TODO: Move this to an open websockets connection
var PopForums;
(function (PopForums) {
    class TimeUpdater {
        Start() {
            PopForums.Ready(() => {
                this.StartUpdater();
            });
        }
        PopulatePostData() {
            let a = [];
            let times = document.querySelectorAll(".fTime");
            times.forEach(time => {
                var t = time.getAttribute("data-utc");
                if (((new Date().getDate() - new Date(t + "Z").getDate()) / 3600000) < 48)
                    a.push(t);
            });
            if (a.length > 0) {
                this.subHourTimes = new FormData();
                a.forEach(t => this.subHourTimes.append("times", t));
            }
        }
        StartUpdater() {
            setTimeout(() => {
                this.StartUpdater();
                this.PopulatePostData();
                if (this.subHourTimes)
                    this.CallForUpdate();
            }, 60000);
        }
        CallForUpdate() {
            fetch(PopForums.AreaPath + "/Time/GetTimes", {
                method: "POST",
                body: this.subHourTimes
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
        }
    }
    __decorate([
        PopForums.WatchProperty
    ], UserState.prototype, "newPmCount", void 0);
    PopForums.UserState = UserState;
})(PopForums || (PopForums = {}));
//# sourceMappingURL=PopForums.js.map