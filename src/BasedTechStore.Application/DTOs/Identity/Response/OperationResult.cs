using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Application.DTOs.Identity.Response
{
    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public IEnumerable<string>? Errors { get; set; }

        public static OperationResult CreateSuccess() =>
            new OperationResult { IsSuccess = true };

        public static OperationResult CreateFailure(IEnumerable<string> errors) =>
            new OperationResult { IsSuccess = false, Errors = errors.ToList() };
    }
}
