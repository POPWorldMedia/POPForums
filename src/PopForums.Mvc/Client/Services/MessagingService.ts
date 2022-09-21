namespace PopForums {

	export class MessagingService {
		private static service: MessagingService;

		static async GetService(): Promise<MessagingService> {
			if (!this.service) {
				let service = new MessagingService();
				await service.start();
				this.service = service;
			}
			return this.service;
		}

		connection: any;

		async start() {
			this.connection = new signalR.HubConnectionBuilder().withUrl("/PopForumsHub").withAutomaticReconnect().build();

			await this.connection.start();
		}
	}
}