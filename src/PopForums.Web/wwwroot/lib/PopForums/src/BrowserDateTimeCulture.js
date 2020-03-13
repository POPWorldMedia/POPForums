// import { timesTranslated } from './times_translated';;

class BrowserDateTimeCulture {
	/* 
	properties:
		public:
			-
		private:
			_localeTimeFormatter;
			_localeDateFormatter
			_translate;
	methods:
		public:
			getLocaleDateTime
			getLocaleFeedTime
		private
			_getDate

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
		this._localeDateFormatter = (new Intl.DateTimeFormat(bestDateCulture, {
			year: 'numeric',
			month: 'numeric',
			day: 'numeric',
			hour: 'numeric',
			minute: 'numeric',
		})).format;
		this._localeTimeFormatter = (new Intl.DateTimeFormat(bestDateCulture, {
			hour: 'numeric',
			minute: 'numeric',
		})).format;
		this._translate = timesTranslated[pageLanguage];
	}
	getLocaleDateTime(dt) {
		dt = this._getDate(dt);
		return dt
			? this._localeDateFormatter(dt)
			: '';
	}
	getLocaleFeedTime(dt) {
		dt = this._getDate(dt);
		if (!dt) {
			return {
				disp: ''
			}
		};
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
			const formattedTime = this._localeTimeFormatter(dt);
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
		return { disp: this._localeDateFormatter(dt) };
	}
	_getDate(dt) {
		if (typeof dt === 'string') {
			dt = dt.trim();
			if (dt === '') { return null }
			dt = new Date(dt); //note IE <= 8 do not parse iso dates, everthing else fine
		}
		if (!dt instanceof Date || isNaN(dt.getTime())) {
			// this will not work across iframes, but that should be irrelevant here
			// potentially future iterations will be typescript?
			throw new TypeError("dt argument of DateCultureHandler.getTime must be iso 8610 string or a Valid Date");
		}
		return dt;
	}
}
/* note possible options are at https://devhints.io/wip/intl-datetime
{
  weekday: 'narrow' | 'short' | 'long',
  era: 'narrow' | 'short' | 'long',
  year: 'numeric' | '2-digit',
  month: 'numeric' | '2-digit' | 'narrow' | 'short' | 'long',
  day: 'numeric' | '2-digit',
  hour: 'numeric' | '2-digit',
  minute: 'numeric' | '2-digit',
  second: 'numeric' | '2-digit',
  timeZoneName: 'short' | 'long',

  // Time zone to express it in
  timeZone: 'Asia/Shanghai',
  // Force 12-hour or 24-hour
  hour12: true | false,

  // Rarely-used options
  hourCycle: 'h11' | 'h12' | 'h23' | 'h24',
  formatMatcher: 'basic' | 'best fit'
}
*/
