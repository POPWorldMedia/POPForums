// import { timesTranslated } from './times_translated';;

class BrowserDateTimeCulture {
	/* properties:
	public:
		getLocaleDateTime;
	private:
		_getLocaleTime;
		_translate;
	*/
	constructor(pageLanguage, userLanguages) {
		if (!userLanguages) {
			userLanguages = [];
		}
		if (typeof userLanguages === 'string') {
			userLanguages = [userLanguages];
		}

		const bestDateCulture = userLanguages.find((l) => l.startsWith(pageLanguage))
			|| 'default'; //could use 'en' here?

		// Intl.DateTimeFormat is OK back to IE11
		this.getLocaleDateTime = (new Intl.DateTimeFormat(bestDateCulture, {
				year: 'numeric',
				month: 'numeric',
				day: 'numeric',
				hour: 'numeric',
				minute: 'numeric',
			})).format;
		this._getLocaleTime = (new Intl.DateTimeFormat(bestDateCulture, {
				hour: 'numeric',
				minute: 'numeric',
			})).format;
		this._translate = timesTranslated[pageLanguage];
	}

	getLocaleFeedTime(dt) {
		if (typeof dt === 'string') {
			dt = new Date(dt); //note IE <= 8 do not parse iso dates, everthing else fine
		}
		if (!dt instanceof Date) {
			// this will not work across iframes, but that should be irrelevant here
			// potentially future iterations will be typescript?
			throw new TypeError("dt argument of DateCultureHandler.getTime must be iso 8610 string or a Date");
		}
		if (isNaN(dt.getTime())) {
			return {
				disp: ''
			};
		}
		const now = new Date();
		const dateDiff = now.getDate() - dt.getDate();
		if (dateDiff <= 1) {
			const msPerMin = 60000;
			const minsAgo = (now.getTime() - dt.getTime()) / msPerMin;
			if (minsAgo < 1) {
				return {
					disp: this._translate.LessThanMinute,
					recalc: Math.max(0, (1 - minsAgo) * msPerMin)
				};
			}
			if (minsAgo < 60) {
				return {
					disp: this._translate.MinutesAgo.replace("{0}", minsAgo.toFixed()),
					recalc: Math.max(0, (1 - minsAgo % 1) * msPerMin)
				};
			}
			const formattedTime = this._getLocaleTime(dt);
			const nextMidnight = new Date(now);
			nextMidnight.setHours(0, 0, 0, 0);
			nextMidnight.setDate(now.getDate() + 1);
			return {
				recalc: nextMidnight.getTime() - now.getTime(),
				disp: dateDiff === 1
					? this._translate.YesterdayTime.replace("{0}", formattedTime)
					: this._translate.TodayTime.replace("{0}", formattedTime)
			};
		}
		return { disp: this.getLocaleDateTime(dt) };
	}
}
