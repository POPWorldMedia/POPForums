const WatchProperty = (target: any, memberName: string) => {
    let currentValue: any = target[memberName];  
    Object.defineProperty(target, memberName, {
        set(this: any, newValue: any) {
            currentValue = newValue;
            this.notify(memberName);
        },
        get() {return currentValue;}
    });
};