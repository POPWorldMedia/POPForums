/// <reference path="../WatchPropertyAttribute.ts" />
/// <reference path="../StateBase.ts" />

namespace PopForums {

export class TopicState extends StateBase {
    constructor() {
        super();
        this.isImageEnabled = false;
        this.isReplyLoaded = false;
    }

    topicID: number;
    isImageEnabled: boolean;
    isReplyLoaded: boolean;

    @WatchProperty
    nextQuote: string;
}

}