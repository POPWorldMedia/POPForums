/// <reference path="../WatchPropertyAttribute.ts" />
/// <reference path="../StateBase.ts" />

namespace PopForums {

export class UserState extends StateBase {
    constructor() {
        super();
        this.isPlainText = false;
        this.newPmCount = 0;
    }
    
    isPlainText: boolean;
    isImageEnabled: boolean;

    @WatchProperty
    newPmCount: number;
}

}