using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;

namespace Journals.Model
{
    [DebuggerDisplay("Status: {Status}")]
    public class OperationStatus
    {

        private bool? status;

        public bool Status
        {
            get
            {
                var result = status ?? RecordsAffected > 0;
                return result;
            }
            set { status = value; }
        }

        public int RecordsAffected { get; set; } = -1;
        public string Message { get; set; }
        public Object OperationID { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionStackTrace { get; set; }
        public string ExceptionInnerMessage { get; set; }
        public string ExceptionInnerStackTrace { get; set; }

        public IList<ValidationResult> ValidationErrors { get; } = new List<ValidationResult>();

        public static OperationStatus CreateFromException(string message, Exception ex)
        {            

            OperationStatus opStatus = new OperationStatus
            {
                Status = false,
                Message = message,
                OperationID = null
            };

            if (ex != null)
            {
                opStatus.ExceptionMessage = ex.Message;
                opStatus.ExceptionStackTrace = ex.StackTrace;
                opStatus.ExceptionInnerMessage = ex.InnerException?.Message;
                opStatus.ExceptionInnerStackTrace = ex.InnerException?.StackTrace;

                var vex = ex as DbEntityValidationException;

                if (vex != null)
                {

                    foreach (var entity in vex.EntityValidationErrors)
                    {
                        foreach (var properties in entity.ValidationErrors)
                        {
                            var entityName = entity.Entry.Entity.GetType().Name;
                            var isValid = entity.IsValid;

                            opStatus.ValidationErrors.Add(
                                new ValidationResult()
                                {
                                    Key = properties.PropertyName,
                                    Message = properties.ErrorMessage,
                                    Entity = entityName,
                                    IsValid = isValid
                                });
                        }
                    }
                }
            }
            return opStatus;
        }
    }

    public class OperationStatus<TResult> : OperationStatus
    {

        public TResult Result { get; set; }

    }
}