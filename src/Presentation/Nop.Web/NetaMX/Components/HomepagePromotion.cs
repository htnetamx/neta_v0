using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Media;
using Nop.Services.Promotion;
using Nop.Web.Framework.Components;
using Nop.Web.Models;

namespace Nop.Web.Components
{
    public class HomepagePromotionViewComponent : NopViewComponent
    {
        private readonly INetaPromotionService _netaPromotionService;
        private readonly IPictureService _pictureService;

        public HomepagePromotionViewComponent(INetaPromotionService netaPromotionService, IPictureService pictureService)
        {
            _netaPromotionService = netaPromotionService;
            _pictureService = pictureService;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var promo = await _netaPromotionService.GetAllNetaPromotionAsync();
            promo = promo.Where(p => p.StartDateUtc <= System.DateTime.Now && p.EndDateUtc > System.DateTime.Now).ToList();
            if (promo.Count > 0)
            {
                var model = new List<PromotionListModel>();
                foreach (var item in promo)
                {
                    var urlImage = string.Empty;
                    var picture = await _pictureService.GetPictureByIdAsync(item.PictureId);
                    var pictureurl = await _pictureService.GetPictureUrlAsync(picture);
                    if (!string.IsNullOrEmpty(pictureurl.Url) && pictureurl.Url != "")
                        urlImage = pictureurl.Url;

                    var promotionModel = new PromotionListModel();
                    promotionModel.Id = item.Id;
                    promotionModel.Name = item.Name;
                    promotionModel.PictureId = item.PictureId;
                    promotionModel.PictureURL = urlImage;
                    model.Add(promotionModel);
                }
                return View(model);
            }

            return Content("");
        }
    }
}