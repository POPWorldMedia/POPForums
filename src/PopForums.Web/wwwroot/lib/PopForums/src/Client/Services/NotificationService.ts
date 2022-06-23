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
                self.userState.notificationCount++;
                let n = Object.assign(new Notification(), data);
                self.userState.notifications.unshift(n);
            });
            this.connection.start();
        }

        private userState: UserState;
        private connection: any;

        async LoadNotifications(): Promise<void> {
            const json = await this.getNotifications();
            let a = new Array<Notification>();
            json.forEach((item: Notification) => {
                let n = Object.assign(new Notification(), item);
                a.push(n);
            });
            this.userState.notifications = a;
        }

        async MarkRead(contextID: number, notificationType: number) : Promise<void> {
            await this.connection.invoke("MarkNotificationRead", contextID, notificationType);
        }

        async MarkAllRead() : Promise<void> {
            await this.connection.send("MarkAllRead");
            let notifications = this.userState.notifications;
            notifications.forEach(item => {
                item.isRead = true;
            });
            this.userState.notificationCount = 0;
            this.userState.notifications = notifications;
        }

        private async getNotifications() {
            const response = await this.connection.invoke("GetNotifications");
            return response;
        }
    }
}