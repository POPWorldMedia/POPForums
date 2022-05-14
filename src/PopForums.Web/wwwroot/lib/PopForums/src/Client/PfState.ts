/// <reference path="WatchPropertyAttribute.ts" />
/// <reference path="StateBase.ts" />

class PfState extends StateBase {
    constructor() {
        super();
        this.newPmCount = 0;
    }

    @WatchProperty
    newPmCount: number;

    isPlainText: boolean;
}