namespace PopForums {

	export class MessagingService {
		private static service: MessagingService;

		static async GetService(): Promise<MessagingService> {
			if (!MessagingService.service) {
				MessagingService.service = new MessagingService();
				await MessagingService.service.start();
			}
			return MessagingService.service;
		}

		connection: any;
		private reconnectHandlers: Array<Function>;

		async start() {
			this.connection = new signalR.HubConnectionBuilder().withUrl("/PopForumsHub").withAutomaticReconnect().build();

			this.connection.onreconnected(async () => {
				for (let f of this.reconnectHandlers)
					f();
			});

			await this.connection.start();
		}

		registerReconnectHandler(f: Function) {
			if (!this.reconnectHandlers)
				this.reconnectHandlers = new Array<Function>();
			this.reconnectHandlers.push(f);
		}
	}
}