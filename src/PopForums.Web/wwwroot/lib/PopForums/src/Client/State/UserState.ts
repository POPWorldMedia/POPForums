namespace PopForums {

export class UserState extends StateBase {
    constructor() {
        super();
        this.isPlainText = false;
        this.newPmCount = 0;
        this.notificationCount = 0;
        this.notificationService = new NotificationService(this);
        this.postImageIds = new Array<string>();
    }

    private notificationService: NotificationService;
    private isLoadingNotifications: boolean;
    private notificationPageCount: number;

    currentNotificationIndex: number;
    isPlainText: boolean;
    isImageEnabled: boolean;
    postImageIds: Array<string>;

    @WatchProperty
    newPmCount: number;
    @WatchProperty
    notificationCount: number;
    @WatchProperty
    notifications: Array<Notification>;

    list: HTMLElement;

    async LoadNotifications(): Promise<void> {
        this.notifications = new Array<Notification>();
        this.isLoadingNotifications = true;
        this.notificationPageCount = await this.notificationService.GetPageCount();
        this.currentNotificationIndex = 1;
        this.notificationService.LoadNotifications();
        this.isLoadingNotifications = false;
    }

    async MarkRead(contextID: number, notificationType: number) : Promise<void> {
        await this.notificationService.MarkRead(contextID, notificationType);
    }

    async MarkAllRead() : Promise<void> {
        await this.notificationService.MarkAllRead();
    }

    ScrollLoad = () => {
        let streamEnd = (document.querySelector("#NotificationBottom") as HTMLElement);
        if (!streamEnd) {
            console.log("Can't find bottom of notifications.");
            return;
        }
        let top = streamEnd.offsetTop;
        let viewEnd = this.list.scrollTop + this.list.clientHeight;
        let distance = top - viewEnd;
        if (!this.isLoadingNotifications && distance < 250 && this.currentNotificationIndex < this.notificationPageCount) {
            this.LoadMoreNotifications();
        }
    };

    private LoadMoreNotifications() {
        this.isLoadingNotifications = true;
        this.currentNotificationIndex++;
        this.notificationService.LoadNotifications();
        this.isLoadingNotifications = false;
    }
}

}