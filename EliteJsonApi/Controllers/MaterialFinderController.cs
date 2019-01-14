using EliteJsonApi.Data;
using EliteJsonApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EliteJsonApi.Controllers
{
    [Route("MaterialFinder")]
    public class MaterialFinderController : Controller
    {
        private readonly MaterialFinderQueryBuilder _finder;
        private readonly EliteDbContext _ctx;

        public MaterialFinderController(EliteDbContext context)
        {
            _ctx = context;
            _finder = new MaterialFinderQueryBuilder(_ctx);
        }

        [Route("Default")]
        public IActionResult Default()
        {
            return View();
        }

        [HttpGet("LoadPartial/{*delimitedMaterialList}")]
        public async Task<IActionResult> LoadPartial(string delimitedMaterialList)
        {
            var list = delimitedMaterialList.Split(',');
            var mats = _ctx.Material.Where(m => list.Contains(m.Name)).ToList();

            if (mats.All(mat => mat.Type == "Raw"))
            {
                var viewModel = _finder.FindRawMaterials(Request.Query, mats.Select(m => m.Name).ToArray());
                TempData["list"] = viewModel.List;
                return RedirectToAction("RawMaterialPartial");
            }

            if (!mats.Any(mat => mat.Type == "Raw"))
            {
                throw new NotImplementedException("Need to implement finder for artificial materials");
                var viewModels = new { };// _finder.(Request.Query, mats.Select(m => m.Name).ToArray());
                return RedirectToAction("ArtificialMaterialPartial", new { matNames = mats.Select(m => m.Name).ToArray() });
            }
            
            //using (var client = new HttpClient())
            //{
            //    HttpResponseMessage response = await client.GetAsync(Url.Link("RawMaterialPartial", list);
            //    if (response.IsSuccessStatusCode)
            //    {
            //        product = await response.Content.ReadAsAsync<RawMaterialBodyResult>();
            //    }
            //}

            return null;
        }

        [HttpGet]
        public PartialViewResult RawMaterialPartial()
        {
            if (!TempData.ContainsKey("list") || TempData["list"] == null)
                throw new Exception("Action cannot be called in this context.");
            return PartialView(TempData["list"] as IEnumerable<IRawMaterialContainer>);
        }

        [HttpGet]
        public PartialViewResult EncodedMaterialPartial(string matName)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public PartialViewResult ManufacturedMaterialPartial(string matName)
        {
            throw new NotImplementedException();
        }
    }
}
