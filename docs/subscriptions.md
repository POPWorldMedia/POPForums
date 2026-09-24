---
layout: default
title: Subscriptions
nav_order: 2.7
---
# Subscriptions

POP Forums v23 introduced subscription integration with [Stripe](https://stripe.com/). This allows the owner of a site with a forum instance to display or hide certain things (like ads) based on the subscription status of a user.

## Integration

To use subscriptions, enable and setup the functionality as described below. Inject `IUserRetrievalShim` into the service or page that you want subscriptions to influence. When you have the user object, check its `IsSubscriber()` method, and use that boolean value in your logic. For example, you could use the following to hide ads for subscribers in a page layou:
```
@inject IUserRetrievalShim UserRetrievalShim
@{
    var user = UserRetrievalShim.GetUser();
    var isSubscriber = user != null && user.IsSubscriber();
}
...
@if (!isSubscriber) {
    <div> *** ad content *** </div>
}
```

`IsSubscriber()` is computed on every request from the user's subscription expiration value, with a one-day grace period past the expiration date to cover the gap between midnight rollover and the daily renewal job running. This also means the `Subscriber` role is available anywhere roles are checked (`user.IsInRole(PermanentRoles.Subscriber)`) and can be assigned as a view or post role for a forum from the admin Forum Permissions screen, making it possible to build a subscriber-only forum.

## Configuring subscriptions

Subscription settings live in the admin console, under **Subscription Config**. The "Subscriptions" link in the user navigation only when subscriptions are turned on.

| Setting | Description                                                                                                                                                                                                                                      |
|---|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Subscriptions Enabled | Master switch. Must be checked for the "Subscriptions" account page, the buy/renewal flow, and the `Subscriber` role computation to be active.                                                                                                   |
| Renewal Interval (Milliseconds) | How often the in-process renewal worker polls the renewal queue, when running in-process background jobs (`AddPopForumsBackgroundJobs()`). Defaults to 60000 (one minute). Has no effect when using Azure functions. |
| Stripe Secret Key | Your Stripe account's secret API key, used server-side for all charge/customer calls.                                                                                                     |
| Stripe Publishable Key | Your Stripe account's publishable key, sent to the browser so Stripe.js can tokenize card details client-side. Safe to expose; this is meant to be public.                                                                                       |
| Currency | The ISO currency code (e.g. `usd`, `eur`, `gbp`) used both for the Stripe charge itself and for formatting prices in the UI (via `decimal.ToCurrencyString`, which maps the code to a matching culture for display). Defaults to `usd`.          |

Renewals are driven by asynchronous services for Azure Functions (`AddPopForumsAzureFunctionsAndQueues()`) or in-process jobs (`AddPopForumsBackgroundJobs()`) in `Program.cs`.
- **Daily enqueue**, at 12:01 PM UTC: finds every user whose `SubscriptionExpiration` is today *and* whose profile has auto-renewal turned on, and pushes them onto the renewal queue.
- **Renewal processing**: dequeues each user and attempts to charge their card on file, extending `SubscriptionExpiration` by the purchased SKU's `Months` on success.

Auto-renewal is a per-user setting (on the user's own Subscriptions page). If it's not on, they will not automatically renew.

## Defining skus

SKUs are the purchasable subscription plans, managed from the admin console's **Subscription Skus** page.

| Field | Notes |
|---|---|
| SKU ID | A string key, set once at creation and immutable afterward. |
| Name | Display name shown to users on the buy page and in subscription history messages. |
| Description | Text shown alongside the plan on the buy page. |
| Price | Decimal amount, charged in whole units of the configured currency. A price of `0` is allowed — see "Free SKUs" below. |
| Months | How many months a successful charge extends `SubscriptionExpiration` by. |
| Active | Whether the SKU can currently be purchased. |

SKUs cannot be deleted, only marked inactive, since existing transactions, subscription history entries, and user profiles reference a SKU by ID. Inactive SKUs are hidden from the buy page but remain visible and editable in the admin list. A renewal is still allowed to use an inactive SKU. Only new purchases are restricted to active plans, so existing subscribers on a retired plan keep renewing normally until they change plans or cancel.

### Ordering

SKUs have a persisted sort order, managed the same way forum/category ordering works with up/down buttons. A newly created SKU is appended to the end of the order automatically.

### Free SKUs

A SKU priced at `0` can be purchased without ever hitting Stripe.

## Managing a user's subscription

**Edit User Sub** in the admin area allows you to find a user and edit their subscription information. There you can view their subscription history, including any failures, and the option to apply a subscription without payment.

## Refunds

The POP Forums subscription system does not handle refunds directly. In order to reverse a charge, you'll have to use Stripe's administration app to do so.
