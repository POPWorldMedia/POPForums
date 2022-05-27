/// <reference path="../WatchPropertyAttribute.ts" />
/// <reference path="../StateBase.ts" />

namespace PopForums {

export class TopicState extends StateBase {
    constructor() {
        super();
    }

    topicID: number;
    isImageEnabled: boolean;
    @WatchProperty
    isReplyLoaded: boolean;
    
	lowPage:number = 1;
	highPage: number = 1;
	lastVisiblePostID:number = null;
    pageIndex: number;
	pageCount: number;
	loadingPosts: boolean = false;
	isScrollAdjusted: boolean = false;

    @WatchProperty
    nextQuote: string;
    @WatchProperty
    isSubscribed: boolean;
    @WatchProperty
    isFavorite: boolean;

    setupTopic() {
        this.isReplyLoaded = false;
        // signalR connections

        // listen for mini profiles

        // listen for more posts button

        // listen for previous posts button

        document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));

        this.scrollToPostFromHash();
        window.addEventListener("scroll", this.scrollLoad);
    }

    loadReply(topicID:number, replyID:number, setupMorePosts:boolean):void {
        if (this.isReplyLoaded) {
            this.scrollToElement("NewReply");
            return;
        }
        window.removeEventListener("scroll", this.scrollLoad);
        var path = PopForums.areaPath + "/Forum/PostReply/" + topicID;
        if (replyID != null) {
            path += "?replyID=" + replyID;
        }
    
        fetch(path)
            .then(response => response.text()
                .then(text => {
                    var n = document.querySelector("#NewReply") as HTMLElement;
                    n.innerHTML = text;
                    n.style.display = "block";
                    this.scrollToElement("NewReply");
    
                    document.querySelector("#MorePostsBeforeReplyButton")?.addEventListener("click", (e) => {
                        var topicPartialPath = PopForums.areaPath + "/Forum/TopicPartial/" + topicID + "?lastPost=" + this.lastVisiblePostID + "&lowpage=" + this.lowPage;
                        fetch(topicPartialPath)
                            .then(response => response.text()
                                .then(text => {
                                    var t = document.createElement("template");
                                    t.innerHTML = text.trim();
                                    var stuff = t.content.firstChild as HTMLElement;
                                    var links = stuff.querySelector(".pagerLinks");
                                    stuff.removeChild(links);
                                    var lastPostID = stuff.querySelector(".lastPostID") as HTMLInputElement;
                                    stuff.removeChild(lastPostID);
                                    this.lastVisiblePostID = Number(lastPostID.value);
                                    var postStream = document.querySelector("#PostStream");
                                    postStream.append(stuff);
                                    document.querySelectorAll(".pagerLinks").forEach(x => x.replaceWith(links.cloneNode(true)));
                                    document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));
                                    (document.querySelector("#MorePostsBeforeReplyButton") as HTMLElement).style.visibility = "hidden";
                                    var moreButton = document.querySelector(".morePostsButton");
                                    if (moreButton)
                                        moreButton.remove();
                                    this.isReplyLoaded = true;
                                    // *********************** this is wrong
                                    this.setReplyMorePosts(this.lastVisiblePostID);
                                }));
                    });
    
                    if (setupMorePosts) {
                        let self = this;
                        let connection = new signalR.HubConnectionBuilder().withUrl("/TopicsHub").build();
                        connection.start()
                            .then(function () {
                                var result = connection.invoke("getLastPostID", topicID)
                                    .then(function (result: any) {
                                        self.setReplyMorePosts(result);
                                    });
                            });
                    }
    
                    PopForums.currentTopicState.isReplyLoaded = true; // TODO: temporary for new library
                }));
    }

    setReplyMorePosts = (lastPostID: number) => {
        var lastPostLoaded = lastPostID === this.lastVisiblePostID;
        var button = document.querySelector("#MorePostsBeforeReplyButton") as HTMLElement;
        if (lastPostLoaded)
            button.style.visibility = "hidden";
        else
            button.style.visibility = "visible";
    };

    loadMorePosts = (topicID: number, clickedButton: HTMLElement) => {
        this.highPage++;
        var nextPage = this.highPage;
        var id = topicID;
        var topicPartialPath = PopForums.areaPath + "/Forum/TopicPage/" + id + "?pageNumber=" + nextPage + "&low=" + this.lowPage + "&high=" + this.highPage;
        fetch(topicPartialPath)
            .then(response => response.text()
                .then(text => {
                    var t = document.createElement("template");
                    t.innerHTML = text.trim();
                    var stuff = t.content.firstChild as HTMLElement;
                    var links = stuff.querySelector(".pagerLinks");
                    stuff.removeChild(links);
                    var lastPostID = stuff.querySelector(".lastPostID") as HTMLInputElement;
                    stuff.removeChild(lastPostID);
                    var newPageCount = stuff.querySelector(".pageCount") as HTMLInputElement;
                    stuff.removeChild(newPageCount);
                    this.lastVisiblePostID = Number(lastPostID.value);
                    this.pageCount = Number(newPageCount.value);
                    var postStream = document.querySelector("#PostStream");
                    postStream.append(stuff);
                    document.querySelectorAll(".pagerLinks").forEach(x => x.replaceWith(links.cloneNode(true)));
                    document.querySelectorAll(".postItem img:not(.avatar)").forEach(x => x.classList.add("postImage"));
                    clickedButton.remove();
                    if (this.highPage != this.pageCount)
                        postStream.append(clickedButton);
                    if (this.highPage == this.pageCount && this.lowPage == 1) {
                        document.querySelectorAll(".pagerLinks").forEach(x => x.remove());
                    }
                    this.loadingPosts = false;
                    if (!this.isScrollAdjusted) {
                        this.scrollToPostFromHash();
                    }
                }));
    };

    scrollLoad = () => {
        var streamEnd = (document.querySelector("#StreamBottom") as HTMLElement).offsetTop;
        var viewEnd = window.scrollY + window.outerHeight;
        var distance = streamEnd - viewEnd;
        if (!this.loadingPosts && distance < 250 && this.highPage < this.pageCount) {
            this.loadingPosts = true;
            var button = document.querySelector(".morePostsButton") as HTMLElement;
            this.loadMorePosts(this.topicID, button);
        }
    };

    scrollToElement = (id: string) => {
        var e = document.getElementById(id) as HTMLElement;
        var t = 0;
        if (e.offsetParent) {
            while (e.offsetParent) {
                t += e.offsetTop;
                e = e.offsetParent as HTMLElement;
            }
        } else if (e.getBoundingClientRect().y) {
            t += e.getBoundingClientRect().y;
        }
        var crumb = document.querySelector("#TopBreadcrumb") as HTMLElement;
        if (crumb)
            t -= crumb.offsetHeight;
        scrollTo(0, t);
    };

    scrollToPostFromHash = () => {
        if (window.location.hash) {
            var hash = window.location.hash;
            while (hash.charAt(0) === '#') hash = hash.substr(1);
            var tag = document.querySelector("div[data-postID='" + hash + "']");
            if (tag) {
                var tagPosition = tag.getBoundingClientRect().top;
                var crumb = document.querySelector("#ForumContainer #TopBreadcrumb");
                var crumbHeight = crumb.getBoundingClientRect().height;
                var e = getComputedStyle(document.querySelector(".postItem"));
                var margin = parseFloat(e.marginTop);
                var newPosition = tagPosition - crumbHeight - margin;
                window.scrollBy({ top: newPosition, behavior: 'auto' });
            }
            this.isScrollAdjusted = true;
        }
    };
}

}