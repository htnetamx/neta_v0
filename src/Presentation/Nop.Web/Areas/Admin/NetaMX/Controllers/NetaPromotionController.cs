using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Promotion;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Promotion;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Promotion;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class NetaPromotionController : BaseAdminController
    {
        #region Fields
        private readonly INetaPromotionService _netaPromotionService;
        private readonly INetaPromotionModelFactory _netaPromotionModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        public NetaPromotionController(INetaPromotionService netaPromotionService,
            INetaPromotionModelFactory netaPromotionModelFactory,
            IPermissionService permissionService,
            INotificationService notificationService,
            ICategoryModelFactory categoryModelFactory,
            IProductService productService,
            ILocalizationService localizationService)
        {
            _netaPromotionService = netaPromotionService;
            _netaPromotionModelFactory = netaPromotionModelFactory;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _categoryModelFactory = categoryModelFactory;
            _productService = productService;
            _localizationService = localizationService;
        }
        #endregion

        #region Methods
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public virtual async Task<IActionResult> PromotionList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return await AccessDeniedDataTablesJson();

            var promotions = await _netaPromotionService.GetAllNetaPromotionAsync();
            var promotionModel = new List<NetaPromotionModel>();
            if (promotions != null)
            {
                foreach (var promotion in promotions)
                {
                    promotionModel.Add(new NetaPromotionModel()
                    {
                        Id = promotion.Id,
                        Name = promotion.Name,
                    });
                }
            }
            var model = new NetaPromotionListModel()
            {
                Data = promotionModel,
                RecordsTotal = promotions.Count,
                Draw = "1"
            };
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = await _netaPromotionModelFactory.PrepareNetaPromotionModelAsync(new NetaPromotionModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Create(NetaPromotionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var netaPromotion = new Neta_Promotion();
            if (ModelState.IsValid)
            {
                netaPromotion.Name = model.Name;
                netaPromotion.PictureId = model.PictureId;
                netaPromotion.StartDateUtc = model.StartDateUtc;
                netaPromotion.EndDateUtc = model.EndDateUtc;
                netaPromotion.Deleted = false;
                netaPromotion.Published = true;

                await _netaPromotionService.InsertNetaPromotionAsync(netaPromotion);

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = netaPromotion.Id });
            }
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var netaPromotion = await _netaPromotionService.GetNetaPromotionByIdAsync(id);
            if (netaPromotion == null || netaPromotion.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = await _netaPromotionModelFactory.PrepareNetaPromotionModelAsync(null, netaPromotion);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Edit(NetaPromotionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var netaPromotion = await _netaPromotionService.GetNetaPromotionByIdAsync(model.Id);
            if (netaPromotion == null || netaPromotion.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                try
                {
                    netaPromotion.Name = model.Name;
                    netaPromotion.PictureId = model.PictureId;
                    netaPromotion.StartDateUtc = model.StartDateUtc;
                    netaPromotion.EndDateUtc = model.EndDateUtc;
                    netaPromotion.Published = true;

                    await _netaPromotionService.UpdateNetaPromotionAsync(netaPromotion);

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("Edit", new { id = netaPromotion.Id });
                }
                catch (Exception exc)
                {
                    _notificationService.ErrorNotification(exc.Message);
                }
            }
            model = await _netaPromotionModelFactory.PrepareNetaPromotionModelAsync(model, netaPromotion);
            return View(model);
        }

        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var promotion = await _netaPromotionService.GetNetaPromotionByIdAsync(id)
                ?? throw new ArgumentException("No promotion mapping found with the specified id", nameof(id));

            await _netaPromotionService.DeleteNetaPromotionAsync(promotion);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Deleted"));

            return RedirectToAction("List");
        }
        #endregion

        #region Products

        [HttpPost]
        public virtual async Task<IActionResult> ProductList(PromotionProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return await AccessDeniedDataTablesJson();

            //try to get a category with the specified id
            var promotion = await _netaPromotionService.GetNetaPromotionByIdAsync(searchModel.PromotionId)
                ?? throw new ArgumentException("No promotion found with the specified id");

            //prepare model
            var model = await _netaPromotionModelFactory.PreparePromotionProductListModelAsync(searchModel, promotion);

            return Json(model);
        }

        public virtual async Task<IActionResult> ProductUpdate(PromotionProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
                        
            var promotionProduct = await _netaPromotionService.GetPromotionProductById(model.Id)
                ?? throw new ArgumentException("No promotion product mapping found with the specified id");

            //fill entity from product
            promotionProduct.DisplayOrder = model.DisplayOrder;
            promotionProduct.AllowToShowProductOnlyPromotion = model.AllowToShowProductOnlyPromotion;
            await _netaPromotionService.UpdatePromotionProductAsync(promotionProduct);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> ProductDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();
                        
            var promotionProduct = await _netaPromotionService.GetPromotionProductById(id)
                ?? throw new ArgumentException("No promotion product mapping found with the specified id", nameof(id));

            await _netaPromotionService.DeletePromotionProductAsync(promotionProduct);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> ProductAddPopup(int promotionId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //prepare model
            var model = await _categoryModelFactory.PrepareAddProductToCategorySearchModelAsync(new AddProductToCategorySearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAddPopupList(AddProductToCategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _categoryModelFactory.PrepareAddProductToCategoryListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> ProductAddPopup(AddProductToPromotionModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //get selected products
            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                var existingProductCategories = await _netaPromotionService.GetPromotionsProductsByPromotionIdAsync(model.PromotionId);
                foreach (var product in selectedProducts)
                {
                    //whether product category with such parameters already exists
                    if (_netaPromotionService.FindProductPromotion(existingProductCategories, product.Id, model.PromotionId) != null)
                        continue;

                    //insert the new product category mapping
                    await _netaPromotionService.InsertPromotionProductAsync(new Neta_Promotion_ProductMapping
                    {
                        Neta_PromotionId = model.PromotionId,
                        ProductId = product.Id,
                        DisplayOrder = 1,
                        AllowToShowProductOnlyPromotion = true
                    });
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddProductToCategorySearchModel());
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnPromtionProductSave")]
        public virtual async Task<IActionResult> ImportNetaPromotionProductExcel(IFormFile importexcelfile, IFormCollection form)
        {
            int.TryParse(form["promotionid"], out int promoId);
            try
            {               

                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await _netaPromotionService.ImportProductsFromXlsxAsync(importexcelfile.OpenReadStream(),promoId);
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));

                    return RedirectToAction("Edit", new { id = promoId });
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Imported"));

                return RedirectToAction("Edit", new { id = promoId });
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);

                return RedirectToAction("Edit", new { id = promoId });
            }
        }


        #endregion
    }
}
