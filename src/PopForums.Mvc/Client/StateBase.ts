﻿namespace PopForums {

// Properties to watch require the @WatchProperty attribute.
export class StateBase {
    constructor() {
        this._subs = new Map<string, Array<Function>>();
    }

    private _subs: Map<string, Array<Function>>;

    subscribe(propertyName: string, eventHandler: Function) {
        if (!this._subs.has(propertyName))
            this._subs.set(propertyName, new Array<Function>());
        const callbacks = this._subs.get(propertyName);
        callbacks.push(eventHandler);
        eventHandler();
    }

    notify(propertyName: string) {
        const callbacks = this._subs.get(propertyName);
        if (callbacks)
            for (let i of callbacks) {
                i();
            }
    }
}

}