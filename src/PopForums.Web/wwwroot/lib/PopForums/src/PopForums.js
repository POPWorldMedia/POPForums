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
            let connection = new signalR.HubConnectionBuilder().withUrl("/FeedHub").withAutomaticReconnect().build();
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
            let connection = new signalR.HubConnectionBuilder().withUrl("/ForumsHub").withAutomaticReconnect().build();
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
            this.connection = new signalR.HubConnectionBuilder().withUrl("/NotificationHub").withAutomaticReconnect().build();
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
            let connection = new signalR.HubConnectionBuilder().withUrl("/ForumsHub").withAutomaticReconnect().build();
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
            var connection = new signalR.HubConnectionBuilder().withUrl("/RecentHub").withAutomaticReconnect().build();
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
                let connection = new signalR.HubConnectionBuilder().withUrl("/TopicsHub").withAutomaticReconnect().build();
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
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiUG9wRm9ydW1zLmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXMiOlsiQ2xpZW50L0RlY2xhcmF0aW9ucy50cyIsIkNsaWVudC9FbGVtZW50QmFzZS50cyIsIkNsaWVudC9TdGF0ZUJhc2UudHMiLCJDbGllbnQvV2F0Y2hQcm9wZXJ0eUF0dHJpYnV0ZS50cyIsIkNsaWVudC9Db21wb25lbnRzL0Fuc3dlckJ1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL0NvbW1lbnRCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9GYXZvcml0ZUJ1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL0ZlZWRVcGRhdGVyLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvRnVsbFRleHQudHMiLCJDbGllbnQvQ29tcG9uZW50cy9Ib21lVXBkYXRlci50cyIsIkNsaWVudC9Db21wb25lbnRzL0xvZ2luRm9ybS50cyIsIkNsaWVudC9Db21wb25lbnRzL01vcmVQb3N0c0JlZm9yZVJlcGx5QnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvTW9yZVBvc3RzQnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvUE1Db3VudC50cyIsIkNsaWVudC9Db21wb25lbnRzL1Bvc3RNaW5pUHJvZmlsZS50cyIsIkNsaWVudC9Db21wb25lbnRzL1Bvc3RNb2RlcmF0aW9uTG9nQnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvUHJldmlld0J1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL1ByZXZpb3VzUG9zdHNCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9RdW90ZUJ1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL1JlcGx5QnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvUmVwbHlGb3JtLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvU3Vic2NyaWJlQnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvVG9waWNCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9Ub3BpY0Zvcm0udHMiLCJDbGllbnQvQ29tcG9uZW50cy9Ub3BpY01vZGVyYXRpb25Mb2dCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9Wb3RlQ291bnQudHMiLCJDbGllbnQvU2VydmljZXMvTm90aWZpY2F0aW9uU2VydmljZS50cyIsIkNsaWVudC9TZXJ2aWNlcy9UaW1lVXBkYXRlci50cyIsIkNsaWVudC9TdGF0ZS9Gb3J1bVN0YXRlLnRzIiwiQ2xpZW50L1N0YXRlL1RvcGljU3RhdGUudHMiLCJDbGllbnQvU3RhdGUvVXNlclN0YXRlLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7Ozs7OztBQUFBLElBQVUsU0FBUyxDQVVsQjtBQVZELFdBQVUsU0FBUztJQUNGLGtCQUFRLEdBQUcsU0FBUyxDQUFDO0lBS2xDLFNBQWdCLEtBQUssQ0FBQyxRQUFhO1FBQy9CLElBQUksUUFBUSxDQUFDLFVBQVUsSUFBSSxTQUFTO1lBQUUsUUFBUSxFQUFFLENBQUM7O1lBQzVDLFFBQVEsQ0FBQyxnQkFBZ0IsQ0FBQyxrQkFBa0IsRUFBRSxRQUFRLENBQUMsQ0FBQztJQUNqRSxDQUFDO0lBSGUsZUFBSyxRQUdwQixDQUFBO0FBQ0wsQ0FBQyxFQVZTLFNBQVMsS0FBVCxTQUFTLFFBVWxCO0FDVkQsSUFBVSxTQUFTLENBNkJsQjtBQTdCRCxXQUFVLFNBQVM7SUFFbkIsTUFBc0IsV0FBWSxTQUFRLFdBQVc7UUFFakQsaUJBQWlCO1lBQ2IsSUFBSSxJQUFJLENBQUMsS0FBSyxJQUFJLElBQUksQ0FBQyxlQUFlO2dCQUNsQyxPQUFPO1lBQ1gsSUFBSSxxQkFBcUIsR0FBRyxJQUFJLENBQUMscUJBQXFCLEVBQUUsQ0FBQztZQUN6RCxJQUFJLENBQUMsS0FBSyxHQUFHLHFCQUFxQixDQUFDLENBQUMsQ0FBQyxDQUFDO1lBQ3RDLElBQUksQ0FBQyxlQUFlLEdBQUcscUJBQXFCLENBQUMsQ0FBQyxDQUFDLENBQUM7WUFDaEQsTUFBTSxRQUFRLEdBQUcsSUFBSSxDQUFDLE1BQU0sQ0FBQyxJQUFJLENBQUMsSUFBSSxDQUFD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UUQsSUFBVSxTQUFTLENBbUJsQjtBQW5CRCxXQUFVLFNBQVM7SUFFbkIsTUFBYSxTQUFVLFNBQVEsVUFBQSxTQUFTO1FBQ3BDO1lBQ0ksS0FBSyxFQUFFLENBQUM7WUFDUixJQUFJLENBQUMsV0FBVyxHQUFHLEtBQUssQ0FBQztZQUN6QixJQUFJLENBQUMsVUFBVSxHQUFHLENBQUMsQ0FBQztZQUNwQixJQUFJLENBQUMsbUJBQW1CLEdBQUcsSUFBSSxVQUFBLG1CQUFtQixDQUFDLElBQUksQ0FBQyxDQUFDO1FBQzdELENBQUM7S0FTSjtJQURHO1FBREMsVUFBQSxhQUFhO2lEQUNLO0lBZFYsbUJBQVMsWUFlckIsQ0FBQTtBQUVELENBQUMsRUFuQlMsU0FBUyxLQUFULFNBQVMsUUFtQmxCIiwic291cmNlc0NvbnRlbnQiOlsibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcbiAgICBleHBvcnQgY29uc3QgQXJlYVBhdGggPSBcIi9Gb3J1bXNcIjtcclxuICAgIGV4cG9ydCB2YXIgY3VycmVudFRvcGljU3RhdGU6IFRvcGljU3RhdGU7XHJcbiAgICBleHBvcnQgdmFyIGN1cnJlbnRGb3J1bVN0YXRlOiBGb3J1bVN0YXRlO1xyXG4gICAgZXhwb3J0IHZhciB1c2VyU3RhdGU6IFVzZXJTdGF0ZTtcclxuXHJcbiAgICBleHBvcnQgZnVuY3Rpb24gUmVhZHkoY2FsbGJhY2s6IGFueSk6IHZvaWQge1xyXG4gICAgICAgIGlmIChkb2N1bWVudC5yZWFkeVN0YXRlICE9IFwibG9hZGluZ1wiKSBjYWxsYmFjaygpO1xyXG4gICAgICAgIGVsc2UgZG9jdW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcIkRPTUNvbnRlbnRMb2FkZWRcIiwgY2FsbGJhY2spO1xyXG4gICAgfVxyXG59XHJcblxyXG5cclxuZGVjbGFyZSBuYW1lc3BhY2UgdGlueW1jZSB7XHJcbiAgICBmdW5jdGlvbiBpbml0KG9wdGlvbnM6YW55KTogYW55O1xyXG4gICAgZnVuY3Rpb24gZ2V0KGlkOnN0cmluZyk6IGFueTtcclxuICAgIGZ1bmN0aW9uIHRyaWdnZXJTYXZlKCk6IGFueTtcclxufVxyXG5cclxuZGVjbGFyZSBuYW1lc3BhY2UgYm9vdHN0cmFwIHtcclxuICAgIGNsYXNzIFRvb2x0aXAge1xyXG4gICAgICAgIGNvbnN0cnVjdG9yKGVsOiBFbGVtZW50LCBvcHRpb25zOmFueSk7XHJcbiAgICB9XHJcbiAgICBjbGFzcyBQb3BvdmVyIHtcclxuICAgICAgICBjb25zdHJ1Y3RvcihlbDogRWxlbWVudCwgb3B0aW9uczphbnkpO1xyXG4gICAgICAgIGVuYWJsZSgpOiB2b2lkO1xyXG4gICAgICAgIGRpc2FibGUoKTogdm9pZDtcclxuICAgIH1cclxufVxyXG5cclxuZGVjbGFyZSBuYW1lc3BhY2Ugc2lnbmFsUiB7XHJcbiAgICBjbGFzcyBIdWJDb25uZWN0aW9uQnVpbGRlciB7XHJcbiAgICAgICAgd2l0aFVybCh1cmw6IHN0cmluZyk6IGFueTtcclxuICAgIH1cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuZXhwb3J0IGFic3RyYWN0IGNsYXNzIEVsZW1lbnRCYXNlIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIGlmICh0aGlzLnN0YXRlICYmIHRoaXMucHJvcGVydHlUb1dhdGNoKVxyXG4gICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgbGV0IHN0YXRlQW5kV2F0Y2hQcm9wZXJ0eSA9IHRoaXMuZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk7XHJcbiAgICAgICAgdGhpcy5zdGF0ZSA9IHN0YXRlQW5kV2F0Y2hQcm9wZXJ0eVswXTtcclxuICAgICAgICB0aGlzLnByb3BlcnR5VG9XYXRjaCA9IHN0YXRlQW5kV2F0Y2hQcm9wZXJ0eVsxXTtcclxuICAgICAgICBjb25zdCBkZWxlZ2F0ZSA9IHRoaXMudXBkYXRlLmJpbmQodGhpcyk7XHJcbiAgICAgICAgdGhpcy5zdGF0ZS5zdWJzY3JpYmUodGhpcy5wcm9wZXJ0eVRvV2F0Y2gsIGRlbGVnYXRlKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIHN0YXRlOiBTdGF0ZUJhc2U7XHJcbiAgICBwcml2YXRlIHByb3BlcnR5VG9XYXRjaDogc3RyaW5nO1xyXG5cclxuICAgIHVwZGF0ZSgpIHtcclxuICAgICAgICBjb25zdCBleHRlcm5hbFZhbHVlID0gdGhpcy5zdGF0ZVt0aGlzLnByb3BlcnR5VG9XYXRjaF07XHJcbiAgICAgICAgdGhpcy51cGRhdGVVSShleHRlcm5hbFZhbHVlKTtcclxuICAgIH1cclxuXHJcbiAgICAvLyBJbXBsZW1lbnRhdGlvbiBzaG91bGQgcmV0dXJuIHRoZSBTdGF0ZUJhc2UgYW5kIHByb3BlcnR5IChhcyBhIHN0cmluZykgdG8gbW9uaXRvclxyXG4gICAgYWJzdHJhY3QgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ107XHJcblxyXG4gICAgLy8gVXNlIGluIHRoZSBpbXBsZW1lbnRhdGlvbiB0byBtYW5pcHVsYXRlIHRoZSBzaGFkb3cgb3IgbGlnaHQgRE9NIG9yIHN0cmFpZ2h0IG1hcmt1cCBhcyBuZWVkZWQgaW4gcmVzcG9uc2UgdG8gdGhlIG5ldyBkYXRhLlxyXG4gICAgYWJzdHJhY3QgdXBkYXRlVUkoZGF0YTogYW55KTogdm9pZDtcclxufVxyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuLy8gUHJvcGVydGllcyB0byB3YXRjaCByZXF1aXJlIHRoZSBAV2F0Y2hQcm9wZXJ0eSBhdHRyaWJ1dGUuXHJcbmV4cG9ydCBjbGFzcyBTdGF0ZUJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgdGhpcy5fc3VicyA9IG5ldyBNYXA8c3RyaW5nLCBBcnJheTxGdW5jdGlvbj4+KCk7XHJcbiAgICB9XHJcblxyXG4gICAgcHJpdmF0ZSBfc3ViczogTWFwPHN0cmluZywgQXJyYXk8RnVuY3Rpb24+PjtcclxuXHJcbiAgICBzdWJzY3JpYmUocHJvcGVydHlOYW1lOiBzdHJpbmcsIGV2ZW50SGFuZGxlcjogRnVuY3Rpb24pIHtcclxuICAgICAgICBpZiAoIXRoaXMuX3N1YnMuaGFzKHByb3BlcnR5TmFtZSkpXHJcbiAgICAgICAgICAgIHRoaXMuX3N1YnMuc2V0KHByb3BlcnR5TmFtZSwgbmV3IEFycmF5PEZ1bmN0aW9uPigpKTtcclxuICAgICAgICBjb25zdCBjYWxsYmFja3MgPSB0aGlzLl9zdWJzLmdldChwcm9wZXJ0eU5hbWUpO1xyXG4gICAgICAgIGNhbGxiYWNrcy5wdXNoKGV2ZW50SGFuZGxlcik7XHJcbiAgICAgICAgZXZlbnRIYW5kbGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgbm90aWZ5KHByb3BlcnR5TmFtZTogc3RyaW5nKSB7XHJcbiAgICAgICAgY29uc3QgY2FsbGJhY2tzID0gdGhpcy5fc3Vicy5nZXQocHJvcGVydHlOYW1lKTtcclxuICAgICAgICBpZiAoY2FsbGJhY2tzKVxyXG4gICAgICAgICAgICBmb3IgKGxldCBpIG9mIGNhbGxiYWNrcykge1xyXG4gICAgICAgICAgICAgICAgaSgpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICB9XHJcbn1cclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbmV4cG9ydCBjb25zdCBXYXRjaFByb3BlcnR5ID0gKHRhcmdldDogYW55LCBtZW1iZXJOYW1lOiBzdHJpbmcpID0+IHtcclxuICAgIGxldCBjdXJyZW50VmFsdWU6IGFueSA9IHRhcmdldFttZW1iZXJOYW1lXTsgIFxyXG4gICAgT2JqZWN0LmRlZmluZVByb3BlcnR5KHRhcmdldCwgbWVtYmVyTmFtZSwge1xyXG4gICAgICAgIHNldCh0aGlzOiBhbnksIG5ld1ZhbHVlOiBhbnkpIHtcclxuICAgICAgICAgICAgY3VycmVudFZhbHVlID0gbmV3VmFsdWU7XHJcbiAgICAgICAgICAgIHRoaXMubm90aWZ5KG1lbWJlck5hbWUpO1xyXG4gICAgICAgIH0sXHJcbiAgICAgICAgZ2V0KCkge3JldHVybiBjdXJyZW50VmFsdWU7fVxyXG4gICAgfSk7XHJcbn07XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIEFuc3dlckJ1dHRvbiBleHRlbmRzIEVsZW1lbnRCYXNlIHtcclxuICAgICAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICAgICAgc3VwZXIoKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldCBhbnN3ZXJzdGF0dXNjbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJhbnN3ZXJzdGF0dXNjbGFzc1wiKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZ2V0IGNob29zZWFuc3dlcnRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiY2hvb3NlYW5zd2VydGV4dFwiKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZ2V0IHRvcGljaWQoKTogc3RyaW5nIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidG9waWNpZFwiKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZ2V0IHBvc3RpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJwb3N0aWRcIik7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGdldCBhbnN3ZXJwb3N0aWQoKTogc3RyaW5nIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYW5zd2VycG9zdGlkXCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBnZXQgdXNlcmlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInVzZXJpZFwiKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZ2V0IHN0YXJ0ZWRieXVzZXJpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJzdGFydGVkYnl1c2VyaWRcIik7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGdldCBpc2ZpcnN0aW50b3BpYygpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJpc2ZpcnN0aW50b3BpY1wiKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgYnV0dG9uOiBIVE1MRWxlbWVudDtcclxuXHJcbiAgICAgICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChcInBcIik7XHJcbiAgICAgICAgICAgIHRoaXMuYW5zd2Vyc3RhdHVzY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgICAgICBpZiAodGhpcy5pc2ZpcnN0aW50b3BpYy50b0xvd2VyQ2FzZSgpID09PSBcImZhbHNlXCIgJiYgdGhpcy51c2VyaWQgPT09IHRoaXMuc3RhcnRlZGJ5dXNlcmlkKSB7XHJcbiAgICAgICAgICAgICAgICAvLyBtYWtlIGl0IGEgYnV0dG9uIGZvciBhdXRob3JcclxuICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLnNldEFuc3dlcihOdW1iZXIodGhpcy5wb3N0aWQpLCBOdW1iZXIodGhpcy50b3BpY2lkKSk7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB0aGlzLmFwcGVuZENoaWxkKHRoaXMuYnV0dG9uKTtcclxuICAgICAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICAgICAgcmV0dXJuIFtQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUsIFwiYW5zd2VyUG9zdElEXCJdO1xyXG4gICAgICAgIH1cclxuICAgICAgICBcclxuICAgICAgICB1cGRhdGVVSShhbnN3ZXJQb3N0SUQ6IG51bWJlcik6IHZvaWQge1xyXG4gICAgICAgICAgICBpZiAodGhpcy5pc2ZpcnN0aW50b3BpYy50b0xvd2VyQ2FzZSgpID09PSBcImZhbHNlXCIgJiYgdGhpcy51c2VyaWQgPT09IHRoaXMuc3RhcnRlZGJ5dXNlcmlkKSB7XHJcbiAgICAgICAgICAgICAgICAvLyB0aGlzIGlzIHF1ZXN0aW9uIGF1dGhvclxyXG4gICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LmFkZChcImFzbnN3ZXJCdXR0b25cIik7XHJcbiAgICAgICAgICAgICAgICBpZiAoYW5zd2VyUG9zdElEICYmIHRoaXMucG9zdGlkID09PSBhbnN3ZXJQb3N0SUQudG9TdHJpbmcoKSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5yZW1vdmUoXCJpY29uLWNoZWNrbWFyazJcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LnJlbW92ZShcInRleHQtbXV0ZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LmFkZChcImljb24tY2hlY2ttYXJrXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJ0ZXh0LXN1Y2Nlc3NcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5zdHlsZS5jdXJzb3IgPSBcImRlZmF1bHRcIjtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5yZW1vdmUoXCJpY29uLWNoZWNrbWFya1wiKTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmJ1dHRvbi5jbGFzc0xpc3QucmVtb3ZlKFwidGV4dC1zdWNjZXNzXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJpY29uLWNoZWNrbWFyazJcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LmFkZChcInRleHQtbXV0ZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5zdHlsZS5jdXJzb3IgPSBcInBvaW50ZXJcIjtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBlbHNlIGlmIChhbnN3ZXJQb3N0SUQgJiYgdGhpcy5wb3N0aWQgPT09IGFuc3dlclBvc3RJRC50b1N0cmluZygpKSB7XHJcbiAgICAgICAgICAgICAgICAvLyBub3QgdGhlIHF1ZXN0aW9uIGF1dGhvciwgYnV0IGl0IGlzIHRoZSBhbnN3ZXJcclxuICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJpY29uLWNoZWNrbWFya1wiKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJ0ZXh0LXN1Y2Nlc3NcIik7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnN0eWxlLmN1cnNvciA9IFwiZGVmYXVsdFwiO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLWFuc3dlcmJ1dHRvbicsIEFuc3dlckJ1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIENvbW1lbnRCdXR0b24gZXh0ZW5kcyBFbGVtZW50QmFzZSB7XHJcbiAgICAgICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgICAgIHN1cGVyKCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBnZXQgYnV0dG9udGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b250ZXh0XCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBcclxuICAgICAgICBnZXQgdG9waWNpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0b3BpY2lkXCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBcclxuICAgICAgICBnZXQgcG9zdGlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInBvc3RpZFwiKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgICAgICB0aGlzLmlubmVySFRNTCA9IENvbW1lbnRCdXR0b24udGVtcGxhdGU7XHJcbiAgICAgICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmJ1dHRvbnRleHQ7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmJ1dHRvbmNsYXNzPy5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICAgICAgdGhpcy5idXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgICAgICBidXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIChlOiBNb3VzZUV2ZW50KSA9PiB7XHJcbiAgICAgICAgICAgICAgICBQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUubG9hZENvbW1lbnQoTnVtYmVyKHRoaXMudG9waWNpZCksIE51bWJlcih0aGlzLnBvc3RpZCkpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICAgICAgcmV0dXJuIFtQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUsIFwiY29tbWVudFJlcGx5SURcIl07XHJcbiAgICAgICAgfVxyXG4gICAgICAgIFxyXG4gICAgICAgIHVwZGF0ZVVJKGRhdGE6IG51bWJlcik6IHZvaWQge1xyXG4gICAgICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgICAgIGlmIChkYXRhICE9PSB1bmRlZmluZWQpIHtcclxuICAgICAgICAgICAgICAgIGJ1dHRvbi5kaXNhYmxlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICBidXR0b24uc3R5bGUuY3Vyc29yID0gXCJkZWZhdWx0XCI7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgYnV0dG9uLmRpc2FibGVkID0gZmFsc2U7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIC8+YDtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1jb21tZW50YnV0dG9uJywgQ29tbWVudEJ1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIEZhdm9yaXRlQnV0dG9uIGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgbWFrZWZhdm9yaXRldGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcIm1ha2VmYXZvcml0ZXRleHRcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgcmVtb3ZlZmF2b3JpdGV0ZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwicmVtb3ZlZmF2b3JpdGV0ZXh0XCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIHRoaXMuaW5uZXJIVE1MID0gU3Vic2NyaWJlQnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b246IEhUTUxJbnB1dEVsZW1lbnQgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICB0aGlzLmJ1dHRvbmNsYXNzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiBidXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0Zhdm9yaXRlcy9Ub2dnbGVGYXZvcml0ZS9cIiArIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS50b3BpY0lELCB7XHJcbiAgICAgICAgICAgICAgICBtZXRob2Q6IFwiUE9TVFwiXHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS5qc29uKCkpXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXN1bHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIHN3aXRjaCAocmVzdWx0LmRhdGEuaXNGYXZvcml0ZSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjYXNlIHRydWU6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUuaXNGYXZvcml0ZSA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSBmYWxzZTpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5pc0Zhdm9yaXRlID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgZGVmYXVsdDpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIC8vIFRPRE86IHNvbWV0aGluZyBlbHNlXHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC5jYXRjaCgoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgLy8gVE9ETzogaGFuZGxlIGVycm9yXHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9KTtcclxuICAgICAgICBzdXBlci5jb25uZWN0ZWRDYWxsYmFjaygpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZSwgXCJpc0Zhdm9yaXRlXCJdO1xyXG4gICAgfVxyXG5cclxuICAgIHVwZGF0ZVVJKGRhdGE6IGJvb2xlYW4pOiB2b2lkIHtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgaWYgKGRhdGEpXHJcbiAgICAgICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMucmVtb3ZlZmF2b3JpdGV0ZXh0O1xyXG4gICAgICAgIGVsc2VcclxuICAgICAgICAgICAgYnV0dG9uLnZhbHVlID0gdGhpcy5tYWtlZmF2b3JpdGV0ZXh0O1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgLz5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLWZhdm9yaXRlYnV0dG9uJywgRmF2b3JpdGVCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBGZWVkVXBkYXRlciBleHRlbmRzIEhUTUxFbGVtZW50IHtcclxuICAgICAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICAgICAgc3VwZXIoKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldCB0ZW1wbGF0ZWlkKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0ZW1wbGF0ZWlkXCIpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgICAgIGxldCBjb25uZWN0aW9uID0gbmV3IHNpZ25hbFIuSHViQ29ubmVjdGlvbkJ1aWxkZXIoKS53aXRoVXJsKFwiL0ZlZWRIdWJcIikud2l0aEF1dG9tYXRpY1JlY29ubmVjdCgpLmJ1aWxkKCk7XHJcbiAgICAgICAgICAgIGxldCBzZWxmID0gdGhpcztcclxuICAgICAgICAgICAgY29ubmVjdGlvbi5vbihcIm5vdGlmeUZlZWRcIiwgZnVuY3Rpb24gKGRhdGE6IGFueSkge1xyXG4gICAgICAgICAgICAgICAgbGV0IGxpc3QgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI0ZlZWRMaXN0XCIpO1xyXG4gICAgICAgICAgICAgICAgbGV0IHJvdyA9IHNlbGYucG9wdWxhdGVGZWVkUm93KGRhdGEpO1xyXG4gICAgICAgICAgICAgICAgbGlzdC5wcmVwZW5kKHJvdyk7XHJcbiAgICAgICAgICAgICAgICByb3cuY2xhc3NMaXN0LnJlbW92ZShcImhpZGRlblwiKTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIGNvbm5lY3Rpb24uc3RhcnQoKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4oZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBjb25uZWN0aW9uLmludm9rZShcImxpc3RlblRvQWxsXCIpO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBwb3B1bGF0ZUZlZWRSb3coZGF0YTogYW55KSB7XHJcbiAgICAgICAgICAgIGxldCB0ZW1wbGF0ZSA9IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKHRoaXMudGVtcGxhdGVpZCk7XHJcbiAgICAgICAgICAgIGlmICghdGVtcGxhdGUpIHtcclxuICAgICAgICAgICAgICAgIGNvbnNvbGUuZXJyb3IoYENhbid0IGZpbmQgSUQgJHt0aGlzLnRlbXBsYXRlaWR9IHRvIG1ha2UgZmVlZCB1cGRhdGVzLmApO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGxldCByb3cgPSB0ZW1wbGF0ZS5jbG9uZU5vZGUodHJ1ZSkgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgICAgIHJvdy5yZW1vdmVBdHRyaWJ1dGUoXCJpZFwiKTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIuZmVlZEl0ZW1UZXh0XCIpLmlubmVySFRNTCA9IGRhdGEubWVzc2FnZTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIuZlRpbWVcIikuc2V0QXR0cmlidXRlKFwiZGF0YS11dGNcIiwgZGF0YS51dGMpO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5mVGltZVwiKS5pbm5lckhUTUwgPSBkYXRhLnRpbWVTdGFtcDtcclxuICAgICAgICAgICAgcmV0dXJuIHJvdztcclxuICAgICAgICB9O1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICBjdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLWZlZWR1cGRhdGVyJywgRmVlZFVwZGF0ZXIpO1xyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIEZ1bGxUZXh0IGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgb3ZlcnJpZGVsaXN0ZW5lcigpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcIm92ZXJyaWRlbGlzdGVuZXJcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGZvcm1JRCgpIHsgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiZm9ybWlkXCIpIH07XHJcblxyXG4gICAgZ2V0IHZhbHVlKCkgeyByZXR1cm4gdGhpcy5fdmFsdWU7fVxyXG4gICAgc2V0IHZhbHVlKHY6IHN0cmluZykgeyB0aGlzLl92YWx1ZSA9IHY7IH1cclxuXHJcbiAgICBfdmFsdWU6IHN0cmluZztcclxuXHJcbiAgICBzdGF0aWMgZm9ybUFzc29jaWF0ZWQgPSB0cnVlO1xyXG5cclxuICAgIHByaXZhdGUgdGV4dEJveDogSFRNTEVsZW1lbnQ7XHJcbiAgICBwcml2YXRlIGV4dGVybmFsRm9ybUVsZW1lbnQ6IEhUTUxFbGVtZW50O1xyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIHZhciBpbml0aWFsVmFsdWUgPSB0aGlzLmdldEF0dHJpYnV0ZShcInZhbHVlXCIpO1xyXG4gICAgICAgIGlmIChpbml0aWFsVmFsdWUpXHJcbiAgICAgICAgICAgIHRoaXMudmFsdWUgPSBpbml0aWFsVmFsdWU7XHJcbiAgICAgICAgaWYgKHVzZXJTdGF0ZS5pc1BsYWluVGV4dCkge1xyXG4gICAgICAgICAgICB0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwidGV4dGFyZWFcIik7XHJcbiAgICAgICAgICAgIHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudC5pZCA9IHRoaXMuZm9ybUlEO1xyXG4gICAgICAgICAgICB0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQuc2V0QXR0cmlidXRlKFwibmFtZVwiLCB0aGlzLmZvcm1JRCk7XHJcbiAgICAgICAgICAgIHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudC5jbGFzc0xpc3QuYWRkKFwiZm9ybS1jb250cm9sXCIpO1xyXG4gICAgICAgICAgICBpZiAodGhpcy52YWx1ZSlcclxuICAgICAgICAgICAgKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBIVE1MVGV4dEFyZWFFbGVtZW50KS52YWx1ZSA9IHRoaXMudmFsdWU7XHJcbiAgICAgICAgICAgICh0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQgYXMgSFRNTFRleHRBcmVhRWxlbWVudCkucm93cyA9IDEyO1xyXG4gICAgICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgICAgIHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwiY2hhbmdlXCIsICgpID0+IHtcclxuICAgICAgICAgICAgICAgIHNlbGYudmFsdWUgPSAodGhpcy5leHRlcm5hbEZvcm1FbGVtZW50IGFzIEhUTUxUZXh0QXJlYUVsZW1lbnQpLnZhbHVlO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgdGhpcy5hcHBlbmRDaGlsZCh0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQpO1xyXG4gICAgICAgICAgICBpZiAodGhpcy5vdmVycmlkZWxpc3RlbmVyPy50b0xvd2VyQ2FzZSgpICE9PSBcInRydWVcIilcclxuICAgICAgICAgICAgICAgIHN1cGVyLmNvbm5lY3RlZENhbGxiYWNrKCk7XHJcbiAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICB9XHJcbiAgICAgICAgbGV0IHRlbXBsYXRlID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChcInRlbXBsYXRlXCIpO1xyXG4gICAgICAgIHRlbXBsYXRlLmlubmVySFRNTCA9IEZ1bGxUZXh0LnRlbXBsYXRlO1xyXG4gICAgICAgIHRoaXMuYXR0YWNoU2hhZG93KHsgbW9kZTogXCJvcGVuXCIgfSk7XHJcbiAgICAgICAgdGhpcy5zaGFkb3dSb290LmFwcGVuZCh0ZW1wbGF0ZS5jb250ZW50LmNsb25lTm9kZSh0cnVlKSk7XHJcbiAgICAgICAgdGhpcy50ZXh0Qm94ID0gdGhpcy5zaGFkb3dSb290LnF1ZXJ5U2VsZWN0b3IoXCIjZWRpdG9yXCIpO1xyXG4gICAgICAgIGlmICh0aGlzLnZhbHVlKVxyXG4gICAgICAgICAgICAodGhpcy50ZXh0Qm94IGFzIEhUTUxUZXh0QXJlYUVsZW1lbnQpLmlubmVyVGV4dCA9IHRoaXMudmFsdWU7XHJcbiAgICAgICAgdGhpcy5lZGl0b3JTZXR0aW5ncy50YXJnZXQgPSB0aGlzLnRleHRCb3g7XHJcbiAgICAgICAgaWYgKCF1c2VyU3RhdGUuaXNJbWFnZUVuYWJsZWQpXHJcbiAgICAgICAgICAgIHRoaXMuZWRpdG9yU2V0dGluZ3MudG9vbGJhciA9IEZ1bGxUZXh0LnBvc3ROb0ltYWdlVG9vbGJhcjtcclxuICAgICAgICB2YXIgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgdGhpcy5lZGl0b3JTZXR0aW5ncy5zZXR1cCA9IGZ1bmN0aW9uIChlZGl0b3I6IGFueSkge1xyXG4gICAgICAgICAgICBlZGl0b3Iub24oXCJpbml0XCIsIGZ1bmN0aW9uICgpIHtcclxuICAgICAgICAgICAgICB0aGlzLm9uKFwiZm9jdXNvdXRcIiwgZnVuY3Rpb24oZTogYW55KSB7XHJcbiAgICAgICAgICAgICAgICBlZGl0b3Iuc2F2ZSgpO1xyXG4gICAgICAgICAgICAgICAgc2VsZi52YWx1ZSA9IChzZWxmLnRleHRCb3ggYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWU7XHJcbiAgICAgICAgICAgICAgICAoc2VsZi5leHRlcm5hbEZvcm1FbGVtZW50IGFzIGFueSkudmFsdWUgPSBzZWxmLnZhbHVlO1xyXG4gICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgIHRoaXMub24oXCJibHVyXCIsIGZ1bmN0aW9uKGU6IGFueSkge1xyXG4gICAgICAgICAgICAgICAgZWRpdG9yLnNhdmUoKTtcclxuICAgICAgICAgICAgICAgIHNlbGYudmFsdWUgPSAoc2VsZi50ZXh0Qm94IGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlO1xyXG4gICAgICAgICAgICAgICAgKHNlbGYuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBhbnkpLnZhbHVlID0gc2VsZi52YWx1ZTtcclxuICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICBlZGl0b3Iuc2F2ZSgpO1xyXG4gICAgICAgICAgICAgIHNlbGYudmFsdWUgPSAoc2VsZi50ZXh0Qm94IGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlO1xyXG4gICAgICAgICAgICAgIChzZWxmLmV4dGVybmFsRm9ybUVsZW1lbnQgYXMgYW55KS52YWx1ZSA9IHNlbGYudmFsdWU7XHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgfTtcclxuICAgICAgICB0aW55bWNlLmluaXQodGhpcy5lZGl0b3JTZXR0aW5ncyk7XHJcbiAgICAgICAgdGhpcy5leHRlcm5hbEZvcm1FbGVtZW50ID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChcImlucHV0XCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQ7XHJcbiAgICAgICAgdGhpcy5leHRlcm5hbEZvcm1FbGVtZW50LmlkID0gdGhpcy5mb3JtSUQ7XHJcbiAgICAgICAgdGhpcy5leHRlcm5hbEZvcm1FbGVtZW50LnNldEF0dHJpYnV0ZShcIm5hbWVcIiwgdGhpcy5mb3JtSUQpO1xyXG4gICAgICAgICh0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQgYXMgSFRNTElucHV0RWxlbWVudCkudHlwZSA9IFwiaGlkZGVuXCI7XHJcbiAgICAgICAgdGhpcy5hcHBlbmRDaGlsZCh0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQpO1xyXG4gICAgICAgIGlmICh0aGlzLm92ZXJyaWRlbGlzdGVuZXI/LnRvTG93ZXJDYXNlKCkgIT09IFwidHJ1ZVwiKVxyXG4gICAgICAgICAgICBzdXBlci5jb25uZWN0ZWRDYWxsYmFjaygpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZSwgXCJuZXh0UXVvdGVcIl07XHJcbiAgICB9XHJcblxyXG4gICAgdXBkYXRlVUkoZGF0YTogYW55KTogdm9pZCB7XHJcbiAgICAgICAgaWYgKGRhdGEgIT09IG51bGwgJiYgZGF0YSAhPT0gdW5kZWZpbmVkKVxyXG4gICAgICAgIHtcclxuICAgICAgICAgICAgaWYgKHVzZXJTdGF0ZS5pc1BsYWluVGV4dCkge1xyXG4gICAgICAgICAgICAgICAgKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBIVE1MVGV4dEFyZWFFbGVtZW50KS52YWx1ZSArPSBkYXRhO1xyXG4gICAgICAgICAgICAgICAgdGhpcy52YWx1ZSA9ICh0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQgYXMgSFRNTFRleHRBcmVhRWxlbWVudCkudmFsdWU7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgZWxzZSB7XHJcbiAgICAgICAgICAgICAgICBsZXQgZWRpdG9yID0gdGlueW1jZS5nZXQoXCJlZGl0b3JcIik7XHJcbiAgICAgICAgICAgICAgICB2YXIgY29udGVudCA9IGVkaXRvci5nZXRDb250ZW50KCk7XHJcbiAgICAgICAgICAgICAgICBjb250ZW50ICs9IGRhdGE7XHJcbiAgICAgICAgICAgICAgICBlZGl0b3Iuc2V0Q29udGVudChjb250ZW50KTtcclxuICAgICAgICAgICAgICAgICh0aGlzLnRleHRCb3ggYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWUgKz0gY29udGVudDtcclxuICAgICAgICAgICAgICAgIGVkaXRvci5zYXZlKCk7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnZhbHVlID0gKHRoaXMudGV4dEJveCBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZTtcclxuICAgICAgICAgICAgICAgICh0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQgYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWUgPSB0aGlzLnZhbHVlO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIFxyXG4gICAgcHJpdmF0ZSBzdGF0aWMgZWRpdG9yQ1NTID0gXCIvbGliL2Jvb3RzdHJhcC9kaXN0L2Nzcy9ib290c3RyYXAubWluLmNzcywvbGliL1BvcEZvcnVtcy9kaXN0L0VkaXRvci5taW4uY3NzXCI7XHJcbiAgICBwcml2YXRlIHN0YXRpYyBwb3N0Tm9JbWFnZVRvb2xiYXIgPSBcImN1dCBjb3B5IHBhc3RlIHwgYm9sZCBpdGFsaWMgfCBidWxsaXN0IG51bWxpc3QgYmxvY2txdW90ZSByZW1vdmVmb3JtYXQgfCBsaW5rXCI7XHJcbiAgICBlZGl0b3JTZXR0aW5ncyA9IHtcclxuICAgICAgICB0YXJnZXQ6IG51bGwgYXMgSFRNTEVsZW1lbnQsXHJcbiAgICAgICAgcGx1Z2luczogXCJsaXN0cyBpbWFnZSBsaW5rXCIsXHJcbiAgICAgICAgY29udGVudF9jc3M6IEZ1bGxUZXh0LmVkaXRvckNTUyxcclxuICAgICAgICBtZW51YmFyOiBmYWxzZSxcclxuICAgICAgICB0b29sYmFyOiBcImN1dCBjb3B5IHBhc3RlIHwgYm9sZCBpdGFsaWMgfCBidWxsaXN0IG51bWxpc3QgYmxvY2txdW90ZSByZW1vdmVmb3JtYXQgfCBsaW5rIHwgaW1hZ2VcIixcclxuICAgICAgICBzdGF0dXNiYXI6IGZhbHNlLFxyXG4gICAgICAgIGxpbmtfdGFyZ2V0X2xpc3Q6IGZhbHNlLFxyXG4gICAgICAgIGxpbmtfdGl0bGU6IGZhbHNlLFxyXG4gICAgICAgIGltYWdlX2Rlc2NyaXB0aW9uOiBmYWxzZSxcclxuICAgICAgICBpbWFnZV9kaW1lbnNpb25zOiBmYWxzZSxcclxuICAgICAgICBpbWFnZV90aXRsZTogZmFsc2UsXHJcbiAgICAgICAgaW1hZ2VfdXBsb2FkdGFiOiBmYWxzZSxcclxuICAgICAgICBpbWFnZXNfZmlsZV90eXBlczogJ2pwZWcsanBnLHBuZyxnaWYnLFxyXG4gICAgICAgIGF1dG9tYXRpY191cGxvYWRzOiBmYWxzZSxcclxuICAgICAgICBicm93c2VyX3NwZWxsY2hlY2sgOiB0cnVlLFxyXG4gICAgICAgIG9iamVjdF9yZXNpemluZzogZmFsc2UsXHJcbiAgICAgICAgcmVsYXRpdmVfdXJsczogZmFsc2UsXHJcbiAgICAgICAgcmVtb3ZlX3NjcmlwdF9ob3N0OiBmYWxzZSxcclxuICAgICAgICBjb250ZXh0bWVudTogXCJcIixcclxuICAgICAgICBwYXN0ZV9hc190ZXh0OiB0cnVlLFxyXG4gICAgICAgIHBhc3RlX2RhdGFfaW1hZ2VzOiBmYWxzZSxcclxuICAgICAgICBzZXR1cDogbnVsbCBhcyBGdW5jdGlvblxyXG4gICAgfTtcclxuXHJcbiAgICBzdGF0aWMgaWQ6IHN0cmluZyA9IFwiRnVsbFRleHRcIjtcclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDx0ZXh0YXJlYSBpZD1cImVkaXRvclwiPjwvdGV4dGFyZWE+XHJcbiAgICBgO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLWZ1bGx0ZXh0JywgRnVsbFRleHQpO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBIb21lVXBkYXRlciBleHRlbmRzIEhUTUxFbGVtZW50IHtcclxuICAgICAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICAgICAgc3VwZXIoKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgICAgICBsZXQgY29ubmVjdGlvbiA9IG5ldyBzaWduYWxSLkh1YkNvbm5lY3Rpb25CdWlsZGVyKCkud2l0aFVybChcIi9Gb3J1bXNIdWJcIikud2l0aEF1dG9tYXRpY1JlY29ubmVjdCgpLmJ1aWxkKCk7XHJcbiAgICAgICAgICAgIGxldCBzZWxmID0gdGhpcztcclxuICAgICAgICAgICAgY29ubmVjdGlvbi5vbihcIm5vdGlmeUZvcnVtVXBkYXRlXCIsIGZ1bmN0aW9uIChkYXRhOiBhbnkpIHtcclxuICAgICAgICAgICAgICAgIHNlbGYudXBkYXRlRm9ydW1TdGF0cyhkYXRhKTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIGNvbm5lY3Rpb24uc3RhcnQoKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4oZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBjb25uZWN0aW9uLmludm9rZShcImxpc3RlblRvQWxsXCIpO1xyXG4gICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICB1cGRhdGVGb3J1bVN0YXRzKGRhdGE6IGFueSkge1xyXG4gICAgICAgICAgICBsZXQgcm93ID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIltkYXRhLWZvcnVtaWQ9J1wiICsgZGF0YS5mb3J1bUlEICsgXCInXVwiKTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIudG9waWNDb3VudFwiKS5pbm5lckhUTUwgPSBkYXRhLnRvcGljQ291bnQ7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLnBvc3RDb3VudFwiKS5pbm5lckhUTUwgPSBkYXRhLnBvc3RDb3VudDtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIubGFzdFBvc3RUaW1lXCIpLmlubmVySFRNTCA9IGRhdGEubGFzdFBvc3RUaW1lO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5sYXN0UG9zdE5hbWVcIikuaW5uZXJIVE1MID0gZGF0YS5sYXN0UG9zdE5hbWU7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLmZUaW1lXCIpLnNldEF0dHJpYnV0ZShcImRhdGEtdXRjXCIsIGRhdGEudXRjKTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIubmV3SW5kaWNhdG9yIC5pY29uLWZpbGUtdGV4dDJcIikuY2xhc3NMaXN0LnJlbW92ZShcInRleHQtbXV0ZWRcIik7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLm5ld0luZGljYXRvciAuaWNvbi1maWxlLXRleHQyXCIpLmNsYXNzTGlzdC5hZGQoXCJ0ZXh0LXdhcm5pbmdcIik7XHJcbiAgICAgICAgfTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1ob21ldXBkYXRlcicsIEhvbWVVcGRhdGVyKTtcclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBMb2dpbkZvcm0gZXh0ZW5kcyBIVE1MRWxlbWVudCB7XHJcbiAgICAgICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgICAgIHN1cGVyKCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBnZXQgdGVtcGxhdGVpZCgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidGVtcGxhdGVpZFwiKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZ2V0IGlzRXh0ZXJuYWxMb2dpbigpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiaXNleHRlcm5hbGxvZ2luXCIpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHJpdmF0ZSBidXR0b246IEhUTUxJbnB1dEVsZW1lbnQ7XHJcbiAgICAgICAgcHJpdmF0ZSBlbWFpbDogSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICBwcml2YXRlIHBhc3N3b3JkOiBIVE1MSW5wdXRFbGVtZW50O1xyXG5cclxuICAgICAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICAgICAgbGV0IHRlbXBsYXRlID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQodGhpcy50ZW1wbGF0ZWlkKSBhcyBIVE1MVGVtcGxhdGVFbGVtZW50O1xyXG4gICAgICAgICAgICBpZiAoIXRlbXBsYXRlKSB7XHJcbiAgICAgICAgICAgICAgICBjb25zb2xlLmVycm9yKGBDYW4ndCBmaW5kIHRlbXBsYXRlSUQgJHt0aGlzLnRlbXBsYXRlaWR9IHRvIG1ha2UgbG9naW4gZm9ybS5gKTtcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB0aGlzLmFwcGVuZCh0ZW1wbGF0ZS5jb250ZW50LmNsb25lTm9kZSh0cnVlKSk7XHJcbiAgICAgICAgICAgIHRoaXMuZW1haWwgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjRW1haWxMb2dpblwiKTtcclxuICAgICAgICAgICAgdGhpcy5wYXNzd29yZCA9IHRoaXMucXVlcnlTZWxlY3RvcihcIiNQYXNzd29yZExvZ2luXCIpO1xyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcIiNMb2dpbkJ1dHRvblwiKTtcclxuICAgICAgICAgICAgdGhpcy5idXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IHtcclxuICAgICAgICAgICAgICAgIHRoaXMuZXhlY3V0ZUxvZ2luKCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG5cdFx0XHR0aGlzLnF1ZXJ5U2VsZWN0b3JBbGwoXCIjRW1haWxMb2dpbiwjUGFzc3dvcmRMb2dpblwiKS5mb3JFYWNoKHggPT5cclxuXHRcdFx0XHR4LmFkZEV2ZW50TGlzdGVuZXIoXCJrZXlkb3duXCIsIChlOiBLZXlib2FyZEV2ZW50KSA9PiB7XHJcblx0XHRcdFx0XHRpZiAoZS5jb2RlID09PSBcIkVudGVyXCIpIHRoaXMuZXhlY3V0ZUxvZ2luKCk7XHJcblx0XHRcdFx0fSlcclxuICAgICAgICAgICAgKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGV4ZWN1dGVMb2dpbigpIHtcclxuICAgICAgICAgICAgbGV0IHBhdGggPSBcIi9JZGVudGl0eS9Mb2dpblwiO1xyXG4gICAgICAgICAgICBpZiAodGhpcy5pc0V4dGVybmFsTG9naW4udG9Mb3dlckNhc2UoKSA9PT0gXCJ0cnVlXCIpXHJcbiAgICAgICAgICAgICAgICBwYXRoID0gXCIvSWRlbnRpdHkvTG9naW5BbmRBc3NvY2lhdGVcIjtcclxuICAgICAgICAgICAgbGV0IHBheWxvYWQgPSBKU09OLnN0cmluZ2lmeSh7IGVtYWlsOiB0aGlzLmVtYWlsLnZhbHVlLCBwYXNzd29yZDogdGhpcy5wYXNzd29yZC52YWx1ZSB9KTtcclxuICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgcGF0aCwge1xyXG4gICAgICAgICAgICAgICAgbWV0aG9kOiBcIlBPU1RcIixcclxuICAgICAgICAgICAgICAgIGhlYWRlcnM6IHtcclxuICAgICAgICAgICAgICAgICAgICAnQ29udGVudC1UeXBlJzogJ2FwcGxpY2F0aW9uL2pzb24nXHJcbiAgICAgICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICAgICAgYm9keTogcGF5bG9hZFxyXG4gICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICAgICAgLnRoZW4oZnVuY3Rpb24ocmVzcG9uc2UpIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gcmVzcG9uc2UuanNvbigpO1xyXG4gICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICAgICAgLnRoZW4oZnVuY3Rpb24gKHJlc3VsdCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHN3aXRjaCAocmVzdWx0LnJlc3VsdCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGNhc2UgdHJ1ZTpcclxuICAgICAgICAgICAgICAgICAgICAgICAgbGV0IGRlc3RpbmF0aW9uID0gKGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjUmVmZXJyZXJcIikgYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGxvY2F0aW9uLmhyZWYgPSBkZXN0aW5hdGlvbjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgICAgICAgICAgICAgZGVmYXVsdDpcclxuICAgICAgICAgICAgICAgICAgICAgICAgbGV0IGxvZ2luUmVzdWx0ID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNMb2dpblJlc3VsdFwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbG9naW5SZXN1bHQuaW5uZXJIVE1MID0gcmVzdWx0Lm1lc3NhZ2U7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGxvZ2luUmVzdWx0LmNsYXNzTGlzdC5yZW1vdmUoXCJkLW5vbmVcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICAgICAgLmNhdGNoKGZ1bmN0aW9uIChlcnJvcikge1xyXG4gICAgICAgICAgICAgICAgICAgIGxldCBsb2dpblJlc3VsdCA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjTG9naW5SZXN1bHRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgbG9naW5SZXN1bHQuaW5uZXJIVE1MID0gXCJUaGVyZSB3YXMgYW4gdW5rbm93biBlcnJvciB3aGlsZSBhdHRlbXB0aW5nIGxvZ2luXCI7XHJcbiAgICAgICAgICAgICAgICAgICAgbG9naW5SZXN1bHQuY2xhc3NMaXN0LnJlbW92ZShcImQtbm9uZVwiKTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG4gICAgXHJcbiAgICBjdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLWxvZ2luZm9ybScsIExvZ2luRm9ybSk7XHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgTW9yZVBvc3RzQmVmb3JlUmVwbHlCdXR0b24gZXh0ZW5kcyBFbGVtZW50QmFzZSB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBidXR0b25jbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbmNsYXNzXCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IGJ1dHRvbnRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b250ZXh0XCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIHRoaXMuaW5uZXJIVE1MID0gTW9yZVBvc3RzQmVmb3JlUmVwbHlCdXR0b24udGVtcGxhdGU7XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQ7XHJcbiAgICAgICAgYnV0dG9uLnZhbHVlID0gdGhpcy5idXR0b250ZXh0O1xyXG4gICAgICAgIGlmICh0aGlzLmJ1dHRvbmNsYXNzPy5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbmNsYXNzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiBidXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoZTogTW91c2VFdmVudCkgPT4ge1xyXG4gICAgICAgICAgICBQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUubG9hZE1vcmVQb3N0cygpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgICAgIHN1cGVyLmNvbm5lY3RlZENhbGxiYWNrKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgIHJldHVybiBbUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLCBcImlzTmV3ZXJQb3N0c0F2YWlsYWJsZVwiXTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgdXBkYXRlVUkoZGF0YTogYm9vbGVhbik6IHZvaWQge1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICBpZiAoIWRhdGEpXHJcbiAgICAgICAgICAgIGJ1dHRvbi5zdHlsZS52aXNpYmlsaXR5ID0gXCJoaWRkZW5cIjtcclxuICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgIGJ1dHRvbi5zdHlsZS52aXNpYmlsaXR5ID0gXCJ2aXNpYmxlXCI7XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGlucHV0IHR5cGU9XCJidXR0b25cIiAvPmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtbW9yZXBvc3RzYmVmb3JlcmVwbHlidXR0b24nLCBNb3JlUG9zdHNCZWZvcmVSZXBseUJ1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIE1vcmVQb3N0c0J1dHRvbiBleHRlbmRzIEVsZW1lbnRCYXNlIHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgYnV0dG9udGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbnRleHRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBNb3JlUG9zdHNCdXR0b24udGVtcGxhdGU7XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQ7XHJcbiAgICAgICAgYnV0dG9uLnZhbHVlID0gdGhpcy5idXR0b250ZXh0O1xyXG4gICAgICAgIGlmICh0aGlzLmJ1dHRvbmNsYXNzPy5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbmNsYXNzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiBidXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoZTogTW91c2VFdmVudCkgPT4ge1xyXG4gICAgICAgICAgICBQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUubG9hZE1vcmVQb3N0cygpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgICAgIHN1cGVyLmNvbm5lY3RlZENhbGxiYWNrKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgIHJldHVybiBbUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLCBcImhpZ2hQYWdlXCJdO1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICB1cGRhdGVVSShkYXRhOiBudW1iZXIpOiB2b2lkIHtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgaWYgKFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5wYWdlQ291bnQgPT09IDEgfHwgZGF0YSA9PT0gUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLnBhZ2VDb3VudClcclxuICAgICAgICAgICAgYnV0dG9uLnN0eWxlLnZpc2liaWxpdHkgPSBcImhpZGRlblwiO1xyXG4gICAgICAgIGVsc2VcclxuICAgICAgICAgICAgYnV0dG9uLnN0eWxlLnZpc2liaWxpdHkgPSBcInZpc2libGVcIjtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIC8+YDtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1tb3JlcG9zdHNidXR0b24nLCBNb3JlUG9zdHNCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBQTUNvdW50IGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXREZXBlbmRlbnRSZWZlcmVuY2UoKTogW1N0YXRlQmFzZSwgc3RyaW5nXSB7XHJcbiAgICAgICAgcmV0dXJuIFtQb3BGb3J1bXMudXNlclN0YXRlLCBcIm5ld1BtQ291bnRcIl07XHJcbiAgICB9XHJcblxyXG4gICAgdXBkYXRlVUkoZGF0YTogbnVtYmVyKTogdm9pZCB7XHJcbiAgICAgICAgaWYgKGRhdGEgPT09IDApXHJcbiAgICAgICAgICAgIHRoaXMuaW5uZXJIVE1MID0gXCJcIjtcclxuICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgIHRoaXMuaW5uZXJIVE1MID0gYDxzcGFuIGNsYXNzPVwiYmFkZ2VcIj4ke2RhdGF9PC9zcGFuPmA7XHJcbiAgICB9XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtcG1jb3VudCcsIFBNQ291bnQpO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBQb3N0TWluaVByb2ZpbGUgZXh0ZW5kcyBIVE1MRWxlbWVudCB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCB1c2VybmFtZSgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInVzZXJuYW1lXCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IHVzZXJuYW1lY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ1c2VybmFtZWNsYXNzXCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IHVzZXJpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInVzZXJpZFwiKTtcclxuICAgIH1cclxuICAgIGdldCBtaW5pUHJvZmlsZUJveENsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwibWluaXByb2ZpbGVib3hjbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIGlzT3BlbjogYm9vbGVhbjtcclxuICAgIHByaXZhdGUgYm94OiBIVE1MRWxlbWVudDtcclxuICAgIHByaXZhdGUgYm94SGVpZ2h0OiBzdHJpbmc7XHJcbiAgICBwcml2YXRlIGlzTG9hZGVkOiBib29sZWFuO1xyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIHRoaXMuaXNMb2FkZWQgPSBmYWxzZTtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFBvc3RNaW5pUHJvZmlsZS50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgbmFtZUhlYWRlciA9IHRoaXMucXVlcnlTZWxlY3RvcihcImgzXCIpIGFzIEhUTUxFbGVtZW50O1xyXG4gICAgICAgIHRoaXMudXNlcm5hbWVjbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gbmFtZUhlYWRlci5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICBuYW1lSGVhZGVyLmlubmVySFRNTCA9IHRoaXMudXNlcm5hbWU7XHJcbiAgICAgICAgbmFtZUhlYWRlci5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICB0aGlzLnRvZ2dsZSgpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgICAgIHRoaXMuYm94ID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiZGl2XCIpO1xyXG4gICAgICAgIHRoaXMubWluaVByb2ZpbGVCb3hDbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gdGhpcy5ib3guY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICB9XHJcblxyXG4gICAgcHJpdmF0ZSB0b2dnbGUoKSB7XHJcbiAgICAgICAgaWYgKCF0aGlzLmlzTG9hZGVkKSB7XHJcbiAgICAgICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0FjY291bnQvTWluaVByb2ZpbGUvXCIgKyB0aGlzLnVzZXJpZClcclxuICAgICAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLnRleHQoKVxyXG4gICAgICAgICAgICAgICAgICAgIC50aGVuKHRleHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBsZXQgc3ViID0gdGhpcy5ib3gucXVlcnlTZWxlY3RvcihcImRpdlwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgc3ViLmlubmVySFRNTCA9IHRleHQ7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGNvbnN0IGhlaWdodCA9IHN1Yi5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKS5oZWlnaHQ7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuYm94SGVpZ2h0ID0gYCR7aGVpZ2h0fXB4YDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5ib3guc3R5bGUuaGVpZ2h0ID0gdGhpcy5ib3hIZWlnaHQ7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuaXNPcGVuID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5pc0xvYWRlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgfSkpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBlbHNlIGlmICghdGhpcy5pc09wZW4pIHtcclxuICAgICAgICAgICAgdGhpcy5ib3guc3R5bGUuaGVpZ2h0ID0gdGhpcy5ib3hIZWlnaHQ7XHJcbiAgICAgICAgICAgIHRoaXMuaXNPcGVuID0gdHJ1ZTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZWxzZSB7XHJcbiAgICAgICAgICAgIHRoaXMuYm94LnN0eWxlLmhlaWdodCA9IFwiMFwiO1xyXG4gICAgICAgICAgICB0aGlzLmlzT3BlbiA9IGZhbHNlO1xyXG4gICAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aDM+PC9oMz5cclxuPGRpdj5cclxuICAgIDxkaXYgY2xhc3M9XCJweS0xIHB4LTMgbWItMlwiPjwvZGl2PlxyXG48L2Rpdj5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXBvc3RtaW5pcHJvZmlsZScsIFBvc3RNaW5pUHJvZmlsZSk7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFBvc3RNb2RlcmF0aW9uTG9nQnV0dG9uIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9udGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbnRleHRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IHBvc3RpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInBvc3RpZFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgcGFyZW50U2VsZWN0b3JUb0FwcGVuZFRvKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwicGFyZW50c2VsZWN0b3J0b2FwcGVuZHRvXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIHRoaXMuaW5uZXJIVE1MID0gVG9waWNNb2RlcmF0aW9uTG9nQnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmJ1dHRvbnRleHQ7XHJcbiAgICAgICAgbGV0IGNsYXNzZXMgPSB0aGlzLmJ1dHRvbmNsYXNzO1xyXG4gICAgICAgIGlmIChjbGFzc2VzPy5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICBjbGFzc2VzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiBidXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgIGxldCBjb250YWluZXI6IEhUTUxEaXZFbGVtZW50O1xyXG4gICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICBpZiAoIWNvbnRhaW5lcikge1xyXG4gICAgICAgICAgICAgICAgbGV0IHBhcmVudENvbnRhaW5lciA9IHNlbGYuY2xvc2VzdCh0aGlzLnBhcmVudFNlbGVjdG9yVG9BcHBlbmRUbyk7XHJcbiAgICAgICAgICAgICAgICBpZiAoIXBhcmVudENvbnRhaW5lcikge1xyXG4gICAgICAgICAgICAgICAgICAgIGNvbnNvbGUuZXJyb3IoYENhbid0IGZpbmQgYSBwYXJlbnQgc2VsZWN0b3IgXCIke3RoaXMucGFyZW50U2VsZWN0b3JUb0FwcGVuZFRvfVwiIHRvIGFwcGVuZCBwb3N0IG1vZGVyYXRpb24gbG9nIHRvLmApO1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGNvbnRhaW5lciA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJkaXZcIik7XHJcbiAgICAgICAgICAgICAgICBwYXJlbnRDb250YWluZXIuYXBwZW5kQ2hpbGQoY29udGFpbmVyKTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBpZiAoY29udGFpbmVyLnN0eWxlLmRpc3BsYXkgIT09IFwiYmxvY2tcIilcclxuICAgICAgICAgICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL01vZGVyYXRvci9Qb3N0TW9kZXJhdGlvbkxvZy9cIiArIHRoaXMucG9zdGlkKVxyXG4gICAgICAgICAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLnRleHQoKVxyXG4gICAgICAgICAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNvbnRhaW5lci5pbm5lckhUTUwgPSB0ZXh0O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgY29udGFpbmVyLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH0pKTtcclxuICAgICAgICAgICAgZWxzZSBjb250YWluZXIuc3R5bGUuZGlzcGxheSA9IFwibm9uZVwiO1xyXG4gICAgICAgIH0pO1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgLz5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoXCJwZi1wb3N0bW9kZXJhdGlvbmxvZ2J1dHRvblwiLCBQb3N0TW9kZXJhdGlvbkxvZ0J1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFByZXZpZXdCdXR0b24gZXh0ZW5kcyBIVE1MRWxlbWVudCB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBsYWJlbFRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJsYWJlbHRleHRcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgdGV4dFNvdXJjZVNlbGVjdG9yKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidGV4dHNvdXJjZXNlbGVjdG9yXCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IGlzUGxhaW5UZXh0U2VsZWN0b3IoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJpc3BsYWludGV4dHNlbGVjdG9yXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIHRoaXMuaW5uZXJIVE1MID0gUHJldmlld0J1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIikgYXMgSFRNTEJ1dHRvbkVsZW1lbnQ7XHJcbiAgICAgICAgYnV0dG9uLnZhbHVlID0gdGhpcy5sYWJlbFRleHQ7XHJcbiAgICAgICAgbGV0IGhlYWRUZXh0ID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaDRcIikgYXMgSFRNTEhlYWRFbGVtZW50O1xyXG4gICAgICAgIGhlYWRUZXh0LmlubmVyVGV4dCA9IHRoaXMubGFiZWxUZXh0O1xyXG4gICAgICAgIHZhciBtb2RhbCA9IHRoaXMucXVlcnlTZWxlY3RvcihcIi5tb2RhbFwiKTtcclxuICAgICAgICBtb2RhbC5hZGRFdmVudExpc3RlbmVyKFwic2hvd24uYnMubW9kYWxcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICB0aGlzLm9wZW5Nb2RhbCgpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgfVxyXG5cclxuICAgIG9wZW5Nb2RhbCgpIHtcclxuICAgICAgICB0aW55bWNlLnRyaWdnZXJTYXZlKCk7XHJcbiAgICAgICAgbGV0IGZ1bGxUZXh0ID0gZG9jdW1lbnQucXVlcnlTZWxlY3Rvcih0aGlzLnRleHRTb3VyY2VTZWxlY3RvcikgYXMgYW55O1xyXG4gICAgICAgIGxldCBtb2RlbCA9IHtcclxuICAgICAgICAgICAgRnVsbFRleHQ6IGZ1bGxUZXh0LnZhbHVlLFxyXG4gICAgICAgICAgICBJc1BsYWluVGV4dDogKGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IodGhpcy5pc1BsYWluVGV4dFNlbGVjdG9yKSBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZS50b0xvd2VyQ2FzZSgpID09PSBcInRydWVcIlxyXG4gICAgICAgIH07XHJcbiAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vUHJldmlld1RleHRcIiwge1xyXG4gICAgICAgICAgICBtZXRob2Q6IFwiUE9TVFwiLFxyXG4gICAgICAgICAgICBib2R5OiBKU09OLnN0cmluZ2lmeShtb2RlbCksXHJcbiAgICAgICAgICAgIGhlYWRlcnM6IHtcclxuICAgICAgICAgICAgICAgIFwiQ29udGVudC1UeXBlXCI6IFwiYXBwbGljYXRpb24vanNvblwiXHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KVxyXG4gICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS50ZXh0KClcclxuICAgICAgICAgICAgICAgIC50aGVuKHRleHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIGxldCByID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiLnBhcnNlZEZ1bGxUZXh0XCIpIGFzIEhUTUxEaXZFbGVtZW50O1xyXG4gICAgICAgICAgICAgICAgICAgIHIuaW5uZXJIVE1MID0gdGV4dDtcclxuICAgICAgICAgICAgICAgIH0pKTtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIGNsYXNzPVwiYnRuIGJ0bi1wcmltYXJ5XCIgZGF0YS1icy10b2dnbGU9XCJtb2RhbFwiIGRhdGEtYnMtdGFyZ2V0PVwiI1ByZXZpZXdNb2RhbFwiPlxyXG48ZGl2IGNsYXNzPVwibW9kYWwgZmFkZVwiIGlkPVwiUHJldmlld01vZGFsXCIgdGFiaW5kZXg9XCItMVwiIHJvbGU9XCJkaWFsb2dcIj5cclxuXHQ8ZGl2IGNsYXNzPVwibW9kYWwtZGlhbG9nXCI+XHJcblx0XHQ8ZGl2IGNsYXNzPVwibW9kYWwtY29udGVudFwiPlxyXG5cdFx0XHQ8ZGl2IGNsYXNzPVwibW9kYWwtaGVhZGVyXCI+XHJcblx0XHRcdFx0PGg0IGNsYXNzPVwibW9kYWwtdGl0bGVcIj48L2g0PlxyXG5cdFx0XHRcdDxidXR0b24gdHlwZT1cImJ1dHRvblwiIGNsYXNzPVwiYnRuLWNsb3NlXCIgZGF0YS1icy1kaXNtaXNzPVwibW9kYWxcIiBhcmlhLWxhYmVsPVwiQ2xvc2VcIj48L2J1dHRvbj5cclxuXHRcdFx0PC9kaXY+XHJcblx0XHRcdDxkaXYgY2xhc3M9XCJtb2RhbC1ib2R5XCI+XHJcblx0XHRcdFx0PGRpdiBjbGFzcz1cInBvc3RJdGVtIHBhcnNlZEZ1bGxUZXh0XCI+PC9kaXY+XHJcblx0XHRcdDwvZGl2PlxyXG5cdFx0XHQ8ZGl2IGNsYXNzPVwibW9kYWwtZm9vdGVyXCI+XHJcblx0XHRcdFx0PGJ1dHRvbiB0eXBlPVwiYnV0dG9uXCIgY2xhc3M9XCJidG4gYnRuLXByaW1hcnlcIiBkYXRhLWJzLWRpc21pc3M9XCJtb2RhbFwiPkNsb3NlPC9idXR0b24+XHJcblx0XHRcdDwvZGl2PlxyXG5cdFx0PC9kaXY+XHJcblx0PC9kaXY+XHJcbjwvZGl2PmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtcHJldmlld2J1dHRvbicsIFByZXZpZXdCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBQcmV2aW91c1Bvc3RzQnV0dG9uIGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuICAgIGdldCBidXR0b250ZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9udGV4dFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFByZXZpb3VzUG9zdHNCdXR0b24udGVtcGxhdGU7XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQ7XHJcbiAgICAgICAgYnV0dG9uLnZhbHVlID0gdGhpcy5idXR0b250ZXh0O1xyXG4gICAgICAgIGlmICh0aGlzLmJ1dHRvbmNsYXNzPy5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbmNsYXNzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiBidXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoZTogTW91c2VFdmVudCkgPT4ge1xyXG4gICAgICAgICAgICBQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUubG9hZFByZXZpb3VzUG9zdHMoKTtcclxuICAgICAgICB9KTtcclxuICAgICAgICBzdXBlci5jb25uZWN0ZWRDYWxsYmFjaygpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZSwgXCJsb3dQYWdlXCJdO1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICB1cGRhdGVVSShkYXRhOiBudW1iZXIpOiB2b2lkIHtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgaWYgKFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5wYWdlQ291bnQgPT09IDEgfHwgZGF0YSA9PT0gMSlcclxuICAgICAgICAgICAgYnV0dG9uLnN0eWxlLnZpc2liaWxpdHkgPSBcImhpZGRlblwiO1xyXG4gICAgICAgIGVsc2VcclxuICAgICAgICAgICAgYnV0dG9uLnN0eWxlLnZpc2liaWxpdHkgPSBcInZpc2libGVcIjtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIC8+YDtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1wcmV2aW91c3Bvc3RzYnV0dG9uJywgUHJldmlvdXNQb3N0c0J1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFF1b3RlQnV0dG9uIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgbmFtZSgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcIm5hbWVcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgY29udGFpbmVyaWQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJjb250YWluZXJpZFwiKTtcclxuICAgIH1cclxuICAgIGdldCBidXR0b25jbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbmNsYXNzXCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IGJ1dHRvbnRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b250ZXh0XCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IHRpcCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInRpcFwiKTtcclxuICAgIH1cclxuICAgIGdldCBwb3N0SUQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJwb3N0aWRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgcHJpdmF0ZSBfdGlwOiBhbnk7XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgbGV0IHRhcmdldFRleHQgPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCh0aGlzLmNvbnRhaW5lcmlkKTtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFF1b3RlQnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICBidXR0b24udGl0bGUgPSB0aGlzLnRpcDtcclxuICAgICAgICBbXCJtb3VzZWRvd25cIixcInRvdWNoc3RhcnRcIl0uZm9yRWFjaCgoZTpzdHJpbmcpID0+IFxyXG4gICAgICAgICAgICB0YXJnZXRUZXh0LmFkZEV2ZW50TGlzdGVuZXIoZSwgKCkgPT4gXHJcbiAgICAgICAgICAgICAgICB7IGlmICh0aGlzLl90aXApIHRoaXMuX3RpcC5oaWRlKCkgfSkpO1xyXG4gICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuYnV0dG9udGV4dDtcclxuICAgICAgICBsZXQgY2xhc3NlcyA9IHRoaXMuYnV0dG9uY2xhc3M7XHJcbiAgICAgICAgaWYgKGNsYXNzZXM/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgIGNsYXNzZXMuc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IGJ1dHRvbi5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICB0aGlzLm9uY2xpY2sgPSAoZTogTW91c2VFdmVudCkgPT4ge1xyXG4gICAgICAgICAgICBsZXQgc2VsZWN0aW9uID0gZG9jdW1lbnQuZ2V0U2VsZWN0aW9uKCk7XHJcbiAgICAgICAgICAgIGlmIChzZWxlY3Rpb24ucmFuZ2VDb3VudCA9PT0gMCB8fCBzZWxlY3Rpb24uZ2V0UmFuZ2VBdCgwKS50b1N0cmluZygpLmxlbmd0aCA9PT0gMCkge1xyXG4gICAgICAgICAgICAgICAgLy8gcHJvbXB0IHRvIHNlbGVjdFxyXG4gICAgICAgICAgICAgICAgdGhpcy5fdGlwID0gbmV3IGJvb3RzdHJhcC5Ub29sdGlwKGJ1dHRvbiwge3RyaWdnZXI6IFwibWFudWFsXCJ9KTtcclxuICAgICAgICAgICAgICAgIHRoaXMuX3RpcC5zaG93KCk7XHJcbiAgICAgICAgICAgICAgICBzZWxlY3Rpb24ucmVtb3ZlQWxsUmFuZ2VzKCk7XHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgbGV0IHJhbmdlID0gc2VsZWN0aW9uLmdldFJhbmdlQXQoMCk7XHJcbiAgICAgICAgICAgIGxldCBmcmFnbWVudCA9IHJhbmdlLmNsb25lQ29udGVudHMoKTtcclxuICAgICAgICAgICAgbGV0IGRpdiA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJkaXZcIik7XHJcbiAgICAgICAgICAgIGRpdi5hcHBlbmRDaGlsZChmcmFnbWVudCk7XHJcbiAgICAgICAgICAgIC8vIGlzIHNlbGVjdGlvbiBpbiB0aGUgY29udGFpbmVyP1xyXG4gICAgICAgICAgICBsZXQgYW5jZXN0b3IgPSByYW5nZS5jb21tb25BbmNlc3RvckNvbnRhaW5lcjtcclxuICAgICAgICAgICAgd2hpbGUgKGFuY2VzdG9yWydpZCddICE9PSB0aGlzLmNvbnRhaW5lcmlkICYmIGFuY2VzdG9yLnBhcmVudEVsZW1lbnQgIT09IG51bGwpIHtcclxuICAgICAgICAgICAgICAgIGFuY2VzdG9yID0gYW5jZXN0b3IucGFyZW50RWxlbWVudDtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBsZXQgaXNJblRleHQgPSBhbmNlc3RvclsnaWQnXSA9PT0gdGhpcy5jb250YWluZXJpZDtcclxuICAgICAgICAgICAgLy8gaWYgbm90LCBpcyBpdCBwYXJ0aWFsbHkgaW4gdGhlIGNvbnRhaW5lcj9cclxuICAgICAgICAgICAgaWYgKCFpc0luVGV4dCkge1xyXG4gICAgICAgICAgICAgICAgbGV0IGNvbnRhaW5lciA9IGRpdi5xdWVyeVNlbGVjdG9yKFwiI1wiICsgdGhpcy5jb250YWluZXJpZCk7XHJcbiAgICAgICAgICAgICAgICBpZiAoY29udGFpbmVyICE9PSBudWxsICYmIGNvbnRhaW5lciAhPT0gdW5kZWZpbmVkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgLy8gaXQncyBwYXJ0aWFsbHkgaW4gdGhlIGNvbnRhaW5lciwgc28ganVzdCBnZXQgdGhhdCBwYXJ0XHJcbiAgICAgICAgICAgICAgICAgICAgZGl2LmlubmVySFRNTCA9IGNvbnRhaW5lci5pbm5lckhUTUw7XHJcbiAgICAgICAgICAgICAgICAgICAgaXNJblRleHQgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHNlbGVjdGlvbi5yZW1vdmVBbGxSYW5nZXMoKTtcclxuICAgICAgICAgICAgaWYgKGlzSW5UZXh0KSB7XHJcbiAgICAgICAgICAgICAgICAvLyBhY3RpdmF0ZSBvciBhZGQgdG8gcXVvdGVcclxuICAgICAgICAgICAgICAgIGxldCByZXN1bHQ6IHN0cmluZztcclxuICAgICAgICAgICAgICAgIGlmICh1c2VyU3RhdGUuaXNQbGFpblRleHQpXHJcbiAgICAgICAgICAgICAgICAgICAgcmVzdWx0ID0gYFtxdW90ZV1baV0ke3RoaXMubmFtZX06Wy9pXVxcclxcbiAke2Rpdi5pbm5lclRleHR9Wy9xdW90ZV1gO1xyXG4gICAgICAgICAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICAgICAgICAgIHJlc3VsdCA9IGA8YmxvY2txdW90ZT48cD48aT4ke3RoaXMubmFtZX06PC9pPjwvcD4ke2Rpdi5pbm5lckhUTUx9PC9ibG9ja3F1b3RlPjxwPjwvcD5gO1xyXG4gICAgICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLm5leHRRdW90ZSA9IHJlc3VsdDtcclxuICAgICAgICAgICAgICAgIGlmICghUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLmlzUmVwbHlMb2FkZWQpXHJcbiAgICAgICAgICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLmxvYWRSZXBseShQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUudG9waWNJRCwgTnVtYmVyKHRoaXMucG9zdElEKSwgdHJ1ZSk7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9O1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgZGF0YS1icy10b2dnbGU9XCJ0b29sdGlwXCIgdGl0bGU9XCJcIiAvPmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtcXVvdGVidXR0b24nLCBRdW90ZUJ1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFJlcGx5QnV0dG9uIGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9udGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbnRleHRcIik7XHJcbiAgICB9XHJcbiAgICBcclxuICAgIGdldCB0b3BpY2lkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidG9waWNpZFwiKTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgZ2V0IHBvc3RpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInBvc3RpZFwiKTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgZ2V0IG92ZXJyaWRlZGlzcGxheSgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcIm92ZXJyaWRlZGlzcGxheVwiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFJlcGx5QnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuYnV0dG9udGV4dDtcclxuICAgICAgICBpZiAodGhpcy5idXR0b25jbGFzcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgdGhpcy5idXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKGU6IE1vdXNlRXZlbnQpID0+IHtcclxuICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLmxvYWRSZXBseShOdW1iZXIodGhpcy50b3BpY2lkKSwgTnVtYmVyKHRoaXMucG9zdGlkKSwgdHJ1ZSk7XHJcbiAgICAgICAgfSk7XHJcbiAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXREZXBlbmRlbnRSZWZlcmVuY2UoKTogW1N0YXRlQmFzZSwgc3RyaW5nXSB7XHJcbiAgICAgICAgcmV0dXJuIFtQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUsIFwiaXNSZXBseUxvYWRlZFwiXTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgdXBkYXRlVUkoZGF0YTogYm9vbGVhbik6IHZvaWQge1xyXG4gICAgICAgIGlmICh0aGlzLm92ZXJyaWRlZGlzcGxheT8udG9Mb3dlckNhc2UoKSA9PT0gXCJ0cnVlXCIpXHJcbiAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgaWYgKGRhdGEpXHJcbiAgICAgICAgICAgIGJ1dHRvbi5zdHlsZS5kaXNwbGF5ID0gXCJub25lXCI7XHJcbiAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICBidXR0b24uc3R5bGUuZGlzcGxheSA9IFwiaW5pdGlhbFwiO1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgLz5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXJlcGx5YnV0dG9uJywgUmVwbHlCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBSZXBseUZvcm0gZXh0ZW5kcyBIVE1MRWxlbWVudCB7XHJcbiAgICAgICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgICAgIHN1cGVyKCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBnZXQgdGVtcGxhdGVJRCgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidGVtcGxhdGVpZFwiKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgYnV0dG9uOiBIVE1MSW5wdXRFbGVtZW50O1xyXG5cclxuICAgICAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICAgICAgbGV0IHRlbXBsYXRlID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQodGhpcy50ZW1wbGF0ZUlEKSBhcyBIVE1MVGVtcGxhdGVFbGVtZW50O1xyXG4gICAgICAgICAgICBpZiAoIXRlbXBsYXRlKSB7XHJcbiAgICAgICAgICAgICAgICBjb25zb2xlLmVycm9yKGBDYW4ndCBmaW5kIHRlbXBsYXRlSUQgJHt0aGlzLnRlbXBsYXRlSUR9IHRvIG1ha2UgcmVwbHkgZm9ybS5gKTtcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB0aGlzLmFwcGVuZCh0ZW1wbGF0ZS5jb250ZW50LmNsb25lTm9kZSh0cnVlKSk7XHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI1N1Ym1pdFJlcGx5XCIpO1xyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5zdWJtaXRSZXBseSgpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHN1Ym1pdFJlcGx5KCkge1xyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbi5zZXRBdHRyaWJ1dGUoXCJkaXNhYmxlZFwiLCBcImRpc2FibGVkXCIpO1xyXG4gICAgICAgICAgICB2YXIgY2xvc2VDaGVjayA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjQ2xvc2VPblJlcGx5XCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQ7XHJcbiAgICAgICAgICAgIHZhciBjbG9zZU9uUmVwbHkgPSBmYWxzZTtcclxuICAgICAgICAgICAgaWYgKGNsb3NlQ2hlY2sgJiYgY2xvc2VDaGVjay5jaGVja2VkKSBjbG9zZU9uUmVwbHkgPSB0cnVlO1xyXG4gICAgICAgICAgICB2YXIgbW9kZWwgPSB7XHJcbiAgICAgICAgICAgICAgICBUaXRsZTogKHRoaXMucXVlcnlTZWxlY3RvcihcIiNOZXdSZXBseSAjVGl0bGVcIikgYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWUsXHJcbiAgICAgICAgICAgICAgICBGdWxsVGV4dDogKHRoaXMucXVlcnlTZWxlY3RvcihcIiNOZXdSZXBseSAjRnVsbFRleHRcIikgYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWUsXHJcbiAgICAgICAgICAgICAgICBJbmNsdWRlU2lnbmF0dXJlOiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1JlcGx5ICNJbmNsdWRlU2lnbmF0dXJlXCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQpLmNoZWNrZWQsXHJcbiAgICAgICAgICAgICAgICBJdGVtSUQ6ICh0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTmV3UmVwbHkgI0l0ZW1JRFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZSxcclxuICAgICAgICAgICAgICAgIENsb3NlT25SZXBseTogY2xvc2VPblJlcGx5LFxyXG4gICAgICAgICAgICAgICAgSXNQbGFpblRleHQ6ICh0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTmV3UmVwbHkgI0lzUGxhaW5UZXh0XCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlLnRvTG93ZXJDYXNlKCkgPT09IFwidHJ1ZVwiLFxyXG4gICAgICAgICAgICAgICAgUGFyZW50UG9zdElEOiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1JlcGx5ICNQYXJlbnRQb3N0SURcIikgYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWVcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vUG9zdFJlcGx5XCIsIHtcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCIsXHJcbiAgICAgICAgICAgICAgICBib2R5OiBKU09OLnN0cmluZ2lmeShtb2RlbCksXHJcbiAgICAgICAgICAgICAgICBoZWFkZXJzOiB7XHJcbiAgICAgICAgICAgICAgICAgICAgXCJDb250ZW50LVR5cGVcIjogXCJhcHBsaWNhdGlvbi9qc29uXCJcclxuICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS5qc29uKCkpXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXN1bHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIHN3aXRjaCAocmVzdWx0LnJlc3VsdCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjYXNlIHRydWU6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB3aW5kb3cubG9jYXRpb24gPSByZXN1bHQucmVkaXJlY3Q7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgZGVmYXVsdDpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciByID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI1Bvc3RSZXNwb25zZU1lc3NhZ2VcIikgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByLmlubmVySFRNTCA9IHJlc3VsdC5tZXNzYWdlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24ucmVtb3ZlQXR0cmlidXRlKFwiZGlzYWJsZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC5jYXRjaChlcnJvciA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHIgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjUG9zdFJlc3BvbnNlTWVzc2FnZVwiKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICByLmlubmVySFRNTCA9IFwiVGhlcmUgd2FzIGFuIHVua25vd24gZXJyb3Igd2hpbGUgdHJ5aW5nIHRvIHBvc3RcIjtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmJ1dHRvbi5yZW1vdmVBdHRyaWJ1dGUoXCJkaXNhYmxlZFwiKTtcclxuICAgICAgICAgICAgICAgICAgICByLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9O1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICBjdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXJlcGx5Zm9ybScsIFJlcGx5Rm9ybSk7XHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgU3Vic2NyaWJlQnV0dG9uIGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgZ2V0IHN1YnNjcmliZXRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJzdWJzY3JpYmV0ZXh0XCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IHVuc3Vic2NyaWJldGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInVuc3Vic2NyaWJldGV4dFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFN1YnNjcmliZUJ1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgYnV0dG9uOiBIVE1MSW5wdXRFbGVtZW50ID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgdGhpcy5idXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9TdWJzY3JpcHRpb24vVG9nZ2xlU3Vic2NyaXB0aW9uL1wiICsgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLnRvcGljSUQsIHtcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCJcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLmpzb24oKSlcclxuICAgICAgICAgICAgICAgIC50aGVuKHJlc3VsdCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgc3dpdGNoIChyZXN1bHQuZGF0YS5pc1N1YnNjcmliZWQpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSB0cnVlOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLmlzU3Vic2NyaWJlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSBmYWxzZTpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5pc1N1YnNjcmliZWQgPSBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkZWZhdWx0OlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gVE9ETzogc29tZXRoaW5nIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICAgICAgLmNhdGNoKCgpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAvLyBUT0RPOiBoYW5kbGUgZXJyb3JcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0pO1xyXG4gICAgICAgIHN1cGVyLmNvbm5lY3RlZENhbGxiYWNrKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgIHJldHVybiBbUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLCBcImlzU3Vic2NyaWJlZFwiXTtcclxuICAgIH1cclxuXHJcbiAgICB1cGRhdGVVSShkYXRhOiBib29sZWFuKTogdm9pZCB7XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpO1xyXG4gICAgICAgIGlmIChkYXRhKVxyXG4gICAgICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLnVuc3Vic2NyaWJldGV4dDtcclxuICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuc3Vic2NyaWJldGV4dDtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIC8+YDtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1zdWJzY3JpYmVidXR0b24nLCBTdWJzY3JpYmVCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBUb3BpY0J1dHRvbiBleHRlbmRzIEVsZW1lbnRCYXNlIHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbnRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b250ZXh0XCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBmb3J1bWlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiZm9ydW1pZFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFRvcGljQnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuYnV0dG9udGV4dDtcclxuICAgICAgICBpZiAodGhpcy5idXR0b25jbGFzcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgdGhpcy5idXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICBjdXJyZW50Rm9ydW1TdGF0ZS5sb2FkTmV3VG9waWMoKTtcclxuICAgICAgICB9KTtcclxuICAgICAgICBzdXBlci5jb25uZWN0ZWRDYWxsYmFjaygpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy5jdXJyZW50Rm9ydW1TdGF0ZSwgXCJpc05ld1RvcGljTG9hZGVkXCJdO1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICB1cGRhdGVVSShkYXRhOiBib29sZWFuKTogdm9pZCB7XHJcbiAgICAgICAgaWYgKGRhdGEpXHJcbiAgICAgICAgICAgIHRoaXMuc3R5bGUuZGlzcGxheSA9IFwibm9uZVwiO1xyXG4gICAgICAgIGVsc2VcclxuICAgICAgICAgICAgdGhpcy5zdHlsZS5kaXNwbGF5ID0gXCJpbml0aWFsXCI7XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGlucHV0IHR5cGU9XCJidXR0b25cIiAvPmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtdG9waWNidXR0b24nLCBUb3BpY0J1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFRvcGljRm9ybSBleHRlbmRzIEhUTUxFbGVtZW50IHtcclxuICAgICAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICAgICAgc3VwZXIoKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldCB0ZW1wbGF0ZUlEKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0ZW1wbGF0ZWlkXCIpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHJpdmF0ZSBidXR0b246IEhUTUxJbnB1dEVsZW1lbnQ7XHJcblxyXG4gICAgICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgICAgICBsZXQgdGVtcGxhdGUgPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCh0aGlzLnRlbXBsYXRlSUQpIGFzIEhUTUxUZW1wbGF0ZUVsZW1lbnQ7XHJcbiAgICAgICAgICAgIGlmICghdGVtcGxhdGUpIHtcclxuICAgICAgICAgICAgICAgIGNvbnNvbGUuZXJyb3IoYENhbid0IGZpbmQgdGVtcGxhdGVJRCAke3RoaXMudGVtcGxhdGVJRH0gdG8gbWFrZSByZXBseSBmb3JtLmApO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHRoaXMuYXBwZW5kKHRlbXBsYXRlLmNvbnRlbnQuY2xvbmVOb2RlKHRydWUpKTtcclxuICAgICAgICAgICAgdGhpcy5idXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjU3VibWl0TmV3VG9waWNcIik7XHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnN1Ym1pdFRvcGljKCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgc3VibWl0VG9waWMoKSB7XHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uLnNldEF0dHJpYnV0ZShcImRpc2FibGVkXCIsIFwiZGlzYWJsZWRcIik7XHJcbiAgICAgICAgICAgIHZhciBtb2RlbCA9IHtcclxuICAgICAgICAgICAgICAgIFRpdGxlOiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1RvcGljICNUaXRsZVwiKSBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZSxcclxuICAgICAgICAgICAgICAgIEZ1bGxUZXh0OiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1RvcGljICNGdWxsVGV4dFwiKWFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlLFxyXG4gICAgICAgICAgICAgICAgSW5jbHVkZVNpZ25hdHVyZTogKHRoaXMucXVlcnlTZWxlY3RvcihcIiNOZXdUb3BpYyAjSW5jbHVkZVNpZ25hdHVyZVwiKWFzIEhUTUxJbnB1dEVsZW1lbnQpLmNoZWNrZWQsXHJcbiAgICAgICAgICAgICAgICBJdGVtSUQ6ICh0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTmV3VG9waWMgI0l0ZW1JRFwiKWFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlLFxyXG4gICAgICAgICAgICAgICAgSXNQbGFpblRleHQ6ICh0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTmV3VG9waWMgI0lzUGxhaW5UZXh0XCIpYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWUudG9Mb3dlckNhc2UoKSA9PT0gXCJ0cnVlXCJcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vUG9zdFRvcGljXCIsIHtcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCIsXHJcbiAgICAgICAgICAgICAgICBib2R5OiBKU09OLnN0cmluZ2lmeShtb2RlbCksXHJcbiAgICAgICAgICAgICAgICBoZWFkZXJzOiB7XHJcbiAgICAgICAgICAgICAgICAgICAgXCJDb250ZW50LVR5cGVcIjogXCJhcHBsaWNhdGlvbi9qc29uXCJcclxuICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS5qc29uKCkpXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXN1bHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIHN3aXRjaCAocmVzdWx0LnJlc3VsdCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjYXNlIHRydWU6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB3aW5kb3cubG9jYXRpb24gPSByZXN1bHQucmVkaXJlY3Q7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgZGVmYXVsdDpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciByID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI1Bvc3RSZXNwb25zZU1lc3NhZ2VcIikgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByLmlubmVySFRNTCA9IHJlc3VsdC5tZXNzYWdlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24ucmVtb3ZlQXR0cmlidXRlKFwiZGlzYWJsZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC5jYXRjaChlcnJvciA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHIgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjUG9zdFJlc3BvbnNlTWVzc2FnZVwiKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICByLmlubmVySFRNTCA9IFwiVGhlcmUgd2FzIGFuIHVua25vd24gZXJyb3Igd2hpbGUgdHJ5aW5nIHRvIHBvc3RcIjtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmJ1dHRvbi5yZW1vdmVBdHRyaWJ1dGUoXCJkaXNhYmxlZFwiKTtcclxuICAgICAgICAgICAgICAgICAgICByLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9O1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICBjdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXRvcGljZm9ybScsIFRvcGljRm9ybSk7XHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgVG9waWNNb2RlcmF0aW9uTG9nQnV0dG9uIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9udGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbnRleHRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IHRvcGljaWQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0b3BpY2lkXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIHRoaXMuaW5uZXJIVE1MID0gVG9waWNNb2RlcmF0aW9uTG9nQnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmJ1dHRvbnRleHQ7XHJcbiAgICAgICAgbGV0IGNsYXNzZXMgPSB0aGlzLmJ1dHRvbmNsYXNzO1xyXG4gICAgICAgIGlmIChjbGFzc2VzPy5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICBjbGFzc2VzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiBidXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgIGxldCBjb250YWluZXIgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJkaXZcIik7XHJcbiAgICAgICAgICAgIGlmIChjb250YWluZXIuc3R5bGUuZGlzcGxheSAhPT0gXCJibG9ja1wiKVxyXG4gICAgICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvTW9kZXJhdG9yL1RvcGljTW9kZXJhdGlvbkxvZy9cIiArIHRoaXMudG9waWNpZClcclxuICAgICAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS50ZXh0KClcclxuICAgICAgICAgICAgICAgICAgICAgICAgLnRoZW4odGV4dCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb250YWluZXIuaW5uZXJIVE1MID0gdGV4dDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNvbnRhaW5lci5zdHlsZS5kaXNwbGF5ID0gXCJibG9ja1wiO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICAgICAgICAgIGVsc2UgY29udGFpbmVyLnN0eWxlLmRpc3BsYXkgPSBcIm5vbmVcIjtcclxuICAgICAgICB9KTtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIC8+XHJcbiAgICA8ZGl2PjwvZGl2PmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZShcInBmLXRvcGljbW9kZXJhdGlvbmxvZ2J1dHRvblwiLCBUb3BpY01vZGVyYXRpb25Mb2dCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBWb3RlQ291bnQgZXh0ZW5kcyBIVE1MRWxlbWVudCB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCB2b3RlcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInZvdGVzXCIpO1xyXG4gICAgfVxyXG4gICAgc2V0IHZvdGVzKHZhbHVlOnN0cmluZykge1xyXG4gICAgICAgIHRoaXMuc2V0QXR0cmlidXRlKFwidm90ZXNcIiwgdmFsdWUpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBwb3N0aWQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJwb3N0aWRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IHZvdGVzY29udGFpbmVyY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ2b3Rlc2NvbnRhaW5lcmNsYXNzXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBiYWRnZWNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYmFkZ2VjbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgdm90ZWJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidm90ZWJ1dHRvbmNsYXNzXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBpc2xvZ2dlZGluKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiaXNsb2dnZWRpblwiKS50b0xvd2VyQ2FzZSgpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBpc2F1dGhvcigpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImlzYXV0aG9yXCIpLnRvTG93ZXJDYXNlKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGlzdm90ZWQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJpc3ZvdGVkXCIpLnRvTG93ZXJDYXNlKCk7XHJcbiAgICB9XHJcblxyXG4gICAgcHJpdmF0ZSBiYWRnZTogSFRNTEVsZW1lbnQ7XHJcbiAgICBwcml2YXRlIHZvdGVyQ29udGFpbmVyOiBIVE1MRWxlbWVudDtcclxuICAgIHByaXZhdGUgcG9wb3ZlcjogYm9vdHN0cmFwLlBvcG92ZXI7XHJcbiAgICBwcml2YXRlIHBvcG92ZXJFdmVudEhhbmRlcjogRXZlbnRMaXN0ZW5lck9yRXZlbnRMaXN0ZW5lck9iamVjdDtcclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFZvdGVDb3VudC50ZW1wbGF0ZTtcclxuICAgICAgICB0aGlzLmJhZGdlID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiZGl2XCIpO1xyXG4gICAgICAgIHRoaXMuYmFkZ2UuaW5uZXJIVE1MID0gXCIrXCIgKyB0aGlzLnZvdGVzO1xyXG4gICAgICAgIGlmICh0aGlzLmJhZGdlY2xhc3M/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgIHRoaXMuYmFkZ2VjbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gdGhpcy5iYWRnZS5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICB2YXIgc3RhdHVzSHRtbCA9IHRoaXMuYnV0dG9uR2VuZXJhdG9yKCk7XHJcbiAgICAgICAgaWYgKHN0YXR1c0h0bWwgIT0gXCJcIikge1xyXG4gICAgICAgICAgICBsZXQgc3RhdHVzID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChcInRlbXBsYXRlXCIpO1xyXG4gICAgICAgICAgICBzdGF0dXMuaW5uZXJIVE1MID0gdGhpcy5idXR0b25HZW5lcmF0b3IoKTtcclxuICAgICAgICAgICAgdGhpcy5hcHBlbmQoc3RhdHVzLmNvbnRlbnQuZmlyc3RDaGlsZCk7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGxldCB2b3RlQnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwic3BhblwiKTtcclxuICAgICAgICBpZiAodm90ZUJ1dHRvbikge1xyXG4gICAgICAgICAgICBpZiAodGhpcy52b3RlYnV0dG9uY2xhc3M/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgICAgICB0aGlzLnZvdGVidXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gdm90ZUJ1dHRvbi5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICAgICAgdHlwZSByZXN1bHRUeXBlID0geyB2b3RlczogbnVtYmVyOyBpc1ZvdGVkOiBib29sZWFuOyB9XHJcbiAgICAgICAgICAgIHZvdGVCdXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IHtcclxuICAgICAgICAgICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0ZvcnVtL1RvZ2dsZVZvdGUvXCIgKyB0aGlzLnBvc3RpZCwgeyBtZXRob2Q6IFwiUE9TVFwifSlcclxuICAgICAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLmpzb24oKVxyXG4gICAgICAgICAgICAgICAgICAgIC50aGVuKChyZXN1bHQ6IHJlc3VsdFR5cGUpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy52b3RlcyA9IHJlc3VsdC52b3Rlcy50b1N0cmluZygpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmJhZGdlLmlubmVySFRNTCA9IFwiK1wiICsgdGhpcy52b3RlcztcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHJlc3VsdC5pc1ZvdGVkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2b3RlQnV0dG9uLmNsYXNzTGlzdC5yZW1vdmUoXCJpY29uLXBsdXNcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2b3RlQnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJpY29uLWNhbmNlbC1jaXJjbGVcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2b3RlQnV0dG9uLmNsYXNzTGlzdC5yZW1vdmUoXCJpY29uLWNhbmNlbC1jaXJjbGVcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2b3RlQnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJpY29uLXBsdXNcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5hcHBseVBvcG92ZXIoKTtcclxuICAgICAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgfVxyXG4gICAgICAgIHRoaXMuc2V0dXBWb3RlclBvcG92ZXIoKTtcclxuICAgICAgICB0aGlzLmFwcGx5UG9wb3ZlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIHByaXZhdGUgc2V0dXBWb3RlclBvcG92ZXIoKTogdm9pZCB7XHJcbiAgICAgICAgdGhpcy52b3RlckNvbnRhaW5lciA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJkaXZcIik7XHJcbiAgICAgICAgaWYgKHRoaXMudm90ZXNjb250YWluZXJjbGFzcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgdGhpcy52b3Rlc2NvbnRhaW5lcmNsYXNzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiB0aGlzLnZvdGVyQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIHRoaXMudm90ZXJDb250YWluZXIuaW5uZXJIVE1MID0gXCJMb2FkaW5nLi4uXCI7XHJcbiAgICAgICAgdGhpcy5wb3BvdmVyID0gbmV3IGJvb3RzdHJhcC5Qb3BvdmVyKHRoaXMuYmFkZ2UsIHtcclxuICAgICAgICAgICAgY29udGVudDogdGhpcy52b3RlckNvbnRhaW5lcixcclxuICAgICAgICAgICAgaHRtbDogdHJ1ZSxcclxuICAgICAgICAgICAgdHJpZ2dlcjogXCJjbGljayBmb2N1c1wiXHJcbiAgICAgICAgfSk7XHJcbiAgICAgICAgdGhpcy5wb3BvdmVyRXZlbnRIYW5kZXIgPSAoZSkgPT4ge1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9Wb3RlcnMvXCIgKyB0aGlzLnBvc3RpZClcclxuICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgdCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ0ZW1wbGF0ZVwiKTtcclxuICAgICAgICAgICAgICAgICAgICB0LmlubmVySFRNTCA9IHRleHQudHJpbSgpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMudm90ZXJDb250YWluZXIuaW5uZXJIVE1MID0gXCJcIjtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLnZvdGVyQ29udGFpbmVyLmFwcGVuZENoaWxkKHQuY29udGVudC5maXJzdENoaWxkKTtcclxuICAgICAgICAgICAgICAgIH0pKTtcclxuICAgICAgICB9O1xyXG4gICAgICAgIHRoaXMuYmFkZ2UuYWRkRXZlbnRMaXN0ZW5lcihcInNob3duLmJzLnBvcG92ZXJcIiwgdGhpcy5wb3BvdmVyRXZlbnRIYW5kZXIpO1xyXG4gICAgfVxyXG5cclxuICAgIHByaXZhdGUgYXBwbHlQb3BvdmVyKCk6IHZvaWQge1xyXG4gICAgICAgIGlmICh0aGlzLnZvdGVzID09PSBcIjBcIikge1xyXG4gICAgICAgICAgICB0aGlzLmJhZGdlLnN0eWxlLmN1cnNvciA9IFwiZGVmYXVsdFwiO1xyXG4gICAgICAgICAgICB0aGlzLnBvcG92ZXIuZGlzYWJsZSgpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICAgdGhpcy5iYWRnZS5zdHlsZS5jdXJzb3IgPSBcInBvaW50ZXJcIjtcclxuICAgICAgICAgICAgdGhpcy5wb3BvdmVyLmVuYWJsZSgpO1xyXG4gICAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIGJ1dHRvbkdlbmVyYXRvcigpOiBzdHJpbmcge1xyXG4gICAgICAgIGlmICh0aGlzLmlzbG9nZ2VkaW4gPT09IFwiZmFsc2VcIiB8fCB0aGlzLmlzYXV0aG9yID09PSBcInRydWVcIilcclxuICAgICAgICAgICAgcmV0dXJuIFwiXCI7XHJcbiAgICAgICAgaWYgKHRoaXMuaXN2b3RlZCA9PT0gXCJ0cnVlXCIpXHJcbiAgICAgICAgICAgIHJldHVybiBWb3RlQ291bnQuY2FuY2VsVm90ZUJ1dHRvbjtcclxuICAgICAgICByZXR1cm4gVm90ZUNvdW50LnZvdGVVcEJ1dHRvbjtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8ZGl2PjwvZGl2PmA7XHJcblxyXG4gICAgc3RhdGljIHZvdGVVcEJ1dHRvbiA9IFwiPHNwYW4gY2xhc3M9XFxcImljb24tcGx1c1xcXCI+PC9zcGFuPlwiO1xyXG4gICAgc3RhdGljIGNhbmNlbFZvdGVCdXR0b24gPSBcIjxzcGFuIGNsYXNzPVxcXCJpY29uLWNhbmNlbC1jaXJjbGVcXFwiPjwvc3Bhbj5cIjtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKFwicGYtdm90ZWNvdW50XCIsIFZvdGVDb3VudCk7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIE5vdGlmaWNhdGlvblNlcnZpY2Uge1xyXG4gICAgICAgIGNvbnN0cnVjdG9yKHVzZXJTdGF0ZTogVXNlclN0YXRlKSB7XHJcbiAgICAgICAgICAgIHRoaXMudXNlclN0YXRlID0gdXNlclN0YXRlO1xyXG4gICAgICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbiA9IG5ldyBzaWduYWxSLkh1YkNvbm5lY3Rpb25CdWlsZGVyKCkud2l0aFVybChcIi9Ob3RpZmljYXRpb25IdWJcIikud2l0aEF1dG9tYXRpY1JlY29ubmVjdCgpLmJ1aWxkKCk7XHJcbiAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbi5vbihcInVwZGF0ZVBNQ291bnRcIiwgZnVuY3Rpb24ocG1Db3VudDogbnVtYmVyKSB7XHJcbiAgICAgICAgICAgICAgICBzZWxmLnVzZXJTdGF0ZS5uZXdQbUNvdW50ID0gcG1Db3VudDtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbi5zdGFydCgpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHJpdmF0ZSB1c2VyU3RhdGU6IFVzZXJTdGF0ZTtcclxuICAgICAgICBwcml2YXRlIGNvbm5lY3Rpb246IGFueTtcclxuICAgIH1cclxufSIsIi8vIFRPRE86IE1vdmUgdGhpcyB0byBhbiBvcGVuIHdlYnNvY2tldHMgY29ubmVjdGlvblxyXG5cclxubmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcbiAgICBleHBvcnQgY2xhc3MgVGltZVVwZGF0ZXIge1xyXG4gICAgICAgIFN0YXJ0KCkge1xyXG4gICAgICAgICAgICBSZWFkeSgoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLlN0YXJ0VXBkYXRlcigpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgdGltZUFycmF5OiBzdHJpbmdbXTtcclxuXHJcbiAgICAgICAgcHJpdmF0ZSBQb3B1bGF0ZUFycmF5KCk6IHZvaWQge1xyXG4gICAgICAgICAgICB0aGlzLnRpbWVBcnJheSA9IFtdO1xyXG4gICAgICAgICAgICBsZXQgdGltZXMgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLmZUaW1lXCIpO1xyXG4gICAgICAgICAgICB0aW1lcy5mb3JFYWNoKHRpbWUgPT4ge1xyXG4gICAgICAgICAgICAgICAgdmFyIHQgPSB0aW1lLmdldEF0dHJpYnV0ZShcImRhdGEtdXRjXCIpO1xyXG4gICAgICAgICAgICAgICAgaWYgKCgobmV3IERhdGUoKS5nZXREYXRlKCkgLSBuZXcgRGF0ZSh0ICsgXCJaXCIpLmdldERhdGUoKSkgLyAzNjAwMDAwKSA8IDQ4KVxyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMudGltZUFycmF5LnB1c2godCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHJpdmF0ZSBTdGFydFVwZGF0ZXIoKTogdm9pZCB7XHJcbiAgICAgICAgICAgIHNldFRpbWVvdXQoKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5TdGFydFVwZGF0ZXIoKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuUG9wdWxhdGVBcnJheSgpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5DYWxsRm9yVXBkYXRlKCk7XHJcbiAgICAgICAgICAgIH0sIDYwMDAwKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgQ2FsbEZvclVwZGF0ZSgpOiB2b2lkIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLnRpbWVBcnJheSB8fCB0aGlzLnRpbWVBcnJheS5sZW5ndGggPT09IDApXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIGxldCBzZXJpYWxpemVkID0gSlNPTi5zdHJpbmdpZnkodGhpcy50aW1lQXJyYXkpO1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9UaW1lL0dldFRpbWVzXCIsIHtcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCIsXHJcbiAgICAgICAgICAgICAgICBib2R5OiBzZXJpYWxpemVkLFxyXG4gICAgICAgICAgICAgICAgaGVhZGVyczoge1xyXG4gICAgICAgICAgICAgICAgICAgIFwiQ29udGVudC1UeXBlXCI6IFwiYXBwbGljYXRpb24vanNvblwiXHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS5qc29uKCkpXHJcbiAgICAgICAgICAgICAgICAudGhlbihkYXRhID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBkYXRhLmZvckVhY2goKHQ6IHsga2V5OiBzdHJpbmc7IHZhbHVlOiBzdHJpbmc7IH0pID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIi5mVGltZVtkYXRhLXV0Yz0nXCIgKyB0LmtleSArIFwiJ11cIikuaW5uZXJIVE1MID0gdC52YWx1ZTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAuY2F0Y2goZXJyb3IgPT4geyBjb25zb2xlLmxvZyhcIlRpbWUgdXBkYXRlIGZhaWx1cmU6IFwiICsgZXJyb3IpOyB9KTtcclxuICAgICAgICB9XHJcbiAgICB9XHJcbn1cclxuXHJcbnZhciB0aW1lVXBkYXRlciA9IG5ldyBQb3BGb3J1bXMuVGltZVVwZGF0ZXIoKTtcclxudGltZVVwZGF0ZXIuU3RhcnQoKTsiLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgRm9ydW1TdGF0ZSBleHRlbmRzIFN0YXRlQmFzZSB7XHJcbiAgICAgICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgICAgIHN1cGVyKCk7XHJcbiAgICAgICAgfVxyXG4gICAgXHJcbiAgICAgICAgZm9ydW1JRDogbnVtYmVyO1xyXG4gICAgICAgIHBhZ2VTaXplOiBudW1iZXI7XHJcbiAgICAgICAgcGFnZUluZGV4OiBudW1iZXI7XHJcbiAgICAgICAgQFdhdGNoUHJvcGVydHlcclxuICAgICAgICBpc05ld1RvcGljTG9hZGVkOiBib29sZWFuO1xyXG5cclxuICAgICAgICBzZXR1cEZvcnVtKCkge1xyXG4gICAgICAgICAgICBQb3BGb3J1bXMuUmVhZHkoKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5pc05ld1RvcGljTG9hZGVkID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmZvcnVtTGlzdGVuKCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgbG9hZE5ld1RvcGljKCkge1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9Qb3N0VG9waWMvXCIgKyB0aGlzLmZvcnVtSUQpXHJcbiAgICAgICAgICAgICAgICAudGhlbigocmVzcG9uc2UpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gcmVzcG9uc2UudGV4dCgpO1xyXG4gICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC50aGVuKChib2R5KSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIG4gPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI05ld1RvcGljXCIpIGFzIEhUTUxFbGVtZW50O1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICghbilcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhyb3coXCJDYW4ndCBmaW5kIGEgI05ld1RvcGljIGVsZW1lbnQgdG8gbG9hZCBpbiB0aGUgbmV3IHRvcGljIGZvcm0uXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIG4uaW5uZXJIVE1MID0gYm9keTtcclxuICAgICAgICAgICAgICAgICAgICBuLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5pc05ld1RvcGljTG9hZGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZm9ydW1MaXN0ZW4oKSB7XHJcbiAgICAgICAgICAgIGxldCBjb25uZWN0aW9uID0gbmV3IHNpZ25hbFIuSHViQ29ubmVjdGlvbkJ1aWxkZXIoKS53aXRoVXJsKFwiL0ZvcnVtc0h1YlwiKS53aXRoQXV0b21hdGljUmVjb25uZWN0KCkuYnVpbGQoKTtcclxuICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICBjb25uZWN0aW9uLm9uKFwibm90aWZ5VXBkYXRlZFRvcGljXCIsIGZ1bmN0aW9uIChkYXRhOiBhbnkpIHsgLy8gVE9ETzogcmVmYWN0b3IgdG8gc3Ryb25nIHR5cGVcclxuICAgICAgICAgICAgICAgIGxldCByZW1vdmFsID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcignI1RvcGljTGlzdCB0cltkYXRhLXRvcGljSUQ9XCInICsgZGF0YS50b3BpY0lEICsgJ1wiXScpO1xyXG4gICAgICAgICAgICAgICAgaWYgKHJlbW92YWwpIHtcclxuICAgICAgICAgICAgICAgICAgICByZW1vdmFsLnJlbW92ZSgpO1xyXG4gICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgcm93cyA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIjVG9waWNMaXN0IHRyOm5vdCgjVG9waWNUZW1wbGF0ZSlcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHJvd3MubGVuZ3RoID09IHNlbGYucGFnZVNpemUpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJvd3Nbcm93cy5sZW5ndGggLSAxXS5yZW1vdmUoKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGxldCByb3cgPSBzZWxmLnBvcHVsYXRlVG9waWNSb3coZGF0YSk7XHJcbiAgICAgICAgICAgICAgICByb3cuY2xhc3NMaXN0LnJlbW92ZShcImhpZGRlblwiKTtcclxuICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjVG9waWNMaXN0IHRib2R5XCIpLnByZXBlbmQocm93KTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIGNvbm5lY3Rpb24uc3RhcnQoKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4oZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBjb25uZWN0aW9uLmludm9rZShcImxpc3RlblRvXCIsIHNlbGYuZm9ydW1JRCk7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHJlY2VudExpc3RlbigpIHtcclxuICAgICAgICAgICAgdmFyIGNvbm5lY3Rpb24gPSBuZXcgc2lnbmFsUi5IdWJDb25uZWN0aW9uQnVpbGRlcigpLndpdGhVcmwoXCIvUmVjZW50SHViXCIpLndpdGhBdXRvbWF0aWNSZWNvbm5lY3QoKS5idWlsZCgpO1xyXG4gICAgICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgICAgIGNvbm5lY3Rpb24ub24oXCJub3RpZnlSZWNlbnRVcGRhdGVcIiwgZnVuY3Rpb24gKGRhdGE6IGFueSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIHJlbW92YWwgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKCcjVG9waWNMaXN0IHRyW2RhdGEtdG9waWNJRD1cIicgKyBkYXRhLnRvcGljSUQgKyAnXCJdJyk7XHJcbiAgICAgICAgICAgICAgICBpZiAocmVtb3ZhbCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJlbW92YWwucmVtb3ZlKCk7XHJcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciByb3dzID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIiNUb3BpY0xpc3QgdHI6bm90KCNUb3BpY1RlbXBsYXRlKVwiKTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAocm93cy5sZW5ndGggPT0gc2VsZi5wYWdlU2l6ZSlcclxuICAgICAgICAgICAgICAgICAgICAgICAgcm93c1tyb3dzLmxlbmd0aCAtIDFdLnJlbW92ZSgpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgdmFyIHJvdyA9IHNlbGYucG9wdWxhdGVUb3BpY1JvdyhkYXRhKTtcclxuICAgICAgICAgICAgICAgIHJvdy5jbGFzc0xpc3QucmVtb3ZlKFwiaGlkZGVuXCIpO1xyXG4gICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNUb3BpY0xpc3QgdGJvZHlcIikucHJlcGVuZChyb3cpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgY29ubmVjdGlvbi5zdGFydCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbm5lY3Rpb24uaW52b2tlKFwicmVnaXN0ZXJcIik7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHBvcHVsYXRlVG9waWNSb3cgPSBmdW5jdGlvbiAoZGF0YTogYW55KSB7XHJcbiAgICAgICAgICAgIGxldCByb3cgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI1RvcGljVGVtcGxhdGVcIikuY2xvbmVOb2RlKHRydWUpIGFzIEhUTUxFbGVtZW50O1xyXG4gICAgICAgICAgICByb3cuc2V0QXR0cmlidXRlKFwiZGF0YS10b3BpY2lkXCIsIGRhdGEudG9waWNJRCk7XHJcbiAgICAgICAgICAgIHJvdy5yZW1vdmVBdHRyaWJ1dGUoXCJpZFwiKTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIuc3RhcnRlZEJ5TmFtZVwiKS5pbm5lckhUTUwgPSBkYXRhLnN0YXJ0ZWRCeU5hbWU7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLmluZGljYXRvckxpbmtcIikuc2V0QXR0cmlidXRlKFwiaHJlZlwiLCBkYXRhLmxpbmspO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi50aXRsZUxpbmtcIikuaW5uZXJIVE1MID0gZGF0YS50aXRsZTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIudGl0bGVMaW5rXCIpLnNldEF0dHJpYnV0ZShcImhyZWZcIiwgZGF0YS5saW5rKTtcclxuICAgICAgICAgICAgdmFyIGZvcnVtVGl0bGUgPSByb3cucXVlcnlTZWxlY3RvcihcIi5mb3J1bVRpdGxlXCIpO1xyXG4gICAgICAgICAgICBpZiAoZm9ydW1UaXRsZSkgZm9ydW1UaXRsZS5pbm5lckhUTUwgPSBkYXRhLmZvcnVtVGl0bGU7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLnZpZXdDb3VudFwiKS5pbm5lckhUTUwgPSBkYXRhLnZpZXdDb3VudDtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIucmVwbHlDb3VudFwiKS5pbm5lckhUTUwgPSBkYXRhLnJlcGx5Q291bnQ7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLmxhc3RQb3N0VGltZVwiKS5pbm5lckhUTUwgPSBkYXRhLmxhc3RQb3N0VGltZTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIubGFzdFBvc3ROYW1lXCIpLmlubmVySFRNTCA9IGRhdGEubGFzdFBvc3ROYW1lO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5mVGltZVwiKS5zZXRBdHRyaWJ1dGUoXCJkYXRhLXV0Y1wiLCBkYXRhLnV0Yyk7XHJcbiAgICAgICAgICAgIHJldHVybiByb3c7XHJcbiAgICAgICAgfTtcclxuICAgIH1cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuZXhwb3J0IGNsYXNzIFRvcGljU3RhdGUgZXh0ZW5kcyBTdGF0ZUJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICB0b3BpY0lEOiBudW1iZXI7XHJcbiAgICBpc0ltYWdlRW5hYmxlZDogYm9vbGVhbjtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcbiAgICBpc1JlcGx5TG9hZGVkOiBib29sZWFuO1xyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuICAgIGFuc3dlclBvc3RJRDogbnVtYmVyO1xyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuXHRsb3dQYWdlOm51bWJlcjtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcblx0aGlnaFBhZ2U6IG51bWJlcjtcclxuXHRsYXN0VmlzaWJsZVBvc3RJRDogbnVtYmVyO1xyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuICAgIGlzTmV3ZXJQb3N0c0F2YWlsYWJsZTogYm9vbGVhbjtcclxuICAgIHBhZ2VJbmRleDogbnVtYmVyO1xyXG5cdHBhZ2VDb3VudDogbnVtYmVyO1xyXG5cdGxvYWRpbmdQb3N0czogYm9vbGVhbiA9IGZhbHNlO1xyXG5cdGlzU2Nyb2xsQWRqdXN0ZWQ6IGJvb2xlYW4gPSBmYWxzZTtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcbiAgICBjb21tZW50UmVwbHlJRDogbnVtYmVyO1xyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuICAgIG5leHRRdW90ZTogc3RyaW5nO1xyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuICAgIGlzU3Vic2NyaWJlZDogYm9vbGVhbjtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcbiAgICBpc0Zhdm9yaXRlOiBib29sZWFuO1xyXG5cclxuICAgIHNldHVwVG9waWMoKSB7XHJcbiAgICAgICAgUG9wRm9ydW1zLlJlYWR5KCgpID0+IHtcclxuICAgICAgICAgICAgdGhpcy5pc1JlcGx5TG9hZGVkID0gZmFsc2U7XHJcbiAgICAgICAgICAgIHRoaXMuaXNOZXdlclBvc3RzQXZhaWxhYmxlID0gZmFsc2U7XHJcbiAgICAgICAgICAgIHRoaXMubG93UGFnZSA9IHRoaXMucGFnZUluZGV4O1xyXG4gICAgICAgICAgICB0aGlzLmhpZ2hQYWdlID0gdGhpcy5wYWdlSW5kZXg7XHJcblxyXG4gICAgICAgICAgICAvLyBzaWduYWxSIGNvbm5lY3Rpb25zXHJcbiAgICAgICAgICAgIGxldCBjb25uZWN0aW9uID0gbmV3IHNpZ25hbFIuSHViQ29ubmVjdGlvbkJ1aWxkZXIoKS53aXRoVXJsKFwiL1RvcGljc0h1YlwiKS53aXRoQXV0b21hdGljUmVjb25uZWN0KCkuYnVpbGQoKTtcclxuICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICAvLyBmb3IgYWxsIHBvc3RzIGxvYWRlZCBidXQgcmVwbHkgbm90IG9wZW5cclxuICAgICAgICAgICAgY29ubmVjdGlvbi5vbihcImZldGNoTmV3UG9zdFwiLCBmdW5jdGlvbiAocG9zdElEOiBudW1iZXIpIHtcclxuICAgICAgICAgICAgICAgIGlmICghc2VsZi5pc1JlcGx5TG9hZGVkICYmIHNlbGYuaGlnaFBhZ2UgPT09IHNlbGYucGFnZUNvdW50KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vUG9zdC9cIiArIHBvc3RJRClcclxuICAgICAgICAgICAgICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgdCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ0ZW1wbGF0ZVwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0LmlubmVySFRNTCA9IHRleHQudHJpbSgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjUG9zdFN0cmVhbVwiKS5hcHBlbmRDaGlsZCh0LmNvbnRlbnQuZmlyc3RDaGlsZCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICAgICAgICAgICAgICAgICAgc2VsZi5sYXN0VmlzaWJsZVBvc3RJRCA9IHBvc3RJRDtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIC8vIGZvciByZXBseSBhbHJlYWR5IG9wZW5cclxuICAgICAgICAgICAgY29ubmVjdGlvbi5vbihcIm5vdGlmeU5ld1Bvc3RzXCIsIGZ1bmN0aW9uICh0aGVMYXN0UG9zdElEOiBudW1iZXIpIHtcclxuICAgICAgICAgICAgICAgIHNlbGYuc2V0TW9yZVBvc3RzQXZhaWxhYmxlKHRoZUxhc3RQb3N0SUQpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgY29ubmVjdGlvbi5zdGFydCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbm5lY3Rpb24uaW52b2tlKFwibGlzdGVuVG9cIiwgc2VsZi50b3BpY0lEKTtcclxuICAgICAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgc2VsZi5jb25uZWN0aW9uID0gY29ubmVjdGlvblxyXG4gICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLnBvc3RJdGVtIGltZzpub3QoLmF2YXRhcilcIikuZm9yRWFjaCh4ID0+IHguY2xhc3NMaXN0LmFkZChcInBvc3RJbWFnZVwiKSk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLnNjcm9sbFRvUG9zdEZyb21IYXNoKCk7XHJcbiAgICAgICAgICAgIHdpbmRvdy5hZGRFdmVudExpc3RlbmVyKFwic2Nyb2xsXCIsIHRoaXMuc2Nyb2xsTG9hZCk7XHJcbiAgICAgICAgfSk7XHJcbiAgICB9XHJcblxyXG4gICAgbG9hZFJlcGx5KHRvcGljSUQ6bnVtYmVyLCByZXBseUlEOm51bWJlciwgc2V0dXBNb3JlUG9zdHM6Ym9vbGVhbik6dm9pZCB7XHJcbiAgICAgICAgaWYgKHRoaXMuaXNSZXBseUxvYWRlZCkge1xyXG4gICAgICAgICAgICB0aGlzLnNjcm9sbFRvRWxlbWVudChcIk5ld1JlcGx5XCIpO1xyXG4gICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgfVxyXG4gICAgICAgIHdpbmRvdy5yZW1vdmVFdmVudExpc3RlbmVyKFwic2Nyb2xsXCIsIHRoaXMuc2Nyb2xsTG9hZCk7XHJcbiAgICAgICAgdmFyIHBhdGggPSBQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9Qb3N0UmVwbHkvXCIgKyB0b3BpY0lEO1xyXG4gICAgICAgIGlmIChyZXBseUlEICE9IG51bGwpIHtcclxuICAgICAgICAgICAgcGF0aCArPSBcIj9yZXBseUlEPVwiICsgcmVwbHlJRDtcclxuICAgICAgICB9XHJcbiAgICBcclxuICAgICAgICBmZXRjaChwYXRoKVxyXG4gICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS50ZXh0KClcclxuICAgICAgICAgICAgICAgIC50aGVuKHRleHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIGxldCBuID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNOZXdSZXBseVwiKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICBuLmlubmVySFRNTCA9IHRleHQ7XHJcbiAgICAgICAgICAgICAgICAgICAgbi5zdHlsZS5kaXNwbGF5ID0gXCJibG9ja1wiO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuc2Nyb2xsVG9FbGVtZW50KFwiTmV3UmVwbHlcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5pc1JlcGx5TG9hZGVkID0gdHJ1ZTtcclxuICAgIFxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChzZXR1cE1vcmVQb3N0cykge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbi5pbnZva2UoXCJnZXRMYXN0UG9zdElEXCIsIHRoaXMudG9waWNJRClcclxuICAgICAgICAgICAgICAgICAgICAgICAgLnRoZW4oZnVuY3Rpb24gKHJlc3VsdDogbnVtYmVyKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxmLnNldE1vcmVQb3N0c0F2YWlsYWJsZShyZXN1bHQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5pc1JlcGx5TG9hZGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmNvbW1lbnRSZXBseUlEID0gMDtcclxuICAgICAgICAgICAgICAgIH0pKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIGNvbm5lY3Rpb246IGFueTtcclxuXHJcbiAgICAvLyB0aGlzIGlzIGludGVuZGVkIHRvIGJlIGNhbGxlZCB3aGVuIHRoZSByZXBseSBib3ggaXMgb3BlblxyXG4gICAgcHJpdmF0ZSBzZXRNb3JlUG9zdHNBdmFpbGFibGUgPSAobmV3ZXN0UG9zdElEb25TZXJ2ZXI6IG51bWJlcikgPT4ge1xyXG4gICAgICAgIHRoaXMuaXNOZXdlclBvc3RzQXZhaWxhYmxlID0gbmV3ZXN0UG9zdElEb25TZXJ2ZXIgIT09IHRoaXMubGFzdFZpc2libGVQb3N0SUQ7XHJcbiAgICB9XHJcblxyXG4gICAgbG9hZENvbW1lbnQodG9waWNJRDogbnVtYmVyLCByZXBseUlEOiBudW1iZXIpOiB2b2lkIHtcclxuICAgICAgICB2YXIgbiA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCJbZGF0YS1wb3N0aWQqPSdcIiArIHJlcGx5SUQgKyBcIiddIC5jb21tZW50SG9sZGVyXCIpO1xyXG4gICAgICAgIGNvbnN0IGJveGlkID0gXCJjb21tZW50Ym94XCI7XHJcbiAgICAgICAgbi5pZCA9IGJveGlkO1xyXG4gICAgICAgIHZhciBwYXRoID0gUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vUG9zdFJlcGx5L1wiICsgdG9waWNJRCArIFwiP3JlcGx5SUQ9XCIgKyByZXBseUlEO1xyXG4gICAgICAgIHRoaXMuY29tbWVudFJlcGx5SUQgPSByZXBseUlEO1xyXG4gICAgICAgIHRoaXMuaXNSZXBseUxvYWRlZCA9IHRydWU7XHJcbiAgICAgICAgZmV0Y2gocGF0aClcclxuICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBuLmlubmVySFRNTCA9IHRleHQ7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5zY3JvbGxUb0VsZW1lbnQoYm94aWQpO1xyXG4gICAgICAgICAgICAgICAgfSkpO1xyXG4gICAgfTtcclxuXHJcbiAgICBsb2FkTW9yZVBvc3RzID0gKCkgPT4ge1xyXG4gICAgICAgIGxldCB0b3BpY1BhZ2VQYXRoOiBzdHJpbmc7XHJcbiAgICAgICAgaWYgKHRoaXMuaGlnaFBhZ2UgPT09IHRoaXMucGFnZUNvdW50KSB7XHJcbiAgICAgICAgICAgIHRvcGljUGFnZVBhdGggPSBQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9Ub3BpY1BhcnRpYWwvXCIgKyB0aGlzLnRvcGljSUQgKyBcIj9sYXN0UG9zdD1cIiArIHRoaXMubGFzdFZpc2libGVQb3N0SUQgKyBcIiZsb3dQYWdlPVwiICsgdGhpcy5sb3dQYWdlO1xyXG4gICAgICAgIH1cclxuICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICAgdGhpcy5oaWdoUGFnZSsrO1xyXG4gICAgICAgICAgICB0b3BpY1BhZ2VQYXRoID0gUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vVG9waWNQYWdlL1wiICsgdGhpcy50b3BpY0lEICsgXCI/cGFnZU51bWJlcj1cIiArIHRoaXMuaGlnaFBhZ2UgKyBcIiZsb3c9XCIgKyB0aGlzLmxvd1BhZ2UgKyBcIiZoaWdoPVwiICsgdGhpcy5oaWdoUGFnZTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZmV0Y2godG9waWNQYWdlUGF0aClcclxuICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgdCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ0ZW1wbGF0ZVwiKTtcclxuICAgICAgICAgICAgICAgICAgICB0LmlubmVySFRNTCA9IHRleHQudHJpbSgpO1xyXG4gICAgICAgICAgICAgICAgICAgIGxldCBzdHVmZiA9IHQuY29udGVudC5maXJzdENoaWxkIGFzIEhUTUxFbGVtZW50O1xyXG4gICAgICAgICAgICAgICAgICAgIGxldCBsaW5rcyA9IHN0dWZmLnF1ZXJ5U2VsZWN0b3IoXCIucGFnZXJMaW5rc1wiKTtcclxuICAgICAgICAgICAgICAgICAgICBzdHVmZi5yZW1vdmVDaGlsZChsaW5rcyk7XHJcbiAgICAgICAgICAgICAgICAgICAgbGV0IGxhc3RQb3N0SUQgPSBzdHVmZi5xdWVyeVNlbGVjdG9yKFwiLmxhc3RQb3N0SURcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICBzdHVmZi5yZW1vdmVDaGlsZChsYXN0UG9zdElEKTtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgbmV3UGFnZUNvdW50ID0gc3R1ZmYucXVlcnlTZWxlY3RvcihcIi5wYWdlQ291bnRcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICBzdHVmZi5yZW1vdmVDaGlsZChuZXdQYWdlQ291bnQpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMubGFzdFZpc2libGVQb3N0SUQgPSBOdW1iZXIobGFzdFBvc3RJRC52YWx1ZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5wYWdlQ291bnQgPSBOdW1iZXIobmV3UGFnZUNvdW50LnZhbHVlKTtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgcG9zdFN0cmVhbSA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjUG9zdFN0cmVhbVwiKTtcclxuICAgICAgICAgICAgICAgICAgICBwb3N0U3RyZWFtLmFwcGVuZChzdHVmZik7XHJcbiAgICAgICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIi5wYWdlckxpbmtzXCIpLmZvckVhY2goeCA9PiB4LnJlcGxhY2VXaXRoKGxpbmtzLmNsb25lTm9kZSh0cnVlKSkpO1xyXG4gICAgICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIucG9zdEl0ZW0gaW1nOm5vdCguYXZhdGFyKVwiKS5mb3JFYWNoKHggPT4geC5jbGFzc0xpc3QuYWRkKFwicG9zdEltYWdlXCIpKTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAodGhpcy5oaWdoUGFnZSA9PSB0aGlzLnBhZ2VDb3VudCAmJiB0aGlzLmxvd1BhZ2UgPT0gMSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLnBhZ2VyTGlua3NcIikuZm9yRWFjaCh4ID0+IHgucmVtb3ZlKCkpO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmxvYWRpbmdQb3N0cyA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICghdGhpcy5pc1Njcm9sbEFkanVzdGVkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuc2Nyb2xsVG9Qb3N0RnJvbUhhc2goKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHRoaXMuaXNSZXBseUxvYWRlZCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbi5pbnZva2UoXCJnZXRMYXN0UG9zdElEXCIsIHRoaXMudG9waWNJRClcclxuICAgICAgICAgICAgICAgICAgICAgICAgLnRoZW4oZnVuY3Rpb24gKHJlc3VsdDogbnVtYmVyKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBzZWxmLnNldE1vcmVQb3N0c0F2YWlsYWJsZShyZXN1bHQpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICB9O1xyXG5cclxuICAgIGxvYWRQcmV2aW91c1Bvc3RzID0gKCkgPT4ge1xyXG4gICAgICAgIHRoaXMubG93UGFnZS0tO1xyXG4gICAgICAgIGxldCB0b3BpY1BhZ2VQYXRoID0gUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vVG9waWNQYWdlL1wiICsgdGhpcy50b3BpY0lEICsgXCI/cGFnZU51bWJlcj1cIiArIHRoaXMubG93UGFnZSArIFwiJmxvdz1cIiArIHRoaXMubG93UGFnZSArIFwiJmhpZ2g9XCIgKyB0aGlzLmhpZ2hQYWdlO1xyXG4gICAgICAgIGZldGNoKHRvcGljUGFnZVBhdGgpXHJcbiAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLnRleHQoKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4odGV4dCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgbGV0IHQgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwidGVtcGxhdGVcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdC5pbm5lckhUTUwgPSB0ZXh0LnRyaW0oKTtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgc3R1ZmYgPSB0LmNvbnRlbnQuZmlyc3RDaGlsZCBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgbGlua3MgPSBzdHVmZi5xdWVyeVNlbGVjdG9yKFwiLnBhZ2VyTGlua3NcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgc3R1ZmYucmVtb3ZlQ2hpbGQobGlua3MpO1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciBwb3N0U3RyZWFtID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNQb3N0U3RyZWFtXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHBvc3RTdHJlYW0ucHJlcGVuZChzdHVmZik7XHJcbiAgICAgICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIi5wYWdlckxpbmtzXCIpLmZvckVhY2goeCA9PiB4LnJlcGxhY2VXaXRoKGxpbmtzLmNsb25lTm9kZSh0cnVlKSkpO1xyXG4gICAgICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIucG9zdEl0ZW0gaW1nOm5vdCguYXZhdGFyKVwiKS5mb3JFYWNoKHggPT4geC5jbGFzc0xpc3QuYWRkKFwicG9zdEltYWdlXCIpKTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAodGhpcy5oaWdoUGFnZSA9PSB0aGlzLnBhZ2VDb3VudCAmJiB0aGlzLmxvd1BhZ2UgPT0gMSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLnBhZ2VyTGlua3NcIikuZm9yRWFjaCh4ID0+IHgucmVtb3ZlKCkpO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH0pKTtcclxuICAgIH1cclxuXHJcbiAgICBzY3JvbGxMb2FkID0gKCkgPT4ge1xyXG4gICAgICAgIGxldCBzdHJlYW1FbmQgPSAoZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNTdHJlYW1Cb3R0b21cIikgYXMgSFRNTEVsZW1lbnQpO1xyXG4gICAgICAgIGlmICghc3RyZWFtRW5kKVxyXG4gICAgICAgICAgICByZXR1cm47IC8vIHRoaXMgaXMgYSBRQSB0b3BpYywgbm8gY29udGludW91cyBwb3N0IHN0cmVhbVxyXG4gICAgICAgIGxldCB0b3AgPSBzdHJlYW1FbmQub2Zmc2V0VG9wO1xyXG4gICAgICAgIGxldCB2aWV3RW5kID0gd2luZG93LnNjcm9sbFkgKyB3aW5kb3cub3V0ZXJIZWlnaHQ7XHJcbiAgICAgICAgbGV0IGRpc3RhbmNlID0gdG9wIC0gdmlld0VuZDtcclxuICAgICAgICBpZiAoIXRoaXMubG9hZGluZ1Bvc3RzICYmIGRpc3RhbmNlIDwgMjUwICYmIHRoaXMuaGlnaFBhZ2UgPCB0aGlzLnBhZ2VDb3VudCkge1xyXG4gICAgICAgICAgICB0aGlzLmxvYWRpbmdQb3N0cyA9IHRydWU7XHJcbiAgICAgICAgICAgIHRoaXMubG9hZE1vcmVQb3N0cygpO1xyXG4gICAgICAgIH1cclxuICAgIH07XHJcblxyXG4gICAgc2Nyb2xsVG9FbGVtZW50ID0gKGlkOiBzdHJpbmcpID0+IHtcclxuICAgICAgICBsZXQgZSA9IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKGlkKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICBsZXQgdCA9IDA7XHJcbiAgICAgICAgaWYgKGUub2Zmc2V0UGFyZW50KSB7XHJcbiAgICAgICAgICAgIHdoaWxlIChlLm9mZnNldFBhcmVudCkge1xyXG4gICAgICAgICAgICAgICAgdCArPSBlLm9mZnNldFRvcDtcclxuICAgICAgICAgICAgICAgIGUgPSBlLm9mZnNldFBhcmVudCBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0gZWxzZSBpZiAoZS5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKS55KSB7XHJcbiAgICAgICAgICAgIHQgKz0gZS5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKS55O1xyXG4gICAgICAgIH1cclxuICAgICAgICBsZXQgY3J1bWIgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI1RvcEJyZWFkY3J1bWJcIikgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgaWYgKGNydW1iKVxyXG4gICAgICAgICAgICB0IC09IGNydW1iLm9mZnNldEhlaWdodDtcclxuICAgICAgICBzY3JvbGxUbygwLCB0KTtcclxuICAgIH07XHJcblxyXG4gICAgc2Nyb2xsVG9Qb3N0RnJvbUhhc2ggPSAoKSA9PiB7XHJcbiAgICAgICAgaWYgKHdpbmRvdy5sb2NhdGlvbi5oYXNoKSB7XHJcbiAgICAgICAgICAgIFByb21pc2UuYWxsKEFycmF5LmZyb20oZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIiNQb3N0U3RyZWFtIGltZ1wiKSlcclxuICAgICAgICAgICAgICAgIC5maWx0ZXIoaW1nID0+ICEoaW1nIGFzIEhUTUxJbWFnZUVsZW1lbnQpLmNvbXBsZXRlKVxyXG4gICAgICAgICAgICAgICAgLm1hcChpbWcgPT4gbmV3IFByb21pc2UocmVzb2x2ZSA9PiB7IChpbWcgYXMgSFRNTEltYWdlRWxlbWVudCkub25sb2FkID0gKGltZyBhcyBIVE1MSW1hZ2VFbGVtZW50KS5vbmVycm9yID0gcmVzb2x2ZTsgfSkpKVxyXG4gICAgICAgICAgICAgICAgICAgIC50aGVuKCgpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbGV0IGhhc2ggPSB3aW5kb3cubG9jYXRpb24uaGFzaDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgd2hpbGUgKGhhc2guY2hhckF0KDApID09PSAnIycpIGhhc2ggPSBoYXNoLnN1YnN0cmluZygxKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbGV0IHRhZyA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCJkaXZbZGF0YS1wb3N0SUQ9J1wiICsgaGFzaCArIFwiJ11cIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGlmICh0YWcpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGxldCB0YWdQb3NpdGlvbiA9IHRhZy5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKS50b3A7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBsZXQgY3J1bWIgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI0ZvcnVtQ29udGFpbmVyICNUb3BCcmVhZGNydW1iXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGV0IGNydW1iSGVpZ2h0ID0gY3J1bWIuZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCkuaGVpZ2h0O1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGV0IGUgPSBnZXRDb21wdXRlZFN0eWxlKGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIucG9zdEl0ZW1cIikpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGV0IG1hcmdpbiA9IHBhcnNlRmxvYXQoZS5tYXJnaW5Ub3ApO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGV0IG5ld1Bvc2l0aW9uID0gdGFnUG9zaXRpb24gLSBjcnVtYkhlaWdodCAtIG1hcmdpbjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHdpbmRvdy5zY3JvbGxCeSh7IHRvcDogbmV3UG9zaXRpb24sIGJlaGF2aW9yOiAnYXV0bycgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5pc1Njcm9sbEFkanVzdGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcbiAgICB9O1xyXG5cclxuICAgIHNldEFuc3dlcihwb3N0SUQ6IG51bWJlciwgdG9waWNJRDogbnVtYmVyKSB7XHJcbiAgICAgICAgdmFyIG1vZGVsID0geyBwb3N0SUQ6IHBvc3RJRCwgdG9waWNJRDogdG9waWNJRCB9O1xyXG4gICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0ZvcnVtL1NldEFuc3dlci9cIiwge1xyXG4gICAgICAgICAgICBtZXRob2Q6IFwiUE9TVFwiLFxyXG4gICAgICAgICAgICBib2R5OiBKU09OLnN0cmluZ2lmeShtb2RlbCksXHJcbiAgICAgICAgICAgIGhlYWRlcnM6IHtcclxuICAgICAgICAgICAgICAgIFwiQ29udGVudC1UeXBlXCI6IFwiYXBwbGljYXRpb24vanNvblwiXHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9KVxyXG4gICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmFuc3dlclBvc3RJRCA9IHBvc3RJRDtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICB9XHJcbn1cclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbmV4cG9ydCBjbGFzcyBVc2VyU3RhdGUgZXh0ZW5kcyBTdGF0ZUJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgICAgICB0aGlzLmlzUGxhaW5UZXh0ID0gZmFsc2U7XHJcbiAgICAgICAgdGhpcy5uZXdQbUNvdW50ID0gMDtcclxuICAgICAgICB0aGlzLm5vdGlmaWNhdGlvblNlcnZpY2UgPSBuZXcgTm90aWZpY2F0aW9uU2VydmljZSh0aGlzKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIG5vdGlmaWNhdGlvblNlcnZpY2U6IE5vdGlmaWNhdGlvblNlcnZpY2U7XHJcbiAgICBcclxuICAgIGlzUGxhaW5UZXh0OiBib29sZWFuO1xyXG4gICAgaXNJbWFnZUVuYWJsZWQ6IGJvb2xlYW47XHJcblxyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuICAgIG5ld1BtQ291bnQ6IG51bWJlcjtcclxufVxyXG5cclxufSJdfQ==