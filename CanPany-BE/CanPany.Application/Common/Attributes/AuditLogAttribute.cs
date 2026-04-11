namespace CanPany.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
public class AuditLogAttribute : Attribute
{
    public string EntityType { get; }
    public string I18nActionKey { get; }

    public AuditLogAttribute(string entityType, string i18nActionKey)
    {
        EntityType = entityType;
        I18nActionKey = i18nActionKey;
    }
}
