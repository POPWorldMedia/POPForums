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
                // get this from topic state's callback/ready method, because iOS loses selection when you touch quote button
                let fragment = PopForums.currentTopicState.documentFragment;
                let ancestor = PopForums.currentTopicState.selectionAncestor;
                if (!fragment) {
                    let selection = document.getSelection();
                    if (!selection || selection.rangeCount === 0 || selection.getRangeAt(0).toString().length === 0) {
                        // prompt to select
                        this._tip = new bootstrap.Tooltip(button, { trigger: "manual" });
                        this._tip.show();
                        return;
                    }
                    let range = selection.getRangeAt(0);
                    ancestor = range.commonAncestorContainer;
                    fragment = range.cloneContents();
                }
                let div = document.createElement("div");
                div.appendChild(fragment);
                // is selection in the container?
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
                let temp = document.getSelection();
                if (temp)
                    temp.removeAllRanges();
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
                // compensate for iOS losing selection when you touch the quote button
                document.querySelectorAll(".postBody").forEach(x => x.addEventListener("touchend", (e) => {
                    let selection = document.getSelection();
                    if (!selection || selection.rangeCount === 0 || selection.getRangeAt(0).toString().length === 0) {
                        return;
                    }
                    let range = selection.getRangeAt(0);
                    this.selectionAncestor = range.commonAncestorContainer;
                    this.documentFragment = range.cloneContents();
                }));
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
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiUG9wRm9ydW1zLmpzIiwic291cmNlUm9vdCI6IiIsInNvdXJjZXMiOlsiQ2xpZW50L0RlY2xhcmF0aW9ucy50cyIsIkNsaWVudC9FbGVtZW50QmFzZS50cyIsIkNsaWVudC9TdGF0ZUJhc2UudHMiLCJDbGllbnQvV2F0Y2hQcm9wZXJ0eUF0dHJpYnV0ZS50cyIsIkNsaWVudC9Db21wb25lbnRzL0Fuc3dlckJ1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL0NvbW1lbnRCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9GYXZvcml0ZUJ1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL0ZlZWRVcGRhdGVyLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvRnVsbFRleHQudHMiLCJDbGllbnQvQ29tcG9uZW50cy9Ib21lVXBkYXRlci50cyIsIkNsaWVudC9Db21wb25lbnRzL0xvZ2luRm9ybS50cyIsIkNsaWVudC9Db21wb25lbnRzL01vcmVQb3N0c0JlZm9yZVJlcGx5QnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvTW9yZVBvc3RzQnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvUE1Db3VudC50cyIsIkNsaWVudC9Db21wb25lbnRzL1Bvc3RNaW5pUHJvZmlsZS50cyIsIkNsaWVudC9Db21wb25lbnRzL1Bvc3RNb2RlcmF0aW9uTG9nQnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvUHJldmlld0J1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL1ByZXZpb3VzUG9zdHNCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9RdW90ZUJ1dHRvbi50cyIsIkNsaWVudC9Db21wb25lbnRzL1JlcGx5QnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvUmVwbHlGb3JtLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvU3Vic2NyaWJlQnV0dG9uLnRzIiwiQ2xpZW50L0NvbXBvbmVudHMvVG9waWNCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9Ub3BpY0Zvcm0udHMiLCJDbGllbnQvQ29tcG9uZW50cy9Ub3BpY01vZGVyYXRpb25Mb2dCdXR0b24udHMiLCJDbGllbnQvQ29tcG9uZW50cy9Wb3RlQ291bnQudHMiLCJDbGllbnQvU2VydmljZXMvTm90aWZpY2F0aW9uU2VydmljZS50cyIsIkNsaWVudC9TZXJ2aWNlcy9UaW1lVXBkYXRlci50cyIsIkNsaWVudC9TdGF0ZS9Gb3J1bVN0YXRlLnRzIiwiQ2xpZW50L1N0YXRlL1RvcGljU3RhdGUudHMiLCJDbGllbnQvU3RhdGUvVXNlclN0YXRlLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7Ozs7OztBQUFBLElBQVUsU0FBUyxDQVVsQjtBQVZELFdBQVUsU0FBUztJQUNGLGtCQUFRLEdBQUcsU0FBUyxDQUFDO0lBS2xDLFNBQWdCLEtBQUssQ0FBQyxRQUFhO1FBQy9CLElBQUksUUFBUSxDQUFDLFVBQVUsSUFBSSxTQUFTO1lBQUUsUUFBUSxFQUFFLENBQUM7O1lBQzVDLFFBQVEsQ0FBQyxnQkFBZ0IsQ0FBQyxrQkFBa0IsRUFBRSxRQUFRLENBQUMsQ0FBQztJQUNqRSxDQUFDO0lBSGUsZUFBSyxRQUdwQixDQUFBO0FBQ0wsQ0FBQyxFQVZTLFNBQVMsS0FBVCxTQUFTLFFBVWxCO0FDVkQsSUFBVSxTQUFTLENBNkJsQjtBQTdCRCxXQUFVLFNBQVM7SUFFbkIsTUFBc0IsV0FBWSxTQUFRLFdBQVc7UUFFakQsaUJBQWlCO1lBQ2IsSUFBSSxJQUFJLENBQUMsS0FBSyxJQUFJLElBQUksQ0FBQyxlQUFlO2dCQUNsQyxPQUFPO1lBQ1gsSUFBSSxxQkFBcUIsR0FBRyxJQUFJLENBQUMscUJBQXFCLEVBQUUsQ0FBQztZQUN6RCxJQUFJLENBQUMsS0FBSyxHQUFHLHFCQUFxQixDQUFDLENBQUMsQ0FBQyxDQUFDO1lBQ3RDLElBQUksQ0FBQyxlQUFlLEdBQUcscUJBQXFCLENBQUMsQ0FBQyxDQUFDLENBQUM7WUFDaEQsTUFBTSxRQUFRLEdBQUcsSUFBSSxDQUFDLE1BQU0sQ0FBQyxJQUFJLENBQUMsSUFBSSxDQUFD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R0FBRyxJQUFJLFVBQUEsbUJBQW1CLENBQUMsSUFBSSxDQUFDLENBQUM7UUFDN0QsQ0FBQztLQVNKO0lBREc7UUFEQyxVQUFBLGFBQWE7aURBQ0s7SUFkVixtQkFBUyxZQWVyQixDQUFBO0FBRUQsQ0FBQyxFQW5CUyxTQUFTLEtBQVQsU0FBUyxRQW1CbEIiLCJzb3VyY2VzQ29udGVudCI6WyJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuICAgIGV4cG9ydCBjb25zdCBBcmVhUGF0aCA9IFwiL0ZvcnVtc1wiO1xyXG4gICAgZXhwb3J0IHZhciBjdXJyZW50VG9waWNTdGF0ZTogVG9waWNTdGF0ZTtcclxuICAgIGV4cG9ydCB2YXIgY3VycmVudEZvcnVtU3RhdGU6IEZvcnVtU3RhdGU7XHJcbiAgICBleHBvcnQgdmFyIHVzZXJTdGF0ZTogVXNlclN0YXRlO1xyXG5cclxuICAgIGV4cG9ydCBmdW5jdGlvbiBSZWFkeShjYWxsYmFjazogYW55KTogdm9pZCB7XHJcbiAgICAgICAgaWYgKGRvY3VtZW50LnJlYWR5U3RhdGUgIT0gXCJsb2FkaW5nXCIpIGNhbGxiYWNrKCk7XHJcbiAgICAgICAgZWxzZSBkb2N1bWVudC5hZGRFdmVudExpc3RlbmVyKFwiRE9NQ29udGVudExvYWRlZFwiLCBjYWxsYmFjayk7XHJcbiAgICB9XHJcbn1cclxuXHJcblxyXG5kZWNsYXJlIG5hbWVzcGFjZSB0aW55bWNlIHtcclxuICAgIGZ1bmN0aW9uIGluaXQob3B0aW9uczphbnkpOiBhbnk7XHJcbiAgICBmdW5jdGlvbiBnZXQoaWQ6c3RyaW5nKTogYW55O1xyXG4gICAgZnVuY3Rpb24gdHJpZ2dlclNhdmUoKTogYW55O1xyXG59XHJcblxyXG5kZWNsYXJlIG5hbWVzcGFjZSBib290c3RyYXAge1xyXG4gICAgY2xhc3MgVG9vbHRpcCB7XHJcbiAgICAgICAgY29uc3RydWN0b3IoZWw6IEVsZW1lbnQsIG9wdGlvbnM6YW55KTtcclxuICAgIH1cclxuICAgIGNsYXNzIFBvcG92ZXIge1xyXG4gICAgICAgIGNvbnN0cnVjdG9yKGVsOiBFbGVtZW50LCBvcHRpb25zOmFueSk7XHJcbiAgICAgICAgZW5hYmxlKCk6IHZvaWQ7XHJcbiAgICAgICAgZGlzYWJsZSgpOiB2b2lkO1xyXG4gICAgfVxyXG59XHJcblxyXG5kZWNsYXJlIG5hbWVzcGFjZSBzaWduYWxSIHtcclxuICAgIGNsYXNzIEh1YkNvbm5lY3Rpb25CdWlsZGVyIHtcclxuICAgICAgICB3aXRoVXJsKHVybDogc3RyaW5nKTogYW55O1xyXG4gICAgfVxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG5leHBvcnQgYWJzdHJhY3QgY2xhc3MgRWxlbWVudEJhc2UgZXh0ZW5kcyBIVE1MRWxlbWVudCB7XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgaWYgKHRoaXMuc3RhdGUgJiYgdGhpcy5wcm9wZXJ0eVRvV2F0Y2gpXHJcbiAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICBsZXQgc3RhdGVBbmRXYXRjaFByb3BlcnR5ID0gdGhpcy5nZXREZXBlbmRlbnRSZWZlcmVuY2UoKTtcclxuICAgICAgICB0aGlzLnN0YXRlID0gc3RhdGVBbmRXYXRjaFByb3BlcnR5WzBdO1xyXG4gICAgICAgIHRoaXMucHJvcGVydHlUb1dhdGNoID0gc3RhdGVBbmRXYXRjaFByb3BlcnR5WzFdO1xyXG4gICAgICAgIGNvbnN0IGRlbGVnYXRlID0gdGhpcy51cGRhdGUuYmluZCh0aGlzKTtcclxuICAgICAgICB0aGlzLnN0YXRlLnN1YnNjcmliZSh0aGlzLnByb3BlcnR5VG9XYXRjaCwgZGVsZWdhdGUpO1xyXG4gICAgfVxyXG5cclxuICAgIHByaXZhdGUgc3RhdGU6IFN0YXRlQmFzZTtcclxuICAgIHByaXZhdGUgcHJvcGVydHlUb1dhdGNoOiBzdHJpbmc7XHJcblxyXG4gICAgdXBkYXRlKCkge1xyXG4gICAgICAgIGNvbnN0IGV4dGVybmFsVmFsdWUgPSB0aGlzLnN0YXRlW3RoaXMucHJvcGVydHlUb1dhdGNoXTtcclxuICAgICAgICB0aGlzLnVwZGF0ZVVJKGV4dGVybmFsVmFsdWUpO1xyXG4gICAgfVxyXG5cclxuICAgIC8vIEltcGxlbWVudGF0aW9uIHNob3VsZCByZXR1cm4gdGhlIFN0YXRlQmFzZSBhbmQgcHJvcGVydHkgKGFzIGEgc3RyaW5nKSB0byBtb25pdG9yXHJcbiAgICBhYnN0cmFjdCBnZXREZXBlbmRlbnRSZWZlcmVuY2UoKTogW1N0YXRlQmFzZSwgc3RyaW5nXTtcclxuXHJcbiAgICAvLyBVc2UgaW4gdGhlIGltcGxlbWVudGF0aW9uIHRvIG1hbmlwdWxhdGUgdGhlIHNoYWRvdyBvciBsaWdodCBET00gb3Igc3RyYWlnaHQgbWFya3VwIGFzIG5lZWRlZCBpbiByZXNwb25zZSB0byB0aGUgbmV3IGRhdGEuXHJcbiAgICBhYnN0cmFjdCB1cGRhdGVVSShkYXRhOiBhbnkpOiB2b2lkO1xyXG59XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4vLyBQcm9wZXJ0aWVzIHRvIHdhdGNoIHJlcXVpcmUgdGhlIEBXYXRjaFByb3BlcnR5IGF0dHJpYnV0ZS5cclxuZXhwb3J0IGNsYXNzIFN0YXRlQmFzZSB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICB0aGlzLl9zdWJzID0gbmV3IE1hcDxzdHJpbmcsIEFycmF5PEZ1bmN0aW9uPj4oKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIF9zdWJzOiBNYXA8c3RyaW5nLCBBcnJheTxGdW5jdGlvbj4+O1xyXG5cclxuICAgIHN1YnNjcmliZShwcm9wZXJ0eU5hbWU6IHN0cmluZywgZXZlbnRIYW5kbGVyOiBGdW5jdGlvbikge1xyXG4gICAgICAgIGlmICghdGhpcy5fc3Vicy5oYXMocHJvcGVydHlOYW1lKSlcclxuICAgICAgICAgICAgdGhpcy5fc3Vicy5zZXQocHJvcGVydHlOYW1lLCBuZXcgQXJyYXk8RnVuY3Rpb24+KCkpO1xyXG4gICAgICAgIGNvbnN0IGNhbGxiYWNrcyA9IHRoaXMuX3N1YnMuZ2V0KHByb3BlcnR5TmFtZSk7XHJcbiAgICAgICAgY2FsbGJhY2tzLnB1c2goZXZlbnRIYW5kbGVyKTtcclxuICAgICAgICBldmVudEhhbmRsZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBub3RpZnkocHJvcGVydHlOYW1lOiBzdHJpbmcpIHtcclxuICAgICAgICBjb25zdCBjYWxsYmFja3MgPSB0aGlzLl9zdWJzLmdldChwcm9wZXJ0eU5hbWUpO1xyXG4gICAgICAgIGlmIChjYWxsYmFja3MpXHJcbiAgICAgICAgICAgIGZvciAobGV0IGkgb2YgY2FsbGJhY2tzKSB7XHJcbiAgICAgICAgICAgICAgICBpKCk7XHJcbiAgICAgICAgICAgIH1cclxuICAgIH1cclxufVxyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuZXhwb3J0IGNvbnN0IFdhdGNoUHJvcGVydHkgPSAodGFyZ2V0OiBhbnksIG1lbWJlck5hbWU6IHN0cmluZykgPT4ge1xyXG4gICAgbGV0IGN1cnJlbnRWYWx1ZTogYW55ID0gdGFyZ2V0W21lbWJlck5hbWVdOyAgXHJcbiAgICBPYmplY3QuZGVmaW5lUHJvcGVydHkodGFyZ2V0LCBtZW1iZXJOYW1lLCB7XHJcbiAgICAgICAgc2V0KHRoaXM6IGFueSwgbmV3VmFsdWU6IGFueSkge1xyXG4gICAgICAgICAgICBjdXJyZW50VmFsdWUgPSBuZXdWYWx1ZTtcclxuICAgICAgICAgICAgdGhpcy5ub3RpZnkobWVtYmVyTmFtZSk7XHJcbiAgICAgICAgfSxcclxuICAgICAgICBnZXQoKSB7cmV0dXJuIGN1cnJlbnRWYWx1ZTt9XHJcbiAgICB9KTtcclxufTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgQW5zd2VyQnV0dG9uIGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgICAgICBzdXBlcigpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZ2V0IGFuc3dlcnN0YXR1c2NsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImFuc3dlcnN0YXR1c2NsYXNzXCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBnZXQgY2hvb3NlYW5zd2VydGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJjaG9vc2VhbnN3ZXJ0ZXh0XCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBnZXQgdG9waWNpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0b3BpY2lkXCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBnZXQgcG9zdGlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInBvc3RpZFwiKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZ2V0IGFuc3dlcnBvc3RpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJhbnN3ZXJwb3N0aWRcIik7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGdldCB1c2VyaWQoKTogc3RyaW5nIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidXNlcmlkXCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBnZXQgc3RhcnRlZGJ5dXNlcmlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInN0YXJ0ZWRieXVzZXJpZFwiKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZ2V0IGlzZmlyc3RpbnRvcGljKCk6IHN0cmluZyB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImlzZmlyc3RpbnRvcGljXCIpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHJpdmF0ZSBidXR0b246IEhUTUxFbGVtZW50O1xyXG5cclxuICAgICAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICAgICAgdGhpcy5idXR0b24gPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwicFwiKTtcclxuICAgICAgICAgICAgdGhpcy5hbnN3ZXJzdGF0dXNjbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gdGhpcy5idXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmlzZmlyc3RpbnRvcGljLnRvTG93ZXJDYXNlKCkgPT09IFwiZmFsc2VcIiAmJiB0aGlzLnVzZXJpZCA9PT0gdGhpcy5zdGFydGVkYnl1c2VyaWQpIHtcclxuICAgICAgICAgICAgICAgIC8vIG1ha2UgaXQgYSBidXR0b24gZm9yIGF1dGhvclxyXG4gICAgICAgICAgICAgICAgdGhpcy5idXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUuc2V0QW5zd2VyKE51bWJlcih0aGlzLnBvc3RpZCksIE51bWJlcih0aGlzLnRvcGljaWQpKTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHRoaXMuYXBwZW5kQ2hpbGQodGhpcy5idXR0b24pO1xyXG4gICAgICAgICAgICBzdXBlci5jb25uZWN0ZWRDYWxsYmFjaygpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZSwgXCJhbnN3ZXJQb3N0SURcIl07XHJcbiAgICAgICAgfVxyXG4gICAgICAgIFxyXG4gICAgICAgIHVwZGF0ZVVJKGFuc3dlclBvc3RJRDogbnVtYmVyKTogdm9pZCB7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmlzZmlyc3RpbnRvcGljLnRvTG93ZXJDYXNlKCkgPT09IFwiZmFsc2VcIiAmJiB0aGlzLnVzZXJpZCA9PT0gdGhpcy5zdGFydGVkYnl1c2VyaWQpIHtcclxuICAgICAgICAgICAgICAgIC8vIHRoaXMgaXMgcXVlc3Rpb24gYXV0aG9yXHJcbiAgICAgICAgICAgICAgICB0aGlzLmJ1dHRvbi5jbGFzc0xpc3QuYWRkKFwiYXNuc3dlckJ1dHRvblwiKTtcclxuICAgICAgICAgICAgICAgIGlmIChhbnN3ZXJQb3N0SUQgJiYgdGhpcy5wb3N0aWQgPT09IGFuc3dlclBvc3RJRC50b1N0cmluZygpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LnJlbW92ZShcImljb24tY2hlY2ttYXJrMlwiKTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmJ1dHRvbi5jbGFzc0xpc3QucmVtb3ZlKFwidGV4dC1tdXRlZFwiKTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmJ1dHRvbi5jbGFzc0xpc3QuYWRkKFwiaWNvbi1jaGVja21hcmtcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LmFkZChcInRleHQtc3VjY2Vzc1wiKTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLnN0eWxlLmN1cnNvciA9IFwiZGVmYXVsdFwiO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LnJlbW92ZShcImljb24tY2hlY2ttYXJrXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuYnV0dG9uLmNsYXNzTGlzdC5yZW1vdmUoXCJ0ZXh0LXN1Y2Nlc3NcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LmFkZChcImljb24tY2hlY2ttYXJrMlwiKTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmJ1dHRvbi5jbGFzc0xpc3QuYWRkKFwidGV4dC1tdXRlZFwiKTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLnN0eWxlLmN1cnNvciA9IFwicG9pbnRlclwiO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGVsc2UgaWYgKGFuc3dlclBvc3RJRCAmJiB0aGlzLnBvc3RpZCA9PT0gYW5zd2VyUG9zdElELnRvU3RyaW5nKCkpIHtcclxuICAgICAgICAgICAgICAgIC8vIG5vdCB0aGUgcXVlc3Rpb24gYXV0aG9yLCBidXQgaXQgaXMgdGhlIGFuc3dlclxyXG4gICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LmFkZChcImljb24tY2hlY2ttYXJrXCIpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5idXR0b24uY2xhc3NMaXN0LmFkZChcInRleHQtc3VjY2Vzc1wiKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuc3R5bGUuY3Vyc29yID0gXCJkZWZhdWx0XCI7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtYW5zd2VyYnV0dG9uJywgQW5zd2VyQnV0dG9uKTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgQ29tbWVudEJ1dHRvbiBleHRlbmRzIEVsZW1lbnRCYXNlIHtcclxuICAgICAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICAgICAgc3VwZXIoKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldCBidXR0b25jbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldCBidXR0b250ZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbnRleHRcIik7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIFxyXG4gICAgICAgIGdldCB0b3BpY2lkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInRvcGljaWRcIik7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIFxyXG4gICAgICAgIGdldCBwb3N0aWQoKTogc3RyaW5nIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwicG9zdGlkXCIpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgICAgIHRoaXMuaW5uZXJIVE1MID0gQ29tbWVudEJ1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQ7XHJcbiAgICAgICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuYnV0dG9udGV4dDtcclxuICAgICAgICAgICAgaWYgKHRoaXMuYnV0dG9uY2xhc3M/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgICAgICB0aGlzLmJ1dHRvbmNsYXNzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiBidXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKGU6IE1vdXNlRXZlbnQpID0+IHtcclxuICAgICAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5sb2FkQ29tbWVudChOdW1iZXIodGhpcy50b3BpY2lkKSwgTnVtYmVyKHRoaXMucG9zdGlkKSk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICBzdXBlci5jb25uZWN0ZWRDYWxsYmFjaygpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZSwgXCJjb21tZW50UmVwbHlJRFwiXTtcclxuICAgICAgICB9XHJcbiAgICAgICAgXHJcbiAgICAgICAgdXBkYXRlVUkoZGF0YTogbnVtYmVyKTogdm9pZCB7XHJcbiAgICAgICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICAgICAgaWYgKGRhdGEgIT09IHVuZGVmaW5lZCkge1xyXG4gICAgICAgICAgICAgICAgYnV0dG9uLmRpc2FibGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgIGJ1dHRvbi5zdHlsZS5jdXJzb3IgPSBcImRlZmF1bHRcIjtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgICAgICBidXR0b24uZGlzYWJsZWQgPSBmYWxzZTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgLz5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLWNvbW1lbnRidXR0b24nLCBDb21tZW50QnV0dG9uKTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgRmF2b3JpdGVCdXR0b24gZXh0ZW5kcyBFbGVtZW50QmFzZSB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBidXR0b25jbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbmNsYXNzXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBtYWtlZmF2b3JpdGV0ZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwibWFrZWZhdm9yaXRldGV4dFwiKTtcclxuICAgIH1cclxuICAgIGdldCByZW1vdmVmYXZvcml0ZXRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJyZW1vdmVmYXZvcml0ZXRleHRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBTdWJzY3JpYmVCdXR0b24udGVtcGxhdGU7XHJcbiAgICAgICAgbGV0IGJ1dHRvbjogSFRNTElucHV0RWxlbWVudCA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpO1xyXG4gICAgICAgIHRoaXMuYnV0dG9uY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IGJ1dHRvbi5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICBidXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IHtcclxuICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRmF2b3JpdGVzL1RvZ2dsZUZhdm9yaXRlL1wiICsgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLnRvcGljSUQsIHtcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCJcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLmpzb24oKSlcclxuICAgICAgICAgICAgICAgIC50aGVuKHJlc3VsdCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgc3dpdGNoIChyZXN1bHQuZGF0YS5pc0Zhdm9yaXRlKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGNhc2UgdHJ1ZTpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5pc0Zhdm9yaXRlID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjYXNlIGZhbHNlOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLmlzRmF2b3JpdGUgPSBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkZWZhdWx0OlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gVE9ETzogc29tZXRoaW5nIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICAgICAgLmNhdGNoKCgpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAvLyBUT0RPOiBoYW5kbGUgZXJyb3JcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0pO1xyXG4gICAgICAgIHN1cGVyLmNvbm5lY3RlZENhbGxiYWNrKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgIHJldHVybiBbUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLCBcImlzRmF2b3JpdGVcIl07XHJcbiAgICB9XHJcblxyXG4gICAgdXBkYXRlVUkoZGF0YTogYm9vbGVhbik6IHZvaWQge1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICBpZiAoZGF0YSlcclxuICAgICAgICAgICAgYnV0dG9uLnZhbHVlID0gdGhpcy5yZW1vdmVmYXZvcml0ZXRleHQ7XHJcbiAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLm1ha2VmYXZvcml0ZXRleHQ7XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGlucHV0IHR5cGU9XCJidXR0b25cIiAvPmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtZmF2b3JpdGVidXR0b24nLCBGYXZvcml0ZUJ1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIEZlZWRVcGRhdGVyIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgICAgICBzdXBlcigpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZ2V0IHRlbXBsYXRlaWQoKSB7XHJcbiAgICAgICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInRlbXBsYXRlaWRcIik7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICAgICAgbGV0IGNvbm5lY3Rpb24gPSBuZXcgc2lnbmFsUi5IdWJDb25uZWN0aW9uQnVpbGRlcigpLndpdGhVcmwoXCIvRmVlZEh1YlwiKS53aXRoQXV0b21hdGljUmVjb25uZWN0KCkuYnVpbGQoKTtcclxuICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICBjb25uZWN0aW9uLm9uKFwibm90aWZ5RmVlZFwiLCBmdW5jdGlvbiAoZGF0YTogYW55KSB7XHJcbiAgICAgICAgICAgICAgICBsZXQgbGlzdCA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjRmVlZExpc3RcIik7XHJcbiAgICAgICAgICAgICAgICBsZXQgcm93ID0gc2VsZi5wb3B1bGF0ZUZlZWRSb3coZGF0YSk7XHJcbiAgICAgICAgICAgICAgICBsaXN0LnByZXBlbmQocm93KTtcclxuICAgICAgICAgICAgICAgIHJvdy5jbGFzc0xpc3QucmVtb3ZlKFwiaGlkZGVuXCIpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgY29ubmVjdGlvbi5zdGFydCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbm5lY3Rpb24uaW52b2tlKFwibGlzdGVuVG9BbGxcIik7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHBvcHVsYXRlRmVlZFJvdyhkYXRhOiBhbnkpIHtcclxuICAgICAgICAgICAgbGV0IHRlbXBsYXRlID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQodGhpcy50ZW1wbGF0ZWlkKTtcclxuICAgICAgICAgICAgaWYgKCF0ZW1wbGF0ZSkge1xyXG4gICAgICAgICAgICAgICAgY29uc29sZS5lcnJvcihgQ2FuJ3QgZmluZCBJRCAke3RoaXMudGVtcGxhdGVpZH0gdG8gbWFrZSBmZWVkIHVwZGF0ZXMuYCk7XHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgbGV0IHJvdyA9IHRlbXBsYXRlLmNsb25lTm9kZSh0cnVlKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgcm93LnJlbW92ZUF0dHJpYnV0ZShcImlkXCIpO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5mZWVkSXRlbVRleHRcIikuaW5uZXJIVE1MID0gZGF0YS5tZXNzYWdlO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5mVGltZVwiKS5zZXRBdHRyaWJ1dGUoXCJkYXRhLXV0Y1wiLCBkYXRhLnV0Yyk7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLmZUaW1lXCIpLmlubmVySFRNTCA9IGRhdGEudGltZVN0YW1wO1xyXG4gICAgICAgICAgICByZXR1cm4gcm93O1xyXG4gICAgICAgIH07XHJcbiAgICB9XHJcbiAgICBcclxuICAgIGN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtZmVlZHVwZGF0ZXInLCBGZWVkVXBkYXRlcik7XHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgRnVsbFRleHQgZXh0ZW5kcyBFbGVtZW50QmFzZSB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBvdmVycmlkZWxpc3RlbmVyKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwib3ZlcnJpZGVsaXN0ZW5lclwiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgZm9ybUlEKCkgeyByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJmb3JtaWRcIikgfTtcclxuXHJcbiAgICBnZXQgdmFsdWUoKSB7IHJldHVybiB0aGlzLl92YWx1ZTt9XHJcbiAgICBzZXQgdmFsdWUodjogc3RyaW5nKSB7IHRoaXMuX3ZhbHVlID0gdjsgfVxyXG5cclxuICAgIF92YWx1ZTogc3RyaW5nO1xyXG5cclxuICAgIHN0YXRpYyBmb3JtQXNzb2NpYXRlZCA9IHRydWU7XHJcblxyXG4gICAgcHJpdmF0ZSB0ZXh0Qm94OiBIVE1MRWxlbWVudDtcclxuICAgIHByaXZhdGUgZXh0ZXJuYWxGb3JtRWxlbWVudDogSFRNTEVsZW1lbnQ7XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdmFyIGluaXRpYWxWYWx1ZSA9IHRoaXMuZ2V0QXR0cmlidXRlKFwidmFsdWVcIik7XHJcbiAgICAgICAgaWYgKGluaXRpYWxWYWx1ZSlcclxuICAgICAgICAgICAgdGhpcy52YWx1ZSA9IGluaXRpYWxWYWx1ZTtcclxuICAgICAgICBpZiAodXNlclN0YXRlLmlzUGxhaW5UZXh0KSB7XHJcbiAgICAgICAgICAgIHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ0ZXh0YXJlYVwiKTtcclxuICAgICAgICAgICAgdGhpcy5leHRlcm5hbEZvcm1FbGVtZW50LmlkID0gdGhpcy5mb3JtSUQ7XHJcbiAgICAgICAgICAgIHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudC5zZXRBdHRyaWJ1dGUoXCJuYW1lXCIsIHRoaXMuZm9ybUlEKTtcclxuICAgICAgICAgICAgdGhpcy5leHRlcm5hbEZvcm1FbGVtZW50LmNsYXNzTGlzdC5hZGQoXCJmb3JtLWNvbnRyb2xcIik7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLnZhbHVlKVxyXG4gICAgICAgICAgICAodGhpcy5leHRlcm5hbEZvcm1FbGVtZW50IGFzIEhUTUxUZXh0QXJlYUVsZW1lbnQpLnZhbHVlID0gdGhpcy52YWx1ZTtcclxuICAgICAgICAgICAgKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBIVE1MVGV4dEFyZWFFbGVtZW50KS5yb3dzID0gMTI7XHJcbiAgICAgICAgICAgIGxldCBzZWxmID0gdGhpcztcclxuICAgICAgICAgICAgdGhpcy5leHRlcm5hbEZvcm1FbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJjaGFuZ2VcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgc2VsZi52YWx1ZSA9ICh0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQgYXMgSFRNTFRleHRBcmVhRWxlbWVudCkudmFsdWU7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICB0aGlzLmFwcGVuZENoaWxkKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCk7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLm92ZXJyaWRlbGlzdGVuZXI/LnRvTG93ZXJDYXNlKCkgIT09IFwidHJ1ZVwiKVxyXG4gICAgICAgICAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuICAgICAgICBsZXQgdGVtcGxhdGUgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwidGVtcGxhdGVcIik7XHJcbiAgICAgICAgdGVtcGxhdGUuaW5uZXJIVE1MID0gRnVsbFRleHQudGVtcGxhdGU7XHJcbiAgICAgICAgdGhpcy5hdHRhY2hTaGFkb3coeyBtb2RlOiBcIm9wZW5cIiB9KTtcclxuICAgICAgICB0aGlzLnNoYWRvd1Jvb3QuYXBwZW5kKHRlbXBsYXRlLmNvbnRlbnQuY2xvbmVOb2RlKHRydWUpKTtcclxuICAgICAgICB0aGlzLnRleHRCb3ggPSB0aGlzLnNoYWRvd1Jvb3QucXVlcnlTZWxlY3RvcihcIiNlZGl0b3JcIik7XHJcbiAgICAgICAgaWYgKHRoaXMudmFsdWUpXHJcbiAgICAgICAgICAgICh0aGlzLnRleHRCb3ggYXMgSFRNTFRleHRBcmVhRWxlbWVudCkuaW5uZXJUZXh0ID0gdGhpcy52YWx1ZTtcclxuICAgICAgICB0aGlzLmVkaXRvclNldHRpbmdzLnRhcmdldCA9IHRoaXMudGV4dEJveDtcclxuICAgICAgICBpZiAoIXVzZXJTdGF0ZS5pc0ltYWdlRW5hYmxlZClcclxuICAgICAgICAgICAgdGhpcy5lZGl0b3JTZXR0aW5ncy50b29sYmFyID0gRnVsbFRleHQucG9zdE5vSW1hZ2VUb29sYmFyO1xyXG4gICAgICAgIHZhciBzZWxmID0gdGhpcztcclxuICAgICAgICB0aGlzLmVkaXRvclNldHRpbmdzLnNldHVwID0gZnVuY3Rpb24gKGVkaXRvcjogYW55KSB7XHJcbiAgICAgICAgICAgIGVkaXRvci5vbihcImluaXRcIiwgZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgIHRoaXMub24oXCJmb2N1c291dFwiLCBmdW5jdGlvbihlOiBhbnkpIHtcclxuICAgICAgICAgICAgICAgIGVkaXRvci5zYXZlKCk7XHJcbiAgICAgICAgICAgICAgICBzZWxmLnZhbHVlID0gKHNlbGYudGV4dEJveCBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZTtcclxuICAgICAgICAgICAgICAgIChzZWxmLmV4dGVybmFsRm9ybUVsZW1lbnQgYXMgYW55KS52YWx1ZSA9IHNlbGYudmFsdWU7XHJcbiAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgdGhpcy5vbihcImJsdXJcIiwgZnVuY3Rpb24oZTogYW55KSB7XHJcbiAgICAgICAgICAgICAgICBlZGl0b3Iuc2F2ZSgpO1xyXG4gICAgICAgICAgICAgICAgc2VsZi52YWx1ZSA9IChzZWxmLnRleHRCb3ggYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWU7XHJcbiAgICAgICAgICAgICAgICAoc2VsZi5leHRlcm5hbEZvcm1FbGVtZW50IGFzIGFueSkudmFsdWUgPSBzZWxmLnZhbHVlO1xyXG4gICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICAgIGVkaXRvci5zYXZlKCk7XHJcbiAgICAgICAgICAgICAgc2VsZi52YWx1ZSA9IChzZWxmLnRleHRCb3ggYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWU7XHJcbiAgICAgICAgICAgICAgKHNlbGYuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBhbnkpLnZhbHVlID0gc2VsZi52YWx1ZTtcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICB9O1xyXG4gICAgICAgIHRpbnltY2UuaW5pdCh0aGlzLmVkaXRvclNldHRpbmdzKTtcclxuICAgICAgICB0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwiaW5wdXRcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICB0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQuaWQgPSB0aGlzLmZvcm1JRDtcclxuICAgICAgICB0aGlzLmV4dGVybmFsRm9ybUVsZW1lbnQuc2V0QXR0cmlidXRlKFwibmFtZVwiLCB0aGlzLmZvcm1JRCk7XHJcbiAgICAgICAgKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBIVE1MSW5wdXRFbGVtZW50KS50eXBlID0gXCJoaWRkZW5cIjtcclxuICAgICAgICB0aGlzLmFwcGVuZENoaWxkKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCk7XHJcbiAgICAgICAgaWYgKHRoaXMub3ZlcnJpZGVsaXN0ZW5lcj8udG9Mb3dlckNhc2UoKSAhPT0gXCJ0cnVlXCIpXHJcbiAgICAgICAgICAgIHN1cGVyLmNvbm5lY3RlZENhbGxiYWNrKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgIHJldHVybiBbUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLCBcIm5leHRRdW90ZVwiXTtcclxuICAgIH1cclxuXHJcbiAgICB1cGRhdGVVSShkYXRhOiBhbnkpOiB2b2lkIHtcclxuICAgICAgICBpZiAoZGF0YSAhPT0gbnVsbCAmJiBkYXRhICE9PSB1bmRlZmluZWQpXHJcbiAgICAgICAge1xyXG4gICAgICAgICAgICBpZiAodXNlclN0YXRlLmlzUGxhaW5UZXh0KSB7XHJcbiAgICAgICAgICAgICAgICAodGhpcy5leHRlcm5hbEZvcm1FbGVtZW50IGFzIEhUTUxUZXh0QXJlYUVsZW1lbnQpLnZhbHVlICs9IGRhdGE7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnZhbHVlID0gKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBIVE1MVGV4dEFyZWFFbGVtZW50KS52YWx1ZTtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICAgICAgIGxldCBlZGl0b3IgPSB0aW55bWNlLmdldChcImVkaXRvclwiKTtcclxuICAgICAgICAgICAgICAgIHZhciBjb250ZW50ID0gZWRpdG9yLmdldENvbnRlbnQoKTtcclxuICAgICAgICAgICAgICAgIGNvbnRlbnQgKz0gZGF0YTtcclxuICAgICAgICAgICAgICAgIGVkaXRvci5zZXRDb250ZW50KGNvbnRlbnQpO1xyXG4gICAgICAgICAgICAgICAgKHRoaXMudGV4dEJveCBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZSArPSBjb250ZW50O1xyXG4gICAgICAgICAgICAgICAgZWRpdG9yLnNhdmUoKTtcclxuICAgICAgICAgICAgICAgIHRoaXMudmFsdWUgPSAodGhpcy50ZXh0Qm94IGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlO1xyXG4gICAgICAgICAgICAgICAgKHRoaXMuZXh0ZXJuYWxGb3JtRWxlbWVudCBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZSA9IHRoaXMudmFsdWU7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgXHJcbiAgICBwcml2YXRlIHN0YXRpYyBlZGl0b3JDU1MgPSBcIi9saWIvYm9vdHN0cmFwL2Rpc3QvY3NzL2Jvb3RzdHJhcC5taW4uY3NzLC9saWIvUG9wRm9ydW1zL2Rpc3QvRWRpdG9yLm1pbi5jc3NcIjtcclxuICAgIHByaXZhdGUgc3RhdGljIHBvc3ROb0ltYWdlVG9vbGJhciA9IFwiY3V0IGNvcHkgcGFzdGUgfCBib2xkIGl0YWxpYyB8IGJ1bGxpc3QgbnVtbGlzdCBibG9ja3F1b3RlIHJlbW92ZWZvcm1hdCB8IGxpbmtcIjtcclxuICAgIGVkaXRvclNldHRpbmdzID0ge1xyXG4gICAgICAgIHRhcmdldDogbnVsbCBhcyBIVE1MRWxlbWVudCxcclxuICAgICAgICBwbHVnaW5zOiBcImxpc3RzIGltYWdlIGxpbmtcIixcclxuICAgICAgICBjb250ZW50X2NzczogRnVsbFRleHQuZWRpdG9yQ1NTLFxyXG4gICAgICAgIG1lbnViYXI6IGZhbHNlLFxyXG4gICAgICAgIHRvb2xiYXI6IFwiY3V0IGNvcHkgcGFzdGUgfCBib2xkIGl0YWxpYyB8IGJ1bGxpc3QgbnVtbGlzdCBibG9ja3F1b3RlIHJlbW92ZWZvcm1hdCB8IGxpbmsgfCBpbWFnZVwiLFxyXG4gICAgICAgIHN0YXR1c2JhcjogZmFsc2UsXHJcbiAgICAgICAgbGlua190YXJnZXRfbGlzdDogZmFsc2UsXHJcbiAgICAgICAgbGlua190aXRsZTogZmFsc2UsXHJcbiAgICAgICAgaW1hZ2VfZGVzY3JpcHRpb246IGZhbHNlLFxyXG4gICAgICAgIGltYWdlX2RpbWVuc2lvbnM6IGZhbHNlLFxyXG4gICAgICAgIGltYWdlX3RpdGxlOiBmYWxzZSxcclxuICAgICAgICBpbWFnZV91cGxvYWR0YWI6IGZhbHNlLFxyXG4gICAgICAgIGltYWdlc19maWxlX3R5cGVzOiAnanBlZyxqcGcscG5nLGdpZicsXHJcbiAgICAgICAgYXV0b21hdGljX3VwbG9hZHM6IGZhbHNlLFxyXG4gICAgICAgIGJyb3dzZXJfc3BlbGxjaGVjayA6IHRydWUsXHJcbiAgICAgICAgb2JqZWN0X3Jlc2l6aW5nOiBmYWxzZSxcclxuICAgICAgICByZWxhdGl2ZV91cmxzOiBmYWxzZSxcclxuICAgICAgICByZW1vdmVfc2NyaXB0X2hvc3Q6IGZhbHNlLFxyXG4gICAgICAgIGNvbnRleHRtZW51OiBcIlwiLFxyXG4gICAgICAgIHBhc3RlX2FzX3RleHQ6IHRydWUsXHJcbiAgICAgICAgcGFzdGVfZGF0YV9pbWFnZXM6IGZhbHNlLFxyXG4gICAgICAgIHNldHVwOiBudWxsIGFzIEZ1bmN0aW9uXHJcbiAgICB9O1xyXG5cclxuICAgIHN0YXRpYyBpZDogc3RyaW5nID0gXCJGdWxsVGV4dFwiO1xyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPHRleHRhcmVhIGlkPVwiZWRpdG9yXCI+PC90ZXh0YXJlYT5cclxuICAgIGA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtZnVsbHRleHQnLCBGdWxsVGV4dCk7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIEhvbWVVcGRhdGVyIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgICAgICBzdXBlcigpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgICAgIGxldCBjb25uZWN0aW9uID0gbmV3IHNpZ25hbFIuSHViQ29ubmVjdGlvbkJ1aWxkZXIoKS53aXRoVXJsKFwiL0ZvcnVtc0h1YlwiKS53aXRoQXV0b21hdGljUmVjb25uZWN0KCkuYnVpbGQoKTtcclxuICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICBjb25uZWN0aW9uLm9uKFwibm90aWZ5Rm9ydW1VcGRhdGVcIiwgZnVuY3Rpb24gKGRhdGE6IGFueSkge1xyXG4gICAgICAgICAgICAgICAgc2VsZi51cGRhdGVGb3J1bVN0YXRzKGRhdGEpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgY29ubmVjdGlvbi5zdGFydCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbm5lY3Rpb24uaW52b2tlKFwibGlzdGVuVG9BbGxcIik7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHVwZGF0ZUZvcnVtU3RhdHMoZGF0YTogYW55KSB7XHJcbiAgICAgICAgICAgIGxldCByb3cgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiW2RhdGEtZm9ydW1pZD0nXCIgKyBkYXRhLmZvcnVtSUQgKyBcIiddXCIpO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi50b3BpY0NvdW50XCIpLmlubmVySFRNTCA9IGRhdGEudG9waWNDb3VudDtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIucG9zdENvdW50XCIpLmlubmVySFRNTCA9IGRhdGEucG9zdENvdW50O1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5sYXN0UG9zdFRpbWVcIikuaW5uZXJIVE1MID0gZGF0YS5sYXN0UG9zdFRpbWU7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLmxhc3RQb3N0TmFtZVwiKS5pbm5lckhUTUwgPSBkYXRhLmxhc3RQb3N0TmFtZTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIuZlRpbWVcIikuc2V0QXR0cmlidXRlKFwiZGF0YS11dGNcIiwgZGF0YS51dGMpO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5uZXdJbmRpY2F0b3IgLmljb24tZmlsZS10ZXh0MlwiKS5jbGFzc0xpc3QucmVtb3ZlKFwidGV4dC1tdXRlZFwiKTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIubmV3SW5kaWNhdG9yIC5pY29uLWZpbGUtdGV4dDJcIikuY2xhc3NMaXN0LmFkZChcInRleHQtd2FybmluZ1wiKTtcclxuICAgICAgICB9O1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICBjdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLWhvbWV1cGRhdGVyJywgSG9tZVVwZGF0ZXIpO1xyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIExvZ2luRm9ybSBleHRlbmRzIEhUTUxFbGVtZW50IHtcclxuICAgICAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICAgICAgc3VwZXIoKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldCB0ZW1wbGF0ZWlkKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0ZW1wbGF0ZWlkXCIpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBnZXQgaXNFeHRlcm5hbExvZ2luKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJpc2V4dGVybmFsbG9naW5cIik7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBwcml2YXRlIGJ1dHRvbjogSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICBwcml2YXRlIGVtYWlsOiBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgIHByaXZhdGUgcGFzc3dvcmQ6IEhUTUxJbnB1dEVsZW1lbnQ7XHJcblxyXG4gICAgICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgICAgICBsZXQgdGVtcGxhdGUgPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCh0aGlzLnRlbXBsYXRlaWQpIGFzIEhUTUxUZW1wbGF0ZUVsZW1lbnQ7XHJcbiAgICAgICAgICAgIGlmICghdGVtcGxhdGUpIHtcclxuICAgICAgICAgICAgICAgIGNvbnNvbGUuZXJyb3IoYENhbid0IGZpbmQgdGVtcGxhdGVJRCAke3RoaXMudGVtcGxhdGVpZH0gdG8gbWFrZSBsb2dpbiBmb3JtLmApO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHRoaXMuYXBwZW5kKHRlbXBsYXRlLmNvbnRlbnQuY2xvbmVOb2RlKHRydWUpKTtcclxuICAgICAgICAgICAgdGhpcy5lbWFpbCA9IHRoaXMucXVlcnlTZWxlY3RvcihcIiNFbWFpbExvZ2luXCIpO1xyXG4gICAgICAgICAgICB0aGlzLnBhc3N3b3JkID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI1Bhc3N3b3JkTG9naW5cIik7XHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI0xvZ2luQnV0dG9uXCIpO1xyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5leGVjdXRlTG9naW4oKTtcclxuICAgICAgICAgICAgfSk7XHJcblx0XHRcdHRoaXMucXVlcnlTZWxlY3RvckFsbChcIiNFbWFpbExvZ2luLCNQYXNzd29yZExvZ2luXCIpLmZvckVhY2goeCA9PlxyXG5cdFx0XHRcdHguYWRkRXZlbnRMaXN0ZW5lcihcImtleWRvd25cIiwgKGU6IEtleWJvYXJkRXZlbnQpID0+IHtcclxuXHRcdFx0XHRcdGlmIChlLmNvZGUgPT09IFwiRW50ZXJcIikgdGhpcy5leGVjdXRlTG9naW4oKTtcclxuXHRcdFx0XHR9KVxyXG4gICAgICAgICAgICApO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZXhlY3V0ZUxvZ2luKCkge1xyXG4gICAgICAgICAgICBsZXQgcGF0aCA9IFwiL0lkZW50aXR5L0xvZ2luXCI7XHJcbiAgICAgICAgICAgIGlmICh0aGlzLmlzRXh0ZXJuYWxMb2dpbi50b0xvd2VyQ2FzZSgpID09PSBcInRydWVcIilcclxuICAgICAgICAgICAgICAgIHBhdGggPSBcIi9JZGVudGl0eS9Mb2dpbkFuZEFzc29jaWF0ZVwiO1xyXG4gICAgICAgICAgICBsZXQgcGF5bG9hZCA9IEpTT04uc3RyaW5naWZ5KHsgZW1haWw6IHRoaXMuZW1haWwudmFsdWUsIHBhc3N3b3JkOiB0aGlzLnBhc3N3b3JkLnZhbHVlIH0pO1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBwYXRoLCB7XHJcbiAgICAgICAgICAgICAgICBtZXRob2Q6IFwiUE9TVFwiLFxyXG4gICAgICAgICAgICAgICAgaGVhZGVyczoge1xyXG4gICAgICAgICAgICAgICAgICAgICdDb250ZW50LVR5cGUnOiAnYXBwbGljYXRpb24vanNvbidcclxuICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgICAgICBib2R5OiBwYXlsb2FkXHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbihyZXNwb25zZSkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiByZXNwb25zZS5qc29uKCk7XHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAocmVzdWx0KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgc3dpdGNoIChyZXN1bHQucmVzdWx0KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgY2FzZSB0cnVlOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICBsZXQgZGVzdGluYXRpb24gPSAoZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNSZWZlcnJlclwiKSBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbG9jYXRpb24uaHJlZiA9IGRlc3RpbmF0aW9uO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICBkZWZhdWx0OlxyXG4gICAgICAgICAgICAgICAgICAgICAgICBsZXQgbG9naW5SZXN1bHQgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI0xvZ2luUmVzdWx0XCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBsb2dpblJlc3VsdC5pbm5lckhUTUwgPSByZXN1bHQubWVzc2FnZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbG9naW5SZXN1bHQuY2xhc3NMaXN0LnJlbW92ZShcImQtbm9uZVwiKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAuY2F0Y2goZnVuY3Rpb24gKGVycm9yKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgbGV0IGxvZ2luUmVzdWx0ID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNMb2dpblJlc3VsdFwiKTtcclxuICAgICAgICAgICAgICAgICAgICBsb2dpblJlc3VsdC5pbm5lckhUTUwgPSBcIlRoZXJlIHdhcyBhbiB1bmtub3duIGVycm9yIHdoaWxlIGF0dGVtcHRpbmcgbG9naW5cIjtcclxuICAgICAgICAgICAgICAgICAgICBsb2dpblJlc3VsdC5jbGFzc0xpc3QucmVtb3ZlKFwiZC1ub25lXCIpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcbiAgICB9XHJcbiAgICBcclxuICAgIGN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtbG9naW5mb3JtJywgTG9naW5Gb3JtKTtcclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBNb3JlUG9zdHNCZWZvcmVSZXBseUJ1dHRvbiBleHRlbmRzIEVsZW1lbnRCYXNlIHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgYnV0dG9udGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbnRleHRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBNb3JlUG9zdHNCZWZvcmVSZXBseUJ1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmJ1dHRvbnRleHQ7XHJcbiAgICAgICAgaWYgKHRoaXMuYnV0dG9uY2xhc3M/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IGJ1dHRvbi5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICBidXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIChlOiBNb3VzZUV2ZW50KSA9PiB7XHJcbiAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5sb2FkTW9yZVBvc3RzKCk7XHJcbiAgICAgICAgfSk7XHJcbiAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXREZXBlbmRlbnRSZWZlcmVuY2UoKTogW1N0YXRlQmFzZSwgc3RyaW5nXSB7XHJcbiAgICAgICAgcmV0dXJuIFtQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUsIFwiaXNOZXdlclBvc3RzQXZhaWxhYmxlXCJdO1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICB1cGRhdGVVSShkYXRhOiBib29sZWFuKTogdm9pZCB7XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpO1xyXG4gICAgICAgIGlmICghZGF0YSlcclxuICAgICAgICAgICAgYnV0dG9uLnN0eWxlLnZpc2liaWxpdHkgPSBcImhpZGRlblwiO1xyXG4gICAgICAgIGVsc2VcclxuICAgICAgICAgICAgYnV0dG9uLnN0eWxlLnZpc2liaWxpdHkgPSBcInZpc2libGVcIjtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIC8+YDtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1tb3JlcG9zdHNiZWZvcmVyZXBseWJ1dHRvbicsIE1vcmVQb3N0c0JlZm9yZVJlcGx5QnV0dG9uKTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgTW9yZVBvc3RzQnV0dG9uIGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuICAgIGdldCBidXR0b250ZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9udGV4dFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IE1vcmVQb3N0c0J1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmJ1dHRvbnRleHQ7XHJcbiAgICAgICAgaWYgKHRoaXMuYnV0dG9uY2xhc3M/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IGJ1dHRvbi5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICBidXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIChlOiBNb3VzZUV2ZW50KSA9PiB7XHJcbiAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5sb2FkTW9yZVBvc3RzKCk7XHJcbiAgICAgICAgfSk7XHJcbiAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXREZXBlbmRlbnRSZWZlcmVuY2UoKTogW1N0YXRlQmFzZSwgc3RyaW5nXSB7XHJcbiAgICAgICAgcmV0dXJuIFtQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUsIFwiaGlnaFBhZ2VcIl07XHJcbiAgICB9XHJcbiAgICBcclxuICAgIHVwZGF0ZVVJKGRhdGE6IG51bWJlcik6IHZvaWQge1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICBpZiAoUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLnBhZ2VDb3VudCA9PT0gMSB8fCBkYXRhID09PSBQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUucGFnZUNvdW50KVxyXG4gICAgICAgICAgICBidXR0b24uc3R5bGUudmlzaWJpbGl0eSA9IFwiaGlkZGVuXCI7XHJcbiAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICBidXR0b24uc3R5bGUudmlzaWJpbGl0eSA9IFwidmlzaWJsZVwiO1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgLz5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLW1vcmVwb3N0c2J1dHRvbicsIE1vcmVQb3N0c0J1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFBNQ291bnQgZXh0ZW5kcyBFbGVtZW50QmFzZSB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy51c2VyU3RhdGUsIFwibmV3UG1Db3VudFwiXTtcclxuICAgIH1cclxuXHJcbiAgICB1cGRhdGVVSShkYXRhOiBudW1iZXIpOiB2b2lkIHtcclxuICAgICAgICBpZiAoZGF0YSA9PT0gMClcclxuICAgICAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBcIlwiO1xyXG4gICAgICAgIGVsc2VcclxuICAgICAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBgPHNwYW4gY2xhc3M9XCJiYWRnZVwiPiR7ZGF0YX08L3NwYW4+YDtcclxuICAgIH1cclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1wbWNvdW50JywgUE1Db3VudCk7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFBvc3RNaW5pUHJvZmlsZSBleHRlbmRzIEhUTUxFbGVtZW50IHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IHVzZXJuYW1lKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidXNlcm5hbWVcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgdXNlcm5hbWVjbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInVzZXJuYW1lY2xhc3NcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgdXNlcmlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidXNlcmlkXCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IG1pbmlQcm9maWxlQm94Q2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJtaW5pcHJvZmlsZWJveGNsYXNzXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIHByaXZhdGUgaXNPcGVuOiBib29sZWFuO1xyXG4gICAgcHJpdmF0ZSBib3g6IEhUTUxFbGVtZW50O1xyXG4gICAgcHJpdmF0ZSBib3hIZWlnaHQ6IHN0cmluZztcclxuICAgIHByaXZhdGUgaXNMb2FkZWQ6IGJvb2xlYW47XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdGhpcy5pc0xvYWRlZCA9IGZhbHNlO1xyXG4gICAgICAgIHRoaXMuaW5uZXJIVE1MID0gUG9zdE1pbmlQcm9maWxlLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBuYW1lSGVhZGVyID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaDNcIikgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgdGhpcy51c2VybmFtZWNsYXNzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiBuYW1lSGVhZGVyLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIG5hbWVIZWFkZXIuaW5uZXJIVE1MID0gdGhpcy51c2VybmFtZTtcclxuICAgICAgICBuYW1lSGVhZGVyLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgIHRoaXMudG9nZ2xlKCk7XHJcbiAgICAgICAgfSk7XHJcbiAgICAgICAgdGhpcy5ib3ggPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJkaXZcIik7XHJcbiAgICAgICAgdGhpcy5taW5pUHJvZmlsZUJveENsYXNzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiB0aGlzLmJveC5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIHRvZ2dsZSgpIHtcclxuICAgICAgICBpZiAoIXRoaXMuaXNMb2FkZWQpIHtcclxuICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvQWNjb3VudC9NaW5pUHJvZmlsZS9cIiArIHRoaXMudXNlcmlkKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAgICAgLnRoZW4odGV4dCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGxldCBzdWIgPSB0aGlzLmJveC5xdWVyeVNlbGVjdG9yKFwiZGl2XCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBzdWIuaW5uZXJIVE1MID0gdGV4dDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgY29uc3QgaGVpZ2h0ID0gc3ViLmdldEJvdW5kaW5nQ2xpZW50UmVjdCgpLmhlaWdodDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5ib3hIZWlnaHQgPSBgJHtoZWlnaHR9cHhgO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmJveC5zdHlsZS5oZWlnaHQgPSB0aGlzLmJveEhlaWdodDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5pc09wZW4gPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmlzTG9hZGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGVsc2UgaWYgKCF0aGlzLmlzT3Blbikge1xyXG4gICAgICAgICAgICB0aGlzLmJveC5zdHlsZS5oZWlnaHQgPSB0aGlzLmJveEhlaWdodDtcclxuICAgICAgICAgICAgdGhpcy5pc09wZW4gPSB0cnVlO1xyXG4gICAgICAgIH1cclxuICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICAgdGhpcy5ib3guc3R5bGUuaGVpZ2h0ID0gXCIwXCI7XHJcbiAgICAgICAgICAgIHRoaXMuaXNPcGVuID0gZmFsc2U7XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxoMz48L2gzPlxyXG48ZGl2PlxyXG4gICAgPGRpdiBjbGFzcz1cInB5LTEgcHgtMyBtYi0yXCI+PC9kaXY+XHJcbjwvZGl2PmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtcG9zdG1pbmlwcm9maWxlJywgUG9zdE1pbmlQcm9maWxlKTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgUG9zdE1vZGVyYXRpb25Mb2dCdXR0b24gZXh0ZW5kcyBIVE1MRWxlbWVudCB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBidXR0b25jbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbmNsYXNzXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBidXR0b250ZXh0KCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9udGV4dFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgcG9zdGlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwicG9zdGlkXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBwYXJlbnRTZWxlY3RvclRvQXBwZW5kVG8oKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJwYXJlbnRzZWxlY3RvcnRvYXBwZW5kdG9cIik7XHJcbiAgICB9XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBUb3BpY01vZGVyYXRpb25Mb2dCdXR0b24udGVtcGxhdGU7XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpO1xyXG4gICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuYnV0dG9udGV4dDtcclxuICAgICAgICBsZXQgY2xhc3NlcyA9IHRoaXMuYnV0dG9uY2xhc3M7XHJcbiAgICAgICAgaWYgKGNsYXNzZXM/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgIGNsYXNzZXMuc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IGJ1dHRvbi5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgbGV0IGNvbnRhaW5lcjogSFRNTERpdkVsZW1lbnQ7XHJcbiAgICAgICAgYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgIGlmICghY29udGFpbmVyKSB7XHJcbiAgICAgICAgICAgICAgICBsZXQgcGFyZW50Q29udGFpbmVyID0gc2VsZi5jbG9zZXN0KHRoaXMucGFyZW50U2VsZWN0b3JUb0FwcGVuZFRvKTtcclxuICAgICAgICAgICAgICAgIGlmICghcGFyZW50Q29udGFpbmVyKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgY29uc29sZS5lcnJvcihgQ2FuJ3QgZmluZCBhIHBhcmVudCBzZWxlY3RvciBcIiR7dGhpcy5wYXJlbnRTZWxlY3RvclRvQXBwZW5kVG99XCIgdG8gYXBwZW5kIHBvc3QgbW9kZXJhdGlvbiBsb2cgdG8uYCk7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgY29udGFpbmVyID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChcImRpdlwiKTtcclxuICAgICAgICAgICAgICAgIHBhcmVudENvbnRhaW5lci5hcHBlbmRDaGlsZChjb250YWluZXIpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGlmIChjb250YWluZXIuc3R5bGUuZGlzcGxheSAhPT0gXCJibG9ja1wiKVxyXG4gICAgICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvTW9kZXJhdG9yL1Bvc3RNb2RlcmF0aW9uTG9nL1wiICsgdGhpcy5wb3N0aWQpXHJcbiAgICAgICAgICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIC50aGVuKHRleHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgY29udGFpbmVyLmlubmVySFRNTCA9IHRleHQ7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb250YWluZXIuc3R5bGUuZGlzcGxheSA9IFwiYmxvY2tcIjtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfSkpO1xyXG4gICAgICAgICAgICBlbHNlIGNvbnRhaW5lci5zdHlsZS5kaXNwbGF5ID0gXCJub25lXCI7XHJcbiAgICAgICAgfSk7XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGlucHV0IHR5cGU9XCJidXR0b25cIiAvPmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZShcInBmLXBvc3Rtb2RlcmF0aW9ubG9nYnV0dG9uXCIsIFBvc3RNb2RlcmF0aW9uTG9nQnV0dG9uKTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgUHJldmlld0J1dHRvbiBleHRlbmRzIEhUTUxFbGVtZW50IHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGxhYmVsVGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImxhYmVsdGV4dFwiKTtcclxuICAgIH1cclxuICAgIGdldCB0ZXh0U291cmNlU2VsZWN0b3IoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0ZXh0c291cmNlc2VsZWN0b3JcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgaXNQbGFpblRleHRTZWxlY3RvcigpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImlzcGxhaW50ZXh0c2VsZWN0b3JcIik7XHJcbiAgICB9XHJcblxyXG4gICAgY29ubmVjdGVkQ2FsbGJhY2soKSB7XHJcbiAgICAgICAgdGhpcy5pbm5lckhUTUwgPSBQcmV2aWV3QnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKSBhcyBIVE1MQnV0dG9uRWxlbWVudDtcclxuICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmxhYmVsVGV4dDtcclxuICAgICAgICBsZXQgaGVhZFRleHQgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJoNFwiKSBhcyBIVE1MSGVhZEVsZW1lbnQ7XHJcbiAgICAgICAgaGVhZFRleHQuaW5uZXJUZXh0ID0gdGhpcy5sYWJlbFRleHQ7XHJcbiAgICAgICAgdmFyIG1vZGFsID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiLm1vZGFsXCIpO1xyXG4gICAgICAgIG1vZGFsLmFkZEV2ZW50TGlzdGVuZXIoXCJzaG93bi5icy5tb2RhbFwiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgIHRoaXMub3Blbk1vZGFsKCk7XHJcbiAgICAgICAgfSk7XHJcbiAgICB9XHJcblxyXG4gICAgb3Blbk1vZGFsKCkge1xyXG4gICAgICAgIHRpbnltY2UudHJpZ2dlclNhdmUoKTtcclxuICAgICAgICBsZXQgZnVsbFRleHQgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKHRoaXMudGV4dFNvdXJjZVNlbGVjdG9yKSBhcyBhbnk7XHJcbiAgICAgICAgbGV0IG1vZGVsID0ge1xyXG4gICAgICAgICAgICBGdWxsVGV4dDogZnVsbFRleHQudmFsdWUsXHJcbiAgICAgICAgICAgIElzUGxhaW5UZXh0OiAoZG9jdW1lbnQucXVlcnlTZWxlY3Rvcih0aGlzLmlzUGxhaW5UZXh0U2VsZWN0b3IpIGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlLnRvTG93ZXJDYXNlKCkgPT09IFwidHJ1ZVwiXHJcbiAgICAgICAgfTtcclxuICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9QcmV2aWV3VGV4dFwiLCB7XHJcbiAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCIsXHJcbiAgICAgICAgICAgIGJvZHk6IEpTT04uc3RyaW5naWZ5KG1vZGVsKSxcclxuICAgICAgICAgICAgaGVhZGVyczoge1xyXG4gICAgICAgICAgICAgICAgXCJDb250ZW50LVR5cGVcIjogXCJhcHBsaWNhdGlvbi9qc29uXCJcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgIH0pXHJcbiAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLnRleHQoKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4odGV4dCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgbGV0IHIgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIucGFyc2VkRnVsbFRleHRcIikgYXMgSFRNTERpdkVsZW1lbnQ7XHJcbiAgICAgICAgICAgICAgICAgICAgci5pbm5lckhUTUwgPSB0ZXh0O1xyXG4gICAgICAgICAgICAgICAgfSkpO1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgY2xhc3M9XCJidG4gYnRuLXByaW1hcnlcIiBkYXRhLWJzLXRvZ2dsZT1cIm1vZGFsXCIgZGF0YS1icy10YXJnZXQ9XCIjUHJldmlld01vZGFsXCI+XHJcbjxkaXYgY2xhc3M9XCJtb2RhbCBmYWRlXCIgaWQ9XCJQcmV2aWV3TW9kYWxcIiB0YWJpbmRleD1cIi0xXCIgcm9sZT1cImRpYWxvZ1wiPlxyXG5cdDxkaXYgY2xhc3M9XCJtb2RhbC1kaWFsb2dcIj5cclxuXHRcdDxkaXYgY2xhc3M9XCJtb2RhbC1jb250ZW50XCI+XHJcblx0XHRcdDxkaXYgY2xhc3M9XCJtb2RhbC1oZWFkZXJcIj5cclxuXHRcdFx0XHQ8aDQgY2xhc3M9XCJtb2RhbC10aXRsZVwiPjwvaDQ+XHJcblx0XHRcdFx0PGJ1dHRvbiB0eXBlPVwiYnV0dG9uXCIgY2xhc3M9XCJidG4tY2xvc2VcIiBkYXRhLWJzLWRpc21pc3M9XCJtb2RhbFwiIGFyaWEtbGFiZWw9XCJDbG9zZVwiPjwvYnV0dG9uPlxyXG5cdFx0XHQ8L2Rpdj5cclxuXHRcdFx0PGRpdiBjbGFzcz1cIm1vZGFsLWJvZHlcIj5cclxuXHRcdFx0XHQ8ZGl2IGNsYXNzPVwicG9zdEl0ZW0gcGFyc2VkRnVsbFRleHRcIj48L2Rpdj5cclxuXHRcdFx0PC9kaXY+XHJcblx0XHRcdDxkaXYgY2xhc3M9XCJtb2RhbC1mb290ZXJcIj5cclxuXHRcdFx0XHQ8YnV0dG9uIHR5cGU9XCJidXR0b25cIiBjbGFzcz1cImJ0biBidG4tcHJpbWFyeVwiIGRhdGEtYnMtZGlzbWlzcz1cIm1vZGFsXCI+Q2xvc2U8L2J1dHRvbj5cclxuXHRcdFx0PC9kaXY+XHJcblx0XHQ8L2Rpdj5cclxuXHQ8L2Rpdj5cclxuPC9kaXY+YDtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1wcmV2aWV3YnV0dG9uJywgUHJldmlld0J1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFByZXZpb3VzUG9zdHNCdXR0b24gZXh0ZW5kcyBFbGVtZW50QmFzZSB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBidXR0b25jbGFzcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbmNsYXNzXCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IGJ1dHRvbnRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b250ZXh0XCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIHRoaXMuaW5uZXJIVE1MID0gUHJldmlvdXNQb3N0c0J1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIikgYXMgSFRNTElucHV0RWxlbWVudDtcclxuICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmJ1dHRvbnRleHQ7XHJcbiAgICAgICAgaWYgKHRoaXMuYnV0dG9uY2xhc3M/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uY2xhc3Muc3BsaXQoXCIgXCIpLmZvckVhY2goKGMpID0+IGJ1dHRvbi5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICBidXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIChlOiBNb3VzZUV2ZW50KSA9PiB7XHJcbiAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5sb2FkUHJldmlvdXNQb3N0cygpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgICAgIHN1cGVyLmNvbm5lY3RlZENhbGxiYWNrKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgIHJldHVybiBbUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLCBcImxvd1BhZ2VcIl07XHJcbiAgICB9XHJcbiAgICBcclxuICAgIHVwZGF0ZVVJKGRhdGE6IG51bWJlcik6IHZvaWQge1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICBpZiAoUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLnBhZ2VDb3VudCA9PT0gMSB8fCBkYXRhID09PSAxKVxyXG4gICAgICAgICAgICBidXR0b24uc3R5bGUudmlzaWJpbGl0eSA9IFwiaGlkZGVuXCI7XHJcbiAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICBidXR0b24uc3R5bGUudmlzaWJpbGl0eSA9IFwidmlzaWJsZVwiO1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgLz5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXByZXZpb3VzcG9zdHNidXR0b24nLCBQcmV2aW91c1Bvc3RzQnV0dG9uKTtcclxuXHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgUXVvdGVCdXR0b24gZXh0ZW5kcyBIVE1MRWxlbWVudCB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBuYW1lKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwibmFtZVwiKTtcclxuICAgIH1cclxuICAgIGdldCBjb250YWluZXJpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImNvbnRhaW5lcmlkXCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IGJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgYnV0dG9udGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbnRleHRcIik7XHJcbiAgICB9XHJcbiAgICBnZXQgdGlwKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidGlwXCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IHBvc3RJRCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInBvc3RpZFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIF90aXA6IGFueTtcclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICBsZXQgdGFyZ2V0VGV4dCA9IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKHRoaXMuY29udGFpbmVyaWQpO1xyXG4gICAgICAgIHRoaXMuaW5uZXJIVE1MID0gUXVvdGVCdXR0b24udGVtcGxhdGU7XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpO1xyXG4gICAgICAgIGJ1dHRvbi50aXRsZSA9IHRoaXMudGlwO1xyXG4gICAgICAgIFtcIm1vdXNlZG93blwiLFwidG91Y2hzdGFydFwiXS5mb3JFYWNoKChlOnN0cmluZykgPT4gXHJcbiAgICAgICAgICAgIHRhcmdldFRleHQuYWRkRXZlbnRMaXN0ZW5lcihlLCAoKSA9PiBcclxuICAgICAgICAgICAgICAgIHsgaWYgKHRoaXMuX3RpcCkgdGhpcy5fdGlwLmhpZGUoKSB9KSk7XHJcbiAgICAgICAgYnV0dG9uLnZhbHVlID0gdGhpcy5idXR0b250ZXh0O1xyXG4gICAgICAgIGxldCBjbGFzc2VzID0gdGhpcy5idXR0b25jbGFzcztcclxuICAgICAgICBpZiAoY2xhc3Nlcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgY2xhc3Nlcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIHRoaXMub25jbGljayA9IChlOiBNb3VzZUV2ZW50KSA9PiB7XHJcbiAgICAgICAgICAgIC8vIGdldCB0aGlzIGZyb20gdG9waWMgc3RhdGUncyBjYWxsYmFjay9yZWFkeSBtZXRob2QsIGJlY2F1c2UgaU9TIGxvc2VzIHNlbGVjdGlvbiB3aGVuIHlvdSB0b3VjaCBxdW90ZSBidXR0b25cclxuICAgICAgICAgICAgbGV0IGZyYWdtZW50ID0gUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLmRvY3VtZW50RnJhZ21lbnQ7XHJcbiAgICAgICAgICAgIGxldCBhbmNlc3RvciA9IFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5zZWxlY3Rpb25BbmNlc3RvcjtcclxuICAgICAgICAgICAgaWYgKCFmcmFnbWVudCkge1xyXG4gICAgICAgICAgICAgICAgbGV0IHNlbGVjdGlvbiA9IGRvY3VtZW50LmdldFNlbGVjdGlvbigpO1xyXG4gICAgICAgICAgICAgICAgaWYgKCFzZWxlY3Rpb24gfHwgc2VsZWN0aW9uLnJhbmdlQ291bnQgPT09IDAgfHwgc2VsZWN0aW9uLmdldFJhbmdlQXQoMCkudG9TdHJpbmcoKS5sZW5ndGggPT09IDApIHtcclxuICAgICAgICAgICAgICAgICAgICAvLyBwcm9tcHQgdG8gc2VsZWN0XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5fdGlwID0gbmV3IGJvb3RzdHJhcC5Ub29sdGlwKGJ1dHRvbiwge3RyaWdnZXI6IFwibWFudWFsXCJ9KTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLl90aXAuc2hvdygpO1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGxldCByYW5nZSA9IHNlbGVjdGlvbi5nZXRSYW5nZUF0KDApO1xyXG4gICAgICAgICAgICAgICAgYW5jZXN0b3IgPSByYW5nZS5jb21tb25BbmNlc3RvckNvbnRhaW5lcjtcclxuICAgICAgICAgICAgICAgIGZyYWdtZW50ID0gcmFuZ2UuY2xvbmVDb250ZW50cygpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGxldCBkaXYgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwiZGl2XCIpO1xyXG4gICAgICAgICAgICBkaXYuYXBwZW5kQ2hpbGQoZnJhZ21lbnQpO1xyXG4gICAgICAgICAgICAvLyBpcyBzZWxlY3Rpb24gaW4gdGhlIGNvbnRhaW5lcj9cclxuICAgICAgICAgICAgd2hpbGUgKGFuY2VzdG9yWydpZCddICE9PSB0aGlzLmNvbnRhaW5lcmlkICYmIGFuY2VzdG9yLnBhcmVudEVsZW1lbnQgIT09IG51bGwpIHtcclxuICAgICAgICAgICAgICAgIGFuY2VzdG9yID0gYW5jZXN0b3IucGFyZW50RWxlbWVudDtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICBsZXQgaXNJblRleHQgPSBhbmNlc3RvclsnaWQnXSA9PT0gdGhpcy5jb250YWluZXJpZDtcclxuICAgICAgICAgICAgLy8gaWYgbm90LCBpcyBpdCBwYXJ0aWFsbHkgaW4gdGhlIGNvbnRhaW5lcj9cclxuICAgICAgICAgICAgaWYgKCFpc0luVGV4dCkge1xyXG4gICAgICAgICAgICAgICAgbGV0IGNvbnRhaW5lciA9IGRpdi5xdWVyeVNlbGVjdG9yKFwiI1wiICsgdGhpcy5jb250YWluZXJpZCk7XHJcbiAgICAgICAgICAgICAgICBpZiAoY29udGFpbmVyICE9PSBudWxsICYmIGNvbnRhaW5lciAhPT0gdW5kZWZpbmVkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgLy8gaXQncyBwYXJ0aWFsbHkgaW4gdGhlIGNvbnRhaW5lciwgc28ganVzdCBnZXQgdGhhdCBwYXJ0XHJcbiAgICAgICAgICAgICAgICAgICAgZGl2LmlubmVySFRNTCA9IGNvbnRhaW5lci5pbm5lckhUTUw7XHJcbiAgICAgICAgICAgICAgICAgICAgaXNJblRleHQgPSB0cnVlO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGlmIChpc0luVGV4dCkge1xyXG4gICAgICAgICAgICAgICAgLy8gYWN0aXZhdGUgb3IgYWRkIHRvIHF1b3RlXHJcbiAgICAgICAgICAgICAgICBsZXQgcmVzdWx0OiBzdHJpbmc7XHJcbiAgICAgICAgICAgICAgICBpZiAodXNlclN0YXRlLmlzUGxhaW5UZXh0KVxyXG4gICAgICAgICAgICAgICAgICAgIHJlc3VsdCA9IGBbcXVvdGVdW2ldJHt0aGlzLm5hbWV9OlsvaV1cXHJcXG4gJHtkaXYuaW5uZXJUZXh0fVsvcXVvdGVdYDtcclxuICAgICAgICAgICAgICAgIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICByZXN1bHQgPSBgPGJsb2NrcXVvdGU+PHA+PGk+JHt0aGlzLm5hbWV9OjwvaT48L3A+JHtkaXYuaW5uZXJIVE1MfTwvYmxvY2txdW90ZT48cD48L3A+YDtcclxuICAgICAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5uZXh0UXVvdGUgPSByZXN1bHQ7XHJcbiAgICAgICAgICAgICAgICBpZiAoIVBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5pc1JlcGx5TG9hZGVkKVxyXG4gICAgICAgICAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5sb2FkUmVwbHkoUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLnRvcGljSUQsIE51bWJlcih0aGlzLnBvc3RJRCksIHRydWUpO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIGxldCB0ZW1wID0gZG9jdW1lbnQuZ2V0U2VsZWN0aW9uKCk7XHJcbiAgICAgICAgICAgIGlmICh0ZW1wKVxyXG4gICAgICAgICAgICAgICAgdGVtcC5yZW1vdmVBbGxSYW5nZXMoKTtcclxuICAgICAgICB9O1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgZGF0YS1icy10b2dnbGU9XCJ0b29sdGlwXCIgdGl0bGU9XCJcIiAvPmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtcXVvdGVidXR0b24nLCBRdW90ZUJ1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFJlcGx5QnV0dG9uIGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9udGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbnRleHRcIik7XHJcbiAgICB9XHJcbiAgICBcclxuICAgIGdldCB0b3BpY2lkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidG9waWNpZFwiKTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgZ2V0IHBvc3RpZCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInBvc3RpZFwiKTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgZ2V0IG92ZXJyaWRlZGlzcGxheSgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcIm92ZXJyaWRlZGlzcGxheVwiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFJlcGx5QnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuYnV0dG9udGV4dDtcclxuICAgICAgICBpZiAodGhpcy5idXR0b25jbGFzcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgdGhpcy5idXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKGU6IE1vdXNlRXZlbnQpID0+IHtcclxuICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLmxvYWRSZXBseShOdW1iZXIodGhpcy50b3BpY2lkKSwgTnVtYmVyKHRoaXMucG9zdGlkKSwgdHJ1ZSk7XHJcbiAgICAgICAgfSk7XHJcbiAgICAgICAgc3VwZXIuY29ubmVjdGVkQ2FsbGJhY2soKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXREZXBlbmRlbnRSZWZlcmVuY2UoKTogW1N0YXRlQmFzZSwgc3RyaW5nXSB7XHJcbiAgICAgICAgcmV0dXJuIFtQb3BGb3J1bXMuY3VycmVudFRvcGljU3RhdGUsIFwiaXNSZXBseUxvYWRlZFwiXTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgdXBkYXRlVUkoZGF0YTogYm9vbGVhbik6IHZvaWQge1xyXG4gICAgICAgIGlmICh0aGlzLm92ZXJyaWRlZGlzcGxheT8udG9Mb3dlckNhc2UoKSA9PT0gXCJ0cnVlXCIpXHJcbiAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICBsZXQgYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgaWYgKGRhdGEpXHJcbiAgICAgICAgICAgIGJ1dHRvbi5zdHlsZS5kaXNwbGF5ID0gXCJub25lXCI7XHJcbiAgICAgICAgZWxzZVxyXG4gICAgICAgICAgICBidXR0b24uc3R5bGUuZGlzcGxheSA9IFwiaW5pdGlhbFwiO1xyXG4gICAgfVxyXG5cclxuICAgIHN0YXRpYyB0ZW1wbGF0ZTogc3RyaW5nID0gYDxpbnB1dCB0eXBlPVwiYnV0dG9uXCIgLz5gO1xyXG59XHJcblxyXG5jdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXJlcGx5YnV0dG9uJywgUmVwbHlCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBSZXBseUZvcm0gZXh0ZW5kcyBIVE1MRWxlbWVudCB7XHJcbiAgICAgICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgICAgIHN1cGVyKCk7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICBnZXQgdGVtcGxhdGVJRCgpIHtcclxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidGVtcGxhdGVpZFwiKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgYnV0dG9uOiBIVE1MSW5wdXRFbGVtZW50O1xyXG5cclxuICAgICAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICAgICAgbGV0IHRlbXBsYXRlID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQodGhpcy50ZW1wbGF0ZUlEKSBhcyBIVE1MVGVtcGxhdGVFbGVtZW50O1xyXG4gICAgICAgICAgICBpZiAoIXRlbXBsYXRlKSB7XHJcbiAgICAgICAgICAgICAgICBjb25zb2xlLmVycm9yKGBDYW4ndCBmaW5kIHRlbXBsYXRlSUQgJHt0aGlzLnRlbXBsYXRlSUR9IHRvIG1ha2UgcmVwbHkgZm9ybS5gKTtcclxuICAgICAgICAgICAgICAgIHJldHVybjtcclxuICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICB0aGlzLmFwcGVuZCh0ZW1wbGF0ZS5jb250ZW50LmNsb25lTm9kZSh0cnVlKSk7XHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI1N1Ym1pdFJlcGx5XCIpO1xyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5zdWJtaXRSZXBseSgpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHN1Ym1pdFJlcGx5KCkge1xyXG4gICAgICAgICAgICB0aGlzLmJ1dHRvbi5zZXRBdHRyaWJ1dGUoXCJkaXNhYmxlZFwiLCBcImRpc2FibGVkXCIpO1xyXG4gICAgICAgICAgICB2YXIgY2xvc2VDaGVjayA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjQ2xvc2VPblJlcGx5XCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQ7XHJcbiAgICAgICAgICAgIHZhciBjbG9zZU9uUmVwbHkgPSBmYWxzZTtcclxuICAgICAgICAgICAgaWYgKGNsb3NlQ2hlY2sgJiYgY2xvc2VDaGVjay5jaGVja2VkKSBjbG9zZU9uUmVwbHkgPSB0cnVlO1xyXG4gICAgICAgICAgICB2YXIgbW9kZWwgPSB7XHJcbiAgICAgICAgICAgICAgICBUaXRsZTogKHRoaXMucXVlcnlTZWxlY3RvcihcIiNOZXdSZXBseSAjVGl0bGVcIikgYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWUsXHJcbiAgICAgICAgICAgICAgICBGdWxsVGV4dDogKHRoaXMucXVlcnlTZWxlY3RvcihcIiNOZXdSZXBseSAjRnVsbFRleHRcIikgYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWUsXHJcbiAgICAgICAgICAgICAgICBJbmNsdWRlU2lnbmF0dXJlOiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1JlcGx5ICNJbmNsdWRlU2lnbmF0dXJlXCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQpLmNoZWNrZWQsXHJcbiAgICAgICAgICAgICAgICBJdGVtSUQ6ICh0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTmV3UmVwbHkgI0l0ZW1JRFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZSxcclxuICAgICAgICAgICAgICAgIENsb3NlT25SZXBseTogY2xvc2VPblJlcGx5LFxyXG4gICAgICAgICAgICAgICAgSXNQbGFpblRleHQ6ICh0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTmV3UmVwbHkgI0lzUGxhaW5UZXh0XCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlLnRvTG93ZXJDYXNlKCkgPT09IFwidHJ1ZVwiLFxyXG4gICAgICAgICAgICAgICAgUGFyZW50UG9zdElEOiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1JlcGx5ICNQYXJlbnRQb3N0SURcIikgYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWVcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vUG9zdFJlcGx5XCIsIHtcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCIsXHJcbiAgICAgICAgICAgICAgICBib2R5OiBKU09OLnN0cmluZ2lmeShtb2RlbCksXHJcbiAgICAgICAgICAgICAgICBoZWFkZXJzOiB7XHJcbiAgICAgICAgICAgICAgICAgICAgXCJDb250ZW50LVR5cGVcIjogXCJhcHBsaWNhdGlvbi9qc29uXCJcclxuICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS5qc29uKCkpXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXN1bHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIHN3aXRjaCAocmVzdWx0LnJlc3VsdCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjYXNlIHRydWU6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB3aW5kb3cubG9jYXRpb24gPSByZXN1bHQucmVkaXJlY3Q7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgZGVmYXVsdDpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciByID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI1Bvc3RSZXNwb25zZU1lc3NhZ2VcIikgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByLmlubmVySFRNTCA9IHJlc3VsdC5tZXNzYWdlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24ucmVtb3ZlQXR0cmlidXRlKFwiZGlzYWJsZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC5jYXRjaChlcnJvciA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHIgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjUG9zdFJlc3BvbnNlTWVzc2FnZVwiKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICByLmlubmVySFRNTCA9IFwiVGhlcmUgd2FzIGFuIHVua25vd24gZXJyb3Igd2hpbGUgdHJ5aW5nIHRvIHBvc3RcIjtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmJ1dHRvbi5yZW1vdmVBdHRyaWJ1dGUoXCJkaXNhYmxlZFwiKTtcclxuICAgICAgICAgICAgICAgICAgICByLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9O1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICBjdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXJlcGx5Zm9ybScsIFJlcGx5Rm9ybSk7XHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgU3Vic2NyaWJlQnV0dG9uIGV4dGVuZHMgRWxlbWVudEJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgZ2V0IHN1YnNjcmliZXRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJzdWJzY3JpYmV0ZXh0XCIpO1xyXG4gICAgfVxyXG4gICAgZ2V0IHVuc3Vic2NyaWJldGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInVuc3Vic2NyaWJldGV4dFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFN1YnNjcmliZUJ1dHRvbi50ZW1wbGF0ZTtcclxuICAgICAgICBsZXQgYnV0dG9uOiBIVE1MSW5wdXRFbGVtZW50ID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiaW5wdXRcIik7XHJcbiAgICAgICAgdGhpcy5idXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9TdWJzY3JpcHRpb24vVG9nZ2xlU3Vic2NyaXB0aW9uL1wiICsgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLnRvcGljSUQsIHtcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCJcclxuICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLmpzb24oKSlcclxuICAgICAgICAgICAgICAgIC50aGVuKHJlc3VsdCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgc3dpdGNoIChyZXN1bHQuZGF0YS5pc1N1YnNjcmliZWQpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSB0cnVlOlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLmlzU3Vic2NyaWJlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgY2FzZSBmYWxzZTpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIFBvcEZvcnVtcy5jdXJyZW50VG9waWNTdGF0ZS5pc1N1YnNjcmliZWQgPSBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBkZWZhdWx0OlxyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgLy8gVE9ETzogc29tZXRoaW5nIGVsc2VcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KVxyXG4gICAgICAgICAgICAgICAgLmNhdGNoKCgpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAvLyBUT0RPOiBoYW5kbGUgZXJyb3JcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH0pO1xyXG4gICAgICAgIHN1cGVyLmNvbm5lY3RlZENhbGxiYWNrKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0RGVwZW5kZW50UmVmZXJlbmNlKCk6IFtTdGF0ZUJhc2UsIHN0cmluZ10ge1xyXG4gICAgICAgIHJldHVybiBbUG9wRm9ydW1zLmN1cnJlbnRUb3BpY1N0YXRlLCBcImlzU3Vic2NyaWJlZFwiXTtcclxuICAgIH1cclxuXHJcbiAgICB1cGRhdGVVSShkYXRhOiBib29sZWFuKTogdm9pZCB7XHJcbiAgICAgICAgbGV0IGJ1dHRvbiA9IHRoaXMucXVlcnlTZWxlY3RvcihcImlucHV0XCIpO1xyXG4gICAgICAgIGlmIChkYXRhKVxyXG4gICAgICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLnVuc3Vic2NyaWJldGV4dDtcclxuICAgICAgICBlbHNlXHJcbiAgICAgICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuc3Vic2NyaWJldGV4dDtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIC8+YDtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKCdwZi1zdWJzY3JpYmVidXR0b24nLCBTdWJzY3JpYmVCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBUb3BpY0J1dHRvbiBleHRlbmRzIEVsZW1lbnRCYXNlIHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYnV0dG9uY2xhc3NcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGJ1dHRvbnRleHQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b250ZXh0XCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBmb3J1bWlkKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiZm9ydW1pZFwiKTtcclxuICAgIH1cclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFRvcGljQnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKSBhcyBIVE1MSW5wdXRFbGVtZW50O1xyXG4gICAgICAgIGJ1dHRvbi52YWx1ZSA9IHRoaXMuYnV0dG9udGV4dDtcclxuICAgICAgICBpZiAodGhpcy5idXR0b25jbGFzcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgdGhpcy5idXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gYnV0dG9uLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIGJ1dHRvbi5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgKCkgPT4ge1xyXG4gICAgICAgICAgICBjdXJyZW50Rm9ydW1TdGF0ZS5sb2FkTmV3VG9waWMoKTtcclxuICAgICAgICB9KTtcclxuICAgICAgICBzdXBlci5jb25uZWN0ZWRDYWxsYmFjaygpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldERlcGVuZGVudFJlZmVyZW5jZSgpOiBbU3RhdGVCYXNlLCBzdHJpbmddIHtcclxuICAgICAgICByZXR1cm4gW1BvcEZvcnVtcy5jdXJyZW50Rm9ydW1TdGF0ZSwgXCJpc05ld1RvcGljTG9hZGVkXCJdO1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICB1cGRhdGVVSShkYXRhOiBib29sZWFuKTogdm9pZCB7XHJcbiAgICAgICAgaWYgKGRhdGEpXHJcbiAgICAgICAgICAgIHRoaXMuc3R5bGUuZGlzcGxheSA9IFwibm9uZVwiO1xyXG4gICAgICAgIGVsc2VcclxuICAgICAgICAgICAgdGhpcy5zdHlsZS5kaXNwbGF5ID0gXCJpbml0aWFsXCI7XHJcbiAgICB9XHJcblxyXG4gICAgc3RhdGljIHRlbXBsYXRlOiBzdHJpbmcgPSBgPGlucHV0IHR5cGU9XCJidXR0b25cIiAvPmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZSgncGYtdG9waWNidXR0b24nLCBUb3BpY0J1dHRvbik7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIFRvcGljRm9ybSBleHRlbmRzIEhUTUxFbGVtZW50IHtcclxuICAgICAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICAgICAgc3VwZXIoKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIGdldCB0ZW1wbGF0ZUlEKCkge1xyXG4gICAgICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0ZW1wbGF0ZWlkXCIpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHJpdmF0ZSBidXR0b246IEhUTUxJbnB1dEVsZW1lbnQ7XHJcblxyXG4gICAgICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgICAgICBsZXQgdGVtcGxhdGUgPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCh0aGlzLnRlbXBsYXRlSUQpIGFzIEhUTUxUZW1wbGF0ZUVsZW1lbnQ7XHJcbiAgICAgICAgICAgIGlmICghdGVtcGxhdGUpIHtcclxuICAgICAgICAgICAgICAgIGNvbnNvbGUuZXJyb3IoYENhbid0IGZpbmQgdGVtcGxhdGVJRCAke3RoaXMudGVtcGxhdGVJRH0gdG8gbWFrZSByZXBseSBmb3JtLmApO1xyXG4gICAgICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIHRoaXMuYXBwZW5kKHRlbXBsYXRlLmNvbnRlbnQuY2xvbmVOb2RlKHRydWUpKTtcclxuICAgICAgICAgICAgdGhpcy5idXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjU3VibWl0TmV3VG9waWNcIik7XHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnN1Ym1pdFRvcGljKCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgc3VibWl0VG9waWMoKSB7XHJcbiAgICAgICAgICAgIHRoaXMuYnV0dG9uLnNldEF0dHJpYnV0ZShcImRpc2FibGVkXCIsIFwiZGlzYWJsZWRcIik7XHJcbiAgICAgICAgICAgIHZhciBtb2RlbCA9IHtcclxuICAgICAgICAgICAgICAgIFRpdGxlOiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1RvcGljICNUaXRsZVwiKSBhcyBIVE1MSW5wdXRFbGVtZW50KS52YWx1ZSxcclxuICAgICAgICAgICAgICAgIEZ1bGxUZXh0OiAodGhpcy5xdWVyeVNlbGVjdG9yKFwiI05ld1RvcGljICNGdWxsVGV4dFwiKWFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlLFxyXG4gICAgICAgICAgICAgICAgSW5jbHVkZVNpZ25hdHVyZTogKHRoaXMucXVlcnlTZWxlY3RvcihcIiNOZXdUb3BpYyAjSW5jbHVkZVNpZ25hdHVyZVwiKWFzIEhUTUxJbnB1dEVsZW1lbnQpLmNoZWNrZWQsXHJcbiAgICAgICAgICAgICAgICBJdGVtSUQ6ICh0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTmV3VG9waWMgI0l0ZW1JRFwiKWFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlLFxyXG4gICAgICAgICAgICAgICAgSXNQbGFpblRleHQ6ICh0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjTmV3VG9waWMgI0lzUGxhaW5UZXh0XCIpYXMgSFRNTElucHV0RWxlbWVudCkudmFsdWUudG9Mb3dlckNhc2UoKSA9PT0gXCJ0cnVlXCJcclxuICAgICAgICAgICAgfTtcclxuICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vUG9zdFRvcGljXCIsIHtcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCIsXHJcbiAgICAgICAgICAgICAgICBib2R5OiBKU09OLnN0cmluZ2lmeShtb2RlbCksXHJcbiAgICAgICAgICAgICAgICBoZWFkZXJzOiB7XHJcbiAgICAgICAgICAgICAgICAgICAgXCJDb250ZW50LVR5cGVcIjogXCJhcHBsaWNhdGlvbi9qc29uXCJcclxuICAgICAgICAgICAgICAgIH0sXHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS5qc29uKCkpXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXN1bHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIHN3aXRjaCAocmVzdWx0LnJlc3VsdCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBjYXNlIHRydWU6XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB3aW5kb3cubG9jYXRpb24gPSByZXN1bHQucmVkaXJlY3Q7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBicmVhaztcclxuICAgICAgICAgICAgICAgICAgICAgICAgZGVmYXVsdDpcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIHZhciByID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiI1Bvc3RSZXNwb25zZU1lc3NhZ2VcIikgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByLmlubmVySFRNTCA9IHJlc3VsdC5tZXNzYWdlO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5idXR0b24ucmVtb3ZlQXR0cmlidXRlKFwiZGlzYWJsZWRcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICByLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC5jYXRjaChlcnJvciA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHIgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCIjUG9zdFJlc3BvbnNlTWVzc2FnZVwiKSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICByLmlubmVySFRNTCA9IFwiVGhlcmUgd2FzIGFuIHVua25vd24gZXJyb3Igd2hpbGUgdHJ5aW5nIHRvIHBvc3RcIjtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmJ1dHRvbi5yZW1vdmVBdHRyaWJ1dGUoXCJkaXNhYmxlZFwiKTtcclxuICAgICAgICAgICAgICAgICAgICByLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9O1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICBjdXN0b21FbGVtZW50cy5kZWZpbmUoJ3BmLXRvcGljZm9ybScsIFRvcGljRm9ybSk7XHJcbn0iLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgVG9waWNNb2RlcmF0aW9uTG9nQnV0dG9uIGV4dGVuZHMgSFRNTEVsZW1lbnQge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9uY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJidXR0b25jbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgYnV0dG9udGV4dCgpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImJ1dHRvbnRleHRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IHRvcGljaWQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ0b3BpY2lkXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGNvbm5lY3RlZENhbGxiYWNrKCkge1xyXG4gICAgICAgIHRoaXMuaW5uZXJIVE1MID0gVG9waWNNb2RlcmF0aW9uTG9nQnV0dG9uLnRlbXBsYXRlO1xyXG4gICAgICAgIGxldCBidXR0b24gPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJpbnB1dFwiKTtcclxuICAgICAgICBidXR0b24udmFsdWUgPSB0aGlzLmJ1dHRvbnRleHQ7XHJcbiAgICAgICAgbGV0IGNsYXNzZXMgPSB0aGlzLmJ1dHRvbmNsYXNzO1xyXG4gICAgICAgIGlmIChjbGFzc2VzPy5sZW5ndGggPiAwKVxyXG4gICAgICAgICAgICBjbGFzc2VzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiBidXR0b24uY2xhc3NMaXN0LmFkZChjKSk7XHJcbiAgICAgICAgYnV0dG9uLmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCAoKSA9PiB7XHJcbiAgICAgICAgICAgIGxldCBjb250YWluZXIgPSB0aGlzLnF1ZXJ5U2VsZWN0b3IoXCJkaXZcIik7XHJcbiAgICAgICAgICAgIGlmIChjb250YWluZXIuc3R5bGUuZGlzcGxheSAhPT0gXCJibG9ja1wiKVxyXG4gICAgICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvTW9kZXJhdG9yL1RvcGljTW9kZXJhdGlvbkxvZy9cIiArIHRoaXMudG9waWNpZClcclxuICAgICAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS50ZXh0KClcclxuICAgICAgICAgICAgICAgICAgICAgICAgLnRoZW4odGV4dCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb250YWluZXIuaW5uZXJIVE1MID0gdGV4dDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGNvbnRhaW5lci5zdHlsZS5kaXNwbGF5ID0gXCJibG9ja1wiO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICAgICAgICAgIGVsc2UgY29udGFpbmVyLnN0eWxlLmRpc3BsYXkgPSBcIm5vbmVcIjtcclxuICAgICAgICB9KTtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8aW5wdXQgdHlwZT1cImJ1dHRvblwiIC8+XHJcbiAgICA8ZGl2PjwvZGl2PmA7XHJcbn1cclxuXHJcbmN1c3RvbUVsZW1lbnRzLmRlZmluZShcInBmLXRvcGljbW9kZXJhdGlvbmxvZ2J1dHRvblwiLCBUb3BpY01vZGVyYXRpb25Mb2dCdXR0b24pO1xyXG5cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuICAgIGV4cG9ydCBjbGFzcyBWb3RlQ291bnQgZXh0ZW5kcyBIVE1MRWxlbWVudCB7XHJcbiAgICBjb25zdHJ1Y3RvcigpIHtcclxuICAgICAgICBzdXBlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCB2b3RlcygpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcInZvdGVzXCIpO1xyXG4gICAgfVxyXG4gICAgc2V0IHZvdGVzKHZhbHVlOnN0cmluZykge1xyXG4gICAgICAgIHRoaXMuc2V0QXR0cmlidXRlKFwidm90ZXNcIiwgdmFsdWUpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBwb3N0aWQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJwb3N0aWRcIik7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IHZvdGVzY29udGFpbmVyY2xhc3MoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJ2b3Rlc2NvbnRhaW5lcmNsYXNzXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBiYWRnZWNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiYmFkZ2VjbGFzc1wiKTtcclxuICAgIH1cclxuXHJcbiAgICBnZXQgdm90ZWJ1dHRvbmNsYXNzKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwidm90ZWJ1dHRvbmNsYXNzXCIpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBpc2xvZ2dlZGluKCk6IHN0cmluZyB7XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuZ2V0QXR0cmlidXRlKFwiaXNsb2dnZWRpblwiKS50b0xvd2VyQ2FzZSgpO1xyXG4gICAgfVxyXG5cclxuICAgIGdldCBpc2F1dGhvcigpOiBzdHJpbmcge1xyXG4gICAgICAgIHJldHVybiB0aGlzLmdldEF0dHJpYnV0ZShcImlzYXV0aG9yXCIpLnRvTG93ZXJDYXNlKCk7XHJcbiAgICB9XHJcblxyXG4gICAgZ2V0IGlzdm90ZWQoKTogc3RyaW5nIHtcclxuICAgICAgICByZXR1cm4gdGhpcy5nZXRBdHRyaWJ1dGUoXCJpc3ZvdGVkXCIpLnRvTG93ZXJDYXNlKCk7XHJcbiAgICB9XHJcblxyXG4gICAgcHJpdmF0ZSBiYWRnZTogSFRNTEVsZW1lbnQ7XHJcbiAgICBwcml2YXRlIHZvdGVyQ29udGFpbmVyOiBIVE1MRWxlbWVudDtcclxuICAgIHByaXZhdGUgcG9wb3ZlcjogYm9vdHN0cmFwLlBvcG92ZXI7XHJcbiAgICBwcml2YXRlIHBvcG92ZXJFdmVudEhhbmRlcjogRXZlbnRMaXN0ZW5lck9yRXZlbnRMaXN0ZW5lck9iamVjdDtcclxuXHJcbiAgICBjb25uZWN0ZWRDYWxsYmFjaygpIHtcclxuICAgICAgICB0aGlzLmlubmVySFRNTCA9IFZvdGVDb3VudC50ZW1wbGF0ZTtcclxuICAgICAgICB0aGlzLmJhZGdlID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwiZGl2XCIpO1xyXG4gICAgICAgIHRoaXMuYmFkZ2UuaW5uZXJIVE1MID0gXCIrXCIgKyB0aGlzLnZvdGVzO1xyXG4gICAgICAgIGlmICh0aGlzLmJhZGdlY2xhc3M/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgIHRoaXMuYmFkZ2VjbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gdGhpcy5iYWRnZS5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICB2YXIgc3RhdHVzSHRtbCA9IHRoaXMuYnV0dG9uR2VuZXJhdG9yKCk7XHJcbiAgICAgICAgaWYgKHN0YXR1c0h0bWwgIT0gXCJcIikge1xyXG4gICAgICAgICAgICBsZXQgc3RhdHVzID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChcInRlbXBsYXRlXCIpO1xyXG4gICAgICAgICAgICBzdGF0dXMuaW5uZXJIVE1MID0gdGhpcy5idXR0b25HZW5lcmF0b3IoKTtcclxuICAgICAgICAgICAgdGhpcy5hcHBlbmQoc3RhdHVzLmNvbnRlbnQuZmlyc3RDaGlsZCk7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGxldCB2b3RlQnV0dG9uID0gdGhpcy5xdWVyeVNlbGVjdG9yKFwic3BhblwiKTtcclxuICAgICAgICBpZiAodm90ZUJ1dHRvbikge1xyXG4gICAgICAgICAgICBpZiAodGhpcy52b3RlYnV0dG9uY2xhc3M/Lmxlbmd0aCA+IDApXHJcbiAgICAgICAgICAgICAgICB0aGlzLnZvdGVidXR0b25jbGFzcy5zcGxpdChcIiBcIikuZm9yRWFjaCgoYykgPT4gdm90ZUJ1dHRvbi5jbGFzc0xpc3QuYWRkKGMpKTtcclxuICAgICAgICAgICAgdHlwZSByZXN1bHRUeXBlID0geyB2b3RlczogbnVtYmVyOyBpc1ZvdGVkOiBib29sZWFuOyB9XHJcbiAgICAgICAgICAgIHZvdGVCdXR0b24uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IHtcclxuICAgICAgICAgICAgICAgIGZldGNoKFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0ZvcnVtL1RvZ2dsZVZvdGUvXCIgKyB0aGlzLnBvc3RpZCwgeyBtZXRob2Q6IFwiUE9TVFwifSlcclxuICAgICAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLmpzb24oKVxyXG4gICAgICAgICAgICAgICAgICAgIC50aGVuKChyZXN1bHQ6IHJlc3VsdFR5cGUpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy52b3RlcyA9IHJlc3VsdC52b3Rlcy50b1N0cmluZygpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmJhZGdlLmlubmVySFRNTCA9IFwiK1wiICsgdGhpcy52b3RlcztcclxuICAgICAgICAgICAgICAgICAgICAgICAgaWYgKHJlc3VsdC5pc1ZvdGVkKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2b3RlQnV0dG9uLmNsYXNzTGlzdC5yZW1vdmUoXCJpY29uLXBsdXNcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2b3RlQnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJpY29uLWNhbmNlbC1jaXJjbGVcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgZWxzZSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2b3RlQnV0dG9uLmNsYXNzTGlzdC5yZW1vdmUoXCJpY29uLWNhbmNlbC1jaXJjbGVcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB2b3RlQnV0dG9uLmNsYXNzTGlzdC5hZGQoXCJpY29uLXBsdXNcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhpcy5hcHBseVBvcG92ZXIoKTtcclxuICAgICAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgfVxyXG4gICAgICAgIHRoaXMuc2V0dXBWb3RlclBvcG92ZXIoKTtcclxuICAgICAgICB0aGlzLmFwcGx5UG9wb3ZlcigpO1xyXG4gICAgfVxyXG5cclxuICAgIHByaXZhdGUgc2V0dXBWb3RlclBvcG92ZXIoKTogdm9pZCB7XHJcbiAgICAgICAgdGhpcy52b3RlckNvbnRhaW5lciA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJkaXZcIik7XHJcbiAgICAgICAgaWYgKHRoaXMudm90ZXNjb250YWluZXJjbGFzcz8ubGVuZ3RoID4gMClcclxuICAgICAgICAgICAgdGhpcy52b3Rlc2NvbnRhaW5lcmNsYXNzLnNwbGl0KFwiIFwiKS5mb3JFYWNoKChjKSA9PiB0aGlzLnZvdGVyQ29udGFpbmVyLmNsYXNzTGlzdC5hZGQoYykpO1xyXG4gICAgICAgIHRoaXMudm90ZXJDb250YWluZXIuaW5uZXJIVE1MID0gXCJMb2FkaW5nLi4uXCI7XHJcbiAgICAgICAgdGhpcy5wb3BvdmVyID0gbmV3IGJvb3RzdHJhcC5Qb3BvdmVyKHRoaXMuYmFkZ2UsIHtcclxuICAgICAgICAgICAgY29udGVudDogdGhpcy52b3RlckNvbnRhaW5lcixcclxuICAgICAgICAgICAgaHRtbDogdHJ1ZSxcclxuICAgICAgICAgICAgdHJpZ2dlcjogXCJjbGljayBmb2N1c1wiXHJcbiAgICAgICAgfSk7XHJcbiAgICAgICAgdGhpcy5wb3BvdmVyRXZlbnRIYW5kZXIgPSAoZSkgPT4ge1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9Wb3RlcnMvXCIgKyB0aGlzLnBvc3RpZClcclxuICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgdCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ0ZW1wbGF0ZVwiKTtcclxuICAgICAgICAgICAgICAgICAgICB0LmlubmVySFRNTCA9IHRleHQudHJpbSgpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMudm90ZXJDb250YWluZXIuaW5uZXJIVE1MID0gXCJcIjtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLnZvdGVyQ29udGFpbmVyLmFwcGVuZENoaWxkKHQuY29udGVudC5maXJzdENoaWxkKTtcclxuICAgICAgICAgICAgICAgIH0pKTtcclxuICAgICAgICB9O1xyXG4gICAgICAgIHRoaXMuYmFkZ2UuYWRkRXZlbnRMaXN0ZW5lcihcInNob3duLmJzLnBvcG92ZXJcIiwgdGhpcy5wb3BvdmVyRXZlbnRIYW5kZXIpO1xyXG4gICAgfVxyXG5cclxuICAgIHByaXZhdGUgYXBwbHlQb3BvdmVyKCk6IHZvaWQge1xyXG4gICAgICAgIGlmICh0aGlzLnZvdGVzID09PSBcIjBcIikge1xyXG4gICAgICAgICAgICB0aGlzLmJhZGdlLnN0eWxlLmN1cnNvciA9IFwiZGVmYXVsdFwiO1xyXG4gICAgICAgICAgICB0aGlzLnBvcG92ZXIuZGlzYWJsZSgpO1xyXG4gICAgICAgIH1cclxuICAgICAgICBlbHNlIHtcclxuICAgICAgICAgICAgdGhpcy5iYWRnZS5zdHlsZS5jdXJzb3IgPSBcInBvaW50ZXJcIjtcclxuICAgICAgICAgICAgdGhpcy5wb3BvdmVyLmVuYWJsZSgpO1xyXG4gICAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICBwcml2YXRlIGJ1dHRvbkdlbmVyYXRvcigpOiBzdHJpbmcge1xyXG4gICAgICAgIGlmICh0aGlzLmlzbG9nZ2VkaW4gPT09IFwiZmFsc2VcIiB8fCB0aGlzLmlzYXV0aG9yID09PSBcInRydWVcIilcclxuICAgICAgICAgICAgcmV0dXJuIFwiXCI7XHJcbiAgICAgICAgaWYgKHRoaXMuaXN2b3RlZCA9PT0gXCJ0cnVlXCIpXHJcbiAgICAgICAgICAgIHJldHVybiBWb3RlQ291bnQuY2FuY2VsVm90ZUJ1dHRvbjtcclxuICAgICAgICByZXR1cm4gVm90ZUNvdW50LnZvdGVVcEJ1dHRvbjtcclxuICAgIH1cclxuXHJcbiAgICBzdGF0aWMgdGVtcGxhdGU6IHN0cmluZyA9IGA8ZGl2PjwvZGl2PmA7XHJcblxyXG4gICAgc3RhdGljIHZvdGVVcEJ1dHRvbiA9IFwiPHNwYW4gY2xhc3M9XFxcImljb24tcGx1c1xcXCI+PC9zcGFuPlwiO1xyXG4gICAgc3RhdGljIGNhbmNlbFZvdGVCdXR0b24gPSBcIjxzcGFuIGNsYXNzPVxcXCJpY29uLWNhbmNlbC1jaXJjbGVcXFwiPjwvc3Bhbj5cIjtcclxufVxyXG5cclxuY3VzdG9tRWxlbWVudHMuZGVmaW5lKFwicGYtdm90ZWNvdW50XCIsIFZvdGVDb3VudCk7XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG4gICAgZXhwb3J0IGNsYXNzIE5vdGlmaWNhdGlvblNlcnZpY2Uge1xyXG4gICAgICAgIGNvbnN0cnVjdG9yKHVzZXJTdGF0ZTogVXNlclN0YXRlKSB7XHJcbiAgICAgICAgICAgIHRoaXMudXNlclN0YXRlID0gdXNlclN0YXRlO1xyXG4gICAgICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbiA9IG5ldyBzaWduYWxSLkh1YkNvbm5lY3Rpb25CdWlsZGVyKCkud2l0aFVybChcIi9Ob3RpZmljYXRpb25IdWJcIikud2l0aEF1dG9tYXRpY1JlY29ubmVjdCgpLmJ1aWxkKCk7XHJcbiAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbi5vbihcInVwZGF0ZVBNQ291bnRcIiwgZnVuY3Rpb24ocG1Db3VudDogbnVtYmVyKSB7XHJcbiAgICAgICAgICAgICAgICBzZWxmLnVzZXJTdGF0ZS5uZXdQbUNvdW50ID0gcG1Db3VudDtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIHRoaXMuY29ubmVjdGlvbi5zdGFydCgpO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHJpdmF0ZSB1c2VyU3RhdGU6IFVzZXJTdGF0ZTtcclxuICAgICAgICBwcml2YXRlIGNvbm5lY3Rpb246IGFueTtcclxuICAgIH1cclxufSIsIi8vIFRPRE86IE1vdmUgdGhpcyB0byBhbiBvcGVuIHdlYnNvY2tldHMgY29ubmVjdGlvblxyXG5cclxubmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcbiAgICBleHBvcnQgY2xhc3MgVGltZVVwZGF0ZXIge1xyXG4gICAgICAgIFN0YXJ0KCkge1xyXG4gICAgICAgICAgICBSZWFkeSgoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLlN0YXJ0VXBkYXRlcigpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgdGltZUFycmF5OiBzdHJpbmdbXTtcclxuXHJcbiAgICAgICAgcHJpdmF0ZSBQb3B1bGF0ZUFycmF5KCk6IHZvaWQge1xyXG4gICAgICAgICAgICB0aGlzLnRpbWVBcnJheSA9IFtdO1xyXG4gICAgICAgICAgICBsZXQgdGltZXMgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLmZUaW1lXCIpO1xyXG4gICAgICAgICAgICB0aW1lcy5mb3JFYWNoKHRpbWUgPT4ge1xyXG4gICAgICAgICAgICAgICAgdmFyIHQgPSB0aW1lLmdldEF0dHJpYnV0ZShcImRhdGEtdXRjXCIpO1xyXG4gICAgICAgICAgICAgICAgaWYgKCgobmV3IERhdGUoKS5nZXREYXRlKCkgLSBuZXcgRGF0ZSh0ICsgXCJaXCIpLmdldERhdGUoKSkgLyAzNjAwMDAwKSA8IDQ4KVxyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMudGltZUFycmF5LnB1c2godCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgcHJpdmF0ZSBTdGFydFVwZGF0ZXIoKTogdm9pZCB7XHJcbiAgICAgICAgICAgIHNldFRpbWVvdXQoKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5TdGFydFVwZGF0ZXIoKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuUG9wdWxhdGVBcnJheSgpO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5DYWxsRm9yVXBkYXRlKCk7XHJcbiAgICAgICAgICAgIH0sIDYwMDAwKTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHByaXZhdGUgQ2FsbEZvclVwZGF0ZSgpOiB2b2lkIHtcclxuICAgICAgICAgICAgaWYgKCF0aGlzLnRpbWVBcnJheSB8fCB0aGlzLnRpbWVBcnJheS5sZW5ndGggPT09IDApXHJcbiAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgIGxldCBzZXJpYWxpemVkID0gSlNPTi5zdHJpbmdpZnkodGhpcy50aW1lQXJyYXkpO1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9UaW1lL0dldFRpbWVzXCIsIHtcclxuICAgICAgICAgICAgICAgIG1ldGhvZDogXCJQT1NUXCIsXHJcbiAgICAgICAgICAgICAgICBib2R5OiBzZXJpYWxpemVkLFxyXG4gICAgICAgICAgICAgICAgaGVhZGVyczoge1xyXG4gICAgICAgICAgICAgICAgICAgIFwiQ29udGVudC1UeXBlXCI6IFwiYXBwbGljYXRpb24vanNvblwiXHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS5qc29uKCkpXHJcbiAgICAgICAgICAgICAgICAudGhlbihkYXRhID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBkYXRhLmZvckVhY2goKHQ6IHsga2V5OiBzdHJpbmc7IHZhbHVlOiBzdHJpbmc7IH0pID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIi5mVGltZVtkYXRhLXV0Yz0nXCIgKyB0LmtleSArIFwiJ11cIikuaW5uZXJIVE1MID0gdC52YWx1ZTtcclxuICAgICAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAuY2F0Y2goZXJyb3IgPT4geyBjb25zb2xlLmxvZyhcIlRpbWUgdXBkYXRlIGZhaWx1cmU6IFwiICsgZXJyb3IpOyB9KTtcclxuICAgICAgICB9XHJcbiAgICB9XHJcbn1cclxuXHJcbnZhciB0aW1lVXBkYXRlciA9IG5ldyBQb3BGb3J1bXMuVGltZVVwZGF0ZXIoKTtcclxudGltZVVwZGF0ZXIuU3RhcnQoKTsiLCJuYW1lc3BhY2UgUG9wRm9ydW1zIHtcclxuXHJcbiAgICBleHBvcnQgY2xhc3MgRm9ydW1TdGF0ZSBleHRlbmRzIFN0YXRlQmFzZSB7XHJcbiAgICAgICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgICAgIHN1cGVyKCk7XHJcbiAgICAgICAgfVxyXG4gICAgXHJcbiAgICAgICAgZm9ydW1JRDogbnVtYmVyO1xyXG4gICAgICAgIHBhZ2VTaXplOiBudW1iZXI7XHJcbiAgICAgICAgcGFnZUluZGV4OiBudW1iZXI7XHJcbiAgICAgICAgQFdhdGNoUHJvcGVydHlcclxuICAgICAgICBpc05ld1RvcGljTG9hZGVkOiBib29sZWFuO1xyXG5cclxuICAgICAgICBzZXR1cEZvcnVtKCkge1xyXG4gICAgICAgICAgICBQb3BGb3J1bXMuUmVhZHkoKCkgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5pc05ld1RvcGljTG9hZGVkID0gZmFsc2U7XHJcbiAgICAgICAgICAgICAgICB0aGlzLmZvcnVtTGlzdGVuKCk7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgbG9hZE5ld1RvcGljKCkge1xyXG4gICAgICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9Qb3N0VG9waWMvXCIgKyB0aGlzLmZvcnVtSUQpXHJcbiAgICAgICAgICAgICAgICAudGhlbigocmVzcG9uc2UpID0+IHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm4gcmVzcG9uc2UudGV4dCgpO1xyXG4gICAgICAgICAgICAgICAgfSlcclxuICAgICAgICAgICAgICAgIC50aGVuKChib2R5KSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIG4gPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI05ld1RvcGljXCIpIGFzIEhUTUxFbGVtZW50O1xyXG4gICAgICAgICAgICAgICAgICAgIGlmICghbilcclxuICAgICAgICAgICAgICAgICAgICAgICAgdGhyb3coXCJDYW4ndCBmaW5kIGEgI05ld1RvcGljIGVsZW1lbnQgdG8gbG9hZCBpbiB0aGUgbmV3IHRvcGljIGZvcm0uXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIG4uaW5uZXJIVE1MID0gYm9keTtcclxuICAgICAgICAgICAgICAgICAgICBuLnN0eWxlLmRpc3BsYXkgPSBcImJsb2NrXCI7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5pc05ld1RvcGljTG9hZGVkID0gdHJ1ZTtcclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgZm9ydW1MaXN0ZW4oKSB7XHJcbiAgICAgICAgICAgIGxldCBjb25uZWN0aW9uID0gbmV3IHNpZ25hbFIuSHViQ29ubmVjdGlvbkJ1aWxkZXIoKS53aXRoVXJsKFwiL0ZvcnVtc0h1YlwiKS53aXRoQXV0b21hdGljUmVjb25uZWN0KCkuYnVpbGQoKTtcclxuICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICBjb25uZWN0aW9uLm9uKFwibm90aWZ5VXBkYXRlZFRvcGljXCIsIGZ1bmN0aW9uIChkYXRhOiBhbnkpIHsgLy8gVE9ETzogcmVmYWN0b3IgdG8gc3Ryb25nIHR5cGVcclxuICAgICAgICAgICAgICAgIGxldCByZW1vdmFsID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcignI1RvcGljTGlzdCB0cltkYXRhLXRvcGljSUQ9XCInICsgZGF0YS50b3BpY0lEICsgJ1wiXScpO1xyXG4gICAgICAgICAgICAgICAgaWYgKHJlbW92YWwpIHtcclxuICAgICAgICAgICAgICAgICAgICByZW1vdmFsLnJlbW92ZSgpO1xyXG4gICAgICAgICAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgcm93cyA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIjVG9waWNMaXN0IHRyOm5vdCgjVG9waWNUZW1wbGF0ZSlcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHJvd3MubGVuZ3RoID09IHNlbGYucGFnZVNpemUpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJvd3Nbcm93cy5sZW5ndGggLSAxXS5yZW1vdmUoKTtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIGxldCByb3cgPSBzZWxmLnBvcHVsYXRlVG9waWNSb3coZGF0YSk7XHJcbiAgICAgICAgICAgICAgICByb3cuY2xhc3NMaXN0LnJlbW92ZShcImhpZGRlblwiKTtcclxuICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjVG9waWNMaXN0IHRib2R5XCIpLnByZXBlbmQocm93KTtcclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIGNvbm5lY3Rpb24uc3RhcnQoKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4oZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBjb25uZWN0aW9uLmludm9rZShcImxpc3RlblRvXCIsIHNlbGYuZm9ydW1JRCk7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHJlY2VudExpc3RlbigpIHtcclxuICAgICAgICAgICAgdmFyIGNvbm5lY3Rpb24gPSBuZXcgc2lnbmFsUi5IdWJDb25uZWN0aW9uQnVpbGRlcigpLndpdGhVcmwoXCIvUmVjZW50SHViXCIpLndpdGhBdXRvbWF0aWNSZWNvbm5lY3QoKS5idWlsZCgpO1xyXG4gICAgICAgICAgICBsZXQgc2VsZiA9IHRoaXM7XHJcbiAgICAgICAgICAgIGNvbm5lY3Rpb24ub24oXCJub3RpZnlSZWNlbnRVcGRhdGVcIiwgZnVuY3Rpb24gKGRhdGE6IGFueSkge1xyXG4gICAgICAgICAgICAgICAgdmFyIHJlbW92YWwgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKCcjVG9waWNMaXN0IHRyW2RhdGEtdG9waWNJRD1cIicgKyBkYXRhLnRvcGljSUQgKyAnXCJdJyk7XHJcbiAgICAgICAgICAgICAgICBpZiAocmVtb3ZhbCkge1xyXG4gICAgICAgICAgICAgICAgICAgIHJlbW92YWwucmVtb3ZlKCk7XHJcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xyXG4gICAgICAgICAgICAgICAgICAgIHZhciByb3dzID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIiNUb3BpY0xpc3QgdHI6bm90KCNUb3BpY1RlbXBsYXRlKVwiKTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAocm93cy5sZW5ndGggPT0gc2VsZi5wYWdlU2l6ZSlcclxuICAgICAgICAgICAgICAgICAgICAgICAgcm93c1tyb3dzLmxlbmd0aCAtIDFdLnJlbW92ZSgpO1xyXG4gICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgdmFyIHJvdyA9IHNlbGYucG9wdWxhdGVUb3BpY1JvdyhkYXRhKTtcclxuICAgICAgICAgICAgICAgIHJvdy5jbGFzc0xpc3QucmVtb3ZlKFwiaGlkZGVuXCIpO1xyXG4gICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNUb3BpY0xpc3QgdGJvZHlcIikucHJlcGVuZChyb3cpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgY29ubmVjdGlvbi5zdGFydCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbm5lY3Rpb24uaW52b2tlKFwicmVnaXN0ZXJcIik7XHJcbiAgICAgICAgICAgICAgICB9KTtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHBvcHVsYXRlVG9waWNSb3cgPSBmdW5jdGlvbiAoZGF0YTogYW55KSB7XHJcbiAgICAgICAgICAgIGxldCByb3cgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI1RvcGljVGVtcGxhdGVcIikuY2xvbmVOb2RlKHRydWUpIGFzIEhUTUxFbGVtZW50O1xyXG4gICAgICAgICAgICByb3cuc2V0QXR0cmlidXRlKFwiZGF0YS10b3BpY2lkXCIsIGRhdGEudG9waWNJRCk7XHJcbiAgICAgICAgICAgIHJvdy5yZW1vdmVBdHRyaWJ1dGUoXCJpZFwiKTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIuc3RhcnRlZEJ5TmFtZVwiKS5pbm5lckhUTUwgPSBkYXRhLnN0YXJ0ZWRCeU5hbWU7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLmluZGljYXRvckxpbmtcIikuc2V0QXR0cmlidXRlKFwiaHJlZlwiLCBkYXRhLmxpbmspO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi50aXRsZUxpbmtcIikuaW5uZXJIVE1MID0gZGF0YS50aXRsZTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIudGl0bGVMaW5rXCIpLnNldEF0dHJpYnV0ZShcImhyZWZcIiwgZGF0YS5saW5rKTtcclxuICAgICAgICAgICAgdmFyIGZvcnVtVGl0bGUgPSByb3cucXVlcnlTZWxlY3RvcihcIi5mb3J1bVRpdGxlXCIpO1xyXG4gICAgICAgICAgICBpZiAoZm9ydW1UaXRsZSkgZm9ydW1UaXRsZS5pbm5lckhUTUwgPSBkYXRhLmZvcnVtVGl0bGU7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLnZpZXdDb3VudFwiKS5pbm5lckhUTUwgPSBkYXRhLnZpZXdDb3VudDtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIucmVwbHlDb3VudFwiKS5pbm5lckhUTUwgPSBkYXRhLnJlcGx5Q291bnQ7XHJcbiAgICAgICAgICAgIHJvdy5xdWVyeVNlbGVjdG9yKFwiLmxhc3RQb3N0VGltZVwiKS5pbm5lckhUTUwgPSBkYXRhLmxhc3RQb3N0VGltZTtcclxuICAgICAgICAgICAgcm93LnF1ZXJ5U2VsZWN0b3IoXCIubGFzdFBvc3ROYW1lXCIpLmlubmVySFRNTCA9IGRhdGEubGFzdFBvc3ROYW1lO1xyXG4gICAgICAgICAgICByb3cucXVlcnlTZWxlY3RvcihcIi5mVGltZVwiKS5zZXRBdHRyaWJ1dGUoXCJkYXRhLXV0Y1wiLCBkYXRhLnV0Yyk7XHJcbiAgICAgICAgICAgIHJldHVybiByb3c7XHJcbiAgICAgICAgfTtcclxuICAgIH1cclxufSIsIm5hbWVzcGFjZSBQb3BGb3J1bXMge1xyXG5cclxuZXhwb3J0IGNsYXNzIFRvcGljU3RhdGUgZXh0ZW5kcyBTdGF0ZUJhc2Uge1xyXG4gICAgY29uc3RydWN0b3IoKSB7XHJcbiAgICAgICAgc3VwZXIoKTtcclxuICAgIH1cclxuXHJcbiAgICB0b3BpY0lEOiBudW1iZXI7XHJcbiAgICBpc0ltYWdlRW5hYmxlZDogYm9vbGVhbjtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcbiAgICBpc1JlcGx5TG9hZGVkOiBib29sZWFuO1xyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuICAgIGFuc3dlclBvc3RJRDogbnVtYmVyO1xyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuXHRsb3dQYWdlOm51bWJlcjtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcblx0aGlnaFBhZ2U6IG51bWJlcjtcclxuXHRsYXN0VmlzaWJsZVBvc3RJRDogbnVtYmVyO1xyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuICAgIGlzTmV3ZXJQb3N0c0F2YWlsYWJsZTogYm9vbGVhbjtcclxuICAgIHBhZ2VJbmRleDogbnVtYmVyO1xyXG5cdHBhZ2VDb3VudDogbnVtYmVyO1xyXG5cdGxvYWRpbmdQb3N0czogYm9vbGVhbiA9IGZhbHNlO1xyXG5cdGlzU2Nyb2xsQWRqdXN0ZWQ6IGJvb2xlYW4gPSBmYWxzZTtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcbiAgICBjb21tZW50UmVwbHlJRDogbnVtYmVyO1xyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuICAgIG5leHRRdW90ZTogc3RyaW5nO1xyXG4gICAgQFdhdGNoUHJvcGVydHlcclxuICAgIGlzU3Vic2NyaWJlZDogYm9vbGVhbjtcclxuICAgIEBXYXRjaFByb3BlcnR5XHJcbiAgICBpc0Zhdm9yaXRlOiBib29sZWFuO1xyXG4gICAgZG9jdW1lbnRGcmFnbWVudDogRG9jdW1lbnRGcmFnbWVudDtcclxuICAgIHNlbGVjdGlvbkFuY2VzdG9yOiBOb2RlO1xyXG5cclxuICAgIHNldHVwVG9waWMoKSB7XHJcbiAgICAgICAgUG9wRm9ydW1zLlJlYWR5KCgpID0+IHtcclxuICAgICAgICAgICAgdGhpcy5pc1JlcGx5TG9hZGVkID0gZmFsc2U7XHJcbiAgICAgICAgICAgIHRoaXMuaXNOZXdlclBvc3RzQXZhaWxhYmxlID0gZmFsc2U7XHJcbiAgICAgICAgICAgIHRoaXMubG93UGFnZSA9IHRoaXMucGFnZUluZGV4O1xyXG4gICAgICAgICAgICB0aGlzLmhpZ2hQYWdlID0gdGhpcy5wYWdlSW5kZXg7XHJcblxyXG4gICAgICAgICAgICAvLyBzaWduYWxSIGNvbm5lY3Rpb25zXHJcbiAgICAgICAgICAgIGxldCBjb25uZWN0aW9uID0gbmV3IHNpZ25hbFIuSHViQ29ubmVjdGlvbkJ1aWxkZXIoKS53aXRoVXJsKFwiL1RvcGljc0h1YlwiKS53aXRoQXV0b21hdGljUmVjb25uZWN0KCkuYnVpbGQoKTtcclxuICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICAvLyBmb3IgYWxsIHBvc3RzIGxvYWRlZCBidXQgcmVwbHkgbm90IG9wZW5cclxuICAgICAgICAgICAgY29ubmVjdGlvbi5vbihcImZldGNoTmV3UG9zdFwiLCBmdW5jdGlvbiAocG9zdElEOiBudW1iZXIpIHtcclxuICAgICAgICAgICAgICAgIGlmICghc2VsZi5pc1JlcGx5TG9hZGVkICYmIHNlbGYuaGlnaFBhZ2UgPT09IHNlbGYucGFnZUNvdW50KSB7XHJcbiAgICAgICAgICAgICAgICAgICAgZmV0Y2goUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vUG9zdC9cIiArIHBvc3RJRClcclxuICAgICAgICAgICAgICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB2YXIgdCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoXCJ0ZW1wbGF0ZVwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB0LmlubmVySFRNTCA9IHRleHQudHJpbSgpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjUG9zdFN0cmVhbVwiKS5hcHBlbmRDaGlsZCh0LmNvbnRlbnQuZmlyc3RDaGlsZCk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICAgICAgICAgICAgICAgICAgc2VsZi5sYXN0VmlzaWJsZVBvc3RJRCA9IHBvc3RJRDtcclxuICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgIC8vIGZvciByZXBseSBhbHJlYWR5IG9wZW5cclxuICAgICAgICAgICAgY29ubmVjdGlvbi5vbihcIm5vdGlmeU5ld1Bvc3RzXCIsIGZ1bmN0aW9uICh0aGVMYXN0UG9zdElEOiBudW1iZXIpIHtcclxuICAgICAgICAgICAgICAgIHNlbGYuc2V0TW9yZVBvc3RzQXZhaWxhYmxlKHRoZUxhc3RQb3N0SUQpO1xyXG4gICAgICAgICAgICB9KTtcclxuICAgICAgICAgICAgY29ubmVjdGlvbi5zdGFydCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgcmV0dXJuIGNvbm5lY3Rpb24uaW52b2tlKFwibGlzdGVuVG9cIiwgc2VsZi50b3BpY0lEKTtcclxuICAgICAgICAgICAgICAgIH0pXHJcbiAgICAgICAgICAgICAgICAudGhlbihmdW5jdGlvbiAoKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgc2VsZi5jb25uZWN0aW9uID0gY29ubmVjdGlvblxyXG4gICAgICAgICAgICAgICAgfSk7XHJcblxyXG4gICAgICAgICAgICBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLnBvc3RJdGVtIGltZzpub3QoLmF2YXRhcilcIikuZm9yRWFjaCh4ID0+IHguY2xhc3NMaXN0LmFkZChcInBvc3RJbWFnZVwiKSk7XHJcblxyXG4gICAgICAgICAgICB0aGlzLnNjcm9sbFRvUG9zdEZyb21IYXNoKCk7XHJcbiAgICAgICAgICAgIHdpbmRvdy5hZGRFdmVudExpc3RlbmVyKFwic2Nyb2xsXCIsIHRoaXMuc2Nyb2xsTG9hZCk7XHJcblxyXG4gICAgICAgICAgICAvLyBjb21wZW5zYXRlIGZvciBpT1MgbG9zaW5nIHNlbGVjdGlvbiB3aGVuIHlvdSB0b3VjaCB0aGUgcXVvdGUgYnV0dG9uXHJcbiAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIucG9zdEJvZHlcIikuZm9yRWFjaCggeCA9PiB4LmFkZEV2ZW50TGlzdGVuZXIoXCJ0b3VjaGVuZFwiLCAoZSkgPT4ge1xyXG4gICAgICAgICAgICAgICAgbGV0IHNlbGVjdGlvbiA9IGRvY3VtZW50LmdldFNlbGVjdGlvbigpO1xyXG4gICAgICAgICAgICAgICAgaWYgKCFzZWxlY3Rpb24gfHwgc2VsZWN0aW9uLnJhbmdlQ291bnQgPT09IDAgfHwgc2VsZWN0aW9uLmdldFJhbmdlQXQoMCkudG9TdHJpbmcoKS5sZW5ndGggPT09IDApIHtcclxuICAgICAgICAgICAgICAgICAgICByZXR1cm47XHJcbiAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICBsZXQgcmFuZ2UgPSBzZWxlY3Rpb24uZ2V0UmFuZ2VBdCgwKTtcclxuICAgICAgICAgICAgICAgIHRoaXMuc2VsZWN0aW9uQW5jZXN0b3IgPSByYW5nZS5jb21tb25BbmNlc3RvckNvbnRhaW5lcjtcclxuICAgICAgICAgICAgICAgIHRoaXMuZG9jdW1lbnRGcmFnbWVudCA9IHJhbmdlLmNsb25lQ29udGVudHMoKTtcclxuICAgICAgICAgICAgfSkpO1xyXG4gICAgICAgIH0pO1xyXG4gICAgfVxyXG5cclxuICAgIGxvYWRSZXBseSh0b3BpY0lEOm51bWJlciwgcmVwbHlJRDpudW1iZXIsIHNldHVwTW9yZVBvc3RzOmJvb2xlYW4pOnZvaWQge1xyXG4gICAgICAgIGlmICh0aGlzLmlzUmVwbHlMb2FkZWQpIHtcclxuICAgICAgICAgICAgdGhpcy5zY3JvbGxUb0VsZW1lbnQoXCJOZXdSZXBseVwiKTtcclxuICAgICAgICAgICAgcmV0dXJuO1xyXG4gICAgICAgIH1cclxuICAgICAgICB3aW5kb3cucmVtb3ZlRXZlbnRMaXN0ZW5lcihcInNjcm9sbFwiLCB0aGlzLnNjcm9sbExvYWQpO1xyXG4gICAgICAgIHZhciBwYXRoID0gUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vUG9zdFJlcGx5L1wiICsgdG9waWNJRDtcclxuICAgICAgICBpZiAocmVwbHlJRCAhPSBudWxsKSB7XHJcbiAgICAgICAgICAgIHBhdGggKz0gXCI/cmVwbHlJRD1cIiArIHJlcGx5SUQ7XHJcbiAgICAgICAgfVxyXG4gICAgXHJcbiAgICAgICAgZmV0Y2gocGF0aClcclxuICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4gcmVzcG9uc2UudGV4dCgpXHJcbiAgICAgICAgICAgICAgICAudGhlbih0ZXh0ID0+IHtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgbiA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjTmV3UmVwbHlcIikgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgICAgICAgICAgICAgbi5pbm5lckhUTUwgPSB0ZXh0O1xyXG4gICAgICAgICAgICAgICAgICAgIG4uc3R5bGUuZGlzcGxheSA9IFwiYmxvY2tcIjtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLnNjcm9sbFRvRWxlbWVudChcIk5ld1JlcGx5XCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuaXNSZXBseUxvYWRlZCA9IHRydWU7XHJcbiAgICBcclxuICAgICAgICAgICAgICAgICAgICBpZiAoc2V0dXBNb3JlUG9zdHMpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmNvbm5lY3Rpb24uaW52b2tlKFwiZ2V0TGFzdFBvc3RJRFwiLCB0aGlzLnRvcGljSUQpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIC50aGVuKGZ1bmN0aW9uIChyZXN1bHQ6IG51bWJlcikge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZi5zZXRNb3JlUG9zdHNBdmFpbGFibGUocmVzdWx0KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuaXNSZXBseUxvYWRlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5jb21tZW50UmVwbHlJRCA9IDA7XHJcbiAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICB9XHJcblxyXG4gICAgcHJpdmF0ZSBjb25uZWN0aW9uOiBhbnk7XHJcblxyXG4gICAgLy8gdGhpcyBpcyBpbnRlbmRlZCB0byBiZSBjYWxsZWQgd2hlbiB0aGUgcmVwbHkgYm94IGlzIG9wZW5cclxuICAgIHByaXZhdGUgc2V0TW9yZVBvc3RzQXZhaWxhYmxlID0gKG5ld2VzdFBvc3RJRG9uU2VydmVyOiBudW1iZXIpID0+IHtcclxuICAgICAgICB0aGlzLmlzTmV3ZXJQb3N0c0F2YWlsYWJsZSA9IG5ld2VzdFBvc3RJRG9uU2VydmVyICE9PSB0aGlzLmxhc3RWaXNpYmxlUG9zdElEO1xyXG4gICAgfVxyXG5cclxuICAgIGxvYWRDb21tZW50KHRvcGljSUQ6IG51bWJlciwgcmVwbHlJRDogbnVtYmVyKTogdm9pZCB7XHJcbiAgICAgICAgdmFyIG4gPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiW2RhdGEtcG9zdGlkKj0nXCIgKyByZXBseUlEICsgXCInXSAuY29tbWVudEhvbGRlclwiKTtcclxuICAgICAgICBjb25zdCBib3hpZCA9IFwiY29tbWVudGJveFwiO1xyXG4gICAgICAgIG4uaWQgPSBib3hpZDtcclxuICAgICAgICB2YXIgcGF0aCA9IFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0ZvcnVtL1Bvc3RSZXBseS9cIiArIHRvcGljSUQgKyBcIj9yZXBseUlEPVwiICsgcmVwbHlJRDtcclxuICAgICAgICB0aGlzLmNvbW1lbnRSZXBseUlEID0gcmVwbHlJRDtcclxuICAgICAgICB0aGlzLmlzUmVwbHlMb2FkZWQgPSB0cnVlO1xyXG4gICAgICAgIGZldGNoKHBhdGgpXHJcbiAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLnRleHQoKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4odGV4dCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgbi5pbm5lckhUTUwgPSB0ZXh0O1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMuc2Nyb2xsVG9FbGVtZW50KGJveGlkKTtcclxuICAgICAgICAgICAgICAgIH0pKTtcclxuICAgIH07XHJcblxyXG4gICAgbG9hZE1vcmVQb3N0cyA9ICgpID0+IHtcclxuICAgICAgICBsZXQgdG9waWNQYWdlUGF0aDogc3RyaW5nO1xyXG4gICAgICAgIGlmICh0aGlzLmhpZ2hQYWdlID09PSB0aGlzLnBhZ2VDb3VudCkge1xyXG4gICAgICAgICAgICB0b3BpY1BhZ2VQYXRoID0gUG9wRm9ydW1zLkFyZWFQYXRoICsgXCIvRm9ydW0vVG9waWNQYXJ0aWFsL1wiICsgdGhpcy50b3BpY0lEICsgXCI/bGFzdFBvc3Q9XCIgKyB0aGlzLmxhc3RWaXNpYmxlUG9zdElEICsgXCImbG93UGFnZT1cIiArIHRoaXMubG93UGFnZTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZWxzZSB7XHJcbiAgICAgICAgICAgIHRoaXMuaGlnaFBhZ2UrKztcclxuICAgICAgICAgICAgdG9waWNQYWdlUGF0aCA9IFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0ZvcnVtL1RvcGljUGFnZS9cIiArIHRoaXMudG9waWNJRCArIFwiP3BhZ2VOdW1iZXI9XCIgKyB0aGlzLmhpZ2hQYWdlICsgXCImbG93PVwiICsgdGhpcy5sb3dQYWdlICsgXCImaGlnaD1cIiArIHRoaXMuaGlnaFBhZ2U7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGZldGNoKHRvcGljUGFnZVBhdGgpXHJcbiAgICAgICAgICAgIC50aGVuKHJlc3BvbnNlID0+IHJlc3BvbnNlLnRleHQoKVxyXG4gICAgICAgICAgICAgICAgLnRoZW4odGV4dCA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgbGV0IHQgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KFwidGVtcGxhdGVcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgdC5pbm5lckhUTUwgPSB0ZXh0LnRyaW0oKTtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgc3R1ZmYgPSB0LmNvbnRlbnQuZmlyc3RDaGlsZCBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgICAgICAgICAgICBsZXQgbGlua3MgPSBzdHVmZi5xdWVyeVNlbGVjdG9yKFwiLnBhZ2VyTGlua3NcIik7XHJcbiAgICAgICAgICAgICAgICAgICAgc3R1ZmYucmVtb3ZlQ2hpbGQobGlua3MpO1xyXG4gICAgICAgICAgICAgICAgICAgIGxldCBsYXN0UG9zdElEID0gc3R1ZmYucXVlcnlTZWxlY3RvcihcIi5sYXN0UG9zdElEXCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQ7XHJcbiAgICAgICAgICAgICAgICAgICAgc3R1ZmYucmVtb3ZlQ2hpbGQobGFzdFBvc3RJRCk7XHJcbiAgICAgICAgICAgICAgICAgICAgbGV0IG5ld1BhZ2VDb3VudCA9IHN0dWZmLnF1ZXJ5U2VsZWN0b3IoXCIucGFnZUNvdW50XCIpIGFzIEhUTUxJbnB1dEVsZW1lbnQ7XHJcbiAgICAgICAgICAgICAgICAgICAgc3R1ZmYucmVtb3ZlQ2hpbGQobmV3UGFnZUNvdW50KTtcclxuICAgICAgICAgICAgICAgICAgICB0aGlzLmxhc3RWaXNpYmxlUG9zdElEID0gTnVtYmVyKGxhc3RQb3N0SUQudmFsdWUpO1xyXG4gICAgICAgICAgICAgICAgICAgIHRoaXMucGFnZUNvdW50ID0gTnVtYmVyKG5ld1BhZ2VDb3VudC52YWx1ZSk7XHJcbiAgICAgICAgICAgICAgICAgICAgbGV0IHBvc3RTdHJlYW0gPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI1Bvc3RTdHJlYW1cIik7XHJcbiAgICAgICAgICAgICAgICAgICAgcG9zdFN0cmVhbS5hcHBlbmQoc3R1ZmYpO1xyXG4gICAgICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIucGFnZXJMaW5rc1wiKS5mb3JFYWNoKHggPT4geC5yZXBsYWNlV2l0aChsaW5rcy5jbG9uZU5vZGUodHJ1ZSkpKTtcclxuICAgICAgICAgICAgICAgICAgICBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLnBvc3RJdGVtIGltZzpub3QoLmF2YXRhcilcIikuZm9yRWFjaCh4ID0+IHguY2xhc3NMaXN0LmFkZChcInBvc3RJbWFnZVwiKSk7XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHRoaXMuaGlnaFBhZ2UgPT0gdGhpcy5wYWdlQ291bnQgJiYgdGhpcy5sb3dQYWdlID09IDEpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIi5wYWdlckxpbmtzXCIpLmZvckVhY2goeCA9PiB4LnJlbW92ZSgpKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5sb2FkaW5nUG9zdHMgPSBmYWxzZTtcclxuICAgICAgICAgICAgICAgICAgICBpZiAoIXRoaXMuaXNTY3JvbGxBZGp1c3RlZCkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLnNjcm9sbFRvUG9zdEZyb21IYXNoKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgICAgIGlmICh0aGlzLmlzUmVwbHlMb2FkZWQpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgbGV0IHNlbGYgPSB0aGlzO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB0aGlzLmNvbm5lY3Rpb24uaW52b2tlKFwiZ2V0TGFzdFBvc3RJRFwiLCB0aGlzLnRvcGljSUQpXHJcbiAgICAgICAgICAgICAgICAgICAgICAgIC50aGVuKGZ1bmN0aW9uIChyZXN1bHQ6IG51bWJlcikge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgc2VsZi5zZXRNb3JlUG9zdHNBdmFpbGFibGUocmVzdWx0KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICAgfSkpO1xyXG4gICAgfTtcclxuXHJcbiAgICBsb2FkUHJldmlvdXNQb3N0cyA9ICgpID0+IHtcclxuICAgICAgICB0aGlzLmxvd1BhZ2UtLTtcclxuICAgICAgICBsZXQgdG9waWNQYWdlUGF0aCA9IFBvcEZvcnVtcy5BcmVhUGF0aCArIFwiL0ZvcnVtL1RvcGljUGFnZS9cIiArIHRoaXMudG9waWNJRCArIFwiP3BhZ2VOdW1iZXI9XCIgKyB0aGlzLmxvd1BhZ2UgKyBcIiZsb3c9XCIgKyB0aGlzLmxvd1BhZ2UgKyBcIiZoaWdoPVwiICsgdGhpcy5oaWdoUGFnZTtcclxuICAgICAgICBmZXRjaCh0b3BpY1BhZ2VQYXRoKVxyXG4gICAgICAgICAgICAudGhlbihyZXNwb25zZSA9PiByZXNwb25zZS50ZXh0KClcclxuICAgICAgICAgICAgICAgIC50aGVuKHRleHQgPT4ge1xyXG4gICAgICAgICAgICAgICAgICAgIGxldCB0ID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChcInRlbXBsYXRlXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHQuaW5uZXJIVE1MID0gdGV4dC50cmltKCk7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIHN0dWZmID0gdC5jb250ZW50LmZpcnN0Q2hpbGQgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgICAgICAgICAgICAgdmFyIGxpbmtzID0gc3R1ZmYucXVlcnlTZWxlY3RvcihcIi5wYWdlckxpbmtzXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgIHN0dWZmLnJlbW92ZUNoaWxkKGxpbmtzKTtcclxuICAgICAgICAgICAgICAgICAgICB2YXIgcG9zdFN0cmVhbSA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjUG9zdFN0cmVhbVwiKTtcclxuICAgICAgICAgICAgICAgICAgICBwb3N0U3RyZWFtLnByZXBlbmQoc3R1ZmYpO1xyXG4gICAgICAgICAgICAgICAgICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIucGFnZXJMaW5rc1wiKS5mb3JFYWNoKHggPT4geC5yZXBsYWNlV2l0aChsaW5rcy5jbG9uZU5vZGUodHJ1ZSkpKTtcclxuICAgICAgICAgICAgICAgICAgICBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLnBvc3RJdGVtIGltZzpub3QoLmF2YXRhcilcIikuZm9yRWFjaCh4ID0+IHguY2xhc3NMaXN0LmFkZChcInBvc3RJbWFnZVwiKSk7XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHRoaXMuaGlnaFBhZ2UgPT0gdGhpcy5wYWdlQ291bnQgJiYgdGhpcy5sb3dQYWdlID09IDEpIHtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIi5wYWdlckxpbmtzXCIpLmZvckVhY2goeCA9PiB4LnJlbW92ZSgpKTtcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9KSk7XHJcbiAgICB9XHJcblxyXG4gICAgc2Nyb2xsTG9hZCA9ICgpID0+IHtcclxuICAgICAgICBsZXQgc3RyZWFtRW5kID0gKGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjU3RyZWFtQm90dG9tXCIpIGFzIEhUTUxFbGVtZW50KTtcclxuICAgICAgICBpZiAoIXN0cmVhbUVuZClcclxuICAgICAgICAgICAgcmV0dXJuOyAvLyB0aGlzIGlzIGEgUUEgdG9waWMsIG5vIGNvbnRpbnVvdXMgcG9zdCBzdHJlYW1cclxuICAgICAgICBsZXQgdG9wID0gc3RyZWFtRW5kLm9mZnNldFRvcDtcclxuICAgICAgICBsZXQgdmlld0VuZCA9IHdpbmRvdy5zY3JvbGxZICsgd2luZG93Lm91dGVySGVpZ2h0O1xyXG4gICAgICAgIGxldCBkaXN0YW5jZSA9IHRvcCAtIHZpZXdFbmQ7XHJcbiAgICAgICAgaWYgKCF0aGlzLmxvYWRpbmdQb3N0cyAmJiBkaXN0YW5jZSA8IDI1MCAmJiB0aGlzLmhpZ2hQYWdlIDwgdGhpcy5wYWdlQ291bnQpIHtcclxuICAgICAgICAgICAgdGhpcy5sb2FkaW5nUG9zdHMgPSB0cnVlO1xyXG4gICAgICAgICAgICB0aGlzLmxvYWRNb3JlUG9zdHMoKTtcclxuICAgICAgICB9XHJcbiAgICB9O1xyXG5cclxuICAgIHNjcm9sbFRvRWxlbWVudCA9IChpZDogc3RyaW5nKSA9PiB7XHJcbiAgICAgICAgbGV0IGUgPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZChpZCkgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgbGV0IHQgPSAwO1xyXG4gICAgICAgIGlmIChlLm9mZnNldFBhcmVudCkge1xyXG4gICAgICAgICAgICB3aGlsZSAoZS5vZmZzZXRQYXJlbnQpIHtcclxuICAgICAgICAgICAgICAgIHQgKz0gZS5vZmZzZXRUb3A7XHJcbiAgICAgICAgICAgICAgICBlID0gZS5vZmZzZXRQYXJlbnQgYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgICAgIH1cclxuICAgICAgICB9IGVsc2UgaWYgKGUuZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCkueSkge1xyXG4gICAgICAgICAgICB0ICs9IGUuZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCkueTtcclxuICAgICAgICB9XHJcbiAgICAgICAgbGV0IGNydW1iID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNUb3BCcmVhZGNydW1iXCIpIGFzIEhUTUxFbGVtZW50O1xyXG4gICAgICAgIGlmIChjcnVtYilcclxuICAgICAgICAgICAgdCAtPSBjcnVtYi5vZmZzZXRIZWlnaHQ7XHJcbiAgICAgICAgc2Nyb2xsVG8oMCwgdCk7XHJcbiAgICB9O1xyXG5cclxuICAgIHNjcm9sbFRvUG9zdEZyb21IYXNoID0gKCkgPT4ge1xyXG4gICAgICAgIGlmICh3aW5kb3cubG9jYXRpb24uaGFzaCkge1xyXG4gICAgICAgICAgICBQcm9taXNlLmFsbChBcnJheS5mcm9tKGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIjUG9zdFN0cmVhbSBpbWdcIikpXHJcbiAgICAgICAgICAgICAgICAuZmlsdGVyKGltZyA9PiAhKGltZyBhcyBIVE1MSW1hZ2VFbGVtZW50KS5jb21wbGV0ZSlcclxuICAgICAgICAgICAgICAgIC5tYXAoaW1nID0+IG5ldyBQcm9taXNlKHJlc29sdmUgPT4geyAoaW1nIGFzIEhUTUxJbWFnZUVsZW1lbnQpLm9ubG9hZCA9IChpbWcgYXMgSFRNTEltYWdlRWxlbWVudCkub25lcnJvciA9IHJlc29sdmU7IH0pKSlcclxuICAgICAgICAgICAgICAgICAgICAudGhlbigoKSA9PiB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGxldCBoYXNoID0gd2luZG93LmxvY2F0aW9uLmhhc2g7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHdoaWxlIChoYXNoLmNoYXJBdCgwKSA9PT0gJyMnKSBoYXNoID0gaGFzaC5zdWJzdHJpbmcoMSk7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGxldCB0YWcgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiZGl2W2RhdGEtcG9zdElEPSdcIiArIGhhc2ggKyBcIiddXCIpO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICBpZiAodGFnKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICBsZXQgdGFnUG9zaXRpb24gPSB0YWcuZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCkudG9wO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgbGV0IGNydW1iID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcihcIiNGb3J1bUNvbnRhaW5lciAjVG9wQnJlYWRjcnVtYlwiKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGxldCBjcnVtYkhlaWdodCA9IGNydW1iLmdldEJvdW5kaW5nQ2xpZW50UmVjdCgpLmhlaWdodDtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGxldCBlID0gZ2V0Q29tcHV0ZWRTdHlsZShkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiLnBvc3RJdGVtXCIpKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGxldCBtYXJnaW4gPSBwYXJzZUZsb2F0KGUubWFyZ2luVG9wKTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgICAgIGxldCBuZXdQb3NpdGlvbiA9IHRhZ1Bvc2l0aW9uIC0gY3J1bWJIZWlnaHQgLSBtYXJnaW47XHJcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICB3aW5kb3cuc2Nyb2xsQnkoeyB0b3A6IG5ld1Bvc2l0aW9uLCBiZWhhdmlvcjogJ2F1dG8nIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHRoaXMuaXNTY3JvbGxBZGp1c3RlZCA9IHRydWU7XHJcbiAgICAgICAgICAgICAgICAgICAgfSk7XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxuXHJcbiAgICBzZXRBbnN3ZXIocG9zdElEOiBudW1iZXIsIHRvcGljSUQ6IG51bWJlcikge1xyXG4gICAgICAgIHZhciBtb2RlbCA9IHsgcG9zdElEOiBwb3N0SUQsIHRvcGljSUQ6IHRvcGljSUQgfTtcclxuICAgICAgICBmZXRjaChQb3BGb3J1bXMuQXJlYVBhdGggKyBcIi9Gb3J1bS9TZXRBbnN3ZXIvXCIsIHtcclxuICAgICAgICAgICAgbWV0aG9kOiBcIlBPU1RcIixcclxuICAgICAgICAgICAgYm9keTogSlNPTi5zdHJpbmdpZnkobW9kZWwpLFxyXG4gICAgICAgICAgICBoZWFkZXJzOiB7XHJcbiAgICAgICAgICAgICAgICBcIkNvbnRlbnQtVHlwZVwiOiBcImFwcGxpY2F0aW9uL2pzb25cIlxyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfSlcclxuICAgICAgICAgICAgLnRoZW4ocmVzcG9uc2UgPT4ge1xyXG4gICAgICAgICAgICAgICAgdGhpcy5hbnN3ZXJQb3N0SUQgPSBwb3N0SUQ7XHJcbiAgICAgICAgICAgIH0pO1xyXG4gICAgfVxyXG59XHJcblxyXG59IiwibmFtZXNwYWNlIFBvcEZvcnVtcyB7XHJcblxyXG5leHBvcnQgY2xhc3MgVXNlclN0YXRlIGV4dGVuZHMgU3RhdGVCYXNlIHtcclxuICAgIGNvbnN0cnVjdG9yKCkge1xyXG4gICAgICAgIHN1cGVyKCk7XHJcbiAgICAgICAgdGhpcy5pc1BsYWluVGV4dCA9IGZhbHNlO1xyXG4gICAgICAgIHRoaXMubmV3UG1Db3VudCA9IDA7XHJcbiAgICAgICAgdGhpcy5ub3RpZmljYXRpb25TZXJ2aWNlID0gbmV3IE5vdGlmaWNhdGlvblNlcnZpY2UodGhpcyk7XHJcbiAgICB9XHJcblxyXG4gICAgcHJpdmF0ZSBub3RpZmljYXRpb25TZXJ2aWNlOiBOb3RpZmljYXRpb25TZXJ2aWNlO1xyXG4gICAgXHJcbiAgICBpc1BsYWluVGV4dDogYm9vbGVhbjtcclxuICAgIGlzSW1hZ2VFbmFibGVkOiBib29sZWFuO1xyXG5cclxuICAgIEBXYXRjaFByb3BlcnR5XHJcbiAgICBuZXdQbUNvdW50OiBudW1iZXI7XHJcbn1cclxuXHJcbn0iXX0=