namespace PopForums {

    export class NotificationService {
        constructor(userState: UserState) {
            this.userState = userState;
        }

        private userState: UserState;
        private connection: any;

        async initialize(): Promise<void> {
            let self = this;
            let service = await MessagingService.GetService();
            this.connection = service.connection;
            this.connection.on("updatePMCount", function (pmCount: number) {
                self.userState.newPmCount = pmCount;
            });
            this.connection.on("notify", function (data: any) {
                let notification: Notification = Object.assign(new Notification(), data);
                let list = self.userState.list.querySelectorAll("pf-notificationitem");
                list.forEach(item => {
                    let nitem = (item as NotificationItem).notification;
                    if (nitem.contextID === notification.contextID && nitem.notificationType === notification.notificationType) {
                        item.remove();
                    }
                });
                self.userState.notificationCount = notification.unreadCount;
                self.userState.notifications.unshift(notification);
            });
            this.connection.onreconnected(async () => {
                let notificationCount = await this.connection.invoke("GetNotificationCount");
                self.userState.notificationCount = notificationCount;
                let pmCount = await this.connection.invoke("GetPMCount");
                self.userState.newPmCount = pmCount;
            });
		}

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