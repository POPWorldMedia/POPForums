namespace PopForums {

    export class Notification {
        userID: number;
        timeStamp: string;
        isRead: boolean;
        notificationType: string;
        contextID: number;
        data: any;
    }
}