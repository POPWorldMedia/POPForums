namespace PopForums {

export class UserState extends StateBase {
    constructor() {
        super();
    }

    private notificationService: NotificationService;
    private isLoadingNotifications: boolean;

    isPlainText: boolean;
    isImageEnabled: boolean;
    postImageIds: Array<string>;
    userID: number;
    lastNotificationDate: Date;
    isNotificationEnd: boolean;

    @WatchProperty
    newPmCount: number;
    @WatchProperty
    notificationCount: number;
    @WatchProperty
    notifications: Array<Notification>;

    list: HTMLElement;

    async initialize(): Promise<void> {
        this.isPlainText = false;
        this.newPmCount = 0;
        this.notificationCount = 0;
        this.postImageIds = new Array<string>();
        this.notificationService = new NotificationService(this);
        await this.notificationService.initialize();
	}

    async LoadNotifications(): Promise<void> {
        this.isLoadingNotifications = true;
        this.lastNotificationDate = new Date(2100, 1, 1);
        this.isNotificationEnd = false;
        this.notifications = new Array<Notification>();
        await this.notificationService.LoadNotifications();
        this.isLoadingNotifications = false;
    }

    async MarkRead(contextID: number, notificationType: number) : Promise<void> {
        await this.notificationService.MarkRead(contextID, notificationType);
    }

    async MarkAllRead() : Promise<void> {
        await this.notificationService.MarkAllRead();
    }

    ScrollLoad = async () => {
        if (this.isNotificationEnd)
            return;
        let streamEnd = (document.querySelector("#NotificationBottom") as HTMLElement);
        if (!streamEnd) {
            console.log("Can't find bottom of notifications.");
            return;
        }
        let top = streamEnd.offsetTop;
        let viewEnd = this.list.scrollTop + this.list.clientHeight;
        let distance = top - viewEnd;
        if (!this.isLoadingNotifications && distance < 250 && !this.isNotificationEnd) {
            await this.LoadMoreNotifications();
        }
    };

    private async LoadMoreNotifications() {
        this.isLoadingNotifications = true;
        await this.notificationService.LoadNotifications();
        this.isLoadingNotifications = false;
    }
}

}