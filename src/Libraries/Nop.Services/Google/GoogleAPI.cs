using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using Nop.Services.Stores;
using Nop.Services.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Shipping;
using Nop.Core.Domain.Stores;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Nop.Services.Google;
using System.Text.RegularExpressions;

namespace Nop.Services.Events
{
    public static class GoogleAPI
    {
        #region Fields
        static readonly string[] _scopes = {SheetsService.Scope.Spreadsheets};
        static readonly string _applicationName = "Neta.Mx-NOP";
        static SheetsService _service;
        static GoogleCredential _credential=Credentials();
        #endregion

        #region Methods
        /// <summary>
        /// Gets Credentials For Google API
        /// </summary>
        public static GoogleCredential Credentials()
        {
            using (var stream =
                new FileStream("Google_Credentials.json", FileMode.Open, FileAccess.Read))
            {
                _credential = GoogleCredential.FromStream(stream).CreateScoped(_scopes);
            }
            CreateGetSetSheetsService();
            return _credential;
        }

        /// <summary>
        /// Sets Service
        /// </summary>
        public static SheetsService CreateGetSetSheetsService()
        {
            _service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = _applicationName,
            });
            return _service;
        }

        /// <summary>
        /// Reads from a Google Spread Sheet
        /// </summary>
        public static IList<IList<object>> ReadSpreadSheet(string spreadsheetId, string sheet, string rangeA1D1Format)
        {
            var range = $"{sheet}!{rangeA1D1Format}";
            var request = _service.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                return values;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Write Data To Google Spread Sheet - Shops Errors
        /// </summary>
        public static AppendValuesResponse AppendOnSpreadSheetInfoShopsErros(string spreadsheetId, string sheet, string rangeA1D1Format,List<InfoShopsErrors> values)
        {
            var range = $"{sheet}!{rangeA1D1Format}";

            var valueRange = new ValueRange();
            var fullInfo = new List<IList<object>>();
            fullInfo.Add(InfoShopsErrors.Headers());
            foreach (var row in values)
            {
                if (row.HasErrors())
                {
                    fullInfo.Add(row.ToStringList());
                }
            }
            valueRange.Values = fullInfo;
            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            return appendResponse;
        }

        /// <summary>
        /// Write Data To Google Spread Sheet - 9 to 9 process Ops
        /// </summary>
        public static AppendValuesResponse AppendOnSpreadSheet929OpsGMV(string spreadsheetId, string sheet, string rangeA1D1Format, List<NineToNineOpsGMV> values, List<object> datesMX, List<object> datesUTC)
        {
            var range = $"{sheet}!{rangeA1D1Format}";

            var valueRange = new ValueRange();
            var fullInfo = new List<IList<object>>();
            fullInfo.Add(datesMX);
            fullInfo.Add(datesUTC);
            fullInfo.Add(NineToNineOpsGMV.Headers());
            foreach (var row in values)
            {
                fullInfo.Add(row.ToStringList());
            }
            valueRange.Values = fullInfo;
            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            return appendResponse;
        }

        public static AppendValuesResponse AppendOnSpreadSheet929Ops(string spreadsheetId, string sheet, string rangeA1D1Format, List<NineToNineOps> values, List<object> datesMX, List<object> datesUTC)
        {
            var range = $"{sheet}!{rangeA1D1Format}";

            var valueRange = new ValueRange();
            var fullInfo = new List<IList<object>>();
            fullInfo.Add(datesMX);
            fullInfo.Add(datesUTC);
            fullInfo.Add(NineToNineOps.Headers());
            foreach (var row in values)
            {
                fullInfo.Add(row.ToStringList());
            }
            valueRange.Values = fullInfo;
            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            return appendResponse;
        }
        public static AppendValuesResponse AppendOnSpreadSheet929OpsStoreUniqueClients(string spreadsheetId, string sheet, string rangeA1D1Format, List<NineToNineOpsUnique> values, List<object> datesMX, List<object> datesUTC)
        {
            var range = $"{sheet}!{rangeA1D1Format}";

            var valueRange = new ValueRange();
            var fullInfo = new List<IList<object>>();
            fullInfo.Add(datesMX);
            fullInfo.Add(datesUTC);
            fullInfo.Add(NineToNineOpsUnique.Headers());
            foreach (var row in values)
            {
                fullInfo.Add(row.ToStringList());
            }
            valueRange.Values = fullInfo;
            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            return appendResponse;
        }
        public static AppendValuesResponse AppendOnSpreadSheet929OpsStoreUniqueClientsC10(string spreadsheetId, string sheet, string rangeA1D1Format, List<NineToNineOpsUniqueC10> values, List<object> datesMX, List<object> datesUTC)
        {
            var range = $"{sheet}!{rangeA1D1Format}";

            var valueRange = new ValueRange();
            var fullInfo = new List<IList<object>>();
            fullInfo.Add(datesMX);
            fullInfo.Add(datesUTC);
            fullInfo.Add(NineToNineOpsUniqueC10.Headers());
            foreach (var row in values)
            {
                fullInfo.Add(row.ToStringList());
            }
            valueRange.Values = fullInfo;
            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            return appendResponse;
        }
        public static AppendValuesResponse AppendOnSpreadSheet929OpsAllStores(string spreadsheetId, string sheet, string rangeA1D1Format, List<NineToNineOpsAllStores> values, List<object> datesMX, List<object> datesUTC)
        {
            var range = $"{sheet}!{rangeA1D1Format}";

            var valueRange = new ValueRange();
            var fullInfo = new List<IList<object>>();
            fullInfo.Add(datesMX);
            fullInfo.Add(datesUTC);
            fullInfo.Add(NineToNineOpsAllStores.Headers());
            foreach (var row in values)
            {
                fullInfo.Add(row.ToStringList());
            }
            valueRange.Values = fullInfo;
            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            return appendResponse;
        }
        public static AppendValuesResponse AppendOnSpreadSheet929OpsDispatch(string spreadsheetId, string sheet, string rangeA1D1Format, List<NineToNineOpsStoresDispatchDecision> values, List<object> datesMX, List<object> datesUTC)
        {
            var range = $"{sheet}!{rangeA1D1Format}";

            var valueRange = new ValueRange();
            var fullInfo = new List<IList<object>>();
            fullInfo.Add(datesMX);
            fullInfo.Add(datesUTC);
            fullInfo.Add(NineToNineOpsStoresDispatchDecision.Headers());
            foreach (var row in values)
            {
                fullInfo.Add(row.ToStringList());
            }
            valueRange.Values = fullInfo;
            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            return appendResponse;
        }

        public static AppendValuesResponse AppendOnSpreadSheet929OpsDispatchTaras(string spreadsheetId, string sheet, string rangeA1D1Format, List<NineToNineOpsStoresDispatchTarasSymmary> values, List<object> datesMX, List<object> datesUTC)
        {
            var range = $"{sheet}!{rangeA1D1Format}";

            var valueRange = new ValueRange();
            var fullInfo = new List<IList<object>>();
            fullInfo.Add(datesMX);
            fullInfo.Add(datesUTC);
            fullInfo.Add(NineToNineOpsStoresDispatchTarasSymmary.Headers());
            foreach (var row in values)
            {
                fullInfo.Add(row.ToStringList());
            }
            valueRange.Values = fullInfo;
            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            return appendResponse;
        }

        public static AppendValuesResponse AppendOnSpreadSheet929OpsDispatchOrders(string spreadsheetId, string sheet, string rangeA1D1Format, List<NineToNineOpsStoresDispatchOrderSummary> values, List<object> datesMX, List<object> datesUTC)
        {
            var range = $"{sheet}!{rangeA1D1Format}";

            var valueRange = new ValueRange();
            var fullInfo = new List<IList<object>>();
            fullInfo.Add(datesMX);
            fullInfo.Add(datesUTC);
            fullInfo.Add(NineToNineOpsStoresDispatchOrderSummary.Headers());
            foreach (var row in values)
            {
                fullInfo.Add(row.ToStringList());
            }
            valueRange.Values = fullInfo;
            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            return appendResponse;
        }
        public static AppendValuesResponse AppendOnSpreadSheetLatLong(string spreadsheetId, string sheet, string rangeA1D1Format, List<LatLong> values)
        {
            var range = $"{sheet}!{rangeA1D1Format}";

            var valueRange = new ValueRange();
            var fullInfo = new List<IList<object>>();
            fullInfo.Add(LatLong.Headers());
            foreach (var row in values)
            {
                fullInfo.Add(row.ToStringList());
            }
            valueRange.Values = fullInfo;
            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
            return appendResponse;
        }
        public static ClearValuesResponse DeleteSpreadSheetContent(string spreadsheetId, string sheet, string rangeA1D1Format)
        {
            ClearValuesRequest clearValuesRequest = new ClearValuesRequest();
            var clearRequest= _service.Spreadsheets.Values.Clear(clearValuesRequest,spreadsheetId, "'"+sheet+"'!"+rangeA1D1Format);
            var clearResponse = clearRequest.Execute();
            return clearResponse;
        }

        #endregion
    }
}
