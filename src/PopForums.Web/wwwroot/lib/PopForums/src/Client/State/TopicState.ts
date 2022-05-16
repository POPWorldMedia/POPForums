/// <reference path="../WatchPropertyAttribute.ts" />
/// <reference path="../StateBase.ts" />

class TopicState extends StateBase {
    constructor() {
        super();
        this.isImageEnabled = false;
        this.isPlainText = false;
        this.isReplyLoaded = false;
    }

    topicID: number;
    isPlainText: boolean;
    isImageEnabled: boolean;
    isReplyLoaded: boolean;

    @WatchProperty
    nextQuote: string;
}