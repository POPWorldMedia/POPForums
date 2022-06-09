namespace PopForums {

    export class LocalizationService {
        static init(): void {
            const path = PopForums.AreaPath + "/Resources";
            fetch(path)
                .then(response => {
                    return response.json();
                })
                .then(json => {
                    PopForums.localizations = Object.assign(new Localizations(), json);
                    return this.signal();
                });
        }

        private static signal() {
            PopForums.Ready(() => {
                if (this.readies) {
                    for (let i of this.readies) {
                        i();
                    }
                }
                this.isSignaled = true;
            });
        }

        static readies: Array<Function>;
        private static isSignaled: boolean = false;

        static subscribe(ready: Function): boolean {
            if (!this.readies)
                this.readies = new Array<Function>();
            this.readies.push(ready);
            return this.isSignaled;
        }
    }
}