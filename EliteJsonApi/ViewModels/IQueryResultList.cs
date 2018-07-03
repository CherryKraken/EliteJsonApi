using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.ViewModels
{
    public interface IQueryResultList
    {
        string Type { get; }
        int Page { get; set; }
    }
}
