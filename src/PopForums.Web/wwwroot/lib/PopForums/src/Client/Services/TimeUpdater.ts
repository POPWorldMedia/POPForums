// TODO: Move this to an open websockets connection

namespace PopForums {
    export class TimeUpdater {
        Start() {
            Ready(() => {
                this.StartUpdater();
            });
        }

        private subHourTimes: FormData;

        private PopulatePostData(): void {
            let a: string[] = [];
            let times = document.querySelectorAll(".fTime");
            times.forEach(time => {
                var t = time.getAttribute("data-utc");
                if (((new Date().getDate() - new Date(t + "Z").getDate()) / 3600000) < 48)
                    a.push(t);
            });
            if (a.length > 0) {
                this.subHourTimes = new FormData();
                a.forEach(t => this.subHourTimes.append("times", t));
            }
        }

        private StartUpdater(): void {
            setTimeout(() => {
                this.StartUpdater();
                this.PopulatePostData();
                if (this.subHourTimes)
                    this.CallForUpdate();
            }, 60000);
        }

        private CallForUpdate(): void {
            fetch(PopForums.AreaPath + "/Time/GetTimes", {
                method: "POST",
                body: this.subHourTimes
            })
                .then(response => response.json())
                .then(data => {
                    data.forEach((t: { key: string; value: string; }) => {
                        document.querySelector(".fTime[data-utc='" + t.key + "']").innerHTML = t.value;
                    });
                })
                .catch(error => { console.log("Time update failure: " + error); });
        }
    }
}

var timeUpdater = new PopForums.TimeUpdater();
timeUpdater.Start();