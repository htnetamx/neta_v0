using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
namespace Nop.Services.Google
{

    public class AComparer : IEqualityComparer<NineToNineOps>
    {

        #region IEqualityComparer<NineToNineOps> Members

        public bool Equals(NineToNineOps x, NineToNineOps y)
        {
            return x.Order.Id == y.Order.Id;
        }

        public int GetHashCode(NineToNineOps obj)
        {
            return obj.Order.Id.GetHashCode();
        }

        #endregion
    }
    public class NineToNineOpsStoresDispatchDecisionsComparer : IEqualityComparer<NineToNineOpsStoresDispatchDecision>
    {

        #region IEqualityComparer<NineToNineOps> Members

        public bool Equals(NineToNineOpsStoresDispatchDecision x, NineToNineOpsStoresDispatchDecision y)
        {
            return x.OrderId == y.OrderId;
        }

        public int GetHashCode([DisallowNull] NineToNineOpsStoresDispatchDecision obj)
        {
            return obj.OrderId.GetHashCode();
        }

        #endregion
    }
    public struct InfoShopsErrors
    {
        public string StoreId;
        public string StoreName;
        public string StoreUrl;
        public string StorePhoneNumber;

        public string Error;

        public static List<object> Headers() => new List<object> { "StoreId", "StoreName", "StoreUrl", "StorePhoneNumber", "Error"};
        public List<object> ToStringList() => new List<object> { StoreId, StoreName, StoreUrl, StorePhoneNumber, Error};
        public bool HasErrors()
        {
            if (StorePhoneNumber==null)
            {
                Error = "Null Number";
                return true;
            }
            else
            {
                if (StorePhoneNumber == "Sin numero")
                {
                    Error = "Sin Numero";
                    return true;
                }
                else
                { 
                    if (!Regex.IsMatch(StorePhoneNumber, "^[0-9]+$"))
                    {
                        Error = "Caracteres especiales";
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

    }

    public struct NineToNineOps
    {
        public Store Store;
        public Order Order;

        public static List<object> Headers() => new List<object> { "Order Id", "Order Total", "Created On Utc","Store Id", "Store Name", "Store Url", "Store Phone Number", "Store Address" };
        public List<object> ToStringList() => new List<object> {Order.Id, Order.OrderTotal, Order.CreatedOnUtc.ToString("dd/MM/yyyy HH:mm:ss"), Store.Id, Store.Name, Store.Url, Store.CompanyPhoneNumber, Store.CompanyAddress};
   
    }
    public struct NineToNineOpsGMV
    {
        public int StoreId;
        public decimal OrderTotal;

        public static List<object> Headers() => new List<object> {"GMV Despacho","Store Id"};
        public List<object> ToStringList() => new List<object> { OrderTotal, StoreId };

    }

    public struct NineToNineOpsUnique
    {
        public int Store;
        public int UniqueCustomers;
        public int UniqueCustomers_Day_Before;
        public static DateTime Yesterday;
        public static DateTime Before_Yesterday;
        public static List<object> Headers() => new List<object> { "Store Id", "Unique Customers Day Before Yesterday ( Until "+ Before_Yesterday.ToString("dd/MM/yyyy HH:mm:ss") + "    UTC)", "Unique Customers Day Yesterday ( Until " + Yesterday.ToString("dd/MM/yyyy HH:mm:ss") + "    UTC)" };
        public List<object> ToStringList() => new List<object> { Store, UniqueCustomers_Day_Before , UniqueCustomers };

    }

    public struct MonitoringSaleAnalysis
    {
        public int OrderId;
        public int ProductId;
        public string Name;
        public decimal Cost;
        public decimal Price;

        public static List<object> Headers() => new List<object> { "Order Id", "Product Id", "Name", "Cost", "Price"};
        public List<object> ToStringList() => new List<object> { OrderId, ProductId, Name,Cost, Price};

    }

    public struct NineToNineOpsUniqueC10
    {
        public int Store;
        public int UniqueCustomers;
        public DateTime FirstBuy;
        public String CustomerPhone;
        public String CustomerIp;

        public static List<object> Headers() => new List<object> { "Store Id","Customer Phone Number","Customer IP", "FirstBuy"};
        public List<object> ToStringList() => new List<object> { Store, CustomerPhone, CustomerIp, FirstBuy.ToString("dd/MM/yyyy HH:mm:ss") };

    }
    public struct NineToNineOpsAllStores
    {
        public Store Store;

        public static List<object> Headers() => new List<object> { "Store Id", "Store Name", "Store Url", "Store Hosts", "Store Phone Number", "Store Address","Fase", "CreatedOnUtc"};
        public List<object> ToStringList() => new List<object> { Store.Id, Store.Name, Store.Url, Store.Hosts, Store.CompanyPhoneNumber, Store.CompanyAddress,Store.DisplayOrder, Store.CreatedOnUtc.ToString("dd/MM/yyyy HH:mm:ss")};

    }
    public struct NineToNineOpsStoresDispatch
    {
        public Store Store;

        public static List<object> Headers() => new List<object> { "Store Id", "Store Name", "Store Url", "Store Hosts", "Store Phone Number", "Store Address", "Fase", "Unique Customers" };
        public List<object> ToStringList() => new List<object> { Store.Id, Store.Name, Store.Url, Store.Hosts, Store.CompanyPhoneNumber, Store.CompanyAddress, Store.DisplayOrder };

    }
    public struct NineToNineOpsStoresDispatchDecision
    {
        public int StoreId;
        public decimal GMV;
        public int OrderId;
        public DateTime CreatedOnUtc;
        public decimal OrderTotal;
        public string Decision;
        public string Sku;
        public string ProductName;
        public int ProductQuantity;
        public decimal PerTaraRatio;
        public decimal CantidadTaras;
        public decimal BatchPrice;
        public string StoreName;
        public string Liberada;
        public static List<object> Headers() => new List<object> { "Store Id", "Store Name", "Order Id","Store Dispatch GMV", "Order Total","% of Store Dispatch GMV","CreatedOnUtc","Sku", "Nombre Producto", "Cantidad Producto", "Batch Price", "Porcentaje de la Orden" ,"PerTaraRatio", "Ocupacion % Tara"};
        public List<object> ToStringList() => new List<object> { StoreId, StoreName,OrderId, GMV, OrderTotal,(OrderTotal/GMV),CreatedOnUtc.ToString("dd/MM/yyyy HH:mm:ss"), Sku, ProductName, ProductQuantity, BatchPrice,(BatchPrice/OrderTotal),  PerTaraRatio, CantidadTaras};

    }

    public struct NineToNineOpsStoresDispatchOrderSummary
    {
        public int OrderId;
        public decimal OrderTotal;
        public decimal ProductQuantity;
        public decimal CantidadTaras;
        public int StoreId;
        public decimal GMV;

        public static List<object> Headers() => new List<object> { "Order Id","Order Total", "Cantidad Productos", "Cantidada Taras De La Orden","Store Id"};
        public List<object> ToStringList() => new List<object> { OrderId, OrderTotal , (int)ProductQuantity, CantidadTaras, StoreId };

    }

    public struct NineToNineOpsStoresDispatchTarasSymmary
    {
        public decimal GMV;
        public decimal OrdenPromedio;
        public decimal CantidadOrdenes;
        public decimal TaraPromedioPorOrden;
        public decimal CantidadTaras;
        public decimal CantidadProductosPromedioPorOrden;
        public decimal CantidadProductos;
        public decimal ProductQuantity;
        public int StoreId;

        public static List<object> Headers() => new List<object> { "GMV", "Orden Promedio", "Cantidad Ordenes", "Tara Promedio Por Orden", "Cantidad Taras", "Cantidad Productos Promedio Por Orden", "Cantidad Productos", "Store Id" };
        public List<object> ToStringList() => new List<object> { GMV,OrdenPromedio, CantidadOrdenes, TaraPromedioPorOrden, Math.Ceiling(CantidadTaras), CantidadProductosPromedioPorOrden,  CantidadProductos,  StoreId };

    }

    public struct LatLong
    {
        public int StoreId;
        public decimal Latitud;
        public decimal Longitud;
        public static List<object> Headers() => new List<object> { "Store Id","Longitud","Latitud"};
        public List<object> ToStringList() => new List<object> { StoreId,Longitud,Latitud };

    }
}