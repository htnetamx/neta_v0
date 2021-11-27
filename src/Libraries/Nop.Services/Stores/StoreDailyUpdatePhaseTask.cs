using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tasks;

namespace Nop.Services.Stores
{
    public partial class StoreDailyBonusFirstWeek : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public StoreDailyBonusFirstWeek(IStoreService storeService, IOrderService orderService, ILogger logger)
        {
            _storeService = storeService;
            _orderService = orderService;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var stores = (await _storeService.GetAllStoresAsync())
                .Where(s => s.DisplayOrder == 1);

            List<string> storesUnlokcingBonus = new List<string>();
            List<string> storesUnlokcingBonusGoalCustomer = new List<string>();

            foreach (var store in stores)
            {
                var orderExists = (await _orderService.GetOrdersByStoreIdsAsync(store.Id)).Any();
                if (orderExists)
                {
                    //Make sure old phase 3 values are updated to new phase 2 (growth phase)
                    if (store.DisplayOrder == 3)
                        store.DisplayOrder = 2;

                    var ordersFromStore = (await _orderService.GetOrdersByStoreIdsAsync(store.Id));
                    var firstOrderDate = ordersFromStore.Select(x => x.CreatedOnUtc).Min();

                    // Bonus cases 7 days after first order && x customer qty achived
                    if (DateTime.UtcNow.DayOfYear - firstOrderDate.DayOfYear > 6)
                    {

                        ////evaluate if there are 8 or more distinct clients
                        var disntinctCustomers = ordersFromStore.Select(x => x.CustomerId).Distinct();
                        var disntinctCustomersCounter = disntinctCustomers.Count();

                        ////SUM of order total value (all)
                        var orderValueGMV = ordersFromStore.Select(x => x.OrderTotal).Sum();

                        ////Bonus CASE 500 GMV, achive 8 distinct customers
                        if (disntinctCustomersCounter > 8 && orderValueGMV > 500 && !store.FirstGmvBonusApplied)
                        {
                            store.NetaCoin += 200;
                            store.FirstGmvBonusApplied = true;
                            storesUnlokcingBonus.Add(store.Name.ToString());
                        }

                        ////Bonus CASE achive 15 distinct customers
                        if (disntinctCustomersCounter > 14 && !store.FirstGoalCustomerBonusApplied)
                        {
                            store.NetaCoin += 200;
                            store.FirstGoalCustomerBonusApplied = true;
                            storesUnlokcingBonusGoalCustomer.Add(store.Name.ToString());
                        }

                        await _storeService.UpdateStoreAsync(store);

                    }

                }

            }

            if (storesUnlokcingBonus.Count > 0)
            {
                var messageBonus = string.Join(",", storesUnlokcingBonus);

                //Log de tiendas que cambian activan bono gmv en primera semana
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Information,
                    "Tiendas que activan bono de crecimiento rápido en primera semana: " + storesUnlokcingBonus.Count.ToString(),
                    messageBonus);

                await SendMessageToAllRegisteredNumbers(storesUnlokcingBonus.Count, messageBonus, "rapid_growth_bonus_unlock");
            }
            else
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Information,
                    "Tiendas que activan bono de crecimiento rápido en primera semana: 0",
                    "Cero tiendas activan bono de crecimiento rápido en primera semana");

                await SendMessageToAllRegisteredNumbers(storesUnlokcingBonus.Count, "Cero tiendas activan bono de crecimiento rápido en primera semana", "rapid_growth_bonus_unlock");
            }


            if (storesUnlokcingBonusGoalCustomer.Count > 0)
            {
                var messageBonus = string.Join(",", storesUnlokcingBonusGoalCustomer);

                //Log de tiendas que logran 15 clientes en una semana
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Information,
                    "Tiendas que reciben bono por alcance de 15 clientes en primera semana: " + storesUnlokcingBonusGoalCustomer.Count.ToString(),
                    messageBonus);

                await SendMessageToAllRegisteredNumbers(storesUnlokcingBonusGoalCustomer.Count, messageBonus, "ultrarapid_growth_bonus_unlock");
            }
            else
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Information,
                    "Tiendas que reciben bono por alcance de 15 clientes en primera semana: 0",
                    "Cero tiendas que reciben bono por alcance de 15 clientes en primera semana");

                await SendMessageToAllRegisteredNumbers(storesUnlokcingBonusGoalCustomer.Count, "Cero tiendas que reciben bono por alcance de 15 clientes en primera semana", "ultrarapid_growth_bonus_unlock");
            }
        }

        public async Task<string> SendMessageToAllRegisteredNumbers(int numStores, string message, string template)
        {
            string allReponses = "";
 
            //Registra a los usuarios que recibiran el mensaje. 
            List<string> registeredNumbers = new List<string>();
            registeredNumbers.Add("5216181028033"); //Jorge Garduno
            registeredNumbers.Add("5215527369519"); //Enrique Roman
            registeredNumbers.Add("5713144115632"); //Samuel Giraldo
            registeredNumbers.Add("5213327428768"); //Miguel Zamora

            DateTime timeUtc = DateTime.UtcNow;
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);

                //Send Messages
                foreach (var number in registeredNumbers)
            {

            string response = await BotmakerMessaging.Send("525545439866",
                number,
                template,
                new Dictionary<string, object> { { "fecha", cstTime.ToString() +  " CST" },
                                                         { "numero_de_tiendas", numStores},
                                                         { "string_de_tiendas", message }
                });

                _ = allReponses.Concat(response);
            }
            return allReponses;
        }
        #endregion
    }
}
