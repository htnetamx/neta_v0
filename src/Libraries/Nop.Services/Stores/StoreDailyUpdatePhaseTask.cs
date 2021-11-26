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
    public partial class StoreDailyUpdatePhaseTask : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public StoreDailyUpdatePhaseTask(IStoreService storeService, IOrderService orderService, ILogger logger)
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
            List<string> storesMovingToCompleteFase = new List<string>();

            foreach (var store in stores)
            {
                var orderExists = (await _orderService.GetOrdersByStoreIdsAsync(store.Id)).Any();
                if (orderExists)
                {
                    //Make sure old phase 3 values are updated to new phase 2 (growth phase)
                    if (store.DisplayOrder == 3)
                        store.DisplayOrder = 2;

                    //All stores are in the new phase scheme now
                    //At least one order exists in the store.
                    //Evaluate if phase 1 (initial) stores can move up to phase 2 (growth)

                    //CONDITIONS to move from display order 1 (Initial Phase) to 2 (Growth Phase)
                    // transition case : 7 days after first order

                    //If user meets, following condition, NetaCoin Bonus is activated, it remains on Initial Fase 
                    // bonus case : 500 GMV, 8 distinct customers

                    var ordersFromStore = (await _orderService.GetOrdersByStoreIdsAsync(store.Id));
                    var firstOrderDate = ordersFromStore.Select(x => x.CreatedOnUtc).Min();

                    ////evaluate if there are 5 distinct clients
                    var disntinctCustomers = ordersFromStore.Select(x => x.CustomerId).Distinct();
                    var disntinctCustomersCounter = disntinctCustomers.Count();

                    ////SUM of order total value (all)
                    var orderValueGMV = ordersFromStore.Select(x => x.OrderTotal).Sum();

                    if (store.DisplayOrder == 1)
                    {
                        ////Bonus CASE 500 GMV, 8 distinct customers
                        if (disntinctCustomersCounter > 8 && orderValueGMV > 500 && !store.FirstGmvBonusApplied)
                        {
                            store.NetaCoin = 200;
                            store.FirstGmvBonusApplied = true;
                            storesUnlokcingBonus.Add(store.Name.ToString());
                        }

                        //Transition Case
                        else if (DateTime.UtcNow.DayOfYear - firstOrderDate.DayOfYear > 6)
                        {
                            store.DisplayOrder = 2;
                            storesMovingToCompleteFase.Add(store.Name.ToString());
                        }
                    }

                    await _storeService.UpdateStoreAsync(store);

                }

            }

            if (storesMovingToCompleteFase.Count > 0)
            {
                var messagePhase = string.Join(",", storesMovingToCompleteFase);

                //Log de tiendas que cambian de Fase Limitada a Fase Completa
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Information,
                    "Tiendas que cambian de Fase Limitada a Fase Completa: " + storesMovingToCompleteFase.Count.ToString(), messagePhase);

                await SendMessageToAllRegisteredNumbers(storesMovingToCompleteFase.Count, messagePhase, "phase_advancement");

            }
            else
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Information,
                    "Tiendas que cambian de Fase Limitada a Fase Completa: 0", "Cero tiendas avanzaron a Fase Completa");

                await SendMessageToAllRegisteredNumbers(storesMovingToCompleteFase.Count, "Cero tiendas avanzaron a Fase Completa", "phase_advancement");
            }

            if (storesUnlokcingBonus.Count > 0)
            {
                var messageBonus = string.Join(",", storesUnlokcingBonus);

                //Log de tiendas que cambian de Fase Limitada a Fase Completa
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Information,
                    "Tiendas que activan bono de crecimiento rápido en Fase Limitada: " + storesUnlokcingBonus.Count.ToString(),
                    messageBonus);

                await SendMessageToAllRegisteredNumbers(storesUnlokcingBonus.Count, messageBonus, "rapid_growth_bonus_unlock");
            }
            else
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Information,
                    "Tiendas que activan bono de crecimiento rápido en Fase Limitada: 0",
                    "Cero tiendas activan bono de crecimiento rápido en Fase Limitada");

                await SendMessageToAllRegisteredNumbers(storesUnlokcingBonus.Count, "Cero tiendas activan bono de crecimiento rápido en Fase Limitada", "rapid_growth_bonus_unlock");
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
