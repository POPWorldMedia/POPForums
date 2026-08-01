---
layout: default
title: Subscriptions
nav_order: 2.7
---
# Subscriptions

POP Forums v23 introduced subscription integration with [Stripe](https://stripe.com/). This allows the owner of a site with a forum instance to display or hide certain things (like ads) based on the subscription status of a user.

## Integration

To use subscriptions, enable and setup the functionality as described below. Inject `IUserRetrievalShim` into the service or page that you want to subscription status to influence. When you have the user object, check its `IsSubscriber()` method, and use that boolean value in your logic. For example, you might find this in a view to hide ads:
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

`IsSubscriber()` is not a stored flag — it's computed on every request from `User.SubscriptionExpiration`, with a one-day grace period past the expiration date to cover the gap between midnight rollover and the daily renewal job actually running. This also means the `Subscriber` role is available anywhere roles are checked, e.g. `user.IsInRole(PermanentRoles.Subscriber)`, and can be assigned as a view or post role for a forum from the admin Forum Permissions screen, making it possible to build a subscriber-only forum.

## Configuring subscriptions

Subscription settings live in the admin console, under **Subscription Config**. This page only appears in the admin nav — and the "Subscriptions" link only appears in the site's user account menu — once subscriptions are turned on.

| Setting | Description                                                                                                                                                                                                                                      |
|---|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Subscriptions Enabled | Master switch. Must be checked for the "Subscriptions" account page, the buy/renewal flow, and the `Subscriber` role computation to be active.                                                                                                   |
| Renewal Interval (Milliseconds) | How often the in-process renewal worker polls the renewal queue, when running in-process background jobs (`AddPopForumsBackgroundJobs()`). Defaults to 60000 (one minute). Has no effect when using Azure functions. |
| Stripe Secret Key | Your Stripe account's secret API key, used server-side for all charge/customer calls. Kept out of view in the admin form (masked input).                                                                                                         |
| Stripe Publishable Key | Your Stripe account's publishable key, sent to the browser so Stripe.js can tokenize card details client-side. Safe to expose; this is meant to be public.                                                                                       |
| Currency | The ISO currency code (e.g. `usd`, `eur`, `gbp`) used both for the Stripe charge itself and for formatting prices in the UI (via `decimal.ToCurrencyString`, which maps the code to a matching culture for display). Defaults to `usd`.          |

Renewals are driven by two moving parts, both gated by whether the app is wired for Azure Functions (`AddPopForumsAzureFunctionsAndQueues()`) or in-process jobs (`AddPopForumsBackgroundJobs()`) in `Program.cs` — the two are mutually exclusive:
- **Daily enqueue**, at 12:01 PM UTC: finds every user whose `SubscriptionExpiration` is today *and* whose profile has auto-renewal turned on, and pushes them onto the renewal queue.
- **Renewal processing**: dequeues each user and attempts to charge their card on file, extending `SubscriptionExpiration` by the purchased SKU's `Months` on success.

Auto-renewal is a per-user setting (toggled from their own Subscriptions page), not global — a user whose subscription lapses with auto-renewal off simply falls out of the `Subscriber` role at the grace-period cutoff, with no charge attempted.

## Defining skus

SKUs are the purchasable subscription plans, managed from the admin console's **Subscription Skus** page.

| Field | Notes |
|---|---|
| SKU ID | A string key, set once at creation and immutable afterward — think of it as a slug, not an auto-incrementing ID. |
| Name | Display name shown to users on the buy page and in subscription history messages. |
| Description | Free text shown alongside the plan on the buy page. |
| Price | Decimal amount, charged in whole units of the configured currency. A price of `0` is allowed — see "Free SKUs" below. |
| Months | How many months a successful charge extends `SubscriptionExpiration` by. |
| Active | Whether the SKU can currently be purchased. |

SKUs cannot be deleted, only marked inactive — since existing transactions, subscription history entries, and user profiles reference a SKU by ID, removing the row outright would orphan that history. Inactive SKUs are hidden from the buy page (`ISkuService.GetAllActive()`) but remain visible and editable in the admin list. A renewal is still allowed to use an inactive SKU — only new purchases are restricted to active plans — so existing subscribers on a retired plan keep renewing normally until they change plans or cancel.

### Ordering

SKUs have a persisted `SortOrder`, managed the same way forum/category ordering works: Up/Down buttons on the admin Subscription Skus list, backed by `ISkuService.MoveSkuUp`/`MoveSkuDown`, which renumber the whole set (`0, 2, 4, ...`) rather than doing a simple swap. The buy page lists SKUs in this same order (`ISkuService.GetAllActive()` now sorts by `SortOrder`, not name). A newly created SKU is appended to the end of the order automatically.

### Free SKUs

A SKU priced at `0` can be purchased without ever hitting Stripe: `IBankChargeRepository.ChargeCustomer` short-circuits for `amount <= 0` and returns a synthetic successful `Transaction` (`Status = "no_charge"`, no `ProcessorID`) instead of calling Stripe's charge API. This exists because Stripe rejects zero-amount charges outright (its API requires the amount be at least 1 in the currency's smallest unit), which previously surfaced as a raw Stripe error ("This value must be greater than or equal to 1.") on the buy page. The same short-circuit covers the renewal path, since both `BuyService` and `RenewalService` charge through this one method.