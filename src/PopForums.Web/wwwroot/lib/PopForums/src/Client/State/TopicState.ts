namespace PopForums {

export class TopicState extends StateBase {
    constructor() {
        super();
    }

    topicID: number;
    isImageEnabled: boolean;
    @WatchProperty
    isReplyLoaded: boolean;
    @WatchProperty
    answerPostID: number;
    @WatchProperty
	lowPage:number;
    @WatchProperty
	highPage: number;
	lastVisiblePostID: number;
    @WatchProperty
    isNewerPostsAvailable: boolean;
    pageIndex: number;
	pageCount: number;
	loadingPosts: boolean = false;
	isScrollAdjusted: boolean = false;
    @WatchProperty
    commentReplyID: number;
    @WatchProperty
    nextQuote: string;
    @WatchProperty
    isSubscribed: boolean;
    @WatchProperty
    isFavorite: boolean;
    selection: Selection;

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
            connection.on("fetchNewPost", function (postID: number) {
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
            connection.on("notifyNewPosts", function (theLastPostID: number) {
                self.setMorePostsAvailable(theLastPostID);
            });
            connection.start()
                .then(function () {
                    return connection.invoke("listenTo", self.topicID);
                })
                .then(function () {
                    self.connection = connection
                });

            document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));

            this.scrollToPostFromHash();
            window.addEventListener("scroll", this.scrollLoad);

            // compensate for iOS losing selection when you touch the quote button
            document.querySelectorAll(".postBody").forEach( x => x.addEventListener("mouseup", (e) => {
                this.selection = document.getSelection();
            }));
        });
    }

    loadReply(topicID:number, replyID:number, setupMorePosts:boolean):void {
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
                    let n = document.querySelector("#NewReply") as HTMLElement;
                    n.innerHTML = text;
                    n.style.display = "block";
                    this.scrollToElement("NewReply");
                    this.isReplyLoaded = true;
    
                    if (setupMorePosts) {
                        let self = this;
                        this.connection.invoke("getLastPostID", this.topicID)
                        .then(function (result: number) {
                            self.setMorePostsAvailable(result);
                        });
                    }
                    this.isReplyLoaded = true;
                    this.commentReplyID = 0;
                }));
    }

    private connection: any;

    // this is intended to be called when the reply box is open
    private setMorePostsAvailable = (newestPostIDonServer: number) => {
        this.isNewerPostsAvailable = newestPostIDonServer !== this.lastVisiblePostID;
    }

    loadComment(topicID: number, replyID: number): void {
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
    };

    loadMorePosts = () => {
        let topicPagePath: string;
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
                    let stuff = t.content.firstChild as HTMLElement;
                    let links = stuff.querySelector(".pagerLinks");
                    stuff.removeChild(links);
                    let lastPostID = stuff.querySelector(".lastPostID") as HTMLInputElement;
                    stuff.removeChild(lastPostID);
                    let newPageCount = stuff.querySelector(".pageCount") as HTMLInputElement;
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
                        .then(function (result: number) {
                            self.setMorePostsAvailable(result);
                        });
                    }
                }));
    };

    loadPreviousPosts = () => {
        this.lowPage--;
        let topicPagePath = PopForums.AreaPath + "/Forum/TopicPage/" + this.topicID + "?pageNumber=" + this.lowPage + "&low=" + this.lowPage + "&high=" + this.highPage;
        fetch(topicPagePath)
            .then(response => response.text()
                .then(text => {
                    let t = document.createElement("template");
                    t.innerHTML = text.trim();
                    var stuff = t.content.firstChild as HTMLElement;
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
    }

    scrollLoad = () => {
        let streamEnd = (document.querySelector("#StreamBottom") as HTMLElement);
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

    scrollToElement = (id: string) => {
        let e = document.getElementById(id) as HTMLElement;
        let t = 0;
        if (e.offsetParent) {
            while (e.offsetParent) {
                t += e.offsetTop;
                e = e.offsetParent as HTMLElement;
            }
        } else if (e.getBoundingClientRect().y) {
            t += e.getBoundingClientRect().y;
        }
        let crumb = document.querySelector("#TopBreadcrumb") as HTMLElement;
        if (crumb)
            t -= crumb.offsetHeight;
        scrollTo(0, t);
    };

    scrollToPostFromHash = () => {
        if (window.location.hash) {
            Promise.all(Array.from(document.querySelectorAll("#PostStream img"))
                .filter(img => !(img as HTMLImageElement).complete)
                .map(img => new Promise(resolve => { (img as HTMLImageElement).onload = (img as HTMLImageElement).onerror = resolve; })))
                    .then(() => {
                        let hash = window.location.hash;
                        while (hash.charAt(0) === '#') hash = hash.substring(1);
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

    setAnswer(postID: number, topicID: number) {
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

}