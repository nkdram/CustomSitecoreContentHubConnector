using System;
using System.Threading;
using System.Threading.Tasks;
using Sitecore.Diagnostics;

namespace CMP.Connector.Helpers
{
	internal static class Retryer
	{
		internal static async Task<T> Retry<T>(Func<Task<T>> func, int maxRetries, int millisecondsDelay, CancellationToken cancellationToken = default(CancellationToken))
		{
			Assert.ArgumentNotNull(func, "func");
			Assert.ArgumentCondition(maxRetries > 0, "maxRetries", "maxRetries should be greater than zero.");
			Assert.ArgumentCondition(millisecondsDelay > 0, "millisecondsDelay", "millisecondsDelay should be greater than zero.");
			while (maxRetries-- > 1)
			{
				cancellationToken.ThrowIfCancellationRequested();
				try
				{
					return await func().ConfigureAwait(continueOnCapturedContext: false);
				}
				catch
				{
					await Task.Delay(millisecondsDelay, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			return await func().ConfigureAwait(continueOnCapturedContext: false);
		}

		internal static async Task Retry(Func<Task> func, int maxRetries, int millisecondsDelay, CancellationToken cancellationToken = default(CancellationToken))
		{
			Assert.ArgumentNotNull(func, "func");
			await Retry(Execute, maxRetries, millisecondsDelay, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			async Task<bool> Execute()
			{
				await func().ConfigureAwait(continueOnCapturedContext: false);
				return true;
			}
		}
	}
}