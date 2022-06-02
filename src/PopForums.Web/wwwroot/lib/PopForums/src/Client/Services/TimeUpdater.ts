// TODO: Move this to an open websockets connection

namespace PopForums {
    export class TimeUpdater {
        Start() {
            Ready(() => {
                this.StartUpdater();
            });
        }

        private timeArray: string[];

        private PopulateArray(): void {
            this.timeArray = [];
            let times = document.querySelectorAll(".fTime");
            times.forEach(time => {
                var t = time.getAttribute("data-utc");
                if (((new Date().getDate() - new Date(t + "Z").getDate()) / 3600000) < 48)
                    this.timeArray.push(t);
            });
        }

        private StartUpdater(): void {
            setTimeout(() => {
                this.StartUpdater();
                this.PopulateArray();
                this.CallForUpdate();
            }, 60000);
        }

        private CallForUpdate(): void {
            if (!this.timeArray || this.timeArray.length === 0)
                return;
            let serialized = JSON.stringify(this.timeArray);
            fetch(PopForums.AreaPath + "/Time/GetTimes", {
                method: "POST",
                body: serialized,
                headers: {
                    "Content-Type": "application/json"
                }
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