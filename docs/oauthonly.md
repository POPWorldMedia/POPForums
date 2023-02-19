---
layout: default
title: OAuth-Only Mode
nav_order: 2.7
---
# OAuth-Only Mode

>Important: OAuth-Only Mode relies entirely on an external identity provider and provisions accounts through it. [External logins](externalloginconfig.md) are not the same as OAuth-Only Mode. Those are simply a shortcut so your users don't need to remember their forum-specific credentials. They still create an account in the forum.
>
> External logins are great for public forums. For corporate or private forums coupled exlcusively to an external identity platform, use OAuth-Only Mode.

Starting in v20, POP Forums has an OAuth-only mode, which means that user authenticaton is handled entirely by a third party. Examples include OAuth providers of corporate identity systems, like Azure Active Directory, Okta and Auth0. In this mode, users can't create an account in the forum, they can only come in via  the external identity provider. The assignment of moderator and admin roles are mapped from claims issued by the identity provider.

> This mode is set at the configuration level (`appsettings.json` locally, or the typical environment variables in regular environments). There are consequences for changing this setting to `true` in an established instance of the forum. Existing users would not be mapped to identities from the external provider. Going the other direction would be possible, though each user would need to reset their password with the email address used by the identity system.

If you don't have a basic understanding of how OAuth works, now's a good time to do a little research. Here's how this mode works in the forum:
* The user can only access a page with a single login button.
* Clicking that button sends the user to the external identity provider.
* The user enters credentials with the provider if they aren't already logged in. You've likely done this before with "social" logins like Google or Facebook.
* The identity provider redirects the user back to the forum, with a JWT token.
* The forum calls the identity provider's token endpoint with the provided token and verifies its authenticity.
* In return, the identity provider returns information about the user called _claims_.
* The forum checks to see if there's a user account associated with the unique identifier from the provider (given in the `sub` claim). If there is no account, it's created with the provided name and email, otherwise it uses the existing one.
* The claims are compared to those configured in the forum for moderators and admins, and those roles are assigned to the user if they match.
* The forum uses an algorithm to reconcile the name and email of the user.
* The user is then logged in and browsing the forum.
* After a configured amount of time, the forum will use the refresh token issued by the provider to make sure the user is still legitimate, without the user having to authenticate again.

This mode uses OpenID Connect (OIDC) claims. The identity provider, in addition to the `sub` claim, must also return a `name` and `email` claim. The provider might need to be configured for this, but those claims should be present if it implements OIDC. We do have to ask for these claims by specifying `scope` in our request, which we'll get to in a minute.

## Configuring your OAuth Provider

The amount of access and configuration that you have in your identity provider varies a ton. At the very least, the provider should have OIDC enabled, and return `email` and `name` claims. Beyond that, it should return specific claims for forum admins and moderators. The forum can assign these roles based on just the presence of a claim, or by the claim and a specific value.

>In Azure Active Directory, for example, you can create a group and assign members to it. In an app registration, you can configure tokens to return groups as claims. The claims will all be named `roles`, with values that are guids that identify the groups' object ID's. So if a group called "Forum Admins" has an object ID of `978efeac-3baf-4e61-a519-9b06eb26a0bf`, the token will have a claim called `roles` with that guid as a value. With other identity providers, it may be possible to simply assign a claim called `ForumAdmin` with no value to represent a forum administrator.

Find out what _scopes_ are required to make sure you're getting the `name` and `email` claims, as well as those that identify your moderators and admins. The typical scope you'll specify, as it relates to the OIDC standard, is:
```
openid email profile offline_access
```
The last one, `offline_access`, is typically required to generate a refresh token, used as described in the flow above.   

Finally, the provider has to know what the valid redirect URL back to the forum is. How you set this varies by identity provider. This follows this pattern:
```
https://localhost:5091/Forums/Identity/CallbackHandler
```
This is what you would use to redirect to a locally running developer instance of the forum. For real environments, you replace `localhost:5091` with your domain, like `example.com`. Most providers require `https`.

## Configure POP Forums for OAuth-Only

Now that you understand the provider's needs, you can set up the configuration for the forum. These are in addition to the settings described on the [Start Here](starthere.md) page.
```
{
  "PopForums": {
    "OAuthOnly": {
      "IsOAuthOnly": true,
      "OAuthClientID": "provided by identity provider",
      "OAuthClientSecret": "provided by identity provider",
      "OAuthLoginBaseUrl": "where the forum redirects users",
      "OAuthTokenUrl": "where the forum validates tokens",
      "OAuthAdminClaimType": "name of the Admin role claim",
      "OAuthAdminClaimValue": "optional, value of the Admin role claim",
      "OAuthModeratorClaimType": "name of the Moderator role claim",
      "OAuthModeratorClaimValue": "optional, value of Moderator role claim",
      "OAuthScopes": "openid email profile offline_access",
      "OAuthRefreshExpirationMinutes": "60"
    }
  }
}
```
Here's what these do:
* `IsOAuthOnly`: The master switch for this mode. When set to true, all of the things the forum would do to manage user accounts, like account creation, passwords, email, goes away, delegating it to the OAuth identity provider.
* `OAuthClientID`: Sometimes called an "application ID," this value comes from the identity provider to understand what service (your forum, in this case) is talking to it.
* `OAuthClientSecret`: The forum uses this value when it calls back to the provider to validate or refresh a token.
* `OAuthLoginBaseUrl`: This is the base URL that the forum uses to redirect users to the identity provider. It appends this with a query string, including the callback URL (handled by the forum), the scope, and a state value.
* `OAuthTokenUrl`: The URL that the forum uses to validate a code or refresh token that came back from the user redirect.
* `OAuthAdminClaimType`: This is the name of the claim that identifies a user as a forum administrator.
* `OAuthAdminClaimValue`: This is the value of the above named claim that identifies a user as a forum administrator. If not set, the presence of a `OAuthAdminClaimType` claim with any or no value designates the user as a forum administrator.
* `OAuthModeratorClaimType`: This is the name of the claim that identifies a user as a forum moderator.
* `OAuthModeratorClaimValue`: This is the value of the above named claim that identifies a user as a forum moderator. If not set, the presence of a `OAuthModeratorClaimType` claim with any or no value designates the user as a forum administrator.
* `OAuthScopes`: The scopes to get from the identity provider so that it returns the `sub`, `name` and `email` claims. This is often how you tell the service to return a refresh token as well. Typically, this setting will use `openid email profile offline_access`.
* `OAuthRefreshExpirationMinutes`: The number of minutes that should pass until the forum asks the identity provider's token endpoint for an updated refresh token. If the user is no longer valid, they will be logged out on their next request. Use a value that is short enough to cause revoked accounts to be shut out, but long enough that every forum request isn't slowed by fetching a refresh token.

## Troubleshooting

Errors should appear right in the user interface. If you need additional context, check the `pf_SecurityLog` table in the database.