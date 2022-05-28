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
    
    @WatchProperty
	lowPage:number;
    @WatchProperty
	highPage: number;
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
        this.lowPage = this.pageIndex;
        this.highPage = this.pageIndex;
        // signalR connections

        // listen for mini profiles

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
                    let n = document.querySelector("#NewReply") as HTMLElement;
                    n.innerHTML = text;
                    n.style.display = "block";
                    this.scrollToElement("NewReply");
                    this.isReplyLoaded = true;
    
                    // if (setupMorePosts) {
                    //     let self = this;
                    //     let connection = new signalR.HubConnectionBuilder().withUrl("/TopicsHub").build();
                    //     connection.start()
                    //         .then(function () {
                    //             var result = connection.invoke("getLastPostID", topicID)
                    //                 .then(function (result: any) {
                    //                     self.setReplyMorePosts(result);
                    //                 });
                    //         });
                    // }
    
                    PopForums.currentTopicState.isReplyLoaded = true; // TODO: temporary for new library
                }));
    }

    loadMorePosts = () => {
        this.highPage++;
        let nextPage = this.highPage;
        let id = this.topicID;
        let topicPartialPath = PopForums.AreaPath + "/Forum/TopicPage/" + id + "?pageNumber=" + nextPage + "&low=" + this.lowPage + "&high=" + this.highPage;
        fetch(topicPartialPath)
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
                }));
    };

    loadPreviousPosts = () => {
        this.lowPage--;
        let topicPartialPath = PopForums.areaPath + "/Forum/TopicPage/" + this.topicID + "?pageNumber=" + this.lowPage + "&low=" + this.lowPage + "&high=" + this.highPage;
        fetch(topicPartialPath)
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
        let streamEnd = (document.querySelector("#StreamBottom") as HTMLElement).offsetTop;
        let viewEnd = window.scrollY + window.outerHeight;
        let distance = streamEnd - viewEnd;
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
            let hash = window.location.hash;
            while (hash.charAt(0) === '#') hash = hash.substr(1);
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
        }
    };
}

}