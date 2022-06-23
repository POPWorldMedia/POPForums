namespace PopForums {

export class UserState extends StateBase {
    constructor() {
        super();
        this.isPlainText = false;
        this.newPmCount = 0;
        this.notifications = new Array<Notification>();
        this.notificationCount = 0;
        this.notificationService = new NotificationService(this);
    }

    private notificationService: NotificationService;
    
    isPlainText: boolean;
    isImageEnabled: boolean;

    @WatchProperty
    newPmCount: number;
    @WatchProperty
    notificationCount: number;
    @WatchProperty
    notifications: Array<Notification>;

    LoadNotifications(): void {
        this.notificationService.LoadNotifications();
    }

    async MarkRead(contextID: number, notificationType: number) : Promise<void> {
        await this.notificationService.MarkRead(contextID, notificationType);
    }
}

}