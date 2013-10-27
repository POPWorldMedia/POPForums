var PopForums = {};

PopForums.areaPath = "/Forums";
PopForums.currentTopicState = null;
PopForums.replyInterval = null;

$(function () {
	var menu = $("#MainMenu");
	menu.css("marginLeft", -($(window).width() * .90));
	menu.css("display", "block");
	$("#MenuToggle, #MenuModal, #MenuCloser").click(function () {
		PopForums.toggleMainMenu();
	});
	PopForums.overrideCheckBox();
});

PopForums.overrideCheckBox = function () {
	$(".pf-check").each(function () {
		var span = $('<span class="pf-check-replacement" />');
		var check = $(this);
		check.after(span);
		span.click(function () {
			if (check.is(":checked"))
				check.removeAttr("checked");
			else
				check.attr("checked", "true");
			check.trigger("change");
		});
		check.css("display", "none");
		check.on("change", function () {
			span.toggleClass("pf-check-replacement-checked", this.checked);
		});
		span.toggleClass("pf-check-replacement-checked", check.is(":checked"));
	});
};

PopForums.toggleMainMenu = function () {
	var menu = $("#MainMenu");
	var modal = $("#MenuModal");
	var w = $(window).width() * .80;
	menu.animate({
		marginLeft: parseInt(menu.css("marginLeft"), 10) == 0 ? -w : 0
	}, 500);
	modal.css("visibility", modal.css("visibility") == "visible" ? "hidden" : "visible");
};

PopForums.processLogin = function () {
	PopForums.processLoginBase("/Authorization/Login");
};

PopForums.processLoginExternal = function () {
	PopForums.processLoginBase("/Authorization/LoginAndAssociate");
};

PopForums.processLoginBase = function (path) {
	$.ajax({
		url: PopForums.areaPath + path,
		type: "POST",
		data: { email: $("#EmailLogin").val(), password: $("#PasswordLogin").val(), persistCookie: $("#PersistCookie").is(":checked") },
		dataType: "json",
		success: function (result) {
			var loginResult = $("#LoginResult");
			switch (result.Result) {
				case true:
					var destination = $("#Referrer").val();
					location = destination;
					break;
				default:
					loginResult.html(result.Message);
			}
		},
		error: function () {
			var loginResult = $("#LoginResult");
			loginResult.html("There was an unknown error while attempting login");
		}
	});
};

PopForums.topicToolSetup = function(topicID) {
	var s = $("#SubscribeButton");
	s.click(function() {
		var asyncResult = $("#AsyncResponse");
		$.ajax({
			url: PopForums.areaPath + "/Subscription/ToggleSubscription/" + topicID,
			type: "POST",
			dataType: "json",
			success: function(result) {
				switch (result.Data.isSubscribed) {
				case true:
					s.prop("value", "Unsubscribe");
					break;
				case false:
					s.prop("value", "Subscribe");
					break;
				default:
					asyncResult.html(result.Message);
				}
			},
			error: function() {
				asyncResult.html("There was an unknown error while attempting to use subscription");
			}
		});
	});
	var f = $("#FavoriteButton");
	f.click(function() {
		var asyncResult = $("#AsyncResponse");
		$.ajax({
			url: PopForums.areaPath + "/Favorites/ToggleFavorite/" + topicID,
			type: "POST",
			dataType: "json",
			success: function(result) {
				switch (result.Data.isFavorite) {
				case true:
					f.prop("value", "Remove From Favorites");
					break;
				case false:
					f.prop("value", "Make Favorite");
					break;
				default:
					asyncResult.html(result.Message);
				}
			},
			error: function() {
				asyncResult.html("There was an unknown error while attempting to use favorites");
			}
		});
	});
	$(document).on("click", ".voteUp", function () {
		var parent = $(this).parents(".postItem");
		var postID = parent.attr("data-postID");
		var countBox = parent.find(".voteCount");
		$.ajax({
			url: PopForums.areaPath + "/Forum/VotePost/" + postID,
			type: "POST",
			success: function (result) {
				countBox.html(result);
				var voted = parent.find(".voteUp");
				voted.replaceWith('Voted');
			}
		});
	});
};

PopForums.postReply = function () {
	$.ajax({
		url: PopForums.areaPath + "/Forum/PostReply",
		type: "POST",
		data: { Title: $("#NewReply #Title").val(), FullText: $("#NewReply #FullText").val(), IncludeSignature: $("#NewReply #IncludeSignature").val(), ItemID: $("#NewReply #ItemID").val(), CloseOnReply: $("#CloseOnReply").is(":checked"), IsPlainText: true, ParentPostID: $("#NewReply #ParentPostID").val() },
		dataType: "json",
		success: function (result) {
			var r = $("#PostResponseMessage");
			switch (result.Result) {
				case true:
					window.location = result.Redirect;
				default:
					r.html(result.Message);
			}
		},
		error: function () {
			var r = $("#PostResponseMessage");
			r.html("There was an unknown error while trying to post");
		}
	});
};

PopForums.postNewTopic = function () {
	$.ajax({
		url: PopForums.areaPath + "/Forum/PostTopic",
		type: "POST",
		data: { Title: $("#NewTopic #Title").val(), FullText: $("#NewTopic #FullText").val(), IncludeSignature: $("#NewTopic #IncludeSignature").val(), ItemID: $("#NewTopic #ItemID").val(), IsPlainText: true },
		dataType: "json",
		success: function (result) {
			var r = $("#PostResponseMessage");
			switch (result.Result) {
				case true:
					window.location = result.Redirect;
				default:
					r.html(result.Message);
			}
		},
		error: function () {
			var r = $("#PostResponseMessage");
			r.html("There was an unknown error while trying to post");
		}
	});
};
