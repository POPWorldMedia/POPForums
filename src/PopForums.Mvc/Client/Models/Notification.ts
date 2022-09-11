namespace PopForums {

    export class Notification {
        userID: number;
        timeStamp: Date;
        isRead: boolean;
        notificationType: number;
        contextID: number;
        data: any;
        unreadCount: number;
    }
}