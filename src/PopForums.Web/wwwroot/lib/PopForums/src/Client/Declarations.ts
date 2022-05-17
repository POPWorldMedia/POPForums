declare var topicState: TopicState;
declare var userState: UserState;

declare namespace tinymce {
    function init(options:any): any;
    function get(id:string): any;
    function triggerSave(): any;
}

declare namespace bootstrap {
    class Tooltip{
        constructor(el: Element, options:any)
    }
}

declare namespace PopForums {
    function loadReply(topicID: number, postID: number, replyID: number, setupMorePosts: boolean): void;
    export interface TopicState{
        replyLoaded: boolean;
    }
    function areaPath(): string;
}
