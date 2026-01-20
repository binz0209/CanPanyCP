namespace CanPany.Application.Common.Constants;

/// <summary>
/// I18N Keys - Định nghĩa tất cả các keys cho I18N
/// Pattern: [Type].[Module].[Action].[SubType]
/// </summary>
public static class I18nKeys
{
    public static class Error
    {
        public static class Common
        {
            public const string NotFound = "Error.Common.NotFound";
            public const string Unauthorized = "Error.Common.Unauthorized";
            public const string InternalServerError = "Error.Common.InternalServerError";
            public const string BadRequest = "Error.Common.BadRequest";
            public const string ValidationFailed = "Error.Common.ValidationFailed";
        }

        public static class User
        {
            public static class Register
            {
                public const string EmailExists = "Error.User.Register.EmailExists";
                public const string Failed = "Error.User.Register.Failed";
            }

            public static class Login
            {
                public const string InvalidCredentials = "Error.User.Login.InvalidCredentials";
                public const string Failed = "Error.User.Login.Failed";
            }

            public const string NotFound = "Error.User.NotFound";
            public const string UpdateFailed = "Error.User.Update.Failed";
            public const string DeleteFailed = "Error.User.Delete.Failed";

            public static class PasswordChange
            {
                public const string Failed = "Error.User.PasswordChange.Failed";
                public const string OldPasswordIncorrect = "Error.User.PasswordChange.OldPasswordIncorrect";
            }
        }

        public static class Job
        {
            public const string NotFound = "Error.Job.NotFound";
            public const string CreateFailed = "Error.Job.CreateFailed";
            public const string UpdateFailed = "Error.Job.UpdateFailed";
            public const string DeleteFailed = "Error.Job.DeleteFailed";
        }

        public static class CV
        {
            public const string NotFound = "Error.CV.NotFound";
            public const string CreateFailed = "Error.CV.CreateFailed";
            public const string UpdateFailed = "Error.CV.UpdateFailed";
            public const string DeleteFailed = "Error.CV.DeleteFailed";
        }
    }

    public static class Validation
    {
        public static class User
        {
            public const string FullNameRequired = "Validation.User.FullNameRequired";
            public const string EmailRequired = "Validation.User.EmailRequired";
            public const string EmailInvalid = "Validation.User.EmailInvalid";
            public const string PasswordRequired = "Validation.User.PasswordRequired";
            public const string PasswordMinLength = "Validation.User.PasswordMinLength";
        }

        public static class Job
        {
            public const string TitleRequired = "Validation.Job.TitleRequired";
            public const string DescriptionRequired = "Validation.Job.DescriptionRequired";
            public const string CompanyIdRequired = "Validation.Job.CompanyIdRequired";
        }
    }

    public static class Success
    {
        public static class User
        {
            public const string Register = "Success.User.Register";
            public const string Login = "Success.User.Login";
            public const string Update = "Success.User.Update";
        }

        public static class Job
        {
            public const string Create = "Success.Job.Create";
            public const string Update = "Success.Job.Update";
            public const string Delete = "Success.Job.Delete";
        }
    }

    public static class Logging
    {
        public static class User
        {
            public static class Register
            {
                public const string Start = "Logging.User.Register.Start";
                public const string Complete = "Logging.User.Register.Complete";
            }

            public static class Login
            {
                public const string Start = "Logging.User.Login.Start";
                public const string Complete = "Logging.User.Login.Complete";
            }
        }
    }

    /// <summary>
    /// I18N Keys for Interceptors - Audit, Security, Performance, Exception
    /// </summary>
    public static class Interceptor
    {
        public static class Audit
        {
            public const string HttpRequest = "Interceptor.Audit.HttpRequest";
            public const string ServiceCall = "Interceptor.Audit.ServiceCall";
            public const string JobExecution = "Interceptor.Audit.JobExecution";
            public const string HostedService = "Interceptor.Audit.HostedService";
            
            public static class Format
            {
                public const string HttpRequestSuccess = "Interceptor.Audit.Format.HttpRequestSuccess";
                public const string HttpRequestFailed = "Interceptor.Audit.Format.HttpRequestFailed";
                public const string ServiceCallSuccess = "Interceptor.Audit.Format.ServiceCallSuccess";
                public const string ServiceCallFailed = "Interceptor.Audit.Format.ServiceCallFailed";
                public const string JobStart = "Interceptor.Audit.Format.JobStart";
                public const string JobComplete = "Interceptor.Audit.Format.JobComplete";
                public const string JobFailed = "Interceptor.Audit.Format.JobFailed";
            }
        }

        public static class Security
        {
            public const string Authentication = "Interceptor.Security.Authentication";
            public const string Authorization = "Interceptor.Security.Authorization";
            public const string DataAccess = "Interceptor.Security.DataAccess";
            public const string TokenAccess = "Interceptor.Security.TokenAccess";
            public const string Exception = "Interceptor.Security.Exception";
            
            public static class Format
            {
                public const string AuthenticationSuccess = "Interceptor.Security.Format.AuthenticationSuccess";
                public const string AuthenticationFailed = "Interceptor.Security.Format.AuthenticationFailed";
                public const string AuthorizationDenied = "Interceptor.Security.Format.AuthorizationDenied";
                public const string UnauthorizedAccess = "Interceptor.Security.Format.UnauthorizedAccess";
            }
        }

        public static class Performance
        {
            public const string OperationTiming = "Interceptor.Performance.OperationTiming";
            public const string SlowOperation = "Interceptor.Performance.SlowOperation";
            
            public static class Format
            {
                public const string OperationCompleted = "Interceptor.Performance.Format.OperationCompleted";
                public const string SlowOperationWarning = "Interceptor.Performance.Format.SlowOperationWarning";
            }
        }

        public static class Exception
        {
            public const string Captured = "Interceptor.Exception.Captured";
            public const string Critical = "Interceptor.Exception.Critical";
            
            public static class Format
            {
                public const string ExceptionOccurred = "Interceptor.Exception.Format.ExceptionOccurred";
                public const string CriticalException = "Interceptor.Exception.Format.CriticalException";
            }
        }
    }
}


