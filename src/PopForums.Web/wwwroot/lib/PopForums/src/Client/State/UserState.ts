/// <reference path="../WatchPropertyAttribute.ts" />
/// <reference path="../StateBase.ts" />

class UserState extends StateBase {
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