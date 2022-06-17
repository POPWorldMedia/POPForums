namespace PopForums {

    export class Notification {
        userID: number;
        timeStamp: string;
        isRead: boolean;
        notificationType: number;
        contextID: number;
        data: any;
    }
}