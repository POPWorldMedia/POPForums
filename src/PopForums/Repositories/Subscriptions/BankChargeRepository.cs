using Stripe;

namespace PopForums.Repositories.Subscriptions;

public interface IBankChargeRepository
{
	Task<BasicServiceResponse<CreateCustomerResult>> CreateCustomer(string token, string email, int userID);

	Task<BasicServiceResponse<Transaction>> ChargeCustomer(string customerID, int userID, decimal amount, DateTime timeStamp, string skuID,
		string skuName, string email);
}

public class BankChargeRepository(IErrorLog errorLog, ISettingsManager settingsManager) : IBankChargeRepository
{
	public async Task<BasicServiceResponse<CreateCustomerResult>> CreateCustomer(string token, string email, int userID)
	{
		var customerOptions = new CustomerCreateOptions
		{
			Source = token,
			Email = email,
			Metadata = new Dictionary<string, string>{{"UserID", userID.ToString()}},
			Expand = ["sources.data.last4"]
		};
		try
		{
			StripeConfiguration.ApiKey = settingsManager.Current.StripeSecretKey;
			var customerService = new CustomerService();
			var customer = await customerService.CreateAsync(customerOptions);
			var last4 = ((Card) customer.Sources?.Data[0])?.Last4;
			var data = new CreateCustomerResult {CustomerID = customer.Id, Last4 = last4};
			return BasicServiceResponse<CreateCustomerResult>.Success(data);
		}
		catch (StripeException exc)
		{
			errorLog.Log(exc, ErrorSeverity.Information, $"Error: {exc.StripeError.Error}; Desc: {exc.StripeError.ErrorDescription}");
			return BasicServiceResponse<CreateCustomerResult>.Failed(exc.StripeError.Message);
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Critical);
			return BasicServiceResponse<CreateCustomerResult>.Failed("There was an error creating your customer card record with our processor.");
		}
	}

	public async Task<BasicServiceResponse<Transaction>> ChargeCustomer(string customerID, int userID, decimal amount, DateTime timeStamp, string skuID, string skuName, string email)
	{
		if (amount <= 0)
		{
			var freeTransaction = new Transaction
			{
				ProcessorID = string.Empty,
				CustomerID = customerID,
				Status = "no_charge",
				UserID = userID,
				TimeStamp = timeStamp,
				SkuID = skuID,
				Amount = amount
			};
			return BasicServiceResponse<Transaction>.Success(freeTransaction);
		}

		var chargeOptions = new ChargeCreateOptions
		{
			Amount = (long)amount * 100,
			Currency = settingsManager.Current.Currency,
			Customer = customerID,
			Description = skuName,
			ReceiptEmail = email
		};
		try
		{
			StripeConfiguration.ApiKey = settingsManager.Current.StripeSecretKey;
			var chargeService = new ChargeService();
			var charge = await chargeService.CreateAsync(chargeOptions);
			var transaction = new Transaction
			{
				ProcessorID = charge.Id,
				CustomerID = charge.CustomerId,
				Status = charge.Status,
				Raw = charge.StripeResponse?.Content,
				Last4 = charge.PaymentMethodDetails?.Card?.Last4,
				UserID = userID,
				TimeStamp = timeStamp,
				SkuID = skuID,
				Amount = amount
			};
			return BasicServiceResponse<Transaction>.Success(transaction);
		}
		catch (StripeException exc)
		{
			errorLog.Log(exc, ErrorSeverity.Information, $"Error: {exc.StripeError?.Error}; Message: {exc.StripeError?.Message}; Desc: {exc.StripeError?.ErrorDescription}");
			return BasicServiceResponse<Transaction>.Failed(exc.Message);
		}
		catch (Exception exc)
		{
			errorLog.Log(exc, ErrorSeverity.Critical);
			return BasicServiceResponse<Transaction>.Failed("There was an error with our processor charging your card.");
		}
	}
}