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
		},
		errorAlert: function() {
			alert("There was an error. Please reload admin.");
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
		})
			.catch(error => this.errorAlert());
	},
	methods: {
		save: function (message) {
			this.startLoad();
			axios.post(basePath + "SaveSettings", this.settings).then(response => {
				this.settings = response.data;
				this.endLoad(message);
			})
				.catch(error => this.errorAlert());
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
		})
			.catch(error => this.errorAlert());
	},
	methods: {
		saveNew: function () {
			this.startLoad();
			axios.post(basePath + "AddCategory", { title: this.newCategory }).then(response => {
				this.categories = response.data;
				this.endLoad();
			})
				.catch(error => this.errorAlert());
			this.newCategory = "";
		},
		deleteCat: function (item) {
			this.startLoad();
			axios.post(basePath + "DeleteCategory/" + item.categoryID).then(response => {
				this.categories = response.data;
				this.endLoad();
			})
				.catch(error => this.errorAlert());
		},
		up: function (item) {
			this.startLoad();
			axios.post(basePath + "MoveCategoryUp/" + item.categoryID).then(response => {
				this.categories = response.data;
				this.endLoad();
			})
				.catch(error => this.errorAlert());
		},
		down: function (item) {
			this.startLoad();
			axios.post(basePath + "MoveCategoryDown/" + item.categoryID).then(response => {
				this.categories = response.data;
				this.endLoad();
			})
				.catch(error => this.errorAlert());
		},
		editCat: function (item) {
			this.editCategory = item.title;
			this.editID = item.categoryID;
			const e = this.$refs.modal;
			var modal = new bootstrap.Modal(e);
			modal.show();
		},
		saveCat: function () {
			this.startLoad();
			axios.post(basePath + "EditCategory", { categoryID: this.editID, title: this.editCategory })
				.then(response => {
					this.categories = response.data;
					this.endLoad();
				})
				.catch(error => this.errorAlert());
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
		})
			.catch(error => this.errorAlert());
		this.resetForum();
	},
	methods: {
		up: function (forum) {
			this.startLoad();
			axios.post(basePath + "MoveForumUp/" + forum.forumID).then(response => {
				this.categories = response.data;
				this.endLoad();
			})
				.catch(error => this.errorAlert());
		},
		down: function (forum) {
			this.startLoad();
			axios.post(basePath + "MoveForumDown/" + forum.forumID).then(response => {
				this.categories = response.data;
				this.endLoad();
			})
				.catch(error => this.errorAlert());
		},
		editForum: function (forum) {
			this.editingForum = forum;
			if (!this.editingForum.categoryID)
				this.editingForum.categoryID = 0;
			const e = this.$refs.modal;
			var modal = new bootstrap.Modal(e);
			modal.show();
		},
		resetForum: function () {
			this.editingForum = { forumID: 0, title: "", description: "", categoryID: 0, isVisible: true, isArchived: false, isQAForum: false, forumAdapterName: null };
		},
		newForum: function () {
			this.resetForum();
			const e = this.$refs.modal;
			var modal = new bootstrap.Modal(e);
			modal.show();
		},
		saveForum: function () {
			this.startLoad();
			axios.post(basePath + "SaveForum", this.editingForum)
				.then(response => {
					this.categories = response.data;
					this.endLoad();
				})
				.catch(error => this.errorAlert());
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
		})
			.catch(error => this.errorAlert());
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
				})
					.catch(error => this.errorAlert());
			}
		},
		modify: function (modifyType, role) {
			this.startLoad();
			axios.post(basePath + "ModifyForumRoles", { forumID: this.selectedForum, modifyType: modifyType, role: role }).then(response => {
				this.endLoad();
				this.updatePerm();
			})
				.catch(error => this.errorAlert());
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
			})
				.catch(error => this.errorAlert());
		},
		createWord: function () {
			this.startLoad();
			axios.post(basePath + "CreateJunkWord/" + this.newWord).then(response => {
				this.endLoad();
				this.newWord = "";
				this.updateJunk();
			})
				.catch(error => this.errorAlert());
		},
		deleteWord: function () {
			this.startLoad();
			axios.post(basePath + "DeleteJunkWord/" + this.selectedWord).then(response => {
				this.endLoad();
				this.updateJunk();
			})
				.catch(error => this.errorAlert());
		}
	}
}

const ExternalLogins = {
	mixins: [settingsMixin, loadingMixin],
	template: "#ExternalLogins"
}

const RecentUsers = {
	mixins: [loadingMixin],
	template: "#RecentUsers",
	data() {
		return {
			results: []
		}
	},
	created: function () {
		this.startLoad();
		axios.get(basePath + "GetRecentUsers").then(response => {
			this.results = response.data;
			this.endLoad();
		})
			.catch(error => this.errorAlert());
	},
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
				})
				.catch(error => this.errorAlert());
		}
	}
}

const EditUserSub = {
	mixins: [loadingMixin],
	template: "#EditUserSub",
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
				})
				.catch(error => this.errorAlert());
		}
	}
}

// Plain JS copy of Client/Services/ImageResizer.ts (admin doesn't load PopForums.js), keep the two in sync.
// Images are resized in the browser before upload, and the server only checks the header of what it receives.
const imageResizer = {
	// Already within limits, these upload untouched: GIF keeps its animation, PNG stays sharp.
	passThroughTypes: ["image/png", "image/gif"],
	minQuality: 0.4,

	prepare: async function (file, limits, errorMessage) {
		const maxBytes = limits.maxkBytes * 1024;
		const url = URL.createObjectURL(file);
		const image = new Image();
		image.src = url;
		try {
			await image.decode();
		}
		catch {
			throw new Error(errorMessage);
		}
		finally {
			URL.revokeObjectURL(url);
		}
		const width = image.naturalWidth;
		const height = image.naturalHeight;
		if (this.passThroughTypes.includes(file.type) && file.size <= maxBytes && width <= limits.maxWidth && height <= limits.maxHeight)
			return file;

		// JPEGs always get re-encoded so EXIF data is stripped and the rotation is baked in
		const target = this.getTarget(width, height, limits);
		const canvas = this.draw(image, target);
		// only sharpen what was actually scaled down, not a JPEG that was just re-encoded or trimmed
		if (target.dw < target.sw || target.dh < target.sh)
			this.sharpen(canvas);
		for (let quality = limits.quality; quality >= this.minQuality - 0.01; quality -= 0.1) {
			const blob = await new Promise(resolve => canvas.toBlob(resolve, "image/jpeg", quality));
			if (!blob)
				throw new Error(errorMessage);
			if (blob.size <= maxBytes)
				return new File([blob], file.name.replace(/\.[^.]*$/, "") + ".jpg", { type: "image/jpeg" });
		}
		throw new Error(errorMessage);
	},

	getTarget: function (width, height, limits) {
		if (!limits.isCrop) {
			const scale = Math.min(1, limits.maxWidth / width, limits.maxHeight / height);
			return { sx: 0, sy: 0, sw: width, sh: height, dw: Math.max(1, Math.round(width * scale)), dh: Math.max(1, Math.round(height * scale)) };
		}
		const scale = Math.min(1, Math.max(limits.maxWidth / width, limits.maxHeight / height));
		const dw = Math.max(1, Math.min(limits.maxWidth, Math.round(width * scale)));
		const dh = Math.max(1, Math.min(limits.maxHeight, Math.round(height * scale)));
		const sw = Math.min(width, dw / scale);
		const sh = Math.min(height, dh / scale);
		return { sx: (width - sw) / 2, sy: (height - sh) / 2, sw: sw, sh: sh, dw: dw, dh: dh };
	},

	draw: function (image, target) {
		let source = image;
		let { sx, sy, sw, sh } = target;
		while (sw / 2 >= target.dw && sh / 2 >= target.dh) {
			const step = this.createCanvas(Math.round(sw / 2), Math.round(sh / 2));
			step.getContext("2d").drawImage(source, sx, sy, sw, sh, 0, 0, step.width, step.height);
			source = step;
			sx = 0;
			sy = 0;
			sw = step.width;
			sh = step.height;
		}
		const canvas = this.createCanvas(target.dw, target.dh);
		const context = canvas.getContext("2d");
		context.fillStyle = "#fff";
		context.fillRect(0, 0, canvas.width, canvas.height);
		context.drawImage(source, sx, sy, sw, sh, 0, 0, target.dw, target.dh);
		return canvas;
	},

	// Matches the light GaussianSharpen(0.5) the server used to apply: twice the original minus a Gaussian blur, rows then columns
	sharpen: function (canvas) {
		const context = canvas.getContext("2d");
		const { width, height } = canvas;
		const image = context.getImageData(0, 0, width, height);
		const data = image.data;
		const sigma = 0.5;
		const side = Math.exp(-1 / (2 * sigma * sigma));
		const edge = -side / (1 + 2 * side);
		const center = 1 - 2 * edge;
		const rows = new Float32Array(data.length);
		for (let y = 0; y < height; y++) {
			for (let x = 0; x < width; x++) {
				const i = (y * width + x) * 4;
				const left = (y * width + Math.max(x - 1, 0)) * 4;
				const right = (y * width + Math.min(x + 1, width - 1)) * 4;
				for (let c = 0; c < 3; c++)
					rows[i + c] = center * data[i + c] + edge * (data[left + c] + data[right + c]);
			}
		}
		for (let y = 0; y < height; y++) {
			for (let x = 0; x < width; x++) {
				const i = (y * width + x) * 4;
				const up = (Math.max(y - 1, 0) * width + x) * 4;
				const down = (Math.min(y + 1, height - 1) * width + x) * 4;
				for (let c = 0; c < 3; c++)
					data[i + c] = center * rows[i + c] + edge * (rows[up + c] + rows[down + c]);
			}
		}
		context.putImageData(image, 0, 0);
	},

	createCanvas: function (width, height) {
		const canvas = document.createElement("canvas");
		canvas.width = width;
		canvas.height = height;
		const context = canvas.getContext("2d");
		context.imageSmoothingEnabled = true;
		context.imageSmoothingQuality = "high";
		return canvas;
	}
}

const EditUserDetail = {
	mixins: [loadingMixin],
	template: "#EditUserDetail",
	data() {
		return {
			user: { newEmail: "", newPassword: "" },
			roles: [],
			imageSettings: {},
			avatarUpload: { busy: false, error: "", success: false },
			photoUpload: { busy: false, error: "", success: false }
		}
	},
	created: function () {
		axios.get(basePath + "GetSettings").then(response => {
			this.imageSettings = response.data;
		})
			.catch(error => this.errorAlert());
		var u = basePath + "GetUser/" + this.$route.params.id;
		axios.get(u).then(response => {
			this.user = response.data;
		})
			.catch(error => this.errorAlert());
		this.startLoad();
		axios.get(basePath + "GetAllRoles").then(response => {
			this.roles = response.data;
			this.endLoad();
		})
			.catch(error => this.errorAlert());
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
		// Like the public ManagePhotos page: images upload as soon as they're picked and delete as soon as
		// the button is clicked. Posting with no file is how the API deletes.
		uploadAvatar: function (target) {
			const s = this.imageSettings;
			this.uploadProfileImage(target, this.avatarUpload, "UpdateUserAvatar/", "avatarFile",
				{ maxWidth: s.userAvatarMaxWidth, maxHeight: s.userAvatarMaxHeight, maxkBytes: s.userAvatarMaxkBytes, isCrop: true, quality: 0.7 },
				data => this.user.avatarID = data.avatarID);
		},
		removeAvatar: function (target) {
			this.removeProfileImage(target, this.avatarUpload, "UpdateUserAvatar/", data => this.user.avatarID = data.avatarID);
		},
		uploadImage: function (target) {
			const s = this.imageSettings;
			this.uploadProfileImage(target, this.photoUpload, "UpdateUserImage/", "imageFile",
				{ maxWidth: s.userImageMaxWidth, maxHeight: s.userImageMaxHeight, maxkBytes: s.userImageMaxkBytes, isCrop: false, quality: 0.7 },
				data => this.user.imageID = data.imageID);
		},
		removeImage: function (target) {
			this.removeProfileImage(target, this.photoUpload, "UpdateUserImage/", data => this.user.imageID = data.imageID);
		},
		uploadProfileImage: async function (target, state, path, field, limits, onSaved) {
			if (!target.files.length)
				return;
			state.error = "";
			state.success = false;
			state.busy = true;
			try {
				let file;
				try {
					file = await imageResizer.prepare(target.files[0], limits, target.dataset.error);
				}
				catch (error) {
					state.error = error.message;
					return;
				}
				const formData = new FormData();
				formData.append(field, file, file.name);
				const response = await axios.post(basePath + path + this.user.userID, formData);
				onSaved(response.data);
				state.success = true;
			}
			catch (error) {
				// validation failures come back as a 400 with the reason as text
				state.error = error.response?.status === 400 && typeof error.response.data === "string" ? error.response.data : target.dataset.failed;
			}
			finally {
				// so picking the same file again still triggers an upload
				target.value = "";
				state.busy = false;
			}
		},
		removeProfileImage: async function (target, state, path, onSaved) {
			state.error = "";
			state.success = false;
			state.busy = true;
			try {
				const response = await axios.post(basePath + path + this.user.userID, null);
				onSaved(response.data);
			}
			catch {
				state.error = target.dataset.failed;
			}
			finally {
				state.busy = false;
			}
        },
        deleteUser: function () {
            axios.post(basePath + "DeleteUser/" + this.user.userID, null).then(response => {
                this.$router.push("/edituser");
			})
				.catch(error => this.errorAlert());
        },
        deleteAndBanUser: function () {
            axios.post(basePath + "DeleteAndBanUser/" + this.user.userID, null).then(response => {
                this.$router.push("/edituser");
			})
				.catch(error => this.errorAlert());
        }
	}
}

const UpdateUserSubscription = {
	mixins: [loadingMixin],
	template: "#UpdateUserSubscription",
	data() {
		return {
			edit: { userID: 0, name: "", skuID: "", expiration: "" },
			skus: [],
			history: []
		}
	},
	created: function () {
		var id = this.$route.params.id;
		this.startLoad();
		axios.get(basePath + "GetUserSubscription/" + id).then(response => {
			this.edit = response.data;
			this.endLoad();
		})
			.catch(error => this.errorAlert());
		axios.get(basePath + "GetSkus").then(response => {
			this.skus = response.data;
		})
			.catch(error => this.errorAlert());
		this.refreshHistory();
	},
	methods: {
		refreshHistory: function () {
			axios.get(basePath + "GetUserSubscriptionHistory/" + this.$route.params.id).then(response => {
				this.history = response.data;
			})
				.catch(error => this.errorAlert());
		},
		apply: function () {
			this.startLoad();
			axios.post(basePath + "ApplyManualSubscriptionEdit", this.edit).then(response => {
				this.endLoad();
				this.refreshHistory();
			})
				.catch(error => this.errorAlert());
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
			})
				.catch(error => this.errorAlert());
		},
		createRole: function () {
			if (this.newRole && this.newRole != "") {
				this.startLoad();
				axios.post(basePath + "CreateRole/" + this.newRole).then(response => {
					this.endLoad();
					this.refreshRoles();
					this.newRole = "";
				})
					.catch(error => this.errorAlert());
			}
		},
		deleteRole: function () {
			if (this.selectedAll) {
				this.startLoad();
				axios.post(basePath + "DeleteRole/" + this.selectedAll).then(response => {
					this.endLoad();
					this.refreshRoles();
				})
					.catch(error => this.errorAlert());
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
			})
				.catch(error => this.errorAlert());
		},
		approveImage: function (id) {
			this.startLoad();
			axios.post(basePath + "ApproveUserImage/" + id).then(response => {
				this.refreshData();
				this.endLoad();
			})
				.catch(error => this.errorAlert());
		},
		deleteImage: function (id) {
			this.startLoad();
			axios.post(basePath + "DeleteUserImage/" + id).then(response => {
				this.refreshData();
				this.endLoad();
			})
				.catch(error => this.errorAlert());
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
			})
				.catch(error => this.errorAlert());
		},
		banEmail: function () {
			this.startLoad();
			axios.post(basePath + "BanEmail", { string: this.newEmail }).then(response => {
				this.endLoad();
				this.newEmail = "";
				this.refreshData();
			})
				.catch(error => this.errorAlert());
		},
		removeEmail: function () {
			this.startLoad();
			axios.post(basePath + "RemoveEmail", { string: this.selectedEmail }).then(response => {
				this.endLoad();
				this.refreshData();
			})
				.catch(error => this.errorAlert());
		},
		banIP: function () {
			this.startLoad();
			axios.post(basePath + "BanIP", { string: this.newIP }).then(response => {
				this.endLoad();
				this.newIP = "";
				this.refreshData();
			})
				.catch(error => this.errorAlert());
		},
		removeIP: function () {
			this.startLoad();
			axios.post(basePath + "RemoveIP", { string: this.selectedIP }).then(response => {
				this.endLoad();
				this.refreshData();
			})
				.catch(error => this.errorAlert());
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
			})
				.catch(error => this.errorAlert());
		},
		deleteEvent: function (event) {
			this.startLoad();
			axios.post(basePath + "DeleteEvent/" + event.eventDefinitionID)
				.then(response => {
					this.refreshData();
					this.endLoad();
				})
				.catch(error => this.errorAlert());
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
			var modal = new bootstrap.Modal(e);
			modal.show();
		},
		createEvent: function () {
			this.startLoad();
			axios.post(basePath + "CreateEvent", this.newEvent)
				.then(response => {
					this.refreshData();
					this.endLoad();
				})
				.catch(error => this.errorAlert());
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
			})
				.catch(error => this.errorAlert());
		},
		deleteAward: function (award) {
			this.startLoad();
			axios.post(basePath + "DeleteAward/" + award.awardDefinitionID)
				.then(response => {
					this.refreshData();
					this.endLoad();
				})
				.catch(error => this.errorAlert());
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
			var modal = new bootstrap.Modal(e);
			modal.show();
		},
		createAward: function () {
			this.startLoad();
			axios.post(basePath + "CreateAward", this.newAward)
				.then(response => {
					this.refreshData();
					this.endLoad();
				})
				.catch(error => this.errorAlert());
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
			})
				.catch(error => this.errorAlert());
		},
		deleteCondition: function (c) {
			this.startLoad();
			axios.post(basePath + "DeleteCondition", { awardDefinitionID: this.award.awardDefinitionID, eventDefinitionID: c.eventDefinitionID })
				.then(response => {
					this.refreshData();
					this.endLoad();
				})
				.catch(error => this.errorAlert());
		},
		createCondition: function () {
			this.newCondition.awardDefinitionID = this.award.awardDefinitionID;
			this.startLoad();
			axios.post(basePath + "CreateCondition", this.newCondition)
				.then(response => {
					this.refreshData();
					this.endLoad();
				})
				.catch(error => this.errorAlert());
		},
		openNewCondition: function () {
			this.newCondition.eventCount = "";
			const e = this.$refs.modal;
			var modal = new bootstrap.Modal(e);
			modal.show();
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
			})
			.catch(error => this.errorAlert());
	},
	methods: {
		openSearch: function () {
			const e = this.$refs.modal;
			var modal = new bootstrap.Modal(e);
			modal.show();
		},
		updateList: function () {
			if (this.searchName.length < 2) return;
			this.startLoad();
			axios.post(basePath + "GetNames", { string: this.searchName })
				.then(response => {
					this.searchResults = response.data;
					this.endLoad();
				})
				.catch(error => this.errorAlert());
		},
		chooseUser: function () {
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
			var copy = Object.assign({}, this.query);
			copy.start = new Date(this.query.start).toISOString();
			copy.end = new Date(this.query.end).toISOString();
			axios.post(basePath + "QueryIPHistory", copy)
				.then(response => {
					this.history = response.data;
					this.endLoad();
				})
				.catch(error => this.errorAlert());
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
			var copy = Object.assign({}, this.query);
			copy.start = new Date(this.query.start).toISOString();
			copy.end = new Date(this.query.end).toISOString();
			axios.post(basePath + "QuerySecurityLog", copy)
				.then(response => {
					this.history = response.data;
					this.endLoad();
				})
				.catch(error => this.errorAlert());
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
			var copy = Object.assign({}, this.query);
			copy.start = new Date(this.query.start).toISOString();
			copy.end = new Date(this.query.end).toISOString();
			axios.post(basePath + "QueryModerationLog", copy)
				.then(response => {
					this.history = response.data;
					this.endLoad();
				})
				.catch(error => this.errorAlert());
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
				})
				.catch(error => this.errorAlert());
		},
		deleteAll() {
			this.startLoad();
			axios.post(basePath + "DeleteAllErrors")
				.then(response => {
					this.errorList.pageIndex = 1;
					this.endLoad();
					this.getErrors();
				})
				.catch(error => this.errorAlert());
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
				})
				.catch(error => this.errorAlert());
		},
		clearAll: function () {
			this.startLoad();
			axios.post(basePath + "ClearServices")
				.then(response => {
					this.list = response.data;
					this.endLoad();
				})
				.catch(error => this.errorAlert());
		}
	}
}

const SubscriptionConfig = {
	mixins: [settingsMixin, loadingMixin],
	template: "#SubscriptionConfig"
}

const SubscriptionSkus = {
	mixins: [loadingMixin],
	template: "#SubscriptionSkus",
	data() {
		return {
			skus: [],
			editingSku: null,
			isNewSku: true
		}
	},
	created: function () {
		this.startLoad();
		axios.get(basePath + "GetSkus").then(response => {
			this.skus = response.data;
			this.endLoad();
		})
			.catch(error => this.errorAlert());
		this.resetSku();
	},
	methods: {
		up: function (sku) {
			this.startLoad();
			axios.post(basePath + "MoveSkuUp/" + sku.skuID).then(response => {
				this.skus = response.data;
				this.endLoad();
			})
				.catch(error => this.errorAlert());
		},
		down: function (sku) {
			this.startLoad();
			axios.post(basePath + "MoveSkuDown/" + sku.skuID).then(response => {
				this.skus = response.data;
				this.endLoad();
			})
				.catch(error => this.errorAlert());
		},
		editSku: function (sku) {
			this.editingSku = sku;
			this.isNewSku = false;
			const e = this.$refs.modal;
			var modal = new bootstrap.Modal(e);
			modal.show();
		},
		resetSku: function () {
			this.editingSku = { skuID: "", name: "", description: "", price: 0, months: 1, isActive: true };
		},
		newSku: function () {
			this.resetSku();
			this.isNewSku = true;
			const e = this.$refs.modal;
			var modal = new bootstrap.Modal(e);
			modal.show();
		},
		saveSku: function () {
			this.startLoad();
			axios.post(basePath + "SaveSku", this.editingSku)
				.then(response => {
					this.skus = response.data;
					this.endLoad();
				})
				.catch(error => this.errorAlert());
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
			{ path: "/recentusers", component: RecentUsers },
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
			{ path: "/services", component: Services },
			{ path: "/editusersub", component: EditUserSub },
			{ path: "/updateusersubscription/:id", component: UpdateUserSubscription },
			{ path: "/subscriptionconfig", component: SubscriptionConfig },
			{ path: "/subscriptionskus", component: SubscriptionSkus }
		]
	},
	{ path: "/:pathMatch(.*)*", redirect: "/general" }
];

const router = VueRouter.createRouter({
	caseSensitive: false,
	routes: routes,
	history: VueRouter.createWebHistory("/forums/admin")
});

const app = Vue.createApp({})
app.use(router);
app.mount("#app");
