namespace PopForums {

	export class MessagingService {
		private static service: MessagingService;
		private static promise: Promise<void>;

		static async GetService(): Promise<MessagingService> {
			if (!this.promise) {
				const service = new MessagingService();
				this.promise = service.start();
				this.service = service;
			}
			await Promise.all([this.promise]);
			return this.service;
		}

		connection: any;

		private async start() {
			this.connection = new signalR.HubConnectionBuilder().withUrl("/PopForumsHub").withAutomaticReconnect().build();
			await this.connection.start();
		}
	}
}