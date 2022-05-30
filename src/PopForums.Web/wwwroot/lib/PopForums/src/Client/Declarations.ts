namespace PopForums {
    export const AreaPath = "/Forums";
    export var currentTopicState: PopForums.TopicState;
    export var userState: PopForums.UserState;

    export function Ready(callback: any): void {
        if (document.readyState != "loading") callback();
        else document.addEventListener("DOMContentLoaded", callback);
    }
}


declare namespace tinymce {
    function init(options:any): any;
    function get(id:string): any;
    function triggerSave(): any;
}

declare namespace bootstrap {
    class Tooltip {
        constructor(el: Element, options:any);
    }
    class Popover {
        constructor(el: Element, options:any);
        enable(): void;
        disable(): void;
    }
}

declare namespace signalR {
    class HubConnectionBuilder {
        withUrl(url: string): any;
    }
}