namespace PopForums {

export declare var topicState: TopicState;
export declare var userState: UserState;

export declare namespace tinymce {
    function init(options:any): any;
    function get(id:string): any;
    function triggerSave(): any;
}

export declare namespace bootstrap {
    class Tooltip{
        constructor(el: Element, options:any)
    }
}

export declare namespace PopForums {
    function loadReply(topicID: number, postID: number, replyID: number, setupMorePosts: boolean): void;
    export interface TopicState{
        replyLoaded: boolean;
    }
    function areaPath(): string;
}

}