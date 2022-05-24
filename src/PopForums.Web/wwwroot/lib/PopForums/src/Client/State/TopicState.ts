/// <reference path="../WatchPropertyAttribute.ts" />
/// <reference path="../StateBase.ts" />

namespace PopForums {

export class TopicState extends StateBase {
    constructor() {
        super();
        this.isReplyLoaded = false;
    }

    topicID: number;
    isImageEnabled: boolean;
    isReplyLoaded: boolean;

    @WatchProperty
    nextQuote: string;

    @WatchProperty
    isSubscribed: boolean;

    @WatchProperty
    isFavorite: boolean;
}

}