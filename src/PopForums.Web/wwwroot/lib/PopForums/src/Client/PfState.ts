/// <reference path="WatchPropertyAttribute.ts" />
/// <reference path="StateBase.ts" />

class PfState extends StateBase {
    constructor() {
        super();
        this.isLoggedIn = false;
        this.newPmCount = 0;
    }

    @WatchProperty
    isLoggedIn: boolean;

    @WatchProperty
    newPmCount: number;
}