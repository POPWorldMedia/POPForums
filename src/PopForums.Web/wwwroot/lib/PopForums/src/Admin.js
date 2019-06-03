const basePath = "/Forums/AdminApi/";

const Top = {
	template: "#Top",
	data() {
		return {
			loading: true
		}
	},
	mounted: function () {

	},
	methods: {
		setLoading: function (isLoading) {
			this.loading = isLoading;
		}
	}
}

var loadingMixin = {
	data() {
		return {
			alert: false,
			message: ""
		}
	},
	props: {
		loading: Boolean
	},
	methods: {
		startLoad: function () {
			this.$emit("setLoading", true);
		},
		endLoad: function (message) {
			this.$emit("setLoading", false);
			if (message) {
				this.alert = true;
				this.message = message;
				var c = this;
				setTimeout(function () { c.alert = false; }, 5000);
			}
		}
	}
}
var settingsMixin = {
	data() {
		return {
			settings: {}
		}
	},
	created: function () {
		this.startLoad();
		axios.get(basePath + "GetSettings").then(response => {
			this.settings = response.data;
			this.endLoad();
		});
	},
	methods: {
		save: function (message) {
			this.startLoad();
			axios.post(basePath + "SaveSettings", this.settings).then(response => {
				this.settings = response.data;
				this.endLoad(message);
			});
		}
	}

}

const General = {
	mixins: [settingsMixin, loadingMixin],
	template: "#General"
}

const Categories = {
	mixins: [loadingMixin],
	template: "#Categories",
	data() {
		return {
			categories: [],
			newCategory: "",
			editCategory: "",
			editID: 0
		}
	},
	created: function () {
		this.startLoad();
		axios.get(basePath + "GetCategories").then(response => {
			this.categories = response.data;
			this.endLoad();
		});
	},
	methods: {
		saveNew: function () {
			this.startLoad();
			axios.post(basePath + "AddCategory", { title: this.newCategory }).then(response => {
				this.categories = response.data;
				this.endLoad();
			});
			this.newCategory = "";
		},
		deleteCat: function (item) {
			this.startLoad();
			axios.post(basePath + "DeleteCategory/" + item.categoryID).then(response => {
				this.categories = response.data;
				this.endLoad();
			});
		},
		up: function (item) {
			this.startLoad();
			axios.post(basePath + "MoveCategoryUp/" + item.categoryID).then(response => {
				this.categories = response.data;
				this.endLoad();
			});
		},
		down: function (item) {
			this.startLoad();
			axios.post(basePath + "MoveCategoryDown/" + item.categoryID).then(response => {
				this.categories = response.data;
				this.endLoad();
			});
		},
		editCat: function (item) {
			this.editCategory = item.title;
			this.editID = item.categoryID;
			const e = this.$refs.modal;
			$(e).modal("show");
		},
		saveCat: function () {
			this.startLoad();
			axios.post(basePath + "EditCategory", { categoryID: this.editID, title: this.editCategory })
				.then(response => {
					this.categories = response.data;
					const e = this.$refs.modal;
					$(e).modal("hide");
					this.endLoad();
				});
		}
	}
}

const Forums = {
	mixins: [loadingMixin],
	template: "#Forums",
	data() {
		return {
			categories: [],
			editingForum: null
		}
	},
	created: function () {
		this.startLoad();
		axios.get(basePath + "GetForums").then(response => {
			this.categories = response.data;
			this.endLoad();
		});
		this.resetForum();
	},
	methods: {
		up: function (forum) {
			this.startLoad();
			axios.post(basePath + "MoveForumUp/" + forum.forumID).then(response => {
				this.categories = response.data;
				this.endLoad();
			});
		},
		down: function (forum) {
			this.startLoad();
			axios.post(basePath + "MoveForumDown/" + forum.forumID).then(response => {
				this.categories = response.data;
				this.endLoad();
			});
		},
		editForum: function (forum) {
			this.editingForum = forum;
			if (!this.editingForum.categoryID)
				this.editingForum.categoryID = 0;
			const e = this.$refs.modal;
			$(e).modal("show");
		},
		resetForum: function () {
			this.editingForum = { forumID: 0, title: "", description: "", categoryID: 0, isVisible: true, isArchived: false, isQAForum: false, forumAdapterName: null };
		},
		newForum: function () {
			this.resetForum();
			const e = this.$refs.modal;
			$(e).modal("show");
		},
		saveForum: function () {
			this.startLoad();
			axios.post(basePath + "SaveForum", this.editingForum)
				.then(response => {
					this.categories = response.data;
					const e = this.$refs.modal;
					$(e).modal("hide");
					this.endLoad();
				});
		}
	}
}

const ForumPermissions = {
	mixins: [loadingMixin],
	template: "#ForumPermissions",
	data() {
		return {
			categories: [],
			selectedForum: null,
			allRoles: [],
			postRoles: [],
			viewRoles: [],
			selectedAll: null,
			selectedPost: null,
			selectedView: null
		}
	},
	created: function () {
		this.startLoad();
		axios.get(basePath + "GetForums").then(response => {
			this.categories = response.data;
			this.endLoad();
		});
		this.updatePerm();
	},
	methods: {
		forumChange: function () {
			this.updatePerm();
		},
		updatePerm: function () {
			if (this.selectedForum) {
				this.startLoad();
				axios.get(basePath + "GetForumPermissions/" + this.selectedForum).then(response => {
					this.allRoles = response.data.allRoles;
					this.viewRoles = response.data.viewRoles;
					this.postRoles = response.data.postRoles;
					this.endLoad();
				});
			}
		},
		modify: function (modifyType, role) {
			this.startLoad();
			axios.post(basePath + "ModifyForumRoles", { forumID: this.selectedForum, modifyType: modifyType, role: role }).then(response => {
				this.endLoad();
				this.updatePerm();
			});
		}
	}
}

const Email = {
	mixins: [settingsMixin, loadingMixin],
	template: "#Email"
}

const Search = {
	mixins: [settingsMixin, loadingMixin],
	template: "#Search",
	data() {
		return {
			junkWords: [],
			selectedWord: null,
			newWord: ""
		}
	},
	created: function () {
		this.updateJunk();
	},
	methods: {
		updateJunk: function () {
			this.startLoad();
			axios.get(basePath + "GetJunkWords").then(response => {
				this.junkWords = response.data;
				this.endLoad();
			});
		},
		createWord: function () {
			this.startLoad();
			axios.post(basePath + "CreateJunkWord/" + this.newWord).then(response => {
				this.endLoad();
				this.newWord = "";
				this.updateJunk();
			});
		},
		deleteWord: function () {
			this.startLoad();
			axios.post(basePath + "DeleteJunkWord/" + this.selectedWord).then(response => {
				this.endLoad();
				this.updateJunk();
			});
		}
	}
}

const ExternalLogins = {
	mixins: [settingsMixin, loadingMixin],
	template: "#ExternalLogins"
}

const EditUser = {
	mixins: [loadingMixin],
	template: "#EditUser",
	data() {
		return {
			searchResults: [],
			searchText: "",
			searchType: "name",
			searchAlert: false
		}
	},
	methods: {
		search: function () {
			this.startLoad();
			axios.post(basePath + "EditUserSearch", { searchType: this.searchType, searchText: this.searchText })
				.then(response => {
					this.searchResults = response.data;
					this.endLoad();
					if (this.searchResults.length === 0)
						this.searchAlert = true;
					else
						this.searchAlert = false;
				});
		}
	}
}

const EditUserDetail = {
	mixins: [loadingMixin],
	template: "#EditUserDetail",
	data() {
		return {
			user: { newEmail: "", newPassword: "" },
			roles: []
		}
	},
	created: function () {
		var u = basePath + "GetUser/" + this.$route.params.id;
		axios.get(u).then(response => {
			this.user = response.data;
		});
		this.startLoad();
		axios.get(basePath + "GetAllRoles").then(response => {
			this.roles = response.data;
			this.endLoad();
		});
	},
	methods: {
		saveUser: function (message) {
			this.startLoad();
			axios.post(basePath + "SaveUser", this.user).then(response => {
				this.endLoad(message);
			}).catch(error => {
				alert(error);
			});
		},
		uploadAvatar: function (target) {
			const formData = new FormData();
			var files = target.files;
			formData.append("avatarFile", files[0], files[0].name);
			axios.post(basePath + "UpdateUserAvatar/" + this.user.userID, formData).then(response => {
				this.user.avatarID = response.data.avatarID;
				target.value = "";
			});
		},
		removeAvatar: function () {
			axios.post(basePath + "UpdateUserAvatar/" + this.user.userID, null).then(response => {
				this.user.avatarID = response.data.avatarID;
			});
		},
		uploadImage: function (target) {
			const formData = new FormData();
			var files = target.files;
			formData.append("imageFile", files[0], files[0].name);
			axios.post(basePath + "UpdateUserImage/" + this.user.userID, formData).then(response => {
				this.user.imageID = response.data.imageID;
				target.value = "";
			});
		},
		removeImage: function () {
			axios.post(basePath + "UpdateUserImage/" + this.user.userID, null).then(response => {
				this.user.imageID = response.data.imageID;
			});
        },
        deleteUser: function () {
            axios.post(basePath + "DeleteUser/" + this.user.userID, null).then(response => {
                this.$router.push("/edituser");
            });
        },
        deleteAndBanUser: function () {
            axios.post(basePath + "DeleteAndBanUser/" + this.user.userID, null).then(response => {
                this.$router.push("/edituser");
            });
        }
	}
}

const UserRoles = {
	mixins: [loadingMixin],
	template: "#UserRoles",
	data() {
		return {
			roles: [],
			newRole: "",
			selectedAll: null
		}
	},
	created: function () {
		this.refreshRoles();
	},
	methods: {
		refreshRoles: function () {
			this.startLoad();
			axios.get(basePath + "GetAllRoles").then(response => {
				this.roles = response.data;
				this.endLoad();
			});
		},
		createRole: function () {
			if (this.newRole && this.newRole != "") {
				this.startLoad();
				axios.post(basePath + "CreateRole/" + this.newRole).then(response => {
					this.endLoad();
					this.refreshRoles();
					this.newRole = "";
				});
			}
		},
		deleteRole: function () {
			if (this.selectedAll) {
				this.startLoad();
				axios.post(basePath + "DeleteRole/" + this.selectedAll).then(response => {
					this.endLoad();
					this.refreshRoles();
				});
			}
		}
	}
}

const UserImageApproval = {
	mixins: [loadingMixin],
	template: "#UserImageApproval",
	data() {
		return {
			isNewUserImageApproved: false,
			unapproved: []
		}
	},
	created: function () {
		this.refreshData();
	},
	methods: {
		refreshData: function () {
			this.startLoad();
			axios.get(basePath + "GetImageApproval").then(response => {
				this.isNewUserImageApproved = response.data.isNewUserImageApproved;
				this.unapproved = response.data.unapproved;
				this.endLoad();
			});
		},
		approveImage: function (id) {
			this.startLoad();
			axios.post(basePath + "ApproveUserImage/" + id).then(response => {
				this.refreshData();
				this.endLoad();
			});
		},
		deleteImage: function (id) {
			this.startLoad();
			axios.post(basePath + "DeleteUserImage/" + id).then(response => {
				this.refreshData();
				this.endLoad();
			});
		}
	}
}

const EmailIpBan = {
	mixins: [loadingMixin],
	template: "#EmailIpBan",
	data() {
		return {
			ips: [],
			emails: [],
			newEmail: "",
			newIP: "",
			selectedIP: null,
			selectedEmail: null
		}
	},
	created: function () {
		this.refreshData();
	},
	methods: {
		refreshData: function () {
			this.startLoad();
			axios.get(basePath + "GetEmailIPBan").then(response => {
				this.emails = response.data.emails;
				this.ips = response.data.ips;
				this.endLoad();
			});
		},
		banEmail: function () {
			this.startLoad();
			axios.post(basePath + "BanEmail", { string: this.newEmail }).then(response => {
				this.endLoad();
				this.newEmail = "";
				this.refreshData();
			});
		},
		removeEmail: function () {
			this.startLoad();
			axios.post(basePath + "RemoveEmail", { string: this.selectedEmail }).then(response => {
				this.endLoad();
				this.refreshData();
			});
		},
		banIP: function () {
			this.startLoad();
			axios.post(basePath + "BanIP", { string: this.newIP }).then(response => {
				this.endLoad();
				this.newIP = "";
				this.refreshData();
			});
		},
		removeIP: function () {
			this.startLoad();
			axios.post(basePath + "RemoveIP", { string: this.selectedIP }).then(response => {
				this.endLoad();
				this.refreshData();
			});
		}
	}
}

const EmailUsers = {
	mixins: [loadingMixin],
	template: "#EmailUsers",
	data() {
		return {
			subject: "",
			body: "",
			htmlBody: "",
			errorMessage: "",
			isErrorVisible: false,
			isSuccess: false
		}
	},
	methods: {
		sendMail: function () {
			this.isErrorVisible = false;
			this.startLoad();
			axios.post(basePath + "EmailUsers", { subject: this.subject, body: this.body, htmlBody: this.htmlBody }).then(response => {
				this.endLoad();
				this.subject = "";
				this.body = "";
				this.htmlBody = "";
				this.isSuccess = true;
			})
				.catch(e => {
					this.endLoad();
					this.isErrorVisible = true;
					this.isSuccess = false;
					this.errorMessage = e.response.data.error;
				});
		}
	}
}

const ScoringGame = {
	mixins: [settingsMixin, loadingMixin],
	template: "#ScoringGame"
}

const EventDefinitions = {
	mixins: [loadingMixin],
	template: "#EventDefinitions",
	data() {
		return {
			allEvents: [],
			staticIDs: [],
			newEvent: {
				eventDefinitionID: "",
				description: "",
				pointValue: "",
				isPublishedToFeed: false
			}
		}
	},
	created: function () {
		this.refreshData();
	},
	methods: {
		refreshData: function () {
			this.startLoad();
			axios.get(basePath + "GetAllEventDefinitions").then(response => {
				this.allEvents = response.data.allEvents;
				this.staticIDs = response.data.staticIDs;
				this.endLoad();
			});
		},
		deleteEvent: function (event) {
			this.startLoad();
			axios.post(basePath + "DeleteEvent/" + event.eventDefinitionID)
				.then(response => {
					this.refreshData();
					this.endLoad();
				});
		},
		resetEvent: function () {
			this.newEvent.eventDefinitionID = "";
			this.newEvent.description = "";
			this.newEvent.pointValue = "";
			this.newEvent.isPublishedToFeed = false;
		},
		openNewEvent: function () {
			this.resetEvent();
			const e = this.$refs.modal;
			$(e).modal("show");
		},
		createEvent: function () {
			this.startLoad();
			axios.post(basePath + "CreateEvent", this.newEvent)
				.then(response => {
					this.refreshData();
					const e = this.$refs.modal;
					$(e).modal("hide");
					this.endLoad();
				});
		}
	}
}

const AwardDefinitions = {
	mixins: [loadingMixin],
	template: "#AwardDefinitions",
	data() {
		return {
			allAwards: [],
			newAward: {
				awardDefinitionID: "",
				title: "",
				description: "",
				isSingleTimeAward: false
			}
		}
	},
	created: function () {
		this.refreshData();
	},
	methods: {
		refreshData: function () {
			this.startLoad();
			axios.get(basePath + "GetAllAwardDefinitions").then(response => {
				this.allAwards = response.data;
				this.endLoad();
			});
		},
		deleteAward: function (award) {
			this.startLoad();
			axios.post(basePath + "DeleteAward/" + award.awardDefinitionID)
				.then(response => {
					this.refreshData();
					this.endLoad();
				});
		},
		resetAward: function () {
			this.newAward.awardDefinitionID = "";
			this.newAward.title = "";
			this.newAward.description = "";
			this.newAward.isSingleTimeAward = false;
		},
		openNewAward: function () {
			this.resetAward();
			const e = this.$refs.modal;
			$(e).modal("show");
		},
		createAward: function () {
			this.startLoad();
			axios.post(basePath + "CreateAward", this.newAward)
				.then(response => {
					this.refreshData();
					const e = this.$refs.modal;
					$(e).modal("hide");
					this.endLoad();
				});
		}
	}
}

const AwardDefinitionDetail = {
	mixins: [loadingMixin],
	template: "#AwardDefinitionDetail",
	data() {
		return {
			award: {},
			conditions: [],
			allEvents: [],
			newCondition: {
				awardDefinitionID: 0,
				eventDefinitionID: "",
				eventCount: 0
			}
		}
	},
	created: function () {
		this.refreshData();
	},
	methods: {
		refreshData: function () {
			this.startLoad();
			var u = basePath + "GetAward/" + this.$route.params.id;
			axios.get(u).then(response => {
				this.award = response.data.award;
				this.conditions = response.data.conditions;
				this.allEvents = response.data.allEvents;
				this.endLoad();
			});
		},
		deleteCondition: function (c) {
			this.startLoad();
			axios.post(basePath + "DeleteCondition", { awardDefinitionID: this.award.awardDefinitionID, eventDefinitionID: c.eventDefinitionID })
				.then(response => {
					this.refreshData();
					this.endLoad();
				});
		},
		createCondition: function () {
			this.newCondition.awardDefinitionID = this.award.awardDefinitionID;
			this.startLoad();
			axios.post(basePath + "CreateCondition", this.newCondition)
				.then(response => {
					this.refreshData();
					const e = this.$refs.modal;
					$(e).modal("hide");
					this.endLoad();
				});
		},
		openNewCondition: function () {
			this.newCondition.eventCount = "";
			const e = this.$refs.modal;
			$(e).modal("show");
		}
	}
}

const ManualEvent = {
	mixins: [loadingMixin],
	template: "#ManualEvent",
	data() {
		return {
			allEvents: [],
			searchName: "",
			searchResults: [],
			selectedUser: {},
			message: "",
			points: 0,
			eventDefinitionID: ""
		}
	},
	created: function () {
		this.startLoad();
		axios.get(basePath + "GetAllEvents")
			.then(response => {
				this.allEvents = response.data;
				this.endLoad();
			});
	},
	methods: {
		openSearch: function () {
			const e = this.$refs.modal;
			$(e).modal("show");
		},
		updateList: function () {
			if (this.searchName.length < 2) return;
			this.startLoad();
			axios.post(basePath + "GetNames", { string: this.searchName })
				.then(response => {
					this.searchResults = response.data;
					this.endLoad();
				});
		},
		chooseUser: function () {
			const e = this.$refs.modal;
			$(e).modal("hide");
		},
		createManualEvent: function () {
			this.startLoad();
			axios.post(basePath + "CreateManualEvent", { userID: this.selectedUser.userID, message: this.message, points: this.points })
				.then(response => {
					this.endLoad();
					this.message = "";
					this.points = 0;
				})
				.catch(error => {
					alert(error.response.data);
					this.endLoad();
				});
		},
		createExistingManualEvent: function () {
			this.startLoad();
			axios.post(basePath + "CreateExistingManualEvent", { userID: this.selectedUser.userID, message: this.message, points: null, eventDefinitionID: this.eventDefinitionID })
				.then(response => {
					this.endLoad();
					this.message = "";
				})
				.catch(error => {
					alert(error.response.data);
					this.endLoad();
				});
		}
	}
}

const IPHistory = {
	mixins: [loadingMixin],
	template: "#IPHistory",
	data() {
		return {
			history: [],
			query: { iP: "", start: "", end: "" }
		}
	},
	methods: {
		getHistory: function () {
			this.startLoad();
			axios.post(basePath + "QueryIPHistory", this.query)
				.then(response => {
					this.history = response.data;
					this.endLoad();
				});
		}
	}
}

const SecurityLog = {
	mixins: [loadingMixin],
	template: "#SecurityLog",
	data() {
		return {
			history: [],
			query: { searchTerm: "", type: "Name", start: "", end: "" }
		}
	},
	methods: {
		getHistory: function () {
			this.startLoad();
			axios.post(basePath + "QuerySecurityLog", this.query)
				.then(response => {
					this.history = response.data;
					this.endLoad();
				});
		}
	}
}

const ModerationLog = {
	mixins: [loadingMixin],
	template: "#ModerationLog",
	data() {
		return {
			history: [],
			query: { start: "", end: "" }
		}
	},
	methods: {
		getHistory: function () {
			this.startLoad();
			axios.post(basePath + "QueryModerationLog", this.query)
				.then(response => {
					this.history = response.data;
					this.endLoad();
				});
		}
	}
}

const ErrorLog = {
	mixins: [loadingMixin],
	template: "#ErrorLog",
	data() {
		return {
			errorList: { pageIndex: 1, pageSize: 20, list: [] }
		}
	},
	created: function () {
		this.getErrors();
	},
	methods: {
		getMore: function (newIndex) {
			this.errorList.pageIndex = newIndex;
			this.getErrors();
		},
		getErrors: function () {
			this.startLoad();
			axios.get(basePath + "GetErrorLog/" + this.errorList.pageIndex)
				.then(response => {
					this.errorList = response.data;
					this.endLoad();
				});
		},
		deleteAll() {
			this.startLoad();
			axios.post(basePath + "DeleteAllErrors")
				.then(response => {
					this.errorList.pageIndex = 1;
					this.endLoad();
					this.getErrors();
				});
		}
	}
}

const Services = {
	mixins: [loadingMixin],
	template: "#Services",
	data() {
		return {
			list: {}
		}
	},
	created: function () {
		this.getData();
	},
	methods: {
		getData: function () {
			this.startLoad();
			axios.get(basePath + "GetServices")
				.then(response => {
					this.list = response.data;
					this.endLoad();
				});
		},
		clearAll: function () {
			this.startLoad();
			axios.post(basePath + "ClearServices")
				.then(response => {
					this.list = response.data;
					this.endLoad();
				});
		}
	}
}

const routes = [
	{
		path: "/", component: Top, redirect: "/general",
		children: [
			{ path: "/general", component: General },
			{ path: "/categories", component: Categories },
			{ path: "/forums", component: Forums },
			{ path: "/forumpermissions", component: ForumPermissions },
			{ path: "/email", component: Email },
			{ path: "/search", component: Search },
			{ path: "/externallogins", component: ExternalLogins },
			{ path: "/edituser", component: EditUser },
			{ path: "/edituser/:id", component: EditUserDetail },
			{ path: "/userroles", component: UserRoles },
			{ path: "/userimageapproval", component: UserImageApproval },
			{ path: "/emailipban", component: EmailIpBan },
			{ path: "/emailusers", component: EmailUsers },
			{ path: "/scoringgame", component: ScoringGame },
			{ path: "/eventdefinitions", component: EventDefinitions },
			{ path: "/awarddefinitions", component: AwardDefinitions },
			{ path: "/awarddefinitions/:id", component: AwardDefinitionDetail },
			{ path: "/manualevent", component: ManualEvent },
			{ path: "/iphistory", component: IPHistory },
			{ path: "/securitylog", component: SecurityLog },
			{ path: "/moderationlog", component: ModerationLog },
			{ path: "/errorlog", component: ErrorLog },
			{ path: "/services", component: Services }
		]
	},
	{ path: "*", redirect: "/general" }
];

const router = new VueRouter({
	caseSensitive: false,
	routes: routes,
	mode: "history",
	base: "/forums/admin"
});

const app = new Vue({
	router
}).$mount("#app");
