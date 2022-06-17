namespace PopForums {

    export class NotificationService {
        constructor(userState: UserState) {
            this.userState = userState;
            let self = this;
            this.connection = new signalR.HubConnectionBuilder().withUrl("/NotificationHub").withAutomaticReconnect().build();
            this.connection.on("updatePMCount", function(pmCount: number) {
                self.userState.newPmCount = pmCount;
            });
            this.connection.on("notify", function(data: any) {
                console.log(JSON.stringify(data));
            });
            this.connection.start();
        }

        private userState: UserState;
        private connection: any;

        LoadNotifications(): void{
            let notifications = fetch(PopForums.AreaPath + "/Api/Notifications")
                .then(response => {
                    return response.json();
                })
                .then(json => {
                    let a = new Array<Notification>();
                    json.forEach((item: Notification) => {
                        let n = Object.assign(new Notification(), item);
                        a.push(n);
                    });
                    return a;
                });
        }
    }
}