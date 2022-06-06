namespace PopForums {

    export class NotificationService {
        constructor(userState: UserState) {
            this.userState = userState;
            let self = this;
            this.connection = new signalR.HubConnectionBuilder().withUrl("/NotificationHub").build();
            this.connection.on("updatePMCount", function(pmCount: number) {
                self.userState.newPmCount = pmCount;
            });
            this.connection.start();
        }

        private userState: UserState;
        private connection: any;
    }
}