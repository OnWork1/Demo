using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Caching
{

	public class Cache
	{
		#region enum CachePriority
		public enum CachePriority
		{
			Default,
			NotRemovable
		} 
		#endregion

		// Constants - Konstanty

		// Delegates - Delegate

		// Events - Eventy

		// Private Fields - Privátní proměné
		private static ObjectCache cache;
		private CacheItemPolicy policy;
		private CacheEntryRemovedCallback callback;

		// Constructors - Konstruktory
		#region Cache()
		public Cache()
		{
			if (Cache.cache == null)
			{
				Cache.cache = MemoryCache.Default;
			}
			this.policy = null;
			this.callback = null;
		} 
		#endregion

		// Private Properties - Privátní vlastnosti

		// Protected Properties - Protected vlastnosti

		// Public Properties - Public vlastnosti

		// Private Methods - Privátní metody
		#region MyCachedItemRemovedCallback(CacheEntryRemovedArguments arguments)
		private void MyCachedItemRemovedCallback(CacheEntryRemovedArguments arguments)
		{
			// Log these values from arguments list 
			String strLog = String.Concat("Reason: ", arguments.RemovedReason.ToString(), " | Key-Name: ",
										  arguments.CacheItem.Key, " | Value-Object: ",
										  arguments.CacheItem.Value.ToString());
			//TODO: implement logging logic
		} 
		#endregion

		// Protected Methods - Protected metody

		// Public Methods - Public metody
		#region AddToCache(String cacheKeyName, Object cacheItem)
		public void AddToCache(String cacheKeyName, Object cacheItem)
		{
			this.AddToCache(cacheKeyName, cacheItem, CachePriority.Default, null);
		} 
		#endregion

		#region AddToCache(String cacheKeyName, Object cacheItem, CachePriority myCacheItemPriority)
		public void AddToCache(String cacheKeyName, Object cacheItem,
			CachePriority myCacheItemPriority)
		{
			this.AddToCache(cacheKeyName, cacheItem, myCacheItemPriority, null);
		} 
		#endregion

		#region AddToCache(String cacheKeyName, Object cacheItem, CachePriority myCacheItemPriority, List<String> filePath)
		public void AddToCache(String cacheKeyName, Object cacheItem,
			CachePriority myCacheItemPriority, List<String> filePath)
		{
			this.callback = this.MyCachedItemRemovedCallback;
			this.policy = new CacheItemPolicy
							{
								Priority = (myCacheItemPriority == CachePriority.Default)
											? CacheItemPriority.Default
											: CacheItemPriority.NotRemovable,
								AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(10000.00),
								RemovedCallback = this.callback
							};
			if (filePath != null)
			{
				this.policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePath));
			}

			// Add inside cache 
			Cache.cache.Set(cacheKeyName, cacheItem, this.policy);
		} 
		#endregion

		#region GetMyCachedItem(String cacheKeyName)
		public Object GetMyCachedItem(String cacheKeyName)
		{
			return Cache.cache[cacheKeyName];
		} 
		#endregion

		#region GetMyCachedItem<T>(String cacheKeyName, out T cachedItem)
		public bool GetMyCachedItem<T>(String cacheKeyName, out T cachedItem)
		{
			cachedItem = default(T);
			try
			{
				if (Cache.cache.Contains(cacheKeyName))
				{
					cachedItem = (T)(Cache.cache[cacheKeyName]);
					return true;
				}
				return false;
			}
			catch
			{
                cachedItem = default(T);
            }

			return false;

		} 
		#endregion

		#region RemoveMyCachedItem(String cacheKeyName)
		public void RemoveMyCachedItem(String cacheKeyName)
		{
			if (Cache.cache.Contains(cacheKeyName))
			{
				Cache.cache.Remove(cacheKeyName);
			}
		} 
		#endregion
		// Event Handlers - Události
	}
}
