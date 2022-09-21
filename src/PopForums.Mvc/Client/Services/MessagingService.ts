namespace PopForums {

	export class MessagingService {
		private static service: MessagingService;

		static async GetService(): Promise<MessagingService> {
			if (!MessagingService.service) {
				let service = new MessagingService();
				await service.start();
				MessagingService.service = service;
			}
			return MessagingService.service;
		}

		connection: any;
		private reconnectHandlers: Array<Function>;

		async start() {
			this.connection = new signalR.HubConnectionBuilder().withUrl("/PopForumsHub").withAutomaticReconnect().build();

			await this.connection.start();

			this.connection.onreconnected(() => {
				if (this.reconnectHandlers)
					for (let f of this.reconnectHandlers)
						f();
			});
		}

		registerReconnectHandler(f: Function) {
			if (!this.reconnectHandlers)
				this.reconnectHandlers = new Array<Function>();
			this.reconnectHandlers.push(f);
		}
	}
}