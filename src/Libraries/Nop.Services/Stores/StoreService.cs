using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nop.Core.Domain.Stores;
using Nop.Data;

namespace Nop.Services.Stores
{
    /// <summary>
    /// Store service
    /// </summary>
    public partial class StoreService : IStoreService
    {
        #region Fields

        private readonly IRepository<Store> _storeRepository;

        #endregion

        #region Ctor

        public StoreService(IRepository<Store> storeRepository)
        {
            _storeRepository = storeRepository;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Parse comma-separated Hosts
        /// </summary>
        /// <param name="store">Store</param>
        /// <returns>Comma-separated hosts</returns>
        protected virtual string[] ParseHostValues(Store store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var parsedValues = new List<string>();
            if (string.IsNullOrEmpty(store.Hosts))
                return parsedValues.ToArray();

            var hosts = store.Hosts.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var host in hosts)
            {
                var tmp = host.Trim();
                if (!string.IsNullOrEmpty(tmp))
                    parsedValues.Add(tmp);
            }

            return parsedValues.ToArray();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a store
        /// </summary>
        /// <param name="store">Store</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteStoreAsync(Store store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var allStores = await GetAllStoresAsync();
            if (allStores.Count == 1)
                throw new Exception("You cannot delete the only configured store");

            await _storeRepository.DeleteAsync(store);
        }

        /// <summary>
        /// Gets all stores
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the stores
        /// </returns>
        /// 
        //
        public virtual async Task<IList<Store>> GetAllStoresAsync(string nameFilter=null)
        {
            var result = await _storeRepository.GetAllAsync(query =>
            {
                return from s in query orderby s.DisplayOrder, s.Id select s;
            }, cache => default);
            
            if (nameFilter != null && nameFilter != "")
            {   
                result = await result.Where(s => (RemoveSpecialCharacters(s.Name.Trim(), true).Contains(RemoveSpecialCharacters(nameFilter.Trim(), true))) || (CalculateSimilarity(RemoveSpecialCharacters(s.Name.Trim(), true),RemoveSpecialCharacters(nameFilter.Trim(), true))>0.79)).ToListAsync();
            }
            return result;
        }
        public static string RemoveSpecialCharacters(string str, bool lowercase)
        {
            var newstr = lowercase ? str.ToLower() : str;
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(newstr);
            newstr= System.Text.Encoding.UTF8.GetString(tempBytes);
            newstr = Regex.Replace(newstr, "[^a-zA-Z0-9_. ]+", "", RegexOptions.Compiled);
            return newstr;
        }
        private int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null))
                return 0;
            if ((source.Length == 0) || (target.Length == 0))
                return 0;
            if (source == target)
                return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++)
                ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++)
                ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }
        private double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null))
                return 0.0;
            if ((source.Length == 0) || (target.Length == 0))
                return 0.0;
            if (source == target)
                return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
        /// <summary>
        /// Gets a store 
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the store
        /// </returns>
        public virtual async Task<Store> GetStoreByIdAsync(int storeId)
        {
            return await _storeRepository.GetByIdAsync(storeId);
        }

        /// <summary>
        /// Inserts a store
        /// </summary>
        /// <param name="store">Store</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertStoreAsync(Store store)
        {
            await _storeRepository.InsertAsync(store);
        }

        /// <summary>
        /// Updates the store
        /// </summary>
        /// <param name="store">Store</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateStoreAsync(Store store)
        {
            await _storeRepository.UpdateAsync(store);
        }

        /// <summary>
        /// Indicates whether a store contains a specified host
        /// </summary>
        /// <param name="store">Store</param>
        /// <param name="host">Host</param>
        /// <returns>true - contains, false - no</returns>
        public virtual bool ContainsHostValue(Store store, string host)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            if (string.IsNullOrEmpty(host))
                return false;

            var contains = ParseHostValues(store).Any(x => x.Equals(host, StringComparison.InvariantCultureIgnoreCase));

            return contains;
        }

        /// <summary>
        /// Returns a list of names of not existing stores
        /// </summary>
        /// <param name="storeIdsNames">The names and/or IDs of the store to check</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of names and/or IDs not existing stores
        /// </returns>
        public async Task<string[]> GetNotExistingStoresAsync(string[] storeIdsNames)
        {
            if (storeIdsNames == null)
                throw new ArgumentNullException(nameof(storeIdsNames));

            var query = _storeRepository.Table;
            var queryFilter = storeIdsNames.Distinct().ToArray();
            //filtering by name
            var filter = await query.Select(store => store.Name)
                .Where(store => queryFilter.Contains(store))
                .ToListAsync();
            queryFilter = queryFilter.Except(filter).ToArray();

            //if some names not found
            if (!queryFilter.Any())
                return queryFilter.ToArray();

            //filtering by IDs
            filter = await query.Select(store => store.Id.ToString())
                .Where(store => queryFilter.Contains(store))
                .ToListAsync();
            queryFilter = queryFilter.Except(filter).ToArray();

            return queryFilter.ToArray();
        }

        #endregion
    }
}