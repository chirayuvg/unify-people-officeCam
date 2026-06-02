using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Unify.People.Services
{
    public class ServiceException : Exception
    {
        public ServiceException(string message) : base(message) { }

        public ServiceException(string message, Exception innerException) : base(message, innerException) { }

        public ProblemDetails GetProblemDetails()
        {
            //primary key sqlException.Number == 2627 || foreign key 547 || unique key = 2601
            if (this.InnerException is DbUpdateException)
            {
                //primary key sqlException.Number == 2627 || foreign key 547
                if (this.InnerException.InnerException != null && this.InnerException.InnerException is SqlException)
                {
                    SqlException sqlException = this.InnerException.InnerException as SqlException;
                    if (sqlException != null && (sqlException.Number == 2601))
                    {
                        ProblemDetails det = new ProblemDetails()
                        {
                            Status = 400,
                            Title = "Name already exists",
                            Detail = "Name already exists"
                        };
                        return det;
                    }
                    else if (sqlException != null && (sqlException.Number == 547))
                    {
                        ProblemDetails det = new ProblemDetails()
                        {
                            Status = 400,
                            Title = "Invalid input",
                            Detail = "Invalid input"
                        };
                        return det;
                    }
                }
            }

            ProblemDetails details = new ProblemDetails()
            {
                Status = 500,
                Title = this.Message,
                Detail = this.InnerException?.Message
            };
            return details;
        }
    }
}
