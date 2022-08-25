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
                let notification = Object.assign(new Notification(), data);
                let isExisting: boolean = false;
                let list = self.userState.list.querySelectorAll("pf-notificationitem");
                list.forEach(item => {
                    let nitem = (item as NotificationItem).notification;
                    if (nitem.contextID === notification.contextID && nitem.notificationType === notification.notificationType) {
                        isExisting = true;
                        item.remove();
                    }
                });
                if (!isExisting)
                    self.userState.notificationCount++;
                self.userState.notifications.unshift(notification);
            });
            this.connection.start();
        }

        private userState: UserState;
        private connection: any;

        async LoadNotifications(): Promise<void> {
            const json = await this.getNotifications();
            let a = new Array<Notification>();
            let isEnd = true;
            json.forEach((item: Notification) => {
                let n = Object.assign(new Notification(), item);
                a.push(n);
                this.userState.lastNotificationDate = n.timeStamp;
                isEnd = false;
            });
            this.userState.isNotificationEnd = isEnd;
            if (!isEnd)
                this.userState.notifications = a;
        }

        async MarkRead(contextID: number, notificationType: number) : Promise<void> {
            await this.connection.invoke("MarkNotificationRead", contextID, notificationType);
        }

        async MarkAllRead() : Promise<void> {
            await this.connection.send("MarkAllRead");
            let list = this.userState.list.querySelectorAll("pf-notificationitem");
            list.forEach(item => {
                (item as NotificationItem).MarkRead();
            });
            this.userState.notificationCount = 0;
        }

        private async getNotifications() {
            const response = await this.connection.invoke("GetNotifications", this.userState.lastNotificationDate);
            return response;
        }
    }
}