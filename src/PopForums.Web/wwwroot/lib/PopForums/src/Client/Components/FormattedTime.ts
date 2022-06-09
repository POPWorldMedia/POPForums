namespace PopForums {

    export class FormattedTime extends HTMLElement {
    constructor() {
        super();
    }

    get utctime(): string {
        return this.getAttribute("utctime");
    }

    private utcTime: number;
    private utcTimeAsDate: Date;
    private static dayInMs = 86400000;

    connectedCallback() {
        this.setBaseTime();
        let now = Date.now();
        let yesterdayMs = now - FormattedTime.dayInMs;
        let yesterdayTemp = new Date(yesterdayMs);
        let yesterday = new Date(yesterdayTemp.getFullYear(), yesterdayTemp.getMonth(), yesterdayTemp.getDate());
        this.innerHTML = this.GetDisplayTime();
        if (this.utcTime > yesterday.getTime())
            this.UpdateTimer();
    }

    private setBaseTime() {
        let baseTime = this.utctime;
        if (!baseTime.endsWith("Z"))
            baseTime = baseTime + "Z";
        this.utcTime = Date.parse(baseTime);
        this.utcTimeAsDate = new Date(baseTime);
    }
    
    private UpdateTimer(): void {
        setTimeout(() => {
            this.UpdateTimer();
            this.innerHTML = this.GetDisplayTime();
        }, 60000);
    }

    private GetDisplayTime(): string {
        let now = Date.now();
        let nowAsDate = new Date();
        let diff = now - this.utcTime;
        let yesterdayMs = now - 86400000;
        let yesterdayTemp = new Date(yesterdayMs);
        let yesterday = new Date(yesterdayTemp.getFullYear(), yesterdayTemp.getMonth(), yesterdayTemp.getDate());
        const dateOptions: Intl.DateTimeFormatOptions = { weekday: "long", year: "numeric", month: "long", day: "numeric" };
        const timeOptions: Intl.DateTimeFormatOptions = { hour: "numeric", minute: "2-digit" };
        if (diff > 3599000) {
          // more than an hour
            if (this.utcTimeAsDate.toLocaleDateString() === nowAsDate.toLocaleDateString())
            return PopForums.timeFormats.todayTime.replace("{0}", this.utcTimeAsDate.toLocaleTimeString(undefined, timeOptions));
            if (this.utcTimeAsDate.toLocaleDateString() === yesterday.toLocaleDateString())
                return PopForums.timeFormats.yesterdayTime.replace("{0}", this.utcTimeAsDate.toLocaleTimeString(undefined, timeOptions));
            return this.utcTimeAsDate.toLocaleDateString(undefined, dateOptions) + " " + this.utcTimeAsDate.toLocaleTimeString(undefined, timeOptions);
        }
        if (diff > 120000)
            return  PopForums.timeFormats.minutesAgo.replace("{0}", Math.round(diff / 60000).toString());
        if (diff > 60000)
            return PopForums.timeFormats.oneMinuteAgo;
        return PopForums.timeFormats.lessThanMinute;
    }

    static get observedAttributes() { return ["utctime"]; }

    attributeChangedCallback(name: string, oldValue: string, newValue: string) {
        if (name === "utctime") {
            this.setBaseTime();
            this.innerHTML = this.GetDisplayTime();
            this.UpdateTimer();
        }
      }
}

customElements.define('pf-formattedtime', FormattedTime);

}