---
layout: default
title: External Login Configuration
nav_order: 5
---
# External Login Configuration
Starting in v16, POP Forums is completely decoupled from the Identity libraries that verify user identity via third party services, including Google, Facebook and Microsoft. We already don't use Identity because it's so tightly coupled to Entity Framework, with strong opinions about how to store user data. Identity also requires that you configure it at app start (or restart if you change it), and it can't be changed at request time. That prevents a multi-tenancy scenario from working. It was time to cut the cord.

We spun off the [PopIdentity](https://github.com/POPWorldMedia/POPIdentity) project to be a lightweight, non-opinionated means to do the necessary round trips to identity providers and just give you the data that you want, mostly the ID, name and email of the user. It does not bake the identity into a `Principal` for general use. Check out the sample project there for more information.

In POP Forums, you can go to the External Logins page of the admin area and configure Google, Facebook, Microsoft and any generic OAuth2 provider that returns JWT's. Check the box, fill in the client ID and secret from the providers. For each, you'll need to specify the callback URL. These are configured:
* In Facebook's developer administration, under the "Facebook Login" and "Products" navigation at left.
* In the Google Cloud Console, drill down to "Credentials" under "API's and Services."
* In Microsoft's Azure portal, search for "Azure Active Directory," choose "App Registrations," choose or create your app, then under "Authentication" enter your redirect URL.

The format for the URL is this, substituting in your domain: `https://whateveryourdomainis/Forums/Identity/CallbackHandler`

WARNING: That URL might be case sensitive in some services. Use the caps in "Forums" and such, because that's how the app will generate the URL.

Once configured, you'll see buttons on the login page for each service you've enabled.

There's also one for any other OAuth2 provider that returns JWT's. For that option, you'll need to fill in the URL's for the base login URL (POP Forums will append the appropriate query string) and token fetching URL (again, we'll handle the query string).