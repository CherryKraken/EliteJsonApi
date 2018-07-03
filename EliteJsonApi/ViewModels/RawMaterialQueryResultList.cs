using EliteJsonApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.ViewModels
{
    public class RawMaterialQueryResultList : IQueryResultList
    {
        public string Type => "Raw";
        public int Page { get; set; }

        public List<IRawMaterialContainer> List { get; set; }

        public void Add(IRawMaterialContainer data)
        {
            List.Add(data);
        }

        public void AddAll(params IRawMaterialContainer[] list)
        {
            foreach (var data in list)
            {
                Add(data);
            }
        }

        public JsonResult ToJsonResult()
        {
            return new JsonResult(this);
        }
    }
}
