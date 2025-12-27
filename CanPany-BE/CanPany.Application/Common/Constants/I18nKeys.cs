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
}


