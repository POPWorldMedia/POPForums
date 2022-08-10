namespace PopForums{

export class PrivateMessageState extends StateBase {
    constructor() {
        super();
    }

    pmID: number;
    users: PrivateMessageUser[];
    messages: PrivateMessage[];

    setupPm() {
        PopForums.Ready(() => {
            
        });
    }
}

}