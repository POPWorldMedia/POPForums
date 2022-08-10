namespace PopForums {
    export const AreaPath = "/Forums";
    export var currentTopicState: TopicState;
    export var currentForumState: ForumState;
    export var currentPmState: PrivateMessageState;
    export var userState: UserState;
    export var localizations: Localizations;

    export function Ready(callback: any): void {
        if (document.readyState != "loading") callback();
        else document.addEventListener("DOMContentLoaded", callback);
    }
}


declare namespace tinymce {
    function init(options:any): any;
    function get(id:string): any;
    function triggerSave(): any;
    let activeEditor: any;
}

declare class BlobInfo {
    id: () => string;
    name: () => string;
    filename: () => string;
    blob: () => Blob;
    base64: () => string;
    blobUri: () => string;
    uri: () => string | undefined;
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
    class Offcanvas {
        constructor(el: Element);
        show(): void;
    }
}

declare namespace signalR {
    class HubConnectionBuilder {
        withUrl(url: string): any;
    }
}