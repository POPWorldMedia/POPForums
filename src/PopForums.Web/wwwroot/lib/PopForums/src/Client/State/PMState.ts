/// <reference path="../WatchPropertyAttribute.ts" />
/// <reference path="../StateBase.ts" />

class PMState extends StateBase {
    constructor() {
        super();
        this.newPmCount = 0;
    }

    @WatchProperty
    newPmCount: number;
}