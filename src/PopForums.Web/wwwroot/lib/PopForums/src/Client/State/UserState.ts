namespace PopForums {

export class UserState extends StateBase {
    constructor() {
        super();
        this.isPlainText = false;
        this.newPmCount = 0;
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
}

}